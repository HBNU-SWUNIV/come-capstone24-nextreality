package main

import (
	"game_http_go/routes"
	"log"
	"os"

	"github.com/gin-gonic/gin"
)

func main() {
	fpLog, err := os.OpenFile("logfile.txt", os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0666)
	if err != nil {
		panic(err)
	}
	defer fpLog.Close()

	// 표준로거를 파일로그로 변경
	log.SetOutput(fpLog)

	router := gin.Default()

	routes.GameRoute(router)

	log.Println("GameServer(HTTP) is starting...")
	err = router.Run("0.0.0.0:8060")
	if err != nil {
		log.Fatalf("Server failed to start: %v", err)
	}
}
