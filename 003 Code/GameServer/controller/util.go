package controller

import "strings"

func findKeyByValue(m map[string]string, value string) (string, bool) {
	for k, v := range m {
		if v == value {
			return k, true
		}
	}
	return "", false
}

func CompareIPAddress(addr1, addr2 string) bool {
	splitedAddr1 := strings.Split(addr1, ":")
	splitedAddr2 := strings.Split(addr2, ":")

	if len(splitedAddr1) != 2 || len(splitedAddr2) != 2 {
		return false
	}
	return splitedAddr1[0] == splitedAddr2[0]
}
