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

package jsoncanonicalizer

import (
	"bytes"
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"testing"
)

func TestCanonicalizationCorpus(t *testing.T) {
	root := filepath.Join("..", "..", "..", "..", "testdata")
	files, err := os.ReadDir(filepath.Join(root, "input"))
	if err != nil {
		t.Fatal(err)
	}
	for _, file := range files {
		t.Run(file.Name(), func(t *testing.T) {
			input, err := os.ReadFile(filepath.Join(root, "input", file.Name()))
			if err != nil {
				t.Fatal(err)
			}
			want, err := os.ReadFile(filepath.Join(root, "output", file.Name()))
			if err != nil {
				t.Fatal(err)
			}
			got, err := Transform(input)
			if err != nil || !bytes.Equal(got, want) {
				t.Fatalf("canonical output = %q, error = %v; want %q", got, err, want)
			}
			again, err := Transform(got)
			if err != nil || !bytes.Equal(again, want) {
				t.Fatalf("canonical round trip changed: %v", err)
			}
		})
	}
}

func BenchmarkTransformRecords(b *testing.B) {
	for _, count := range []int{1, 64, 1024} {
		b.Run(fmt.Sprintf("records=%d", count), func(b *testing.B) {
			records := make([]map[string]interface{}, count)
			for i := range records {
				records[i] = map[string]interface{}{
					"reference": fmt.Sprintf("refs/tags/v1.0.%d", i),
					"object_id": "0123456789abcdef0123456789abcdef01234567",
					"source":    "remote:origin", "annotated": false,
				}
			}
			input, err := json.Marshal(map[string]interface{}{"records": records})
			if err != nil {
				b.Fatal(err)
			}
			// These ASCII keys and unescaped string values are already in
			// canonical form after encoding/json sorts the map keys.
			got, err := Transform(input)
			if err != nil || !bytes.Equal(got, input) {
				b.Fatal("fixture is not canonical", err)
			}
			b.ReportAllocs()
			b.SetBytes(int64(len(input)))
			b.ResetTimer()
			for i := 0; i < b.N; i++ {
				got, err := Transform(input)
				if err != nil || !bytes.Equal(got, input) {
					b.Fatal("canonical output changed", err)
				}
			}
		})
	}
}

func TestReversedSurrogatePairIsError(t *testing.T) {
	got, err := Transform([]byte(`["\ude00\ud83d"]`))
	if err == nil {
		t.Fatalf("reversed surrogate pair was accepted: %q", got)
	}
}

func TestInvalidUTF8InStringIsError(t *testing.T) {
	// ["<0xff>"]
	got, err := Transform([]byte{0x5b, 0x22, 0xff, 0x22, 0x5d})
	if err == nil {
		t.Fatalf("invalid UTF-8 was accepted: %q", got)
	}
}

func TestHighHighSurrogatePairIsError(t *testing.T) {
	got, err := Transform([]byte(`["\ud800\ud800"]`))
	if err == nil {
		t.Fatalf("high/high surrogate pair was accepted: %q", got)
	}
}

func TestLoneSurrogateStillErrors(t *testing.T) {
	if _, err := Transform([]byte(`["\ud83d"]`)); err == nil {
		t.Fatal("lone high surrogate was accepted")
	}
	if _, err := Transform([]byte(`["\ude00"]`)); err == nil {
		t.Fatal("lone low surrogate was accepted")
	}
}

func TestValidSurrogatePairStillCanonicalizes(t *testing.T) {
	got, err := Transform([]byte(`["\ud83d\ude00"]`))
	if err != nil {
		t.Fatal(err)
	}
	want := []byte("[\"\U0001F600\"]")
	if !bytes.Equal(got, want) {
		t.Fatalf("got %q want %q", got, want)
	}
}

func TestReplacementCharacterStillCanonicalizes(t *testing.T) {
	got, err := Transform([]byte(`["\ufffd"]`))
	if err != nil {
		t.Fatal(err)
	}
	want := []byte("[\"\uFFFD\"]")
	if !bytes.Equal(got, want) {
		t.Fatalf("got %q want %q", got, want)
	}
}

func TestRawReplacementCharacterStillCanonicalizes(t *testing.T) {
	in := []byte("[\"\uFFFD\"]")
	got, err := Transform(in)
	if err != nil {
		t.Fatal(err)
	}
	if !bytes.Equal(got, in) {
		t.Fatalf("got %q want %q", got, in)
	}
}

func TestRawUTF8StillCanonicalizes(t *testing.T) {
	got, err := Transform([]byte("[\"é\"]"))
	if err != nil {
		t.Fatal(err)
	}
	want := []byte("[\"é\"]")
	if !bytes.Equal(got, want) {
		t.Fatalf("got %q want %q", got, want)
	}
}
