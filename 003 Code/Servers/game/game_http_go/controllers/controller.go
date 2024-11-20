package controllers

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"reflect"
	"strconv"

	// "fmt"
	"log"
	"net/http"
	"time"

	// "GameServer/controller"
	"game_http_go/configs"
	"game_http_go/responses"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
	// "go.mongodb.org/mongo-driver/bson"
	// "go.mongodb.org/mongo-driver/mongo/options"
)

var creatorCollection *mongo.Collection = configs.GetCollection(configs.DB, "creators")

func GetExistCreatorList() gin.HandlerFunc {
	return func(c *gin.Context) {

		ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()

		var result bson.M

		mapId := c.Query("map_id")

		intmapid, _ := strconv.Atoi(mapId)

		filter := bson.M{"map_id": intmapid}

		fmt.Println(filter)

		err := creatorCollection.FindOne(ctx, filter).Decode(&result)
		// fmt.Println("mapinfo", mapinfo)

		if err != nil {
			if err == mongo.ErrNoDocuments {

				c.JSON(http.StatusNotFound, responses.DefaultReponse{Code: 1, Message: "false"})
				return
			}
			c.JSON(http.StatusInternalServerError, responses.DefaultReponse{Code: 0, Message: err.Error()})
			return
		}

		// JSON으로 결과 반환

		c.JSON(http.StatusOK, responses.DefaultReponse{Code: 1, Message: "true"})
	}
}

// TODO :

func GetCreatorList() gin.HandlerFunc {
	return func(c *gin.Context) {

		ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()

		mapId := c.Query("map_id")

		fmt.Println(mapId)
		fmt.Println(reflect.TypeOf(mapId))

		creatorList := responses.CreatorListResponseMessage{}

		intmapid, _ := strconv.Atoi(mapId)

		filter := bson.M{"map_id": intmapid}
		projection := bson.M{"_id": 0}

		fmt.Println(filter)

		err := creatorCollection.FindOne(ctx, filter, options.FindOne().SetProjection(projection)).Decode(&creatorList)
		// fmt.Println("mapinfo", mapinfo)

		if err != nil {
			if err == mongo.ErrNoDocuments {

				c.JSON(http.StatusNotFound, responses.DefaultReponse{Code: 1, Message: "Successed, but no results found"})
				log.Println("(info)NoDocumentsErr :", err)
				return
			}
			c.JSON(http.StatusInternalServerError, responses.DefaultReponse{Code: 0, Message: err.Error()})
			return
		}

		// JSON으로 결과 반환

		c.JSON(http.StatusOK, responses.CreatorListResponse{Code: 1, Message: creatorList})
		log.Println("(list)Success")
	}
}

func GetCreatorListAll() gin.HandlerFunc {
	return func(c *gin.Context) {

		_, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()

		filter := bson.M{}
		projection := bson.M{"_id": 0}

		cursor, err := creatorCollection.Find(context.TODO(), filter, options.Find().SetProjection(projection))
		if err != nil {
			log.Fatal(err)
		}
		defer cursor.Close(context.TODO())

		// 결과 출력
		for cursor.Next(context.TODO()) {
			var result bson.M
			if err := cursor.Decode(&result); err != nil {
				log.Fatal(err)
			}
			fmt.Println(result)
		}

		if err := cursor.Err(); err != nil {
			log.Fatal(err)
		}

		c.JSON(http.StatusOK, responses.DefaultReponse{Code: 1, Message: "All List is done. Look Logs"})
		log.Println("(list)Success")
	}
}

var LoginServerEndpoint = "http://127.0.0.1:8000"

func HttpGet(url string) []byte {
	client := &http.Client{
		Timeout: 10 * time.Second,
	}

	// GET 요청 생성
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		fmt.Printf("Failed to create HTTP request: %s\n", err)
		return nil
	}

	// 필요 시 헤더 추가
	// req.Header.Set("Key", "Value")

	// 요청 보내기
	response, err := client.Do(req)
	if err != nil {
		fmt.Printf("HTTP request failed: %s\n", err)
		return nil
	}
	defer response.Body.Close()

	// 응답 본문 읽기
	body, err := io.ReadAll(response.Body)
	if err != nil {
		fmt.Printf("Failed to read response body: %s\n", err)
		return nil
	}

	// 응답 상태 코드 및 본문 출력
	// fmt.Printf("Response status code: %d\n", response.StatusCode)
	// fmt.Printf("Response body: %s\n", body)

	return body
}

func GetUserInfo(userid string) responses.CreatorListResponse {
	body := HttpGet(LoginServerEndpoint + "/info?userid=" + userid)
	var result responses.CreatorListResponse
	err := json.Unmarshal(body, &result)
	if err != nil {
		fmt.Printf("Failed to unmarshal JSON response : %s\n", err)
		return responses.CreatorListResponse{}
	}
	return result
}

type TestType struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
}

func GetTest() gin.HandlerFunc {
	return func(c *gin.Context) {

		_, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()

		c.JSON(http.StatusOK, TestType{Code: 1, Message: "good"})
		log.Println("(HTTP) Test OK")
	}
}
