package controllerdb

import (
	"GameServer/controller"
	"context"
	"fmt"
	"log"

	"github.com/logrusorgru/aurora"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

func ConnectDB(uri string) {
	clientOptions := options.Client().ApplyURI(uri)
	client, err := mongo.Connect(context.TODO(), clientOptions)
	if err != nil {
		fmt.Println(aurora.Sprintf(
			aurora.Red("MongoDB Connection Error : %s"), err))
		return
	}
	fmt.Println(aurora.Green("MongoDB Connection Success"))
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
		creatorMap[creatorList.MapId] = creatorList.Creators
	}

	if err := cursor.Err(); err != nil {
		return nil, err
	}

	return creatorMap, nil
}
