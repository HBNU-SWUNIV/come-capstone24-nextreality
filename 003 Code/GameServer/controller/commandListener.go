package controller

import (
	controllerhttp "GameServer/controller-http"
	"context"
	"fmt"
	"net"
	"sort"
	"strconv"
	"time"

	"go.mongodb.org/mongo-driver/bson"
)

// Listener는 ReceiveMessage와 연결 정보를 받고
// 서버에 유효한 결과를 도출했는지 bool로 반환함
// 에러가 있을 경우 string도 추가로 반환
type listeners func(*net.UDPConn, ReceiveMessage, string) (bool, string)

/*
func ValidateMessage(m ReceiveMessage) {

}
*/
func ItemLock(userId string, itemId string) {
	mapId := UserMapid[userId]

	_, lockedListResult := MapidLockedList[mapId]
	// if !lockedListResult => 해당 방에 Lock된 아이템이 없음 => 새로 만들어줘야함
	if !lockedListResult {
		MapidLockedList[mapId] = []string{itemId}
	} else { // 해당 방에 Lock 된 아이템이 있음 => 뒤에 append
		MapidLockedList[mapId] = append(MapidLockedList[mapId], itemId)
	}
	LockObjUser[itemId] = userId // 해당 아이템이 userId에 의해 Lock 된 것임을 확인
}

func ItemUnlock(userId string, itemId string) {
	mapId := UserMapid[userId]

	lockedList := MapidLockedList[mapId]

	var itemIndex int

	for index, item := range lockedList {
		if item == itemId {
			itemIndex = index
		}
	}

	if itemIndex != -1 {
		lockedList = append(lockedList[:itemIndex], lockedList[itemIndex+1:]...)
		MapidLockedList[mapId] = lockedList
	}

	delete(LockObjUser, itemId)
	//fmt.Printf("Item [%s] Unlocked\n", itemId)
}

func isLocked(userId string, itemId string) bool {
	mapId := UserMapid[userId]

	lockedList, lockedListResult := MapidLockedList[mapId]
	if !lockedListResult {
		return false
	}
	for _, item := range lockedList {
		if item == itemId {
			return true
		}
	}
	return false
}

func otherMessageLengthCheck(commandName string, messageLength int) bool {
	switch commandName {
	case "AssetCreate":
		return messageLength == 8
	case "AssetMove":
		return messageLength == 4
	case "PlayerJoin", "PlayerMove":
		return messageLength == 2
	case "AssetDelete", "AssetSelect", "AssetDeselect":
		return messageLength == 1
	case "PlayerLeave", "MapReady", "PlayerJump", "MapInit":
		return messageLength == 0
	}
	return false
}

func isUserExists(userId string, addr string) bool {
	_, usrExt := UserAddr[userId]
	_, addExt := AddrUser[addr]
	mapid, mapExt := UserMapid[userId]
	userListExt := false
	if mapExt {
		mapidUserList := MapidUserList[mapid]
		mapidUserIndex := sort.SearchStrings(mapidUserList, userId)

		if mapidUserIndex != len(mapidUserList) {
			userListExt = true
		}
	}
	return usrExt && addExt && mapExt && userListExt
}
func createCreatorList(mapid string) bool {
	datas := CreatorLists{
		MapId:    mapid,
		Creators: []string{},
	}
	_, err := DBClient.Collection("creators").InsertOne(context.TODO(), datas)
	return err == nil
}

func isCreator(userId string) bool {
	if isAdmin(userId) {
		return true
	}
	mapid, err := UserMapid[userId]
	if !err {
		return false
	}
	creators, err := MapCreatorList[mapid]
	if !err {
		createCreatorList(mapid)
		return false
	}

	for _, creator := range creators {
		if userId == creator {
			return true
		}
	}

	return false
}

func isAdmin(userId string) bool {
	adminList := []string{"1234"}

	for _, name := range adminList {
		if name == userId {
			return true
		}
	}
	return false
}

