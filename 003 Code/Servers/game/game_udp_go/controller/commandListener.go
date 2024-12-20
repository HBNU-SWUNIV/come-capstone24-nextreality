package controller

import (
	"context"
	"fmt"
	"log"
	"net"
	"strconv"
	"strings"
	"time"

	"github.com/logrusorgru/aurora"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

// Listener는 ReceiveMessage와 연결 정보를 받고
// 서버에 유효한 결과를 도출했는지 bool로 반환함
// 에러가 있을 경우 string도 추가로 반환
type listeners func(*net.UDPConn, ReceiveMessage) (bool, string)

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
	fmt.Printf("Item [%s] Unlocked\n", itemId)
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

func CreatorListLoad() error {
	creatorCollection := GameDB.Collection("creators")

	cursor, err := creatorCollection.Find(context.TODO(), bson.D{})
	if err != nil {
		log.Fatal(err)
		return err
	}
	defer cursor.Close(context.TODO())

	// 기존의 creatorlist 초기화
	MapidCreatorList = make(map[string][]string)

	// 결과 출력
	for cursor.Next(context.TODO()) {
		var result CreatorLists
		if err := cursor.Decode(&result); err != nil {

			log.Fatal(err)
			return err
		}
		fmt.Println(result)
		MapidCreatorList[strconv.Itoa(result.Map_id)] = result.Creator_list
	}

	fmt.Printf("Creator List Loaded\n")
	return nil
}

func otherMessageLengthCheck(commandName string, messageLength int) bool {
	switch commandName {
	case "AssetCreate":
		return messageLength == 8
	case "AssetMove":
		return messageLength == 4
	case "PlayerJoin":
		return messageLength == 3
	case "PlayerMove", "ManagerEdit":
		return messageLength == 2
	case "AssetDelete", "AssetSelect", "AssetDeselect":
		return messageLength == 1
	case "PlayerLeave", "MapReady", "PlayerJump", "MapInit":
		return messageLength == 0
	}
	return false
}

func isUserExists(userId string) bool {
	userAddr, usrExt := UserAddr[userId]
	_, addExt := AddrUser[userAddr]
	mapid, mapExt := UserMapid[userId]
	userListExt := false
	if mapExt {
		mapidUserList := MapidUserList[mapid]
		/*
			mapidUserIndex := sort.SearchStrings(mapidUserList, userId)

			if mapidUserIndex != len(mapidUserList) {
				userListExt = true
			}
		*/
		for index, user := range mapidUserList {
			if index != len(mapidUserList)-1 && user != userId {
				continue
			}
			if user == userId {
				userListExt = true
				break
			} else if index == len(mapidUserList)-1 && user != userId {
				userListExt = false
			}
		}
	}

	fmt.Printf("\nUser ID : %s\nUser Exist : %t\nAddress Exist : %t\nMap Exist : %t\nUser List Exist : %t\n", userId, usrExt, addExt, mapExt, userListExt)

	return usrExt && addExt && mapExt && userListExt
}

func isCreator(userId string) bool {
	if isAdmin(userId) {
		return true
	}

	mapid, mapResult := UserMapid[userId]
	if !mapResult {
		return false
	}

	for _, creators := range MapidCreatorList[mapid] {
		if userId == creators {
			return true
		}
	}

	return false
}

func isAdmin(userId string) bool {
	if userId == "1234" {
		return true
	} else {
		return false
	}
}

func PlayerJoin(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// PlayerJoin 메시지 형태 :
	// PlayerJoin$sendUserId;SendTime;SendUserNickname;MapId;SendUserIP:Port;
	// otherMessage length: 3

	// 중요 : PlayerJoin은 Original Message 그대로 주는게 아니라 SendUserIP:Port 부분을 TCP 유저 IP:Port로 바꾸거나, 비워야함 (Main Server쓸 때)

	// otherMessage 길이 체크
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		var returnMessage []byte
		var modeMessage string
		var alreadyIn bool

		mapid := m.OtherMessage[1]
		sendUserAddr := m.OtherMessage[2]

		fmt.Println(
			aurora.Sprintf(
				aurora.Gray(12, "Player [%s] Join Map [%s] | (%s)"), m.SendUserId, mapid, sendUserAddr))

		userAddr, isUserExists := UserAddr[m.SendUserId]
		_, isAddrExists := AddrUser[userAddr]
		/*
			fmt.Println(
				aurora.Sprintf(
					aurora.Gray(12, "Users Address : %s"), usersAddress))

			fmt.Println(
				aurora.Sprintf(
					aurora.Gray(12, "Users Name : %s"), usersName))
		*/

		// id와 주소가 접속자 map에 없을 때 or 있지만 같은 맵에 들어오려고 할 때
		if (!isUserExists && !isAddrExists) || (isUserExists && isAddrExists && UserMapid[m.SendUserId] == mapid) {
			UserAddr[m.SendUserId] = sendUserAddr
			AddrUser[sendUserAddr] = m.SendUserId
			UserMapid[m.SendUserId] = mapid

			mapUsers := MapidUserList[mapid]

			alreadyIn = isUserExists && isAddrExists && UserMapid[m.SendUserId] == mapid

			/*
				if len(mapUsers) > 0 {
					// IP:Port 형태를 UDPAddr 로 변경해서 저장 시도
					udpAddr, err := net.ResolveUDPAddr("udp", addr)
					if err != nil {
						fmt.Println(aurora.Sprintf(aurora.Red("Error : Resolve UDP Address Error Occured.\nError Message : %s"), err))
					} else {
						for _, user := range mapUsers {
							conn.WriteToUDP([]byte("PlayerJoin$"+user+";12345678;"+user+";"+mapid+";s"), udpAddr)
						}
					}
				}
			*/
			udpAddr, err := net.ResolveUDPAddr("udp", sendUserAddr)
			if err != nil {
				fmt.Printf("Error : Resolve UDP Address Error Occured.\nError Message : %s\n", err)
				return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Resolve UDP Address"))
			}
			fmt.Printf("Player Join User IP:Port = %s\n", udpAddr)

			if len(mapUsers) == 0 { // 맵에 유저가 아무도 없을 때 => 크리에이터 리스트 전체 로드
				CreatorListLoad()
				returnMessage = []byte("PlayerJoin$" + m.SendUserId + ";" + m.SendTime + ";" + m.SendUserId + ";" + mapid + ";;s")
				modeMessage = "Main Server Mode"
			} else if len(mapUsers) > 0 { // 맵에 유저가 있을 때
				loadedPlayerList := FindLoadedUser(UserMapid[m.SendUserId])

				if len(loadedPlayerList) > 0 { // 맵에 ""로딩된 유저"" 가 있을 때
					userString := strings.Join(loadedPlayerList, ";")
					returnMessage = []byte("PlayerJoin$" + m.SendUserId + ";" + m.SendTime + ";" + m.SendUserId + ";" + mapid + ";" + userString + ";s")
					modeMessage = "TCP Mode"

				} else { // 맵에 ""로딩된 유저"" 가 없을 때
					fmt.Printf("Map [%s] Player is not empty. but we can find MapReady User.\n", mapid)
					returnMessage = []byte("PlayerJoin$" + m.SendUserId + ";" + m.SendTime + ";" + m.SendUserId + ";" + mapid + ";;s")
					modeMessage = "Main Server Mode"
				}
			}

			fmt.Printf("Return Message : %s\n", returnMessage)
			conn.WriteToUDP(returnMessage, udpAddr)
			/*
				intvalue, udpError := conn.WriteToUDP(returnMessage, udpAddr)
				fmt.Printf("UDP Result : %d\n", intvalue)
				if udpError != nil {
					fmt.Printf("Error : Udp Return Error [%s]\n", udpError)
					return false, aurora.Sprintf(aurora.Yellow("Error : Udp Return Error [%s]"), udpError)
				}
			*/
			if !alreadyIn {
				MapidUserList[mapid] = append(MapidUserList[mapid], m.SendUserId)
			}

			// if len(mapUsers) > 1 {
			// sort.Strings(mapUsers) // 굳이 정렬할 필요 없을듯
			// }
			fmt.Printf("map Users : %v\n", mapUsers)

			return true, aurora.Sprintf(aurora.Green("Success : User [%s] joined Map [%s] | Mode : %s"), m.SendUserId, mapid, modeMessage)
		} else if isUserExists { // 이미 유저ID가 있을 경우
			return false, aurora.Sprintf(aurora.Yellow("Error : User [%s] is already in this game."), m.SendUserId)
		} else if isAddrExists { // 이미 접속한 IP가 등록되어 있었을 경우
			return false, aurora.Sprintf(aurora.Yellow("Error : Address [%s] is already in this game."), sendUserAddr)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : PlayerJoin Message Length Error : need 3, received %d"), len(m.OtherMessage))
	}
	return false, aurora.Sprintf(aurora.Yellow("Error : Unknown (in PlayerJoin)"))
}

func contains(slice []string, value string) bool {
	for _, v := range slice {
		if v == value {
			return true
		}
	}
	return false
}

func remove(slice []string, value string) []string {
	for i, v := range slice {
		if v == value {
			// 특정 값을 찾으면 해당 인덱스를 기준으로 슬라이스를 다시 결합하여 반환
			return append(slice[:i], slice[i+1:]...)
		}
	}
	return slice
}

func MapReady(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
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
				if !contains(MapidLoadedList[UserMapid[m.SendUserId]], m.SendUserId) {
					MapidLoadedList[UserMapid[m.SendUserId]] = append(MapidLoadedList[UserMapid[m.SendUserId]], m.SendUserId)
				}
				return true, aurora.Sprintf(aurora.Green("Send After Log Complete"))
			} else {
				return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Send After Log"))
			}
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}

	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : MapReady Message Length Error : need 0, received %d"), len(m.OtherMessage))
	}
}

