using NextReality.Asset;
using NextReality.Data;
using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		private Dictionary<string, UserRoomAuthority> allUserAuthorityMap = new Dictionary<string, UserRoomAuthority>();

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

			ResetUserList();

		}

		public void ResetUserList()
		{
			int mapId = Managers.Map.map_id;

			if (mapId < 0)
			{
				Debug.Log("Map Id Fail");
				return;
			}

			Dictionary<string, string> queryPair = new Dictionary<string, string>
			{
				{ "map_id", mapId.ToString() }
			};

			StartCoroutine(httpRequests.RequestGet(creatorListServerUrl, queryPair, (result) =>
			{
				try
				{
					CreatorListResponseData response = JsonUtility.FromJson<CreatorListResponseData>(result);
					if (response.CheckResult())
					{


                        foreach (var item in response.message.creator_list)
                        {

							SetUserRoomAuthority(item, RoomAuthority.Manager);
						}
                    }
					else
					{
						
						Debug.Log("Load Fail");
					}
				}
				catch
				{
					Debug.Log("Json Fail");
				}

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

		public UserRoomAuthorityListElement GetUser(UserRoomAuthority user)
		{
			return userListView.GetUserRoomAuthorityListElement(user);
		}

		public void SetUserRoomAuthority(string userId, RoomAuthority roomAuthority = RoomAuthority.Normal, int? mapId = null)
		{
			UserData user;
			if (!Managers.Client.GetUserMap.TryGetValue(userId, out user))
			{
				user = new UserData();
				user.user_id = userId;
			}

			UserRoomAuthority userRoomAuthority;
			if(allUserAuthorityMap.TryGetValue(user.user_id, out userRoomAuthority))
			{
				if(userRoomAuthority.room_authority == RoomAuthority.Normal) userRoomAuthority.room_authority = roomAuthority;
			} else
			{
				if (mapId == null) mapId = Managers.Map.map_id;
				userRoomAuthority = new UserRoomAuthority();
				userRoomAuthority.user = user;
				userRoomAuthority.room_authority = roomAuthority;
				userRoomAuthority.map_id = mapId.Value;
			}

			if(roomAuthority == RoomAuthority.Normal)
			{
				userListView.AddUserRoomAuthority(userRoomAuthority);
				managerListView.RemoveUserRoomAuthority(userRoomAuthority);
			} else if(roomAuthority == RoomAuthority.Manager || roomAuthority == RoomAuthority.Master)
			{
				managerListView.AddUserRoomAuthority(userRoomAuthority);
			}
		}

		public void RemoveUser(string userId)
		{
			UserRoomAuthority userRoomAuthority;
			if (allUserAuthorityMap.TryGetValue(userId, out userRoomAuthority))
			{
				if(userRoomAuthority.room_authority == RoomAuthority.Normal && Managers.Client.GetUserMap.ContainsKey(userId))
				{
					allUserAuthorityMap.Remove(userId);
				}
			}
		}

		public UserRoomAuthorityListElement GetUManager(UserRoomAuthority user)
		{
			return managerListView.GetUserRoomAuthorityListElement(user);
		}

		public void SendConvertAuthority(UserRoomAuthority user)
		{

			if (user.room_authority == RoomAuthority.Manager)
			{
				user.room_authority = RoomAuthority.Normal;
			}
			else if (user.room_authority == RoomAuthority.Normal)
			{
				user.room_authority = RoomAuthority.Manager;
				
			}

			RefreshAuthorityState(user);
		}

		public void RefreshAuthorityState(UserRoomAuthority user)
		{
			GetUser(user)?.RefreshButton();
			SetUserRoomAuthority(user.user.user_id, user.room_authority);
		}

		public static Color AddAuthorityColor { get { return Instance.addAuthorityColor; } }
		public static Color RemoveAuthorityColor { get { return Instance.removeAuthorityColor; } }
		public static Color MasterAuthorityColor { get { return Instance.masterAuthorityColor; } }
		public static Color UnabledAuthorityColor { get { return Instance.unabledAuthorityColor; } }
	}

}