func PlayerJoin(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// PlayerJoin 메시지 형태 :
	// PlayerJoin$sendUserId;SendTime;SendUserNickname;MapId
	// otherMessage length: 2

	// otherMessage 길이 체크
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		mapid := m.OtherMessage[1]

		fmt.Printf("%s | Player [%s] Join Map [%s] (Address : %s)\n", time.Now().Format("2006-01-02 15:04:05.000"), m.SendUserId, mapid, addr)

		existAddr, isUserExists := UserAddr[m.SendUserId]
		_, isAddrExists := AddrUser[addr]

		sameIp := CompareIPAddress(existAddr, addr)
		/*
			fmt.Println(
				fmt.Sprintf(
					 .Gray(12, "Users Address : %s"), usersAddress))

			fmt.Println(
				fmt.Sprintf(
					 .Gray(12, "Users Name : %s"), usersName))
		*/

		// id와 주소가 접속자 map에 있을 때
		if isUserExists && sameIp {
			// 같은 맵에 접속하려고 하는거면 true.
			if UserMapid[m.SendUserId] == mapid {
				// TODO : 이전에 접속해 있던 유저 나가게 만들어야함
				delete(AddrUser, existAddr)
				UserAddr[m.SendUserId] = addr
				AddrUser[addr] = m.SendUserId
				fmt.Printf("%s | User [%s] Join same map [%s]\n", time.Now().Format("2006-01-02 15:04:05.000"), m.SendUserId, mapid)
				//fmt.Printf("UserAddr : %s\nAddrUser : %s\n", UserAddr[m.SendUserId], AddrUser[addr])
				return true, fmt.Sprintf("Success : User [%s] joined Map [%s]\n", m.SendUserId, mapid)
			}
		}

		// id는 접속되어있는데 주소가 다를 때
		if isUserExists && !sameIp {
			// 이전에 접속한 사람을 끊어버림
			// 근데 그러면 PlayerLeave도 클라이언트에서 받았을때
			// 메인화면으로 나가게 하는게 필요함
			leaveMessage := "PlayerLeave$" + m.SendUserId + ";" + strconv.FormatInt(time.Now().UnixMilli(), 10)
			structLeaveMessage, err := MessageParser(leaveMessage)

			if !err {
				fmt.Printf("leave message parsing error\n")
				return false, "Error : Cannot Parsing Leave Message\n"
			}

			logData := LogMessage{
				Timestamp:       time.Now().UTC(),
				MapId:           UserMapid[m.SendUserId],
				Message:         structLeaveMessage,
				OriginalMessage: leaveMessage,
			}

			_, insertErr := DBClient.Collection("log").InsertOne(context.TODO(), logData)
			if insertErr != nil {
				fmt.Printf("insert result : %s\n", insertErr)
			}
			leaveResult, leaveResultText := PlayerLeave(conn, structLeaveMessage, addr)
			if !leaveResult {
				return false, leaveResultText
			}
		}

		// id와 주소가 접속자 map에 없을 때
		if !isUserExists && !isAddrExists {
			UserAddr[m.SendUserId] = addr
			AddrUser[addr] = m.SendUserId
			UserMapid[m.SendUserId] = mapid

			mapUsers := MapidUserList[mapid]

			if len(mapUsers) > 0 {
				// IP:Port 형태를 UDPAddr 로 변경해서 저장 시도
				udpAddr, err := net.ResolveUDPAddr("udp", addr)
				if err != nil {
					fmt.Printf("Error : Resolve UDP Address Error Occured.\nError Message : %s\n", err)
				} else {
					for _, user := range mapUsers {
						conn.WriteToUDP([]byte("PlayerJoin$"+user+";12345678;"+user+";"+mapid+";s"), udpAddr)
					}
				}
			}

			mapUsers = append(MapidUserList[mapid], m.SendUserId)
			if len(mapUsers) > 1 {
				sort.Strings(mapUsers)
			}
			MapidUserList[mapid] = mapUsers

			//fmt.Printf("%s | Map Users : %v\n", time.Now().Format("2006-01-02 15:04:05.000"), mapUsers)

			return true, fmt.Sprintf("Success : User [%s] joined Map [%s]\n", m.SendUserId, mapid)
		} else if isUserExists { // 이미 유저ID가 있을 경우
			return false, fmt.Sprintf("Error : User [%s] is already in this game.\n", m.SendUserId)
		} else if isAddrExists { // 이미 접속한 IP가 등록되어 있었을 경우
			return false, fmt.Sprintf("Error : Address [%s] is already in this game.\n", addr)
		}
	} else {
		return false, fmt.Sprintf("Error : PlayerJoin Message Length Error : need 2, received %d\n", len(m.OtherMessage))
	}
	return false, "Error : Unknown (in PlayerJoin)\n"
}

