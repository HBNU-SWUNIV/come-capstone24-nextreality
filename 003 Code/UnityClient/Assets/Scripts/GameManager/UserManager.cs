using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
	public class UserManager : MonoBehaviour
	{
		// �α����� �������� �� ���⿡ ���� �����͸� ����.
		// ���� DontDestroy�� ������ �̵��� ����

		private string id;
		private string nickname;
		private string email;

		// �̱���
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


		// ���� ���� ����� ������ ������ �� ���� id, �г���, �̸����� ���ÿ� �ϰ���
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
