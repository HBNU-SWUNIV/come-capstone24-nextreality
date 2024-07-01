using NextReality.Asset;
using NextReality.Data.Schema;
using NextReality.Game;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace NextReality.Data
{

	public class BroadcastHandler : MonoBehaviour
	{

		private static BroadcastHandler instance = null;
		public static Dictionary<string, Action<ZSchema>> OnBroadcastSchemaMap = new Dictionary<string, Action<ZSchema>>();
		public static Dictionary<string, Func<string, ZSchema>> SchemaTypeMap = new Dictionary<string, Func<string, ZSchema>>();

		private void Awake()
		{
			if (null == instance)
			{
				instance = this;
			}
			else
			{
				Destroy(this.gameObject);
			}
		}

		public static BroadcastHandler Instance
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

		void Start()
		{
			PackageAddListener();
			Debug.Log("Listener On");
		}


		public void AddListener<S>(Action<S> handler) where S : ZSchema, new()
		{
			S typeSchema = new S();
			string typeName = typeSchema.SchemaType;
			if (!OnBroadcastSchemaMap.ContainsKey(typeName))
			{
				OnBroadcastSchemaMap.Add(typeName, (ZSchema raw) =>
				{
					Debug.Log("[BroadCastHandler] Invoke:   " + typeName);
					handler((S)raw);
				});
			}
			else
			{
				OnBroadcastSchemaMap[typeName] += (ZSchema raw) =>
				{
					Debug.Log("[BroadCastHandler] Invoke:   " + typeName);
					handler((S)raw);
				};
			}

			if (!SchemaTypeMap.ContainsKey(typeName))
			{
				SchemaTypeMap.Add(typeName, message => (S)Activator.CreateInstance(typeof(S), message));
			}
		}


		// 이벤트 리스너를 삭제하는 메서드 // FIXME
		public void RemoveListener(string command, Action<ZSchema> handler)
		{
			if (OnBroadcastSchemaMap.ContainsKey(command))
			{
				OnBroadcastSchemaMap[command] -= handler;
				if (OnBroadcastSchemaMap[command] == null)
				{
					OnBroadcastSchemaMap.Remove(command);
				}
			}
		}

		// 이벤트를 실행하는 메서드
		public void InvokeEvent(string wholeMessage)
		{

			string[] log = wholeMessage.Split(ProtocolConverter.commandSeparator);
			if (log.Length >= 2)
			{
				InvokeEventByCommand(log[0], log[1]);
			}
		}


		public void InvokeEventByCommand(string command, string message)
		{
			if (OnBroadcastSchemaMap.ContainsKey(command))
			{
				var schema = SchemaTypeMap[command](message);
				OnBroadcastSchemaMap[command]?.Invoke(schema);
			}
			else
			{
				Debug.Log(command + " not in Key");
			}
		}

		// 이벤트 리스너를 초기화하는 메서드
		public void PackageAddListener()
		{
			OnBroadcastSchemaMap.Clear();

			AddListener<AssetCreateSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					Managers.Gltf.CreateObject(schema.objData);
				}
				else
				{
					Debug.Log("You are not Creator");
				}
			});
			AddListener<AssetSelectSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					if (Managers.User.Id == schema.userId)
					{
						Managers.ObjectEditor.SelectObjectEvent(schema.objectId);
					}

					Managers.ObjectEditor.SetAssetOwner(schema.objectId, schema.userId);
				}
				else
				{
					Debug.Log("You are not Creator");
				}
			});
			AddListener<AssetMoveSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					if (Managers.Client.JoinTime < schema.messageTime)
					{
						if (Managers.User && schema.userId == Managers.User.Id) return;
					}

					AssetObject obj = Managers.Gltf.GetAssetObjectById(int.Parse(schema.objectId));

					if (obj != null)
					{
						obj.TransformBySchema(schema);
					}
				}
				else
				{
					//Debug.Log("You are not Creator");
				}
			});
			AddListener<AssetDeselectSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					if (Managers.User.Id == schema.userId)
					{
						Managers.ObjectEditor.EndTransformObject();
					}

					Managers.ObjectEditor.UnSetAssetOwner(schema.objectId);
				}
				else
				{
					Debug.Log("You are not Creator");
				}
			});
			AddListener<AssetDeleteSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					Managers.ObjectEditor.RemoveCustomObjectByObjID(schema.obj_id);
				}
				else
				{
					Debug.Log("You are not Creator");
				}
			});
			AddListener<MapChangeSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					//StartCoroutine(Managers.Map.MapLoad(schema.map_id));
				}
				else
				{
					Debug.Log("You are not Creator");
				}
			});
			AddListener<MapInitSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					StartCoroutine(Managers.Map.MapInit(schema.userId));
				}
				else
				{
					Debug.Log("You are not Creator");
				}
			});
			AddListener<PlayerJoinSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					if (!Managers.Client.GetContainPlayer(schema.joinPlayerId))
						Managers.Client.JoinPlayer(schema.joinPlayerId, schema.joinPlayerNickname, schema.messageTime);
				}
				else
				{
					Debug.Log("Join Fail");
				}
			});

			AddListener<PlayerJumpSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					var player = Managers.Client.GetPlayer(schema.jumpPlayerId);
					if (player != null && !player.userInfo.isLocal)
					{
						Managers.Client.GetPlayer(schema.jumpPlayerId)?.Jump(Managers.Chacacter.jumpSpeed);
					}

				}
				else
				{
					Debug.Log("Jump Fail");
				}
			});

			AddListener<PlayerMoveSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					Managers.Client.MovePlayer(schema.movePlayerId, schema.movePosition, schema.moveRotation, schema.messageTime);
				}
				else
				{
					Debug.Log("Move Fail");
				}
			});

			AddListener<PlayerLeaveSchema>((schema) =>
			{
				if (schema.isSuccess == "s")
				{
					if (Managers.User.IsLocalUserId(schema.leavePlayerId))
					{
						Managers.Client.ExitGame();
					}
					else
					{
						Managers.Client.LeavePlayer(schema.leavePlayerId);
					}
				}
				else
				{
					Debug.Log("Leave Fail");
				}
			});
		}
	}
}