func MapReady(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// MapReady 메시지 형태 :
	// MapReady$sendUserId;SendTime;

	// fmt.Printf("MapReady Start\n")
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		_, isUserAddrExists := UserAddr[m.SendUserId]

		if isUserAddrExists {
			boolChan := make(chan bool)
			go SendBeforeLog(conn, UserMapid[m.SendUserId], m.SendUserId, boolChan)
			result := <-boolChan
			if result {
				return true, "Send After Log Complete\n"
			} else {
				return false, "Error : Cannot Send After Log\n"
			}
		} else {
			return false, fmt.Sprintf("Error : Cannot Found User [%s]\n", m.SendUserId)
		}

	} else {
		return false, fmt.Sprintf("Error : MapReady Message Length Error : need 0, received %d\n", len(m.OtherMessage))
	}
}

// TODO : 유정이 답장오면 만들기
func SendBeforeLog(conn *net.UDPConn, mapid string, userId string, result chan bool) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("Recovered from panic: %v\n", r)
			result <- false
		}
	}()

	mapSaveTimeString := controllerhttp.GetMapTime(mapid, MapServerURL)
	mapSaveTime, err := time.Parse(time.RFC3339Nano, mapSaveTimeString.Message)
	if err != nil {
		fmt.Printf("Error in Parsing Saved Time : %s\n", err)
		result <- false
		return
	}

	logResult, err := FindDocumentsAfterTime(mapSaveTime, mapid)
	if err != nil {
		fmt.Printf("Error in Load Log Result : %s\n", err)
		result <- false
		return
	}

	udpAddr, err := net.ResolveUDPAddr("udp", UserAddr[userId])
	if err != nil {
		fmt.Printf("Error : Resolve UDP Address Error Occured.\nError Message : %s\n", err)
		result <- false
		return
	}

	for _, logOne := range logResult {
		// fmt.Printf("Log : %s\n", logOne.OriginalMessage)
		_, err := conn.WriteToUDP([]byte(logOne.OriginalMessage+";s"), udpAddr)
		if err != nil {
			fmt.Printf("%s | SendBeforeLog -> UDP Error : %s (%s)\n", time.Now().Format("2006-01-02 15:04:05.000"), err, logOne.OriginalMessage)
			continue
		}
		time.Sleep(100 * time.Millisecond)
	}
	result <- true
}

func FindDocumentsAfterTime(parsedTimeFromMaptime time.Time, mapid string) ([]LogResponseData, error) {
	collection := DBClient.Collection("log")

	//fmt.Printf("Find Log Start\n\n")
	currentTime := time.Now().UTC()
	//fmt.Printf("Parsed Time : %s\nCurrent Time : %s\nTime Duration : %f\n", parsedTimeFromMaptime, currentTime, currentTime.Sub(parsedTimeFromMaptime).Seconds())

	//fmt.Printf("Parsed Time : %s\nCurrent Time : %s\n", strconv.FormatInt(parsedTimeFromMaptime.UnixMilli(), 10), strconv.FormatInt(currentTime.UnixMilli(), 10))

	/*
		filter := bson.M{
			"timestamp": bson.M{
				"$gt": parsedTimeFromMaptime,
				"$lt": currentTime,
			},
			"mapid":           mapid,
			"originalmessage": bson.M{"$regex": "^Asset"},
		}
	*/

	filter := bson.M{
		"timestamp": bson.M{
			"$gt": parsedTimeFromMaptime,
			"$lt": currentTime,
		},
		"mapid": mapid,
		"$or": []bson.M{
			{"originalmessage": bson.M{"$regex": "^AssetCreate"}},
			{"originalmessage": bson.M{"$regex": "^AssetMove"}},
			{"originalmessage": bson.M{"$regex": "^AssetDelete"}},
		},
		"success": true,
	}

	cursor, err := collection.Find(context.TODO(), filter)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(context.TODO())

	var results []LogResponseData
	//fmt.Printf("Next Cursor Start\n")
	for cursor.Next(context.TODO()) {
		var elem LogResponseData
		err := cursor.Decode(&elem)
		if err != nil {
			fmt.Printf("Error : %s\n", err)
			return nil, err
		}
		//fmt.Printf("LogResponseData : %s\n", elem.OriginalMessage)
		results = append(results, elem)
	}
	if err := cursor.Err(); err != nil {
		fmt.Printf("Error : %s\n", err)
		return nil, err
	}

	return results, nil
}

