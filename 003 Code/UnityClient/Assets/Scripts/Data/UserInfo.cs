using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data
{
	[Serializable]
	public class UserInfo
	{
		public string userId;
		public string nickname;
		public bool isLocal;

		public UserInfo() { }
		public UserInfo(
			string userId,
			string nicname,
			bool isLocal
			)
		{
			this.userId = userId;
			this.nickname = nicname;
			this.isLocal = isLocal;
		}
	}

}
