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
    "fmt"
    "io/ioutil"
    "runtime"
    "path/filepath"
)

func check(e error) {
    if e != nil {
        panic(e)
    }
}

var testdata string

func read(fileName string, directory string) []byte {
    data, err := ioutil.ReadFile(filepath.Join(filepath.Join(testdata, directory), fileName))
    check(err)
    return data
}

func verify(fileName string) {
    fmt.Println(fileName)
    inputData := read(fileName, "input")
    fmt.Println(string(inputData))
    outputData := read(fileName, "output")
    fmt.Println(string(outputData))
    var utf8InHex = "\nFile: " + fileName
    var byteCount = 0
    var next = false
    for _, b := range outputData {
        if byteCount % 32 == 0 {
            utf8InHex = utf8InHex + "\n"
            next = false
        }
        byteCount++
        if next {
            utf8InHex = utf8InHex + " "
        }
        next = true
        utf8InHex = utf8InHex + fmt.Sprintf("%02x", b)
    }
    fmt.Println(utf8InHex + "\n")
}
 
func main() {
    _, executable, _, _ := runtime.Caller(0)
    testdata = filepath.Join(filepath.Dir(filepath.Dir(filepath.Dir(executable))), "testdata")
    fmt.Println(testdata)
    files, err := ioutil.ReadDir(filepath.Join(testdata, "input"))
    check(err)
    for _, file := range files {
        verify(file.Name())
    }
}