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

// This package transforms JSON data in UTF-8 according to:
// https://tools.ietf.org/html/draft-rundgren-json-canonicalization-scheme-02

package jsoncanonicalizer

import (
	"container/list"
	"errors"
	"fmt"
	"strconv"
	"strings"
	"unicode/utf16"
)

type nameValueType struct {
	name    string
	sortKey []uint16
	value   string
}

// JSON standard escapes (modulo \u)
var asciiEscapes = []byte{'\\', '"', 'b', 'f', 'n', 'r', 't'}
var binaryEscapes = []byte{'\\', '"', '\b', '\f', '\n', '\r', '\t'}

// JSON literals
var literals = []string{"true", "false", "null"}

func Transform(jsonData []byte) ([]byte, error) {
	// JSON data MUST be UTF-8 encoded
	// Current pointer in jsonData
	var index int

	//TODO: replace this with parse element which keeps current functionality for parse array and object
	//but adds parsing of simple types and strings
	transformed, err := parseElement(jsonData, &index)
	if err != nil {
		return nil, err
	}

	for index < len(jsonData) {
		if !isWhiteSpace(jsonData[index]) {
			return nil, errors.New("Improperly terminated JSON object")
		}
		index++
	}
	return []byte(transformed), err
}

func isWhiteSpace(c byte) bool {
	return c == 0x20 || c == 0x0a || c == 0x0d || c == 0x09
}

func nextChar(jsonData []byte, index *int) (byte, error) {
	if *index < len(jsonData) {
		c := jsonData[*index]
		if c > 0x7f {
			return 0, errors.New("Unexpected non-ASCII character")
		}
		*index++
		return c, nil
	}
	return 0, errors.New("Unexpected EOF reached")
}

//scan advances index on jsonData to the first non whitespace character and returns it.
func scan(jsonData []byte, index *int) (byte, error) {
	for {
		c, err := nextChar(jsonData, index)
		if err != nil {
			return 0, err
		}

		if isWhiteSpace(c) {
			continue
		}

		return c, nil
	}
}

func scanFor(jsonData []byte, index *int, expected byte) error {
	c, err := scan(jsonData, index)
	if err != nil {
		return err
	}
	if c != expected {
		return fmt.Errorf("Expected %s but got %s", string(expected), string(c))
	}
	return nil
}

func getUEscape(jsonData []byte, index *int) (rune, error) {
	start := *index
	for i := 0; i < 4; i++ {
		_, err := nextChar(jsonData, index)
		if err != nil {
			return 0, err
		}
	}

	u16, err := strconv.ParseUint(string(jsonData[start:*index]), 16, 64)
	if err != nil {
		return 0, err
	}
	return rune(u16), nil
}

func decorateString(rawUTF8 string) string {
	var quotedString strings.Builder
	quotedString.WriteByte('"')

CoreLoop:
	for _, c := range []byte(rawUTF8) {
		// Is this within the JSON standard escapes?
		for i, esc := range binaryEscapes {
			if esc == c {
				quotedString.WriteByte('\\')
				quotedString.WriteByte(asciiEscapes[i])

				continue CoreLoop
			}
		}
		if c < 0x20 {
			// Other ASCII control characters must be escaped with \uhhhh
			quotedString.WriteString(fmt.Sprintf("\\u%04x", c))
		} else {
			quotedString.WriteByte(c)
		}
	}
	quotedString.WriteByte('"')

	return quotedString.String()
}

func parseQuotedString(jsonData []byte, index *int) (string, error) {
	var rawString strings.Builder

CoreLoop:
	for {
		var c byte
		if *index < len(jsonData) {
			c = jsonData[*index]
			*index++
		} else {
			return "", errors.New("Unexpected EOF reached")
		}

		if c == '"' {
			break
		}

		if c < ' ' {
			return "", errors.New("Unterminated string literal")
		} else if c == '\\' {
			// Escape sequence
			c, err := nextChar(jsonData, index)
			if err != nil {
				return "", err
			}

			if c == 'u' {
				// The \u escape
				firstUTF16, err := getUEscape(jsonData, index)
				if err != nil {
					return "", err
				}

				if utf16.IsSurrogate(firstUTF16) {
					// If the first UTF-16 code unit has a certain value there must be
					// another succeeding UTF-16 code unit as well
					backslash, err := nextChar(jsonData, index)
					if err != nil {
						return "", err
					}
					u, err := nextChar(jsonData, index)
					if err != nil {
						return "", err
					}

					if backslash != '\\' || u != 'u' {
						return "", errors.New("Missing surrogate")
					}

					// Output the UTF-32 code point as UTF-8
					uEscape, err := getUEscape(jsonData, index)
					if err != nil {
						return "", err
					}
					rawString.WriteRune(utf16.DecodeRune(firstUTF16, uEscape))

				} else {
					// Single UTF-16 code identical to UTF-32.  Output as UTF-8
					rawString.WriteRune(firstUTF16)
				}
			} else if c == '/' {
				// Benign but useless escape
				rawString.WriteByte('/')
			} else {
				// The JSON standard escapes
				for i, esc := range asciiEscapes {
					if esc == c {
						rawString.WriteByte(binaryEscapes[i])
						continue CoreLoop
					}
				}
				return "", fmt.Errorf("Unexpected escape: \\%s", string(c))
			}
		} else {
			// Just an ordinary ASCII character alternatively a UTF-8 byte
			// outside of ASCII.
			// Note that properly formatted UTF-8 never clashes with ASCII
			// making byte per byte search for ASCII break characters work
			// as expected.
			rawString.WriteByte(c)
		}
	}

	return rawString.String(), nil
}