func PlayerLeave(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// PlayerLeave 형태 :
	// PlayerLeave$SendUserId;SendTime
	// otherMessage length : 0
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		_, isUserAddrExists := UserAddr[m.SendUserId]
		_, isUserMapidExists := UserMapid[m.SendUserId]

		if isUserAddrExists {
			delete(UserAddr, m.SendUserId)
			delete(AddrUser, addr)
		} else {
			return false, fmt.Sprintf(("Error : Cannot Found User [%s]\n"), m.SendUserId)
		}
		if isUserMapidExists {
			userMapid := UserMapid[m.SendUserId]
			mapidUserList := MapidUserList[userMapid]
			mapidUserIndex := -1

			if len(mapidUserList) == 1 {
				delete(MapidUserList, userMapid)
			} else {
				for index, users := range mapidUserList {
					if users == m.SendUserId {
						mapidUserIndex = index
					}
				}

				if mapidUserIndex != -1 {
					mapidUserList = append(mapidUserList[:mapidUserIndex], mapidUserList[mapidUserIndex+1:]...)
					MapidUserList[userMapid] = mapidUserList
				}
			}

			lockedList, lockedListResult := MapidLockedList[userMapid]
			if lockedListResult {
				for _, item := range lockedList {
					if LockObjUser[item] == m.SendUserId {
						ItemUnlock(m.SendUserId, item)

						unlockMessage := "AssetDeselect$" + m.SendUserId + ";" + strconv.FormatInt(time.Now().UnixMilli(), 10) + ";" + item

						//fmt.Printf("Unlock Message : %s\n", unlockMessage)

						structUnlockMessage, err := MessageParser(unlockMessage)

						if !err {
							fmt.Printf("unlock message parsing error\n")
							return false, "Error : Cannot Parsing Unlock Message"
						}

						logData := LogMessage{
							Timestamp:       time.Now().UTC(),
							MapId:           userMapid,
							Message:         structUnlockMessage,
							OriginalMessage: unlockMessage,
						}

						_, insertErr := DBClient.Collection("log").InsertOne(context.TODO(), logData)
						if insertErr != nil {
							fmt.Printf("insert result : %s\n", insertErr)
						}
					}
				}
			}

			// delete(UserMapid, m.SendUserId)
			fmt.Printf("%s | Player [%s] left Map [%s] (Address : %s)\n", time.Now().Format("2006-01-02 15:04:05.000"), m.SendUserId, userMapid, addr)

			return true, fmt.Sprintf(("Success : User [%s] left Map [%s]\n"), m.SendUserId, userMapid)
		} else {
			return false, fmt.Sprintf(("Error : Cannot Found Map ID of User [%s]\n"), m.SendUserId)
		}
	}

	return false, "Error : Unknown (in PlayerLeave)"
}

func PlayerMove(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// PlayerMove 형태 :
	// PlayerMove$SendUserId;SendTime;Position;Rotation
	// otherMessage length : 2

	// 보낸 유저가 있는 유저면 return true
	// 딱히 더 할 작업은 없음
	if isUserExists(m.SendUserId, addr) {
		return true, fmt.Sprintf(("Success : User [%s] move\n"), m.SendUserId)
	} else {
		return false, fmt.Sprintf(("Error : Cannot Found User [%s]\n"), m.SendUserId)
	}
}

func AssetCreate(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// AssetCreate 형태 :
	// AssetCreate$SendUserId;SendTime;AssetId;ObjectId;Position;Rotation;Scale;Type;MeshCollider;Rigidbody
	// otherMessage length : 8

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId, addr) {
			if isCreator(m.SendUserId) {
				return true, fmt.Sprintf(("Success : User [%s] Asset [%s] Create\n"), m.SendUserId, m.OtherMessage[1])
			}
			return false, fmt.Sprintf(("Error : User [%s] is not Creator\n"), m.SendUserId)
		} else {
			return false, fmt.Sprintf(("Error : Cannot Found User [%s]\n"), m.SendUserId)
		}
	} else {
		return false, fmt.Sprintf(("Error : AssetCreate Message Length Error : need 8, received %d\n"), len(m.OtherMessage))
	}
}

