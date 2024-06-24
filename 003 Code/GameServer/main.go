package main

import (
	"GameServer/controller"
	controllerdb "GameServer/controller-db"
	"encoding/json"
	"fmt"
	"io"
	"net"
	"os"
	"os/signal"
	"syscall"
)

type Config struct {
	MapServerURL   string `json:"MapServerURL"`
	MongoURL       string `json:"MongoURL"`
	GameServerPort string `json:"GameServerPort"`
}

var mainConfig Config

func main() {

	// TODO : 로그 파일 만들기
	/*
		// 로그 파일 생성
		logFile, err := os.OpenFile("log.txt", os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0666)
		if err != nil {
			fmt.Println("Failed to open log file:", err)
			return
		}
		defer logFile.Close()

		// MultiWriter를 사용하여 로그 출력을 터미널과 파일로 동시에 리다이렉트
		multiWriter := io.MultiWriter(os.Stdout, logFile)

		// os.Stdout을 multiWriter로 변경
		r, w, _ := os.Pipe()
		os.Stdout = w

		// stdout 복사 고루틴 시작
		go func() {
			_, _ = io.Copy(multiWriter, r)
		}()

	*/

	// Config 파일 불러오기
	_config, err := LoadConfig("ServerConfig.json")

	if err != nil {
		fmt.Printf("Config File Load Failed\nError : %s", err)
	}

	mainConfig = _config

	// Creator List 불러오기
	controllerdb.ConnectDB(mainConfig.MongoURL)
	controller.MapCreatorList, err = controllerdb.GetCreatorList(controller.DBClient)
	controller.MapServerURL = mainConfig.MapServerURL
	// fmt.Printf("Creators : %v", controller.MapCreatorList)

	if err != nil {
		fmt.Printf("Config File Load Failed\nError : %s", err)
	}

	// UDP 서버 소켓 생성
	addr, err := net.ResolveUDPAddr("udp", ":"+mainConfig.GameServerPort)
	if err != nil {
		fmt.Printf("Error : resolving UDP address: %s\n", err)
		return
	}

	conn, err := net.ListenUDP("udp", addr)
	if err != nil {
		fmt.Printf("Error : listening: %s\n", err)
		return
	}
	defer conn.Close()

	fmt.Printf("============= Game Server Started =============\n")

	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, syscall.SIGINT, syscall.SIGTERM)
	go ExitTask(sigChan)

	// go GetOutboundIP()

	// 클라이언트 요청 처리
	for {
		// 클라이언트 요청
		controller.GetRequest(conn)
	}
}

/*
func GetOutboundIP() {
	conn, err := net.Dial("udp", "0.0.0.0:"+mainConfig.GameServerPort)
	if err != nil {
		log.Fatal(err)
	}
	defer conn.Close()
	localAddr := conn.LocalAddr().(*net.UDPAddr)

	// fmt.Printf("Server Address : %s\n", localAddr.String())
	// fmt.Println(
	// 	aurora.Sprintf(
	// 		aurora.Gray(12, "Server Address : %s"), localAddr.String()))
}
*/

func LoadConfig(filename string) (Config, error) {
	var config Config
	file, err := os.Open(filename)
	if err != nil {
		return config, err
	}
	defer file.Close()

	data, err := io.ReadAll(file)
	if err != nil {
		return config, err
	}
	err = json.Unmarshal(data, &config)

	return config, err
}

func ExitTask(sigChan chan os.Signal) {
	sig := <-sigChan
	fmt.Printf("\n\nReceived Signal : %s\n", sig)
	fmt.Printf("============= Game Server Closed =============\n")

	os.Exit(0)
}
