package jsoncanonicalizer

import (
	"bufio"
	"math"
	"os"
	"strconv"
	"strings"
	"testing"

	"github.com/beeemT/Packages/fileutil"
	"github.com/stretchr/testify/require"
)

const testFile = "/home/user/test/es6testfile100m.txt"

const invalidNumber = "null"

type numberParsingTestCase struct {
	desc     string
	ieeeHex  string
	expected string
}

func TestNumberToJSON(t *testing.T) {
	r := require.New(t)

	testCases := []numberParsingTestCase{
		{
			ieeeHex:  "4340000000000001",
			expected: "9007199254740994",
		},
		{
			ieeeHex:  "4340000000000002",
			expected: "9007199254740996",
		},
		{
			ieeeHex:  "444b1ae4d6e2ef50",
			expected: "1e+21",
		},
		{
			ieeeHex:  "3eb0c6f7a0b5ed8d",
			expected: "0.000001",
		},
		{
			ieeeHex:  "3eb0c6f7a0b5ed8c",
			expected: "9.999999999999997e-7",
		},
		{
			ieeeHex:  "8000000000000000",
			expected: "0",
		},
		{
			ieeeHex:  "7fffffffffffffff",
			expected: invalidNumber,
		},
		{
			ieeeHex:  "7ff0000000000000",
			expected: invalidNumber,
		},
		{
			ieeeHex:  "fff0000000000000",
			expected: invalidNumber,
		},
	}

	exists, err := fileutil.Exists(testFile)
	r.NoErrorf(err, "Failed at testing if numbers test file exists: %s\n", err)
	if exists {
		file, err := os.Open(testFile)
		r.NoErrorf(err, "Failed at reading test sample file: %s\n", err)

		scanner := bufio.NewScanner(file)
		var lineCount int
		for scanner.Scan() {
			lineCount++
			line := scanner.Text()
			lineSl := strings.Split(line, ",")
			r.Equalf(len(lineSl), 2, "Failed because a comma is missing in line %v\n", lineCount)

			hex := lineSl[0]
			expected := lineSl[1]
			t.Run(expected, func(t *testing.T) {
				testNumberParsing(t, r, numberParsingTestCase{
					desc:     expected,
					ieeeHex:  hex,
					expected: expected,
				})
			})
		}
		r.NoErrorf(err, "Failed at scanning test file: %s\n", err)
		file.Close()

	}

	for _, tC := range testCases {
		t.Run(tC.desc, func(t *testing.T) {
			testNumberParsing(t, r, tC)
		})
	}
}

func testNumberParsing(t *testing.T, r *require.Assertions, tC numberParsingTestCase) {
	for len(tC.ieeeHex) < 16 {
		tC.ieeeHex = "0" + tC.ieeeHex
	}
	ieeeU64, err := strconv.ParseUint(tC.ieeeHex, 16, 64)
	r.NoErrorf(err, "Failed at parsing tC.ieeeHex [%s]: %s\n", tC.ieeeHex, err)

	ieeeF64 := math.Float64frombits(ieeeU64)
	es6Created, err := NumberToJSON(ieeeF64)
	if tC.expected == invalidNumber {
		r.Errorf(err, "Failed because parsing number to json did not return an error even though the expected JSON is null\n")
	} else {
		r.NoErrorf(err, "Failed at converting float to json: %s\n", err)
	}

	r.Equalf(tC.expected, es6Created, "Failed because converted number [%s] (tC.ieeeHex [%s]) does not match tC.expected [%s]\n", es6Created, tC.ieeeHex, tC.expected)
}