func SendBeforeLog(conn *net.UDPConn, mapid string, userId string, result chan bool) {
	mapTimeData := bson.M{"mapCTime": ""}

	intmapid, _ := strconv.Atoi(mapid)

	filter := bson.M{"map_id": intmapid, "mapCTime": bson.M{"$exists": true}}
	projection := bson.M{"_id": 0, "mapCTime": 1}

	err := MapDB.Collection("map").FindOne(context.TODO(), filter, options.FindOne().SetProjection(projection)).Decode(&mapTimeData)
	if err != nil {
		fmt.Println(aurora.Sprintf(aurora.Red("Error in Getting Data from MapDB : %s"), err))
		result <- false
	}

	mapSaveTimeString := mapTimeData["mapCTime"].(string)

	mapSaveTime, err := time.Parse(time.RFC3339Nano, mapSaveTimeString)
	if err != nil {
		fmt.Println(aurora.Sprintf(aurora.Red("Error in Parsing Saved Time : %s"), err))
		result <- false
	}
	fmt.Printf("Map %s Saved at %s(in string : %s)\n", mapid, mapSaveTime, mapSaveTimeString)

	logResult, err := FindDocumentsAfterTime(mapSaveTime, mapid)
	if err != nil {
		fmt.Println(aurora.Sprintf(aurora.Red("Error in Load Log Result : %s"), err))
		result <- false
	}

	udpAddr, err := net.ResolveUDPAddr("udp", UserAddr[userId])
	if err != nil {
		fmt.Println(aurora.Sprintf(aurora.Red("Error : Resolve UDP Address Error Occured.\nError Message : %s"), err))
		result <- false
	}

	for _, logOne := range logResult {
		// fmt.Printf("Log : %s\n", logOne.OriginalMessage)
		conn.WriteToUDP([]byte(logOne.OriginalMessage+";s"), udpAddr)
		time.Sleep(50 * time.Millisecond)
	}
	result <- true
}

