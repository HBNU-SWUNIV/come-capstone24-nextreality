using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality;
using System;
using TMPro;
using NextReality.Data;

using NextReality.Data.Schema;
using NextReality.Game.UI;
using System.Linq;


namespace NextReality.Game
{

	public class ClientManager : MonoBehaviour
	{
		public delegate void PlayerAvatarAction(GameAvatar avatar, string userId);

		private Dictionary<string, GameAvatar> players = new Dictionary<string, GameAvatar>();

		private Dictionary<string, UserData> userMap = new Dictionary<string, UserData>();

		private static ClientManager instance = null;

		public GameObject playersSpace;
		public GameAvatar playerAvatarPrefabs;

		public Transform spawnPoint;

		public GameAvatar myPlayerAvatar;

		protected PlayerAvatarAction localPlayerJoinEvents;
		protected PlayerAvatarAction playerJoinEvents;
		protected PlayerAvatarAction localPlayerLeaveEvents;
		protected PlayerAvatarAction playerLeaveEvents;

		public DateTime JoinTime;

		// 자신 Join Player 요청 시간
		public float sendWaitTickSeconds = 2.0f;
		public float maxSendWaitTickSeconds = 10f;
		private float curWaitTickSeconds = 0;

		public float sendMoveDelta = 0.1f;
		public float sendIdleDelta = 5.0f;


		private void Awake()
		{
			if (instance != null) 
			{
				Destroy(ClientManager.instance.gameObject);
			}

			instance = this;
		}

		private void OnDestroy()
		{
			if (ClientManager.instance == this)
			{
				ClientManager.instance = null;
			}
		}

		public static ClientManager Instance
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

		void LeaveLocalPlayer()
		{
			PlayerLeaveSchema schema = new PlayerLeaveSchema(
				Managers.User.Id,
				null
			);

			Debug.Log("[ClientManager] Send:	" + schema.StringifyData());

			Managers.Network.SendMessage(schema.StringifyData());
		}

		// Start is called before the first frame update
		void Start()
		{

			AddJoinLocalPlayerEvent((player, userId) =>
			{
				StartCoroutine(StartSendMoveMessage());
			});

			StartCoroutine(StartSendMyAvatar());

			Managers.Network.AddEndUDPEvnets(LeaveLocalPlayer);
		}

		// 자신의 캐릭터가 생성될 때까지 JoinPlayer Send
		IEnumerator StartSendMyAvatar()
		{

			var wait = new WaitForSeconds(sendWaitTickSeconds);
			do
			{
				PlayerJoinSchema schema = new PlayerJoinSchema(
					Managers.User.Nickname,
					Managers.Map.map_id
				);

				Managers.Network.SendMessage(schema.StringifyData());

				Debug.Log("[ClientManager] Send:	" + schema.StringifyData());
				yield return wait;
			
				if(myPlayerAvatar != null)
				{
					LoadingPage.Instance?.EndLoading();
					yield break;
				}
				curWaitTickSeconds += sendWaitTickSeconds;
			} while (curWaitTickSeconds < maxSendWaitTickSeconds);

			Debug.Log("[ClientManager] Denied Send PlayerJoin");

			StopAllCoroutines();
			ExitGame();

		}

		public void JoinPlayer(string playerId, string playerNickname, DateTime JoinTime)
		{
			if (!players.ContainsKey(playerId))
			{
				try
				{
					myPlayerAvatar = Instantiate(playerAvatarPrefabs);

					myPlayerAvatar.Init(playerId);

					myPlayerAvatar.transform.position = spawnPoint.position;
					myPlayerAvatar.transform.rotation = spawnPoint.rotation;
					myPlayerAvatar.transform.SetParent(playersSpace.transform);

					AddPlayerList(playerId, myPlayerAvatar );

					bool isLocal = Managers.User.IsLocalUserId(playerId);

					myPlayerAvatar.SetUserInfo(
						new Data.UserInfo(playerId, playerNickname, isLocal)
					);

					if (isLocal)
					{
						Managers.Chacacter.InitLocalPlayer(myPlayerAvatar);
						Managers.Client.JoinTime = JoinTime;
					}

					if (playerJoinEvents != null) playerJoinEvents(myPlayerAvatar, playerId);
					if (isLocal && localPlayerJoinEvents != null) localPlayerJoinEvents(myPlayerAvatar, playerId);

					Debug.Log("[ClientManager] Join Player:	" + playerId);

					// Debug.Log("Players : " + players);
				}
				catch (Exception e)
				{
					Debug.Log("Player Join Failed.\n Error Message : " + e);
					if (players.ContainsKey(playerId))
					{
						RemovePlayerList(playerId);
					}
				}

			}
			else
			{
				Debug.Log("[ClientManager] Join Player is already Joined" + playerId);
			}
		}

