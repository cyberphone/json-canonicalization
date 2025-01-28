package jsoncanonicalizer

import "testing"

func FuzzTestNumberToJSON(t *testing.F) {
	t.Fuzz(func(t *testing.T, data float64) {
		_, err := NumberToJSON(data)
		if err != nil {
			t.Skipf("NumberToJSON(%v) = %v", data, err)
		}
	})
}
