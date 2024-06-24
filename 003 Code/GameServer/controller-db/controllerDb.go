package controllerdb

import (
	"GameServer/controller"
	"context"
	"fmt"
	"log"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

func ConnectDB(uri string) {
	clientOptions := options.Client().ApplyURI(uri)
	client, err := mongo.Connect(context.TODO(), clientOptions)
	if err != nil {
		fmt.Printf("MongoDB Connection Error : %s\n", err)
		return
	}
	fmt.Printf("MongoDB Connection Success\n")
	db := client.Database("GameServer")
	controller.DBClient = db
}

func GetCreatorList(db *mongo.Database) (map[string][]string, error) {
	collection := db.Collection("creators")
	cursor, err := collection.Find(context.TODO(), bson.D{})
	if err != nil {
		return nil, err
	}
	defer cursor.Close(context.TODO())

	creatorMap := make(map[string][]string)

	for cursor.Next(context.TODO()) {
		var creatorList controller.CreatorLists
		if err := cursor.Decode(&creatorList); err != nil {
			log.Println("Decode error:", err)
			continue
		}
		// fmt.Printf("Map %s Creators : %v\n", creatorList.MapId, creatorList.Creators)
		creatorMap[creatorList.MapId] = creatorList.Creators
	}

	if err := cursor.Err(); err != nil {
		return nil, err
	}

	return creatorMap, nil
}
