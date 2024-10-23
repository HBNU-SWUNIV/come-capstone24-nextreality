package routes

import (
	"map_http_go/controllers"

	"github.com/gin-gonic/gin"
)

func MapRoute(router *gin.Engine) {
	// map 관련 모든 routes 관리
	router.POST("/map_data", controllers.SaveMap())
	router.GET("/map_data", controllers.GetMap())
	router.GET("/map_list", controllers.GetList())
	router.GET("/map_time", controllers.GetTime())
	router.GET("/create_map", controllers.CreateNewMap())
}