func FindDocumentsAfterTime(parsedTimeFromMaptime time.Time, mapid string) ([]LogResponseData, error) {
	collection := GameDB.Collection("log")

	fmt.Printf("Find start\n\n")
	currentTime := time.Now().UTC()
	// fmt.Printf("Parsed Time : %s\nCurrent Time : %s\nTime Duration : %f\n", parsedTimeFromMaptime, currentTime, currentTime.Sub(parsedTimeFromMaptime).Seconds())

	// fmt.Printf("Parsed Time : %s\nCurrent Time : %s\n", strconv.FormatInt(parsedTimeFromMaptime.UnixMilli(), 10), strconv.FormatInt(currentTime.UnixMilli(), 10))

	filter := bson.M{
		"timestamp": bson.M{
			"$gt": parsedTimeFromMaptime,
			"$lt": currentTime,
		},
		"mapid":           mapid,
		"originalmessage": bson.M{"$regex": "^Asset"},
	}

	cursor, err := collection.Find(context.TODO(), filter)
	if err != nil {
		return nil, err
	}
	defer cursor.Close(context.TODO())

	var results []LogResponseData
	// fmt.Printf("Next Cursor Start\n")
	for cursor.Next(context.TODO()) {
		var elem LogResponseData
		err := cursor.Decode(&elem)
		if err != nil {
			fmt.Printf("Error : %s\n", err)
			return nil, err
		}
		// fmt.Printf("LogResponseData : %s\n", elem.OriginalMessage)
		results = append(results, elem)
	}
	if err := cursor.Err(); err != nil {
		fmt.Printf("Error : %s\n", err)
		return nil, err
	}

	return results, nil
}

