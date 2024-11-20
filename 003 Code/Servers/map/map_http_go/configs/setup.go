package configs

import (
	"context"
	"fmt"
	"log"

	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

func ConnectDB(uri string) *mongo.Client {

	// 접속할 MongoDB 주소 설정
	clientOptions := options.Client().ApplyURI(uri)

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

	return client
}

// Client instance
var MapDB *mongo.Client = ConnectDB("mongodb://map_mongo:27016")
var GameDB *mongo.Client = ConnectDB("mongodb://game_mongo:27017")

// getting database collections
func GetCollection(client *mongo.Client, databaseName string, collectionName string) *mongo.Collection {
	collection := client.Database(databaseName).Collection(collectionName)
	return collection
}
