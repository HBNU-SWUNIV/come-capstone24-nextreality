package controllerdb

import (
	"context"
	"fmt"
	"log"

	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

func ConnectDB() *mongo.Database {
	// 접속할 MongoDB 주소 설정
	clientOptions := options.Client().ApplyURI("mongodb://mongo:27017")

	// MongoDB 연결
	client, err := mongo.Connect(context.TODO(), clientOptions)
	if err != nil {
		log.Fatal("mongo connect err :", err)
	}

	// 연결 확인
	err = client.Ping(context.TODO(), nil)
	if err != nil {
		log.Fatal("mongo ping err :", err)
	}

	fmt.Println("Connected to MongoDB!")

	return client.Database("GameServer")
}
