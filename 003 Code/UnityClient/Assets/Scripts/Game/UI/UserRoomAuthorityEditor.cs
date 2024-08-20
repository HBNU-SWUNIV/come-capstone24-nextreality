using NextReality.Asset;
using NextReality.Data;
using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game.UI
{
	
	public class UserRoomAuthorityEditor : MonoBehaviour
	{

		[SerializeField]
		Color addAuthorityColor;
		[SerializeField]
		Color removeAuthorityColor;
		[SerializeField]
		Color masterAuthorityColor;
		[SerializeField]
		Color unabledAuthorityColor;

		HttpRequests httpRequests;

		private static UserRoomAuthorityEditor instance = null;

		private Dictionary<string, UserRoomAuthority> userMap = new Dictionary<string, UserRoomAuthority>();
		private Dictionary<string, UserRoomAuthority> managerMap = new Dictionary<string, UserRoomAuthority>();

		public UserRoomAuthorityListView userListView;
		public UserRoomAuthorityListView managerListView;

		public UserRoomAuthorityListElement listElementPrefab;

		private string creatorListServerUrl;


		private void Awake()
		{
			if (UserRoomAuthorityEditor.instance == null)
			{
				UserRoomAuthorityEditor.instance = this;
			}
			else
			{
				Destroy(UserRoomAuthorityEditor.instance.gameObject);
				Debug.Log("Destroy UserRoomAuthorityEditor Object");
			}
		}

		private void Start()
		{
			httpRequests = Utilities.HttpUtil;

			creatorListServerUrl = httpRequests.GetServerUrl(HttpRequests.ServerEndpoints.CreatorList);

			UserRoomAuthority user = new UserRoomAuthority();
			user.user_id = "aaa";
			user.roomAuthority = RoomAuthority.Manager;
			
			UserRoomAuthority user2 = new UserRoomAuthority();
			user2.user_id = "abcd";
			user2.roomAuthority = RoomAuthority.Normal;

			AddUser(user);
			AddUser(user2);

			UserRoomAuthority user4 = new UserRoomAuthority();
			user4.user_id = "abc";
			user4.roomAuthority = RoomAuthority.Manager;

			AddManager(user);
			AddManager(user4);

			}));
		}

		private void OnDestroy()
		{
			if (UserRoomAuthorityEditor.instance == this)
			{
				UserRoomAuthorityEditor.instance = null;
			}
		}

		public static UserRoomAuthorityEditor Instance
		{
			get { return instance; }
		}


		// Update is called once per frame
		void Update()
		{

		}

		public void AddUser(UserRoomAuthority user)
		{
			if (userMap.ContainsKey(user.user_id)) return;
			userMap.Add(user.user_id, user);
			userListView.AddUserRoomAuthority(user);
		}

		public void RemoveUser(UserRoomAuthority user)
		{
			if (!userMap.ContainsKey(user.user_id)) return;
			userMap.Remove(user.user_id);
			userListView.RemoveUserRoomAuthority(user);
		}

		public UserRoomAuthorityListElement GetUser(UserRoomAuthority user)
		{
			return userListView.GetUserRoomAuthorityListElement(user);
		}

		public void AddManager(UserRoomAuthority user)
		{
			if (managerMap.ContainsKey(user.user_id)) return;
			managerMap.Add(user.user_id, user);
			managerListView.AddUserRoomAuthority(user);
		}

		public void RemoveManager(UserRoomAuthority user)
		{
			if (!managerMap.ContainsKey(user.user_id)) return;
			managerMap.Remove(user.user_id);
			managerListView.RemoveUserRoomAuthority(user);
		}
		public UserRoomAuthorityListElement GetUManager(UserRoomAuthority user)
		{
			return managerListView.GetUserRoomAuthorityListElement(user);
		}

		public void SendConvertAuthority(UserRoomAuthority user)
		{
			if (user.roomAuthority == RoomAuthority.Manager)
			{
				user.roomAuthority = RoomAuthority.Normal;
			}
			else if (user.roomAuthority == RoomAuthority.Normal)
			{
				user.roomAuthority = RoomAuthority.Manager;
			}

			ConvertAuthority(user);
		}

		public void ConvertAuthority(UserRoomAuthority user)
		{
			if (user.roomAuthority == RoomAuthority.Normal)
			{
				if (managerMap.ContainsKey(user.user_id))
				{
					RemoveManager(user);
				}	
				if(userMap.ContainsKey(user.user_id))
				{
					GetUser(user).RefreshButton();
				}
			}
			else if (user.roomAuthority == RoomAuthority.Manager)
			{
				if (!managerMap.ContainsKey(user.user_id))
				{
					AddManager(user);
				}
				if (userMap.ContainsKey(user.user_id))
				{
					GetUser(user).RefreshButton();
				}
			}

		}

		public static Color AddAuthorityColor { get { return Instance.addAuthorityColor; } }
		public static Color RemoveAuthorityColor { get { return Instance.removeAuthorityColor; } }
		public static Color MasterAuthorityColor { get { return Instance.masterAuthorityColor; } }
		public static Color UnabledAuthorityColor { get { return Instance.unabledAuthorityColor; } }
	}

}