func parseSimpleType(jsonData []byte, index *int) (string, error) {
	var token strings.Builder

	*index--
	//no condition is needed here.
	//if the buffer reaches EOF scan returns an error, or we terminate because the
	//json simple type terminates
	for {
		c, err := scan(jsonData, index)
		if err != nil {
			return "", err
		}

		if c == ',' || c == ']' || c == '}' {
			*index--
			break
		}

		token.WriteByte(c)
	}

	if token.Len() == 0 {
		return "", errors.New("Missing argument")
	}

	value := token.String()
	// Is it a JSON literal?
	for _, literal := range literals {
		if literal == value {
			return literal, nil
		}
	}

	// Apparently not so we assume that it is a I-JSON number
	ieeeF64, err := strconv.ParseFloat(value, 64)
	if err != nil {
		return "", err
	}

	value, err = NumberToJSON(ieeeF64)
	if err != nil {
		return "", err
	}

	return value, nil
}

func parseElement(jsonData []byte, index *int) (string, error) {
	c, err := scan(jsonData, index)
	if err != nil {
		return "", err
	}

	switch c {
	case '{':
		return parseObject(jsonData, index)
	case '"':
		str, err := parseQuotedString(jsonData, index)
		if err != nil {
			return "", err
		}
		return decorateString(str), nil
	case '[':
		return parseArray(jsonData, index)
	default:
		return parseSimpleType(jsonData, index)
	}
}

func peek(jsonData []byte, index *int) (byte, error) {
	c, err := scan(jsonData, index)
	if err != nil {
		return 0, err
	}

	*index--
	return c, nil
}

func parseArray(jsonData []byte, index *int) (string, error) {
	var arrayData strings.Builder
	var next bool

	arrayData.WriteByte('[')

	for {
		c, err := peek(jsonData, index)
		if err != nil {
			return "", err
		}

		if c == ']' {
			*index++
			break
		}

		if next {
			err = scanFor(jsonData, index, ',')
			if err != nil {
				return "", err
			}
			arrayData.WriteByte(',')
		} else {
			next = true
		}

		element, err := parseElement(jsonData, index)
		if err != nil {
			return "", err
		}
		arrayData.WriteString(element)
	}

	arrayData.WriteByte(']')
	return arrayData.String(), nil
}

func lexicographicallyPrecedes(sortKey []uint16, e *list.Element) (bool, error) {
	// Find the minimum length of the sortKeys
	oldSortKey := e.Value.(nameValueType).sortKey
	minLength := len(oldSortKey)
	if minLength > len(sortKey) {
		minLength = len(sortKey)
	}
	for q := 0; q < minLength; q++ {
		diff := int(sortKey[q]) - int(oldSortKey[q])
		if diff < 0 {
			// Smaller => Precedes
			return true, nil
		} else if diff > 0 {
			// Bigger => No match
			return false, nil
		}
		// Still equal => Continue
	}
	// The sortKeys compared equal up to minLength
	if len(sortKey) < len(oldSortKey) {
		// Shorter => Precedes
		return true, nil
	}
	if len(sortKey) == len(oldSortKey) {
		return false, fmt.Errorf("Duplicate key: %s", e.Value.(nameValueType).name)
	}
	// Longer => No match
	return false, nil
}

func parseObject(jsonData []byte, index *int) (string, error) {
	nameValueList := list.New()
	var next bool = false
CoreLoop:
	for {
		c, err := peek(jsonData, index)
		if err != nil {
			return "", err
		}

		if c == '}' {
			//advance index because of peeked '}'
			*index++
			break
		}

		if next {
			err = scanFor(jsonData, index, ',')
			if err != nil {
				return "", err
			}
		}
		next = true

		err = scanFor(jsonData, index, '"')
		if err != nil {
			return "", err
		}
		rawUTF8, err := parseQuotedString(jsonData, index)
		if err != nil {
			break
		}
		// Sort keys on UTF-16 code units
		// Since UTF-8 doesn't have endianess this is just a value transformation
		// In the Go case the transformation is UTF-8 => UTF-32 => UTF-16
		sortKey := utf16.Encode([]rune(rawUTF8))
		err = scanFor(jsonData, index, ':')
		if err != nil {
			return "", err
		}

		element, err := parseElement(jsonData, index)
		if err != nil {
			return "", err
		}
		nameValue := nameValueType{rawUTF8, sortKey, element}
		for e := nameValueList.Front(); e != nil; e = e.Next() {
			// Check if the key is smaller than a previous key
			if precedes, err := lexicographicallyPrecedes(sortKey, e); err != nil {
				return "", err
			} else if precedes {
				// Precedes => Insert before and exit sorting
				nameValueList.InsertBefore(nameValue, e)
				continue CoreLoop
			}
			// Continue searching for a possibly succeeding sortKey
			// (which is straightforward since the list is ordered)
		}
		// The sortKey is either the first or is succeeding all previous sortKeys
		nameValueList.PushBack(nameValue)
	}

	// Now everything is sorted so we can properly serialize the object
	var objectData strings.Builder
	objectData.WriteByte('{')
	next = false
	for e := nameValueList.Front(); e != nil; e = e.Next() {
		if next {
			objectData.WriteByte(',')
		}
		next = true
		nameValue := e.Value.(nameValueType)
		objectData.WriteString(decorateString(nameValue.name))
		objectData.WriteByte(':')
		objectData.WriteString(nameValue.value)
	}
	objectData.WriteByte('}')
	return objectData.String(), nil
}
