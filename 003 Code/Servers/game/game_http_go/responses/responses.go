package responses

type CreatorListResponseMessage struct {
	Map_id       int      `bson:"map_id" json:"map_id"`
	Admin_id     string   `bson:"admin_id" json:"admin_id"`
	Creator_list []string `bson:"creator_list" json:"creator_list"`
}

type CreatorListResponse struct {
	Code    int                        `bson:"code" json:"code"`
	Message CreatorListResponseMessage `bson:"message" json:"message"`
}

type DefaultReponse struct {
	Code    int    `bson:"code" json:"code"`
	Message string `bson:"message" json:"message"`
}
