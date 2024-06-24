using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NextReality.Data
{
    [System.Serializable]

    public class UserLoginData
    {
        private int user_key; // DB���� �ڵ� �����Ǵ� primary key �ǹ�
        public string user_id; // ���̵�
        public string user_pw; // ��й�ȣ

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
    
}

