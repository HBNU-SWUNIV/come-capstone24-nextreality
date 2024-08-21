using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NextReality.Data
{
    [System.Serializable]

    public class UserLoginData
    {
        private int user_key; // DB에서 자동 생성되는 primary key 의미
        public string user_id; // 아이디
        public string user_pw; // 비밀번호

        public UserLoginData()
        {

        }
        public UserLoginData(string _id, string _pw)
        {
            this.user_id = _id;
            this.user_pw = _pw;
        }

    }

    [System.Serializable]

    public class UserData : UserLoginData
    {

        public string nickname;
        public string email;
    }

    public enum RoomAuthority
    {
        Error = -1,
        Normal = 0,
        Manager = 1,
        Master = 2,
    }

	[System.Serializable]
	public class UserRoomAuthority
	{
        public UserData user;
        public RoomAuthority room_authority = RoomAuthority.Normal;
        public int map_id;
    }
    
}

