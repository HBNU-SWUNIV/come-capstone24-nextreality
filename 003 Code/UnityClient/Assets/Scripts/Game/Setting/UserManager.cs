using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Game.UI;

namespace NextReality.Game
{
    public class UserManager : MonoBehaviour
    {
        // 로그인이 성공했을 때 여기에 유저 데이터를 저장.
        // 이후 DontDestroy로 데이터 이동할 예정

        private string userId = "testId";
        private string nickname = "testNickname";
        private string email = "test@test.com";
        private bool isLogin = false;

        // 싱글톤
        private static UserManager instance = null;

        private void Awake()
        {
            if (null == instance)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public static UserManager Instance
        {
            get
            {
                if (null == instance)
                {
                    return null;
                }
                return instance;
            }
        }

        public string Id
        {
            get
            {
                if (null != instance)
                    return instance.userId;
                else
                    return null;
            }
        }

        public string Nickname
        {
            get
            {
                if (null != instance)
                    return instance.nickname;
                else
                    return null;
            }
        }

        public string Email
        {
            get
            {
                if (null != instance)
                    return instance.email;
                else
                    return null;
            }
        }

        public bool IsLogin
        {
            get
            {
                return isLogin;
            }
        }

        public bool IsLocalUserId(string userId)
        {
            return userId == this.userId;
        }


        // 유저 정보 저장과 삭제는 무조건 한 번에 id, 닉네임, 이메일을 동시에 하게함
        public void SetUser(string _id, string _nickname, string _email)
        {
            instance.userId = _id;
            instance.nickname = _nickname;
            instance.email = _email;
            instance.isLogin = true;

        }

        public void ResetUser()
        {
            instance.userId = null;
            instance.nickname = null;
            instance.email = null;
            instance.isLogin = false;
        }

    }

}
