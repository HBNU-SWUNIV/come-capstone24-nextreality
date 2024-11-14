package main

import (
	"GameServer/controller"
	controllerdb "GameServer/controller-db"
	"fmt"
	"log"
	"net"
	"os"

	"github.com/logrusorgru/aurora"
)

func main() {
	fpLog, err := os.OpenFile("logfile.txt", os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0666)
	if err != nil {
		panic(err)
	}
	defer fpLog.Close()

	// 표준로거를 파일로그로 변경
	log.SetOutput(fpLog)

	controller.GameDB = controllerdb.ConnectDB("mongodb://mongo:27017", "GameServer")
	controller.MapDB = controllerdb.ConnectDB("mongodb://mongo:27016", "go_map")
	controller.CreatorListLoad()

	// UDP 서버 소켓 생성
	addr, err := net.ResolveUDPAddr("udp", ":8050")
	if err != nil {
		log.Println("Error : resolving UDP address:", err)
		fmt.Println("Error : resolving UDP address:", err)
		return
	}

	conn, err := net.ListenUDP("udp", addr)
	if err != nil {
		log.Println("Error : listening:", err)
		fmt.Println("Error : listening:", err)
		return
	}
	defer conn.Close()

	GetOutboundIP()

	log.Println(aurora.Green("============= Game Server Started ============="))
	fmt.Println(aurora.Green("============= Game Server Started ============="))

	for {
		controller.GetRequest(conn)
	}
}

func GetOutboundIP() {
	conn, err := net.Dial("udp", "0.0.0.0:8050")
	if err != nil {
		log.Fatal(err)
	}
	defer conn.Close()
	localAddr := conn.LocalAddr().(*net.UDPAddr)

	// fmt.Printf("Server Address : %s\n", localAddr.String())
	fmt.Println(
		aurora.Sprintf(
			aurora.Gray(12, "Server Address : %s"), localAddr.String()))
}

func ExitTask(sigChan chan os.Signal) {
	sig := <-sigChan
	fmt.Printf("\n\nReceived Signal : %s\n", sig)
	log.Println(aurora.Green("============= Game Server Closed ============="))
	fmt.Println(aurora.Green("============= Game Server Closed ============="))

	os.Exit(0)
}