		public void LeavePlayer(string playerId)
		{
			Debug.Log("[ClientManager] Player Leave:	" + playerId);
			// check player dictionary 
			bool isRegistered = players.ContainsKey(playerId);

			// Debug.Log("Player is Registered:	" + isRegistered.ToString());

			if (!isRegistered)
			{
				return;
			}

			// check hirarchy object
			GameAvatar leavePlayer = players[playerId];

			// Debug.Log("Leave Player Name:	" + leavePlayer.name);


			if (playerLeaveEvents != null) playerLeaveEvents(leavePlayer, playerId);
			if (leavePlayer.IsLocal && localPlayerLeaveEvents != null) localPlayerLeaveEvents(leavePlayer, playerId);

			if (isRegistered) RemovePlayerList(playerId);
			if (leavePlayer != null) Destroy(leavePlayer.gameObject);
		}
		public void MovePlayer(string playerId, Vector3 position, Vector3 rotation, DateTime dateTime)
		{
			if (Managers.User.IsLocalUserId(playerId)) return;
			if (players.TryGetValue(playerId, out GameAvatar player))
			{
				//	position = player.transform.TransformDirection(position);
				if(!player.IsFirst)
				{
					player.Teleport(position, rotation);
				}
				else if (player.lastMoveTime< dateTime)
				{
					player.lastMoveTime = dateTime;
					player.Move(position);
					player.SetRoation(rotation);
				}
				Debug.Log(player.transform.position + " " + position);
				Debug.Log("[MovePlayer]:	" + dateTime.ToString());
			}
		}


		public void AddJoinPlayerEvent(PlayerAvatarAction action)
		{
			if (playerJoinEvents == null) playerJoinEvents = action;
			else playerJoinEvents += action;
		}

		public void AddJoinLocalPlayerEvent(PlayerAvatarAction action)
		{
			if (localPlayerJoinEvents == null) localPlayerJoinEvents = action;
			else localPlayerJoinEvents += action;

			Debug.Log("[ClientManager] Add Local Join");
		}

		public void AddLeavePlayerEvent(PlayerAvatarAction action)
		{
			if (playerLeaveEvents == null) playerLeaveEvents = action;
			else playerLeaveEvents += action;
		}

		public void AddLeaveLocalPlayerEvent(PlayerAvatarAction action)
		{
			if (localPlayerLeaveEvents == null) localPlayerLeaveEvents = action;
			else localPlayerLeaveEvents += action;
		}


		float curTime = 0;

		Vector3 tempPosition;
		Vector3 tempRotation;
		Vector3 tempScale;



		protected IEnumerator StartSendMoveMessage()
		{
			var wait = new WaitForSeconds(0.2f);
			GameAvatar localCharacter = null;

			while (localCharacter == null)
			{
				localCharacter = Managers.Chacacter.localChacter;
				yield return null;
			}

			bool isMoved = localCharacter.IsMoved;

			while (gameObject.activeInHierarchy)
			{
				

				if (!localCharacter.transform.position.Equals(tempPosition) || !localCharacter.transform.eulerAngles.Equals(tempRotation) || !localCharacter.transform.localScale.Equals(tempScale))
				{
					PlayerMoveSchema schema = new PlayerMoveSchema(
					localCharacter.transform.position,
					localCharacter.GetSchemaRotation()
					);
					Managers.Network.SendMessage(schema.StringifyData());
				}

				tempPosition = localCharacter.transform.position;
				tempRotation = localCharacter.transform.eulerAngles;
				tempScale = localCharacter.transform.localScale;



				// Utilities.UdpUtil.RequestsUdp(schema.StringifyData());
				// Managers.Network.SendMessage(schema.StringifyData());

				if(isMoved != localCharacter.IsMoved)
				{
					if (localCharacter.GetMoveTime() > sendIdleDelta)
					{
						wait = new WaitForSeconds(sendIdleDelta);
						isMoved = false;
					} else
					{
						wait = new WaitForSeconds(sendMoveDelta);
						isMoved = true;
					}
				}

				//Debug.Log("[ClientManager] Send PlayerMove" + schema.StringifyData());

				yield return wait;
			}
		}


		public GameAvatar GetPlayer(string userId)
		{
			if(players.TryGetValue(userId, out var player))
			{
				return player;
			}
			return null;
    }

		public bool GetContainPlayer(string userId)
		{
			return players.ContainsKey(userId);
		}

		public void ExitGame()
		{
			Managers.Scene.LoadMainMenu();
		}

		private void AddPlayerList(string playerId, GameAvatar avatar)
		{
			players.Add(playerId, avatar);
			AddUser(playerId);
		}

		private void RemovePlayerList(string playerId)
		{
			players.Remove(playerId);
			RemoveUser(playerId);
		}

		private void AddUser(string userId)
		{
			if (userMap.ContainsKey(userId)) return;

			UserData user;
			if(Managers.User.Id == userId)
			{
				user = Managers.User.GetLocalUserData();
			} else
			{
				 user = new UserData();
			}
			user.user_id = userId;

			userMap.Add(userId, user);
		}

		private void RemoveUser(string userId)
		{
			if (!userMap.ContainsKey(userId)) return;

			userMap.Remove(userId);
		}

		public Dictionary<string, UserData> GetUserMap
		{
			get
			{
				return userMap;
			}
		}

	}

}
