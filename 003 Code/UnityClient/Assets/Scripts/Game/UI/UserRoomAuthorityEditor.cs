using NextReality.Asset;
using NextReality.Data;
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

		private static UserRoomAuthorityEditor instance = null;

		private List<UserRoomAuthority> userList = new List<UserRoomAuthority>();
		private List<UserRoomAuthority> managerList = new List<UserRoomAuthority>();

		public UserRoomAuthorityListView userListView;
		public UserRoomAuthorityListView managerListView;

		public UserRoomAuthorityListElement listElementPrefab;


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
			UserRoomAuthority user = new UserRoomAuthority();
			user.user_id = "aaa";
			user.roomAuthority = RoomAuthority.Manager;
			
			UserRoomAuthority user2 = new UserRoomAuthority();
			user2.user_id = "abcd";
			user2.roomAuthority = RoomAuthority.Normal;

			userList.Add(user);
			userList.Add(user2);

			UserRoomAuthority user3 = new UserRoomAuthority();
			user.user_id = "aaa";
			user.roomAuthority = RoomAuthority.Manager;

			UserRoomAuthority user4 = new UserRoomAuthority();
			user2.user_id = "abc";
			user2.roomAuthority = RoomAuthority.Normal;

			managerList.Add(user3);
			managerList.Add(user4);

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

		public void AddManager(UserRoomAuthority user)
		{
			managerListView.AddUserRoomAuthority(user);
		}

		public void RemoveManager(UserRoomAuthority user)
		{
			managerListView.RemoveUserRoomAuthority(user);
		}

		public static Color AddAuthorityColor { get { return Instance.addAuthorityColor; } }
		public static Color RemoveAuthorityColor { get { return Instance.removeAuthorityColor; } }
		public static Color MasterAuthorityColor { get { return Instance.masterAuthorityColor; } }
		public static Color UnabledAuthorityColor { get { return Instance.unabledAuthorityColor; } }
	}

}