func AssetMove(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// AssetMove 형태 :
	// AssetMove$SendUserId;SendTime;ObjectId;Position;Rotation;Scale
	// otherMessage length : 4

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId, addr) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]
				if isLocked(m.SendUserId, itemId) {
					if LockObjUser[itemId] == m.SendUserId {
						return true, fmt.Sprintf(("Success : User [%s] Asset [%s] Move\n"), m.SendUserId, m.OtherMessage[0])
					} else {
						return false, fmt.Sprintf(("Error : Item [%s] is not locked by [%s]\n"), itemId, m.SendUserId)
					}
				} else {
					return false, "Error : Unavailable Access to Asset Move\n"
				}
			}
			return false, fmt.Sprintf(("Error : User [%s] is not Creator\n"), m.SendUserId)
		} else {
			return false, fmt.Sprintf(("Error : Cannot Found User [%s]\n"), m.SendUserId)
		}
	} else {
		return false, fmt.Sprintf(("Error : AssetMove Message Length Error : need 8, received %d\n"), len(m.OtherMessage))
	}
}

func AssetDelete(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// PlayerMove 형태 :
	// PlayerMove$sendUserId;sendTime;ObjectId
	// otherMessage length : 1

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId, addr) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]
				if isLocked(m.SendUserId, itemId) {
					if LockObjUser[itemId] == m.SendUserId {
						ItemUnlock(m.SendUserId, itemId)
						return true, fmt.Sprintf(("Success : User [%s] Asset [%s] Delete\n"), m.SendUserId, m.OtherMessage[0])
					} else {
						return false, fmt.Sprintf(("Error : Item [%s] is not locked by [%s]\n"), itemId, m.SendUserId)
					}
				} else {
					return false, "Error : Unavailable Access to Asset Delete\n"
				}
			}
			return false, fmt.Sprintf(("Error : User [%s] is not Creator\n"), m.SendUserId)
		} else {
			return false, fmt.Sprintf(("Error : Cannot Found User [%s]\n"), m.SendUserId)
		}
	} else {
		return false, fmt.Sprintf(("Error : AssetDelete Message Length Error : need 8, received %d\n"), len(m.OtherMessage))
	}
}

func AssetSelect(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// AssetSelect 형태 :
	// AssetSelect$SendUserId;SendTime;ObjectId;
	// otherMessage length : 1

	// Lock 함. 이미 누가 Lock 해놨으면 false 날리기
	// if Lock 해둔 사람이 나임 -> true
	// if 이미 뭔가를 lock 한 사람이 다른 물체를 lock 함 -> 기존 물체 deselect 하고 새 물체 lock으로 해야함

	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId, addr) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]

				if !isLocked(m.SendUserId, itemId) {
					resultKey, resultBool := findKeyByValue(LockObjUser, m.SendUserId) // 값으로 키 찾기
					if resultBool {                                                    // 이미 lock 한 오브젝트가 있으면 그걸 unlock 하는게 우선
						// ItemUnlock(m.SendUserId, resultKey)

						// AssetDeselect$SendUserId;SendTime;ObjectId;
						unlockMessage := "AssetDeselect$" + m.SendUserId + ";" + strconv.FormatInt(time.Now().UnixMilli(), 10) + ";" + resultKey
						structUnlockMessage, err := MessageParser(unlockMessage)

						if !err {
							fmt.Printf("leave message parsing error\n")
							return false, "Error : Cannot Parsing Leave Message\n"
						}

						logData := LogMessage{
							Timestamp:       time.Now().UTC(),
							MapId:           UserMapid[m.SendUserId],
							Message:         structUnlockMessage,
							OriginalMessage: unlockMessage,
						}

						_, insertErr := DBClient.Collection("log").InsertOne(context.TODO(), logData)
						if insertErr != nil {
							fmt.Printf("insert result : %s\n", insertErr)
						}
						unlockResult, unlockResultText := AssetDeselect(conn, structUnlockMessage, addr)
						if !unlockResult {
							return false, unlockResultText
						}
						broadcast(conn, m.SendUserId, unlockMessage, true)
					}
					ItemLock(m.SendUserId, itemId)
					//fmt.Printf("MapidLockedList : %v\n", MapidLockedList[UserMapid[m.SendUserId]])
					//fmt.Printf("LockObjUser : %s\n", LockObjUser[itemId])

					return true, fmt.Sprintf("Success : User [%s] Asset [%s] Select\n", m.SendUserId, itemId)
				} else if isLocked(m.SendUserId, itemId) && LockObjUser[itemId] == m.SendUserId {
					//fmt.Printf("MapidLockedList : %v\n", MapidLockedList[UserMapid[m.SendUserId]])
					//fmt.Printf("LockObjUser : %s (Again)\n", LockObjUser[itemId])
					return true, fmt.Sprintf("Success : User[%s] Asset [%s] Select Again\n", m.SendUserId, itemId)
				}

				return false, fmt.Sprintf("Error : Item [%s] is Locked\n", itemId)
			}
			return false, fmt.Sprintf("Error : User [%s] is not Creator\n", m.SendUserId)
		} else {
			return false, fmt.Sprintf("Error : Cannot Found User [%s]\n", m.SendUserId)
		}
	} else {
		return false, fmt.Sprintf("Error : AssetSelect Message Length Error : need 1, received %d\n", len(m.OtherMessage))
	}
}

