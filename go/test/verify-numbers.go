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
 
// This program tests the JSON number serializer using both a few discrete
// values as well as the 100 million value test suite

package main

import (
    "bufio"
    "strings"
    "strconv"
    "math"
    "fmt"
    "os"
    "webpki.org/jsoncanonicalizer"
)

func check(e error) {
    if e != nil {
        panic(e)
    }
}

// Change the file name to suit your environment
const testFile = "c:\\es6\\numbers\\es6testfile100m.txt"

const invalidNumber = "null"

var conversionErrors int = 0

func verify(ieeeHex string, expected string) {
    for len(ieeeHex) < 16 {
        ieeeHex = "0" + ieeeHex
    }
    ieeeU64, err := strconv.ParseUint(ieeeHex, 16, 64)
    check(err)
    ieeeF64 := math.Float64frombits(ieeeU64)
    es6Created, err := jsoncanonicalizer.NumberToJSON(ieeeF64)
    if expected == invalidNumber {
        if err == nil {
            panic("Missing error")
        }
        return
    } else {
        check(err);
    }
    if es6Created != expected {
        conversionErrors++
        fmt.Println("\n" + ieeeHex)
        fmt.Println(es6Created)
        fmt.Println(expected)
    }
    esParsed, err := strconv.ParseFloat(expected, 64)
    check(err)
    if esParsed != ieeeF64 {
        panic("Parsing error ieeeHex: " + ieeeHex + " expected: " + expected)
    }
}

func main() {
    verify("4340000000000001", "9007199254740994")
    verify("4340000000000002", "9007199254740996")
    verify("444b1ae4d6e2ef50", "1e+21")
    verify("3eb0c6f7a0b5ed8d", "0.000001")
    verify("3eb0c6f7a0b5ed8c", "9.999999999999997e-7")
    verify("8000000000000000", "0")
    verify("7fffffffffffffff", invalidNumber)
    verify("7ff0000000000000", invalidNumber)
    verify("fff0000000000000", invalidNumber)

    file, err := os.Open(testFile)
    check(err)
    defer file.Close()
    scanner := bufio.NewScanner(file)
    var lineCount int = 0
    for scanner.Scan() {
        lineCount++
        if lineCount % 1000000 == 0 {
            fmt.Printf("line: %d\n", lineCount)
        }
        line := scanner.Text()
        comma := strings.IndexByte(line, ',')
        if comma <= 0 {
            panic("Missing comma!")
        }
        verify(line[:comma], line[comma + 1:])
    }
    check(scanner.Err())
    if conversionErrors == 0 {
        fmt.Printf("\nSuccessful Operation. Lines read: %d\n", lineCount)
    } else {
        fmt.Printf("\n****** ERRORS: %d *******\n", conversionErrors)
    }
}

