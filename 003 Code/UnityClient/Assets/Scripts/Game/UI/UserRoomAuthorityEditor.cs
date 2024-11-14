using NextReality.Asset;
using NextReality.Data;
using NextReality.Data.Schema;
using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

		[SerializeField] private Button closeButton;
		[SerializeField] private GameObject editorViewer;

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
			httpRequests = Utilities.HttpUtil;
			ResetUserList();

			closeButton.onClick.AddListener(() =>
			{
				SetActiveViewer(false);
			});
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

			StartCoroutine(httpRequests.RequestGet(httpRequests.GetServerUrl(HttpRequests.ServerEndpoints.CreatorList), queryPair, (result) =>
			{
				try
				{
					Debug.Log("UserRoomAuthorityEditor: " + result);
					CreatorListResponseData response = JsonUtility.FromJson<CreatorListResponseData>(result);
					if (response.CheckResult())
					{


                        foreach (var item in response.message.creator_list)
                        {

							SetUserRoomAuthority(item, item==response.message.admin_id? RoomAuthority.Master : RoomAuthority.Manager);
						}

						if (response.message.admin_id == Managers.User.Id) Managers.GameSettingController.ActiveRoomAuthorityEditButton();
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


		public void SetActiveViewer(bool active)
		{
			editorViewer.SetActive(active);
		}

		public UserRoomAuthorityListElement GetUser(UserRoomAuthority user)
		{
			return userListView.GetUserRoomAuthorityListElement(user);
		}

		public void SetUserRoomAuthority(UserData user, RoomAuthority roomAuthority = RoomAuthority.Normal, int? mapId = null)
		{
			if (roomAuthority == RoomAuthority.Error) return;

			UserRoomAuthority userRoomAuthority;
			if (allUserAuthorityMap.TryGetValue(user.user_id, out userRoomAuthority))
			{
				if (userRoomAuthority.room_authority == RoomAuthority.Normal) userRoomAuthority.room_authority = roomAuthority;
			}
			else
			{
				if (mapId == null) mapId = Managers.Map.map_id;
				userRoomAuthority = new UserRoomAuthority();
				userRoomAuthority.user = user;
				userRoomAuthority.room_authority = roomAuthority;
				userRoomAuthority.map_id = mapId.Value;
				allUserAuthorityMap.Add(user.user_id, userRoomAuthority);
			}

			if (Managers.Client.GetUserMap.ContainsKey(user.user_id))
			{
				userListView.AddUserRoomAuthority(userRoomAuthority);
			}
			if (userRoomAuthority.room_authority == RoomAuthority.Manager || userRoomAuthority.room_authority == RoomAuthority.Master)
			{
				managerListView.AddUserRoomAuthority(userRoomAuthority);
			} else
			{
				managerListView.RemoveUserRoomAuthority(userRoomAuthority);
				if (!Managers.Client.GetUserMap.ContainsKey(user.user_id))
				{
					allUserAuthorityMap.Remove(user.user_id);
				}
			}

			//Debug.Log("UserRoomAuthorityEditor: List:" + String.Join(",",allUserAuthorityMap.Keys.Count));
		}

		public void SetUserRoomAuthority(string userId, RoomAuthority roomAuthority = RoomAuthority.Normal, int? mapId = null)
		{
			UserData user;
			if (!Managers.Client.GetUserMap.TryGetValue(userId, out user))
			{
				user = new UserData();
				user.user_id = userId;

				if(roomAuthority == RoomAuthority.Normal)
				{
					UserRoomAuthority userRoomAuthority;
					if (allUserAuthorityMap.TryGetValue(userId, out userRoomAuthority))
					{
						allUserAuthorityMap.Remove(userId);
						userListView.RemoveUserRoomAuthority(userId);
						//Debug.Log("UserRoomAuthorityEditor: List: " + String.Join(",", allUserAuthorityMap.Keys.Count));
						return;
					}
				}
			}

			SetUserRoomAuthority(user, roomAuthority, mapId);
		}

		public void SetUserRoomAuthority(string userId, string roomAuthorityString)
		{

			RoomAuthority targetAuthority = RoomAuthority.Error;
			switch(roomAuthorityString)
			{
				case "Add": targetAuthority = RoomAuthority.Manager ; break;
				case "Delete": targetAuthority = RoomAuthority.Normal; break;
			}

			SetUserRoomAuthority(userId, targetAuthority);
		}

		//public void RemoveUser(string userId)
		//{
		//	UserRoomAuthority userRoomAuthority;
		//	if (allUserAuthorityMap.TryGetValue(userId, out userRoomAuthority))
		//	{
		//		if(userRoomAuthority.room_authority == RoomAuthority.Normal && !Managers.Client.GetUserMap.ContainsKey(userId))
		//		{
		//			allUserAuthorityMap.Remove(userId);
		//			userListView.RemoveUserRoomAuthority(userId);
		//		}
		//	}
		//}

		public UserRoomAuthorityListElement GetUManager(UserRoomAuthority user)
		{
			return managerListView.GetUserRoomAuthorityListElement(user);
		}

		public void SendConvertAuthority(UserRoomAuthority user, RoomAuthority roomAuthority)
		{

			//user.room_authority = roomAuthority;

			ManagerEditSchema schema = new ManagerEditSchema();
			if (roomAuthority == RoomAuthority.Normal) schema.SetActionDelete();
			else if (roomAuthority == RoomAuthority.Manager) schema.SetActionAdd();

			schema.editorUserId = user.user.user_id;

			Managers.Network.SendMessage(schema.StringifyData());
		}

		//public void RefreshAuthorityState(UserRoomAuthority user)
		//{
		//	GetUser(user)?.RefreshButton();
		//	SetUserRoomAuthority(user.user.user_id, user.room_authority, user.map_id);
		//}

		public static Color AddAuthorityColor { get { return Instance.addAuthorityColor; } }
		public static Color RemoveAuthorityColor { get { return Instance.removeAuthorityColor; } }
		public static Color MasterAuthorityColor { get { return Instance.masterAuthorityColor; } }
		public static Color UnabledAuthorityColor { get { return Instance.unabledAuthorityColor; } }
	}

}
