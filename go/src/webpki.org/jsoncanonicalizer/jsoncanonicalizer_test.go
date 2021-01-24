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
	"bytes"
	"fmt"
	"io/ioutil"
	"path/filepath"
	"testing"

	"github.com/stretchr/testify/require"
)

const (
	pathTestData                 = "../../../../testdata"
	pathInputRelativeToTestData  = "/input"
	pathOutputRelativeToTestData = "/output"
)

func failedBecause(errormsg string) string {
	return fmt.Sprintf("Failed because %s", errormsg)
}

func errorOccurred(activity string, err error) string {
	return failedBecause(fmt.Sprintf("an error occurred while %s: %s\n", activity, err))
}

func doesNotMatchExpected(expectedField, expectedValue, actualField, actualValue string) string {
	return failedBecause(fmt.Sprintf("%s [%s] does not match expected %s [%s]\n", actualField, actualValue, expectedField, expectedValue))
}

func TestTransform(t *testing.T) {
	r := require.New(t)

	testCases := []struct {
		desc     string
		filename string
	}{
		{
			desc:     "Arrays",
			filename: "arrays.json",
		},
		{
			desc:     "French",
			filename: "french.json",
		},
		{
			desc:     "Structures",
			filename: "structures.json",
		},
		{
			desc:     "Unicode",
			filename: "unicode.json",
		},
		{
			desc:     "Values",
			filename: "values.json",
		},
		{
			desc:     "Weird",
			filename: "weird.json",
		},
	}
	for _, tC := range testCases {
		t.Run(tC.desc, func(t *testing.T) {
			input, err := ioutil.ReadFile(filepath.Join(pathTestData,
				pathInputRelativeToTestData, tC.filename))
			r.NoError(err, errorOccurred("reading test input json", err))

			output, err := ioutil.ReadFile(filepath.Join(pathTestData,
				pathOutputRelativeToTestData, tC.filename))
			r.NoError(err, errorOccurred("reading expected transformed output sample", err))

			transformed, err := Transform(input)
			r.NoError(err, errorOccurred("transforming test input", err))

			r.True(bytes.Equal(transformed, output), doesNotMatchExpected("JSON", string(output), "transformed JSON", string(transformed)))
		})
	}
}
