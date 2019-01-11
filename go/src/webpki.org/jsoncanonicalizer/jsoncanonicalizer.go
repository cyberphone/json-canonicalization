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

func Transform(jsonData []byte) (result []byte, e error) {

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
    var LITERALS    = []string{"true", "false", "null"}

    var globalError error = nil
    var transformed string
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

    getUEscape := func() rune {
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
      CoreLoop:
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
                // Quoted strings are the only tokens that may contain non-ASCII characters
                index--;
                r, size := utf8.DecodeRune(jsonData[index:])
                quotedString.WriteRune(r)
                index += size;
            } else if c == BACK_SLASH {
                // Escape sequence
                c = nextChar()
                if c == 'u' {
                    // The \u escape
                    firstUTF16 := getUEscape()
                    if utf16.IsSurrogate(firstUTF16) {
                        // If the first UTF16 code unit has a certain value there must be
                        // another succeeding UTF16 code unit as well
                        if nextChar() != '\\' || nextChar() != 'u' {
                            setError("Surrogate expected")
                        } else {
                            // Output the UTF-32 code point as UTF-8
                            quotedString.WriteRune(utf16.DecodeRune(firstUTF16, getUEscape()))
                        }
                    } else {
                        // Single UTF16 code is identical to UTF32
                        // Now the value must be checked
                        for i, esc := range BIN_ESCAPES {
                            // Is this within the JSON standard escapes
                            if rune(esc) == firstUTF16 {
                                quotedString.WriteByte('\\')
                                quotedString.WriteByte(ASC_ESCAPES[i])
                                continue CoreLoop
                            }
                        }
                        if firstUTF16 < ' ' {
                            // Control characters must be escaped
                            quotedString.WriteString(fmt.Sprintf("\\u%04x", firstUTF16))
                        } else {
                            // Not control, output code unit as is but UTF-8 encoded
                            quotedString.WriteRune(firstUTF16)
                        }
                    }
                } else if c == '/' {
                    // Benign but useless escape
                    quotedString.WriteByte('/')
                } else {
                    // The JSON standard escapes
                    quotedString.WriteByte('\\')
                    for _, esc := range ASC_ESCAPES {
                        if esc == c {
                            quotedString.WriteByte(c)
                            continue CoreLoop
                        }
                    }
                    setError("Unexpected escape: \\" + string(c))
                }
            } else {
                // Just an ordinary ASCII character
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
        // Is it a JSON literal?
        for _, literal := range LITERALS {
            if literal == value {
                return literal
            }
        }
        // Apparently not so we assume that it is a I-JSON number
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
        nameValueList := list.New()
        var next bool = false
      ParsingLoop:
        for globalError == nil && testNextNonWhiteSpaceChar() != RIGHT_CURLY_BRACKET {
            if next {
                scanFor(COMMA_CHARACTER)
            }
            next = true
            scanFor(DOUBLE_QUOTE)
            name := parseQuotedString()
            if globalError != nil {
                break;
            }
            // Sort keys on UTF-16 code units
            // Since UTF-8 doesn't have endianess this is just a value transformation
            sortKey := utf16.Encode([]rune(name[1:len(name) - 1]))
            scanFor(COLON_CHARACTER)
            nameValue := keyEntry{name, sortKey, parseElement()}
          SortingLoop:
            for e := nameValueList.Front(); e != nil; e = e.Next() {
                // Check if the key is smaller than a previous key
                oldSortKey := e.Value.(keyEntry).sortKey
                // Find the minimum length of the sortKeys
                minLength := len(oldSortKey)
                if minLength > len(sortKey) {
                    minLength = len(sortKey)
                }
                for q := 0; q < minLength; q++ {
                    diff := int(sortKey[q]) - int(oldSortKey[q])
                    if diff < 0 {
                        // Smaller => Insert before and exit sorting
                        nameValueList.InsertBefore(nameValue, e)
                        continue ParsingLoop
                    } else if diff > 0 {
                        // Bigger => Continue searching for a possibly even bigger sortKey
                        // (which is straightforward since the list is ordered)
                        continue SortingLoop
                    }
                    // Still equal => Continue
                }
                // The sortKeys compared equal up to minLength
                // Shorter => Smaller => Insert before and exit sorting
                if len(sortKey) < len(oldSortKey) {
                    nameValueList.InsertBefore(nameValue, e)
                    continue ParsingLoop
                }
                if len(sortKey) == len(oldSortKey) {
                    setError("Duplicate key: " + name)
                }
            }
            // The sortKey is either the first or bigger than any previous sortKey
            nameValueList.PushBack(nameValue)
        }
        scan()
        // Now everything is sorted so we can properly serialize the object
        var objectData strings.Builder
        next = false
        objectData.WriteByte(LEFT_CURLY_BRACKET)
        for e := nameValueList.Front(); e != nil; e = e.Next() {
            if next {
                objectData.WriteByte(COMMA_CHARACTER)
            }
            next = true
            nameValue := e.Value.(keyEntry)
            objectData.WriteString(nameValue.name)
            objectData.WriteByte(COLON_CHARACTER)
            objectData.WriteString(nameValue.value)
        }
        objectData.WriteByte(RIGHT_CURLY_BRACKET)
        return objectData.String()
    }

    if testNextNonWhiteSpaceChar() == LEFT_BRACKET {
        scan()
        transformed = parseArray()
    } else {
        scanFor(LEFT_CURLY_BRACKET)
        transformed = parseObject()
    }
    for index < jsonDataLength {
        if !isWhiteSpace(jsonData[index]) {
            setError("Improperly terminated JSON object")
            break;
        }
        index++
    }
    return []byte(transformed), globalError
}