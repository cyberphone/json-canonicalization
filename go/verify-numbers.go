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
    "webpki.org/es6numfmt"
)

    
func check(e error) {
    if e != nil {
        panic(e)
    }
}

func verify(hex string, expected string, pass bool) {
    for len(hex) < 16 {
        hex = "0" + hex
    }
    ieeeU64, err := strconv.ParseUint(hex, 16, 64)
    check(err)
    ieeeF64 := math.Float64frombits(ieeeU64)
    es6Created, err := es6numfmt.Convert(ieeeF64)
    if pass {
        check(err);
    } else {
        if err == nil {
            panic("Missing error")
        }
        return
    }
    esParsed, err := strconv.ParseFloat(expected, 64)
    check(err)
    if esParsed != ieeeF64 {
        fmt.Println("\n" + hex)
        fmt.Println(expected)
    }
    if es6Created != expected {
        fmt.Println("\n" + hex)
        fmt.Println(es6Created)
        fmt.Println(expected)
    }
}

func main() {
    verify("4340000000000001", "9007199254740994", true)
    verify("4340000000000002", "9007199254740996", true)
    verify("444b1ae4d6e2ef50", "1e+21", true)
    verify("3eb0c6f7a0b5ed8d", "0.000001", true)
    verify("3eb0c6f7a0b5ed8c", "9.999999999999997e-7", true)
    verify("8000000000000000", "0", true)
    verify("7fffffffffffffff", "0", false)
    verify("7ff0000000000000", "0", false)
    verify("fff0000000000000", "0", false)
    var count int = 0
    file, err := os.Open("c:\\es6\\numbers\\es6testfile100m.txt")
    check(err)
    defer file.Close()
    scanner := bufio.NewScanner(file)
    for scanner.Scan() {
        count++
        if count % 1000000 == 0 {
            fmt.Printf("line: %d\n", count)
        }
        line := scanner.Text()
        comma := strings.IndexByte(line, ',')
        if comma <= 0 {
            panic("Missing comma!")
        }
        verify(line[0:comma], line[comma + 1:], true)
    }
    check(scanner.Err())
}

