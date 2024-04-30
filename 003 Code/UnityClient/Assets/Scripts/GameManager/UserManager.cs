using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
	public class UserManager : MonoBehaviour
	{
		// 로그인이 성공했을 때 여기에 유저 데이터를 저장.
		// 이후 DontDestroy로 데이터 이동할 예정

		private string id;
		private string nickname;
		private string email;

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
					return instance.id;
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


		// 유저 정보 저장과 삭제는 무조건 한 번에 id, 닉네임, 이메일을 동시에 하게함
		public void SetUser(string _id, string _nickname, string _email)
		{
			instance.id = _id;
			instance.nickname = _nickname;
			instance.email = _email;
		}

		public void ResetUser()
		{
			instance.id = null;
			instance.nickname = null;
			instance.email = null;
		}

	}

}