func FindLoadedUser(mapid string) []string {
	// mapid 맵 내에 있는 플레이어 중에서 맵 로딩이 모두 완료된 플레이어의 아이디를 랜덤으로 골라 Return
	// 241107 변경사항 : 맵 로딩이 완료된 모든 플레이어의 ID를 return
	// 다섯 번 검사하는 동안 아무도 로딩이 안되면 그냥 nil

	/*

		var selectedNumber []int

		for {
			randomNumber := rand.Intn(len(MapidUserList[mapid]))
			for !contains(selectedNumber, randomNumber) {
				randomNumber = rand.Intn(len(MapidUserList[mapid]))
			}

			randomUser := MapidUserList[mapid][randomNumber]
			loaded, exists := UserLoaded[randomUser]
			if exists && loaded {
				result <- randomUser
			} else {
				selectedNumber = append(selectedNumber, randomNumber)
				if len(selectedNumber) == len(MapidUserList[mapid]) {
					selectedNumber = []int{}
					time.Sleep(1 * time.Second)
				}
				continue
			}
		}
	*/

	fmt.Printf("Find Loaded Player Start\n")

	howMuchUsers := 1
	epoch := 0

	/*
		var loadedPlayersCount int


			if len(MapidLoadedList[mapid]) <= 10 {
				loadedPlayersCount = 10
			} else {
				loadedPlayersCount = int(float64(len(MapidLoadedList[mapid])) * 0.1)
			}
	*/

	loadedUsers := MapidLoadedList[mapid]

	if len(loadedUsers) == 0 {
		return nil
	}

	selectedUsers := []string{}

	for epoch < howMuchUsers {
		fmt.Printf("Epoch %d\n", epoch+1)
		fmt.Printf("Loaded Users : %s\n", loadedUsers)

		firstLoadedUsers := loadedUsers[0]
		selectedUsers = append(selectedUsers, firstLoadedUsers)
		loadedUsers = append(loadedUsers[1:], firstLoadedUsers)

		epoch += 1
	}

	MapidLoadedList[mapid] = loadedUsers

	for index, userid := range selectedUsers {
		selectedUsers[index] = UserAddr[userid]
	}

	fmt.Printf("Mapid Loaded List : %s\n", MapidLoadedList[mapid])

	fmt.Printf("Selected Users : %s\n", selectedUsers)

	return selectedUsers

}

