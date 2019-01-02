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

func main() {
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
        hex := line[0:comma]
        for len(hex) < 16 {
            hex = "0" + hex
        }
        ieeeU64, err := strconv.ParseUint(hex, 16, 64)
        check(err);
        ieeeF64 := math.Float64frombits(ieeeU64)
        es6Created := es6numfmt.Convert(ieeeF64)
        es6Original := line[comma + 1:]
        esParsed, err := strconv.ParseFloat(es6Original, 64)
        check(err)
        if esParsed != ieeeF64 {
            fmt.Println("\n" + hex)
            fmt.Println(es6Original)
        }
        if es6Created != es6Original {
            fmt.Println("\n" + hex)
            fmt.Println(es6Created)
            fmt.Println(es6Original)
        }
    }
    check(scanner.Err())
}

