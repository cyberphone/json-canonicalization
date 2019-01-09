//
//  Copyright 2006-2019 WebPKI.org (http://webpki.org).
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      https://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
//
 
// This package transforms JSON data in UTF-8 according to:
// https://tools.ietf.org/html/draft-rundgren-json-canonicalization-scheme-02

package jsoncanonicalizer

import (
    "errors"
    "container/list"
    "fmt"
    "strconv"
    "strings"
    "unicode/utf8"
    "unicode/utf16"
    "webpki.org/es6numfmt"
)

type keyEntry struct {
    name string
    sortKey []uint16
    value string
}

func Transform(jsonData []byte) (res string, e error) {

    const LEFT_CURLY_BRACKET byte  = '{'
    const RIGHT_CURLY_BRACKET byte = '}'
    const DOUBLE_QUOTE byte        = '"'
    const COLON_CHARACTER byte     = ':'
    const LEFT_BRACKET byte        = '['
    const RIGHT_BRACKET byte       = ']'
    const COMMA_CHARACTER byte     = ','
    const BACK_SLASH byte          = '\\'

    var ASC_ESCAPES = []byte{'\\', '"', 'b',  'f',  'n',  'r',  't'}
    var BIN_ESCAPES = []byte{'\\', '"', '\b', '\f', '\n', '\r', '\t'}

    var globalError error = nil
    var result string
    var index int = 0
    var jsonDataLength int = len(jsonData)

    var parseElement func() string
    var parseSimpleType func() string
    var parseQuotedString func() string
    var parseObject func() string
    var parseArray func() string

    setError := func(msg string) {
        if globalError == nil {
            globalError = errors.New(msg)
        }
    }

    checkError := func(e error) {
        if globalError == nil {
            globalError = e
        }
    }
    
    isWhiteSpace := func(c byte) bool {
        return c == 0x20 || c == 0x0A || c == 0x0D || c == 0x09
    }

    nextChar := func() byte {
        if index < jsonDataLength {
            c := jsonData[index]
            if c > 127 {
                setError("Unexpected non-ASCII character")
            }
            index++
            return c
        }
        setError("Unexpected EOF reached")
        return DOUBLE_QUOTE
    }

    scan := func() byte {
        for {
            c := nextChar()
            if isWhiteSpace(c) {
                continue;
            }
            return c
        }
    }

    scanFor := func(expected byte) {
        c := scan()
        if c != expected {
            setError("Expected '" + string(expected) + "' but got '" + string(c) + "'")
        }
    }

    getU4 := func() rune {
        start :=index
        nextChar()
        nextChar()
        nextChar()
        nextChar()
        if globalError != nil {
            return 0
        }
        u16, err := strconv.ParseUint(string(jsonData[start:index]), 16, 64)
        checkError(err)
        return rune(u16)
    }

    testNextNonWhiteSpaceChar := func() byte {
        save := index
        c := scan()
        index = save
        return c
    }

    parseElement = func() string {
        switch scan() {
            case LEFT_CURLY_BRACKET:
                return parseObject()

            case DOUBLE_QUOTE:
                return parseQuotedString()

            case LEFT_BRACKET:
                return parseArray()

            default:
                return parseSimpleType()
        }
    }

    parseQuotedString = func() string {
        var quotedString strings.Builder
        quotedString.WriteByte(DOUBLE_QUOTE)
        for globalError == nil {
            var c byte
            if index < jsonDataLength {
                c = jsonData[index]
                index++
            } else {
                nextChar()
                break
            }
            if (c == DOUBLE_QUOTE) {
                break;
            }
            if c < ' ' {
                setError("Unterminated string literal")
            } else if c > 127 {
                index--;
                r, size := utf8.DecodeRune(jsonData[index:])
                quotedString.WriteRune(r)
                index += size;
            } else if c == BACK_SLASH {
                c = nextChar()
                if c == 'u' {
                    firstUTF16 := getU4()
                    if utf16.IsSurrogate(firstUTF16) {
                    } else {
                        for i, esc := range BIN_ESCAPES {
                            if rune(esc) == firstUTF16 {
                                quotedString.WriteByte('\\')
                                quotedString.WriteByte(ASC_ESCAPES[i])
                                goto DoneWithValueEscaping
                            }
                        }
                        if firstUTF16 < ' ' {
                            quotedString.WriteString(fmt.Sprintf("\\u%04x", firstUTF16))
                        } else {
                            quotedString.WriteRune(firstUTF16)
                        }
                      DoneWithValueEscaping:
                    }
                } else if c == '/' {
                    quotedString.WriteByte('/')
                } else {
                    quotedString.WriteByte('\\')
                    for _, esc := range ASC_ESCAPES {
                        if esc == c {
                            quotedString.WriteByte(c)
                            goto DoneWithStdEscaping
                        }
                    }
                    setError("Unsupported escape:" + string(c))
                  DoneWithStdEscaping:
                }
            } else {
                quotedString.WriteByte(c)
            }
        }
        quotedString.WriteByte(DOUBLE_QUOTE)
        return quotedString.String()
    }

    parseSimpleType = func() string {
        var token strings.Builder
        index--
        for globalError == nil {
            c := testNextNonWhiteSpaceChar()
            if c == COMMA_CHARACTER || c == RIGHT_BRACKET || c == RIGHT_CURLY_BRACKET {
                break;
            }
            c = nextChar()
            if isWhiteSpace(c) {
                break
            }
            token.WriteByte(c)
        }
        if token.Len() == 0 {
            setError("Missing argument")
        }
        value := token.String()
        for _, literal := range []string{"true", "false", "null"} {
            if literal == value {
                return literal
            }
        }
        ieeeF64, err := strconv.ParseFloat(value, 64)
        checkError(err)
        value, err = es6numfmt.Convert(ieeeF64)
        checkError(err)
        return value
    }

    parseArray = func() string {
        var arrayData strings.Builder
        arrayData.WriteByte(LEFT_BRACKET)
        var next bool = false
        for globalError == nil && testNextNonWhiteSpaceChar() != RIGHT_BRACKET {
            if next {
                scanFor(COMMA_CHARACTER)
                arrayData.WriteByte(COMMA_CHARACTER)
            } else {
                next = true
            }
            arrayData.WriteString(parseElement())
        }
        scan()
        arrayData.WriteByte(RIGHT_BRACKET)
        return arrayData.String()
    }

    parseObject = func() string {
        values :=list.New()
        var next bool = false
        for globalError == nil && testNextNonWhiteSpaceChar() != RIGHT_CURLY_BRACKET {
            if next {
                scanFor(COMMA_CHARACTER)
            }
            next = true
            scanFor(DOUBLE_QUOTE)
            name := parseQuotedString()
            sortKey := utf16.Encode([]rune(name))
            scanFor(COLON_CHARACTER)
            entry := keyEntry{name, sortKey, parseElement()}
             for e := values.Front(); e != nil; e = e.Next() {
                oldSortKey := e.Value.(keyEntry).sortKey
                l := len(oldSortKey)
                if l > len(sortKey) {
                    l = len(sortKey)
                }
                for q := 0; q < l; q++ {
                    diff := int(sortKey[q]) - int(oldSortKey[q])
                    if diff < 0 {
                        values.InsertBefore(entry, e)
                        goto DoneSorting
                    } else if diff > 0 {
                        goto NextTurnPlease
                    }
                }
                if len(sortKey) < len(oldSortKey) {
                    values.InsertBefore(entry, e)
                    goto DoneSorting
                }
              NextTurnPlease:
            }
            values.PushBack(entry)
          DoneSorting:
        }
        scan()
        var objectData strings.Builder
        next = false
        objectData.WriteByte(LEFT_CURLY_BRACKET)
        for e := values.Front(); e != nil; e = e.Next() {
            if next {
                objectData.WriteByte(COMMA_CHARACTER)
            }
            next = true
            entry := e.Value.(keyEntry)
            objectData.WriteString(entry.name)
            objectData.WriteByte(COLON_CHARACTER)
            objectData.WriteString(entry.value)
        }
        objectData.WriteByte(RIGHT_CURLY_BRACKET)
        return objectData.String()
    }

    if testNextNonWhiteSpaceChar() == LEFT_BRACKET {
        scan()
        result = parseArray()
    } else {
        scanFor(LEFT_CURLY_BRACKET)
        result = parseObject()
    }
    for index < jsonDataLength {
        if !isWhiteSpace(jsonData[index]) {
            setError("Improperly terminated JSON object")
            break;
        }
        index++
    }
    return result, globalError
}