func AssetDeselect(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// AssetDeselect 형태 :
	// AssetDeselect$SendUserId;SendTime;ObjectId;
	// otherMessage length : 1

	// Unlock 함.

	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId, addr) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]

				if isLocked(m.SendUserId, itemId) {
					lockUser := LockObjUser[itemId]
					if m.SendUserId == lockUser {
						ItemUnlock(m.SendUserId, itemId)
						return true, fmt.Sprintf(("Success : User [%s] Asset [%s] Deselect\n"), m.SendUserId, m.OtherMessage[0])
					} else {
						return false, fmt.Sprintf(("Error : Item [%s] is Locked\n"), itemId)
					}
				} else {
					fmt.Printf("Error : User [%s] not lock Asset [%s]\n", m.SendUserId, itemId)
					fmt.Printf("Map [%s] Lock List : %v \n", UserMapid[m.SendUserId], MapidLockedList[UserMapid[m.SendUserId]])
					return false, fmt.Sprintf(("Error : Unavailable Access to Asset Deselect [%s]\n"), itemId)
				}

			}
			return false, fmt.Sprintf("Error : User [%s] is not Creator\n", m.SendUserId)
		} else {
			return false, fmt.Sprintf("Error : Cannot Found User [%s]\n", m.SendUserId)
		}
	} else {
		return false, fmt.Sprintf("Error : AssetSelect Message Length Error : need 1, received %d\n", len(m.OtherMessage))
	}
}

func PlayerJump(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// PlayerJump 형태 :
	// PlayerJump$SendUserId;SendTime
	// otherMessage length : 0

	// 보낸 유저가 있는 유저면 return true
	// 딱히 더 할 작업은 없음
	if isUserExists(m.SendUserId, addr) {
		return true, fmt.Sprintf("Success : User [%s] Jump\n", m.SendUserId)
	} else {
		return false, fmt.Sprintf("Error : Cannot Found User [%s]\n", m.SendUserId)
	}
}

func MapInit(conn *net.UDPConn, m ReceiveMessage, addr string) (bool, string) {
	// MapInit 형태 :
	// MapInit$SendUserId;SendTime
	// otherMessage length : 0

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId, addr) {
			if isCreator(m.SendUserId) {
				return true, fmt.Sprintf("Success : User [%s] Init Map [%s]\n", m.SendUserId, UserMapid[m.SendUserId])
			}
			return false, fmt.Sprintf("Error : User [%s] is not Creator\n", m.SendUserId)
		} else {
			return false, fmt.Sprintf("Error : Cannot Found User [%s]\n", m.SendUserId)
		}
	} else {
		return false, fmt.Sprintf("Error : MapInit Message Length Error : need 0, received %d\n", len(m.OtherMessage))
	}
}