func PlayerLeave(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// PlayerLeave 형태 :
	// PlayerLeave$SendUserId;SendTime
	// otherMessage length : 0
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		userAddr, isUserAddrExists := UserAddr[m.SendUserId]
		_, isUserMapidExists := UserMapid[m.SendUserId]

		if isUserAddrExists {
			delete(UserAddr, m.SendUserId)
			delete(AddrUser, userAddr)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
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

						fmt.Printf("Unlock Message : %s\n", unlockMessage)

						structUnlockMessage, err := MessageParser(unlockMessage)

						if !err {
							fmt.Printf("unlock message parsing error\n")
							return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Parsing Unlock Message"))
						}

						logData := LogMessage{
							Timestamp:       time.Now().UTC(),
							MapId:           userMapid,
							Message:         structUnlockMessage,
							OriginalMessage: unlockMessage,
						}

						_, insertErr := GameDB.Collection("log").InsertOne(context.TODO(), logData)
						if insertErr != nil {
							fmt.Printf("insert result : %s\n", insertErr)
						}
					}
				}
			}

			// delete(UserMapid, m.SendUserId)
			fmt.Println(
				aurora.Sprintf(
					aurora.Gray(12, "Player [%s] left Map [%s] | (%s)"), m.SendUserId, userMapid, userAddr))

			MapidLoadedList[userMapid] = remove(MapidLoadedList[userMapid], m.SendUserId) // 로딩 된 플레이어 목록에서 삭제

			return true, aurora.Sprintf(aurora.Green("Success : User [%s] left Map [%s]\n"), m.SendUserId, userMapid)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found Map ID of User [%s]"), m.SendUserId)
		}
	}

	return false, aurora.Sprintf(aurora.Yellow("Error : Unknown (in PlayerLeave)"))
}

func PlayerMove(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// PlayerMove 형태 :
	// PlayerMove$SendUserId;SendTime;Position;Rotation
	// otherMessage length : 2

	// 보낸 유저가 있는 유저면 return true
	// 딱히 더 할 작업은 없음
	if isUserExists(m.SendUserId) {
		return true, aurora.Sprintf(aurora.Green("Success : User [%s] move\n"), m.SendUserId)
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
	}
}

func AssetCreate(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// AssetCreate 형태 :
	// AssetCreate$SendUserId;SendTime;AssetId;ObjectId;Position;Rotation;Scale;Type;MeshCollider;Rigidbody
	// otherMessage length : 8

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) {
			if isCreator(m.SendUserId) {
				return true, aurora.Sprintf(aurora.Green("Success : User [%s] Asset [%s] Create\n"), m.SendUserId, m.OtherMessage[1])
			}
			return false, aurora.Sprintf(aurora.Yellow("Error : User [%s] is not Creator"), m.SendUserId)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : AssetCreate Message Length Error : need 8, received %d"), len(m.OtherMessage))
	}
}

func AssetMove(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// AssetMove 형태 :
	// AssetMove$SendUserId;SendTime;ObjectId;Position;Rotation;Scale
	// otherMessage length : 4

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]
				if isLocked(m.SendUserId, itemId) {
					if LockObjUser[itemId] == m.SendUserId {
						return true, aurora.Sprintf(aurora.Green("Success : User [%s] Asset [%s] Move\n"), m.SendUserId, m.OtherMessage[0])
					} else {
						return false, aurora.Sprintf(aurora.Yellow("Error : Item [%s] is not locked by [%s]"), itemId, m.SendUserId)
					}
				} else {
					return false, aurora.Sprintf(aurora.Yellow("Error : Unavailable Access to Asset Move"))
				}
			}
			return false, aurora.Sprintf(aurora.Yellow("Error : User [%s] is not Creator"), m.SendUserId)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : AssetMove Message Length Error : need 8, received %d"), len(m.OtherMessage))
	}
}

