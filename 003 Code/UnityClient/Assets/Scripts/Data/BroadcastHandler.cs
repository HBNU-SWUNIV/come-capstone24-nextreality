using NextReality.Asset;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data
{
	public class BroadcastHandler : MonoBehaviour
	{
		public static Dictionary<string, Action<string>> OnBroadcastReceiver = new Dictionary<string, Action<string>>();

		void Awake()
		{
			PackageAddListener();
			Debug.Log("Listener On");
		}

		// 이벤트 리스너를 추가하는 메서드
		public void AddListener(string command, Action<string> handler)
		{
			if (!OnBroadcastReceiver.ContainsKey(command))
			{
				OnBroadcastReceiver.Add(command, handler);
			}
			else
			{
				OnBroadcastReceiver[command] += handler;
			}
		}

		// 이벤트 리스너를 삭제하는 메서드
		public void RemoveListener(string command, Action<string> handler)
		{
			if (OnBroadcastReceiver.ContainsKey(command))
			{
				OnBroadcastReceiver[command] -= handler;
				if (OnBroadcastReceiver[command] == null)
				{
					OnBroadcastReceiver.Remove(command);
				}
			}
		}

		// 이벤트를 실행하는 메서드
		public void InvokeEvent(string command, string message)
		{
			if (OnBroadcastReceiver.ContainsKey(command))
			{
				OnBroadcastReceiver[command]?.Invoke(message);
			}
			else
			{
				Debug.Log(command + " not in Key");
			}
		}

		// 이벤트 리스너를 초기화하는 메서드
		public void PackageAddListener()
		{
			OnBroadcastReceiver.Clear();

			AddListener("AssetCreate", (message) =>
			{
				Debug.Log("AssetCreate");
				AssetCreateSchema astCreate = new AssetCreateSchema(message);

				GltfRoutineManager.Instance.CreateObject(astCreate.objData);
			});

			AddListener("AssetDelete", (message) =>
			{
				Debug.Log("AssetDelete");
			});

			AddListener("MapChange", (message) =>
			{
				Debug.Log("MapChange");
				MapChangeSchema mapChange = new MapChangeSchema(message);

				StartCoroutine(MapDataController.Instance.MapLoad(mapChange.map_id));
			});

			AddListener("PlayerJoin", (message) =>
			{
				Debug.Log("PlayerJoin");
				PlayerJoinSchema playerJoin = new PlayerJoinSchema(message);

				StartCoroutine(Managers.Client.PlayerJoin(playerJoin.joinPlayerId, playerJoin.joinPlayerNickname));

			});

			AddListener("PlayerLeave", (message) =>
			{
				Debug.Log("PlayerLeave");
				PlayerLeaveSchema playerLeave = new PlayerLeaveSchema(message);

				StartCoroutine(Managers.Client.PlayerLeave(playerLeave.leavePlayerId));
			});

			AddListener("PlayerMove", (message) =>
			{
				Debug.Log("PlayerMove");
				PlayerMoveSchema playerMove = new PlayerMoveSchema(message);

				StartCoroutine(Managers.Client.PlayerMove(playerMove.movePlayerId, playerMove.movePosition, playerMove.moveRotation));
			});
		}
	}
}