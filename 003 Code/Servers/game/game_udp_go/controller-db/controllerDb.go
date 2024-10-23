package controllerdb

import (
	"context"
	"fmt"
	"log"

	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

func ConnectDB(mongoUri string, databaseName string) *mongo.Database {
	// 접속할 MongoDB 주소 설정
	clientOptions := options.Client().ApplyURI(mongoUri)

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

	fmt.Printf("Connected to %s!\n", mongoUri)

	return client.Database(databaseName)
}