func AssetDelete(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// PlayerMove 형태 :
	// PlayerMove$sendUserId;sendTime;ObjectId
	// otherMessage length : 1

	// 보낸 유저가 있는 유저고, Creator List에 있으면 return true
	// 딱히 더 할 작업은 없음
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]
				if isLocked(m.SendUserId, itemId) {
					if LockObjUser[itemId] == m.SendUserId {
						ItemUnlock(m.SendUserId, itemId)
						return true, aurora.Sprintf(aurora.Green("Success : User [%s] Asset [%s] Delete\n"), m.SendUserId, m.OtherMessage[0])
					} else {
						return false, aurora.Sprintf(aurora.Yellow("Error : Item [%s] is not locked by [%s]"), itemId, m.SendUserId)
					}
				} else {
					return false, aurora.Sprintf(aurora.Yellow("Error : Unavailable Access to Asset Delete"))
				}
			}
			return false, aurora.Sprintf(aurora.Yellow("Error : User [%s] is not Creator"), m.SendUserId)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : AssetDelete Message Length Error : need 8, received %d"), len(m.OtherMessage))
	}
}

func AssetSelect(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// AssetSelect 형태 :
	// AssetSelect$SendUserId;SendTime;ObjectId;
	// otherMessage length : 1

	// Lock 함. 이미 누가 Lock 해놨으면 false 날리기

	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]

				if !isLocked(m.SendUserId, itemId) {
					ItemLock(m.SendUserId, itemId)
					fmt.Printf("MapidLockedList : %v\n", MapidLockedList[UserMapid[m.SendUserId]])
					fmt.Printf("LockObjUser : %s\n", LockObjUser[itemId])
					return true, aurora.Sprintf(aurora.Green("Success : User [%s] Asset [%s] Select\n"), m.SendUserId, m.OtherMessage[0])
				}
				return false, aurora.Sprintf(aurora.Yellow("Error : Item [%s] is Locked"), itemId)
			}
			return false, aurora.Sprintf(aurora.Yellow("Error : User [%s] is not Creator"), m.SendUserId)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : AssetSelect Message Length Error : need 1, received %d"), len(m.OtherMessage))
	}
}

func AssetDeselect(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// AssetDeselect 형태 :
	// AssetDeselect$SendUserId;SendTime;ObjectId;
	// otherMessage length : 1

	// Unlock 함.

	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) {
			if isCreator(m.SendUserId) {
				itemId := m.OtherMessage[0]

				if isLocked(m.SendUserId, itemId) {
					lockUser := LockObjUser[itemId]
					if m.SendUserId == lockUser {
						ItemUnlock(m.SendUserId, itemId)
						return true, aurora.Sprintf(aurora.Green("Success : User [%s] Asset [%s] Deselect\n"), m.SendUserId, m.OtherMessage[0])
					} else {
						return false, aurora.Sprintf(aurora.Yellow("Error : Item [%s] is Locked"), itemId)
					}
				} else {
					return false, aurora.Sprintf(aurora.Yellow("Error : Unavailable Access to Asset Deselect"), itemId)
				}

			}
			return false, aurora.Sprintf(aurora.Yellow("Error : User [%s] is not Creator"), m.SendUserId)
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : AssetSelect Message Length Error : need 1, received %d"), len(m.OtherMessage))
	}
}

func PlayerJump(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// PlayerJump 형태 :
	// PlayerJump$SendUserId;SendTime
	// otherMessage length : 0

	// 보낸 유저가 있는 유저면 return true
	// 딱히 더 할 작업은 없음
	if isUserExists(m.SendUserId) {
		return true, aurora.Sprintf(aurora.Green("Success : User [%s] Jump\n"), m.SendUserId)
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Errodr : Cannot Found User [%s]"), m.SendUserId)
	}
}

