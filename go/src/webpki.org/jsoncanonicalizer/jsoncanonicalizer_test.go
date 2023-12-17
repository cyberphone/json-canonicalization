package jsoncanonicalizer

import "testing"

func FuzzTestTransform(f *testing.F) {
	f.Fuzz(func(t *testing.T, data []byte) {
		_, err := Transform(data)
		if err != nil {
			t.Skipf("Transform(%v) = %v", data, err)
		}
	})
}
