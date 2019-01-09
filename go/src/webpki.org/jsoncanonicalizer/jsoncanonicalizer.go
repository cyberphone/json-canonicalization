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
    "strconv"
    "strings"
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

    var globalError error = nil
    var result string
    var index int = 0
    var jsonDataLength int = len(jsonData)

    var ParseElement func() string
    var ParseSimpleType func() string
    var ParseQuotedString func() string
    var ParseObject func() string
    var ParseArray func() string

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

    IsWhiteSpace := func(c byte) bool {
        return c == 0x20 || c == 0x0A || c == 0x0D || c == 0x09
    }

    NextChar := func() byte {
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

    Scan := func() byte {
        for {
            c := NextChar()
            if IsWhiteSpace(c) {
                continue;
            }
            return c
        }
    }

    ScanFor := func(expected byte) {
        c := Scan()
        if c != expected {
            setError("Expected '" + string(expected) + "' but got '" + string(c) + "'")
        }
    }

    TestNextNonWhiteSpaceChar := func() byte {
        save := index
        c := Scan()
        index = save
        return c
    }

    ParseElement = func() string {
        switch Scan() {
            case LEFT_CURLY_BRACKET:
                return ParseObject()

            case DOUBLE_QUOTE:
                return ParseQuotedString()

            case LEFT_BRACKET:
                return ParseArray()

            default:
                return ParseSimpleType()
        }
    }

    ParseQuotedString = func() string {
        var element strings.Builder
        return element.String()
    }

    ParseSimpleType = func() string {
        var token strings.Builder
        for globalError == nil {
            c := TestNextNonWhiteSpaceChar()
            if c == COMMA_CHARACTER || c == RIGHT_BRACKET || c == RIGHT_CURLY_BRACKET {
                break;
            }
            c = NextChar()
            if IsWhiteSpace(c) {
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

    ParseArray = func() string {
        var arrayData strings.Builder
        arrayData.WriteByte(LEFT_BRACKET)
        var next bool = false
        for TestNextNonWhiteSpaceChar() != RIGHT_BRACKET {
            if next {
                ScanFor(COMMA_CHARACTER)
                arrayData.WriteByte(COMMA_CHARACTER)
            } else {
                next = true
            }
            arrayData.WriteString(ParseElement())
        }
        Scan()
        arrayData.WriteByte(RIGHT_BRACKET)
        return arrayData.String()
    }

    ParseObject = func() string {
        values :=list.New()
        var next bool = false
        for TestNextNonWhiteSpaceChar() != RIGHT_CURLY_BRACKET {
            if next {
                ScanFor(COMMA_CHARACTER)
            }
            next = true
            ScanFor(DOUBLE_QUOTE)
            name := ParseQuotedString()
            sortKey := utf16.Encode([]rune(name))
            ScanFor(COLON_CHARACTER)
            entry := keyEntry{name, sortKey, ParseElement()}
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
        Scan()
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

    if TestNextNonWhiteSpaceChar() == LEFT_BRACKET {
        Scan()
        result = ParseArray()
    } else {
        ScanFor(LEFT_CURLY_BRACKET)
        result = ParseObject()
    }
    for index < jsonDataLength {
        if !IsWhiteSpace(jsonData[index]) {
            setError("Improperly terminated JSON object")
            break;
        }
        index++
    }
    return result, globalError
}