func ManagerEdit(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// NewMapCreate 형태 :
	// ManagerEdit$SendUserId;SendTime;EditUserId;{Add|Delete}
	// otherMessage length : 2

	// 1. SendUserId가 존재하는 유저고,
	// 2-1. {Add의 경우} SendUserId가 있는 맵의 Creator List 중 EditUserId가 없어야 함 (있으면 그냥 break)
	// 2-2. {Delete의 경우} SendUserId가 있는 맵의 Creator List 중 EditUserId가 있어야 함 (없으면 그냥 break)
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) {
			sendUserMapid, err := strconv.Atoi(UserMapid[m.SendUserId])
			if err != nil {
				log.Fatal()
				fmt.Printf("Error : Cannot Convert Mapid [%s]", err)
				return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Convert Mapid [%s]"), err)
			}

			creatorList := CreatorLists{}

			filter := bson.M{"map_id": sendUserMapid}

			err = GameDB.Collection("creators").FindOne(context.TODO(), filter).Decode(&creatorList)

			if err != nil {
				if err == mongo.ErrNoDocuments {
					fmt.Printf("Error : No Creator Lists in Map [%d]", sendUserMapid)
					return false, aurora.Sprintf(aurora.Yellow("Error : No Creator Lists in Map [%s]"), sendUserMapid)
				}
				fmt.Printf("Error : MongoDB Error [%s]", err)
				return false, aurora.Sprintf(aurora.Yellow("Error : MongoDB Error [%s]"), err)
			}

			var newCreatorList []string

			if m.OtherMessage[1] == "Add" {
				newCreatorList = append(creatorList.Creator_list, m.OtherMessage[0])

			} else if m.OtherMessage[1] == "Delete" {
				for _, creator := range creatorList.Creator_list {
					if m.OtherMessage[0] == creator {
						continue
					} else {
						newCreatorList = append(newCreatorList, creator)
					}
				}
			} else {
				fmt.Printf("Error : Unavailable Command [%s]", m.OtherMessage[1])
				return false, aurora.Sprintf(aurora.Yellow("Error : Unavailable Command [%s]"), m.OtherMessage[1])
			}

			fmt.Printf("New Creator List : %s\n", newCreatorList)

			filter = bson.M{"map_id": sendUserMapid}
			update := bson.M{
				"$set": bson.M{
					"creator_list": newCreatorList,
				},
			}

			_, err = GameDB.Collection("creators").UpdateOne(context.TODO(), filter, update)

			if err != nil {
				log.Fatal()
				fmt.Printf("Error : Creator List Update Error [%s]", err)
				return false, aurora.Sprintf(aurora.Yellow("Error : Creator List Update Error [%s]"), err)
			}

			err = CreatorListLoad()
			if err != nil {
				log.Fatal()
				fmt.Printf("Error : Creator List Error [%s]", err)
				return false, aurora.Sprintf(aurora.Yellow("Error : Refresh Creator List Error [%s]"), err.Error())
			}

			return true, aurora.Sprintf(aurora.Green("Success : User [%s] %s Map [%s]\n"), m.OtherMessage[0], m.OtherMessage[1], sendUserMapid)

		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : Cannot Found User [%s]"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : NewMapCreate Message Length Error : need 1, received %d"), len(m.OtherMessage))
	}
}

func MapInit(conn *net.UDPConn, m ReceiveMessage) (bool, string) {
	// MapInit 형태 :
	// MapInit$SendUserId;SendTime
	// otherMessage length : 0
	if otherMessageLengthCheck(m.CommandName, len(m.OtherMessage)) {
		if isUserExists(m.SendUserId) && isCreator(m.SendUserId) {
			return true, aurora.Sprintf(aurora.Green("Success : Map [%s] is Initialized\n"), UserMapid[m.SendUserId])
		} else {
			return false, aurora.Sprintf(aurora.Yellow("Error : MapInit User Error. %s is not creator or not in game"), m.SendUserId)
		}
	} else {
		return false, aurora.Sprintf(aurora.Yellow("Error : MapInit Message Length Error : need 0, received %d"), len(m.OtherMessage))
	}
}
