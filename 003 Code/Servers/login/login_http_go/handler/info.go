package handler

import (
	"database/sql"
	"log"
	"net/http"

	"login_http_go/db"
	"login_http_go/models"

	"github.com/labstack/echo"
)

func Info(c echo.Context) error {
	userId := c.QueryParam("user_id")
	user := new(models.User)

	// ---------- 전송 코드 -------------------
	e_1 := &models.Code{
		Code:    0,
		Message: "bad request",
	}

	e_2 := &models.Code{
		Code:    0,
		Message: "user not found",
	}
	// -----------------------------------------

	if err := c.Bind(user); err != nil {
		log.Println(err)
		return c.JSON(http.StatusBadRequest, e_1)
	}

	// db 연결
	db := db.GetConnector()
	log.Println("Connected DB")

	var send_Nickname string
	var send_Email string

	// 로그인 성공후 반환
	err := db.QueryRow("SELECT nickname, email FROM users WHERE user_id = ?", userId).Scan(&send_Nickname, &send_Email)
	if err != nil {
		if err == sql.ErrNoRows {
			return c.JSON(http.StatusOK, e_2)
		}
		log.Println("returnerr :", err)
		return err
	}

	c_1 := &models.CodeInfo{
		Code: 1,
		Message: models.UserInfo{
			User_id:  userId,
			Nickname: send_Nickname,
			Email:    send_Email,
		},
	}

	log.Println("find user :", userId) // 로그 출력
	return c.JSON(http.StatusOK, c_1)
}
