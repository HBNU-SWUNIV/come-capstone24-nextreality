package controller

import (
	"strings"
	"time"

	"go.mongodb.org/mongo-driver/bson/primitive"
)

// 모든 receiveMessage 는 commandName, senduserId, sendTime을 필수적으로 가지며,
// commandName에 따라 otherMessage의 길이가 달라질 수 있음.
// 따라서, 우선적으로 otherMessage의 길이를 검사함.
type ReceiveMessage struct {
	CommandName  string   `bson:"CommandName"`
	SendUserId   string   `bson:"SendUserId"`
	SendTime     string   `bson:"SendTime"`
	OtherMessage []string `bson:"OtherMessage,omitempty"`
}

type LogMessage struct {
	Timestamp       time.Time
	MapId           string
	Message         ReceiveMessage
	OriginalMessage string
}

type CreatorLists struct {
	ID           primitive.ObjectID `bson:"_id"`
	Map_id       int                `bson:"map_id"`
	Admin_id     string             `bson:"admin_id"`
	Creator_list []string           `bson:"creator_list"`
}

type LogResponseData struct {
	ID              primitive.ObjectID `bson:"_id"`
	OriginalMessage string             `bson:"originalmessage"`
}

// 형식에 맞지 않는 메시지가 오면
// 빈 ReceiveMessage와 false가 반환 됨
func MessageParser(msg string) (ReceiveMessage, bool) {
	// fmt.Println("MessageParser messages : ", messages)

	splitCommand := strings.Split(msg, "$")
	if len(splitCommand) < 2 {
		return ReceiveMessage{}, false
	}
	parsingMessage := strings.Split(splitCommand[1], ";")
	if len(parsingMessage) < 2 {
		return ReceiveMessage{}, false
	}

	var _otherMessage []string = nil

	if len(parsingMessage) > 2 {
		_otherMessage = parsingMessage[2:]
		// fmt.Printf("Other Message : %s\n", _otherMessage)
		// fmt.Printf("Other Message Length : %d\n", len(_otherMessage))
	}

	recvMsg := ReceiveMessage{
		CommandName:  splitCommand[0],
		SendUserId:   parsingMessage[0],
		SendTime:     parsingMessage[1],
		OtherMessage: _otherMessage,
	}

	return recvMsg, true
}

func reverseParser(msg ReceiveMessage, othersLength int) (string, bool) {
	if len(msg.OtherMessage) < othersLength {
		return "", false
	}

	result := msg.CommandName + "$" + msg.SendUserId + ";" + msg.SendTime

	for i := 0; i < othersLength; i++ {
		result = result + ";" + msg.OtherMessage[i]
	}

	return result, true
}
