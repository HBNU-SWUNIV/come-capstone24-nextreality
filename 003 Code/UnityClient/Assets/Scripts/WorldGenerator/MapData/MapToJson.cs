using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;

namespace NextReality.Asset
{
	public class MapToJson : MonoBehaviour
	{
		protected static string mapServer = "localhost:1901"; // map 서버 주소

		private MapData mapData;
		private List<MapObjectData> mapObjDatas = new List<MapObjectData>();

		private bool isSaveSuccess = false;
		private bool isLoadSuccess = false;

		// 맵 데이터를 저장하는 메서드
		public IEnumerator SaveMapData((MapData, List<MapObjectData>) mapDataTuple)
		{
			isSaveSuccess = false;
			mapData = mapDataTuple.Item1;
			mapObjDatas = mapDataTuple.Item2;

			int mapUpLoadFailCount = 0;
			while (mapUpLoadFailCount < 5)
			{
				string metaData = JsonUtility.ToJson(mapData);

				WWWForm metaForm = new WWWForm();
				metaForm.AddField("mapId", mapData.map_id);
				metaForm.AddField("Num", 0);
				metaForm.AddField("json", metaData);

				UnityWebRequest metaRequest = UnityWebRequest.Post(mapServer, metaForm);
				yield return metaRequest.SendWebRequest();

				if (metaRequest.result == UnityWebRequest.Result.Success)
				{
					for (int i = 0; i < mapObjDatas.Count; i++)
					{
						while (mapUpLoadFailCount < 5)
						{
							string objData = JsonUtility.ToJson(mapObjDatas[i]);

							WWWForm objForm = new WWWForm();
							objForm.AddField("mapId", mapData.map_id);
							objForm.AddField("Num", i + 1);
							objForm.AddField("json", objData);

							UnityWebRequest objRequest = UnityWebRequest.Post(mapServer, objForm);
							yield return objRequest.SendWebRequest();

							if (objRequest.result == UnityWebRequest.Result.Success)
							{
								Debug.Log("MapData Save in Server");
								break;
							}
							else
							{
								mapUpLoadFailCount++;
							}
						}
					}
					break;
				}
				else
				{
					mapUpLoadFailCount++;
					Debug.Log("Save Fail" + metaRequest.result);
				}
			}

			if (mapUpLoadFailCount < 5) { isSaveSuccess = true; }
			else { isSaveSuccess = false; }
		}

		// 맵 데이터를 불러오는 메서드
		public IEnumerator LoadMapData(int map_id)
		{
			isLoadSuccess = false;
			mapObjDatas = new List<MapObjectData>();
			int mapDownFailCount = 0;
			while (mapDownFailCount < 5)
			{
				string command = string.Format("/asset/map?map_id={0}&ast_id={1}", map_id, 0);
				Debug.Log(command);
				UnityWebRequest metaRequest = UnityWebRequest.Get(mapServer + command); // 맵 에코 서버 연결
				yield return metaRequest.SendWebRequest();

				// 웹 서버와 연결 성공하면 메타 데이터 다운
				if (metaRequest.result == UnityWebRequest.Result.Success)
				{
					string metaData = Encoding.UTF8.GetString(metaRequest.downloadHandler.data);
					mapData = JsonUtility.FromJson<MapData>(metaData);

					for (int i = 0; i < mapData.objCount; i += 15)
					{
						while (mapDownFailCount < 5)
						{
							command = string.Format("/asset/map?map_id={0}&ast_id={1}", map_id, i / 15 + 1);
							Debug.Log(command);
							UnityWebRequest objRequest = UnityWebRequest.Get(mapServer + command); // 맵 에코 서버 연결
							yield return objRequest.SendWebRequest();

							// 웹 서버와 연결 성공하면 오브젝트 데이터 다운
							if (objRequest.result == UnityWebRequest.Result.Success)
							{
								string mapObjData = Encoding.UTF8.GetString(objRequest.downloadHandler.data);
								mapObjDatas.Add(JsonUtility.FromJson<MapObjectData>(mapObjData));
								Debug.Log("MapData Load from Server");
								break;
							}
							else
							{
								mapDownFailCount++;
							}
						}
					}
					break;
				}
				else
				{
					mapDownFailCount++;
				}
			}

			if (mapDownFailCount < 5)
			{
				int objCount = 0;
				for (int i = 0; i < mapObjDatas.Count; i++)
				{
					objCount += mapObjDatas[i].objList.Count;
				}
				if (mapData.objCount == objCount) { isLoadSuccess = true; }
				else
				{
					isLoadSuccess = false;
					Debug.Log("Map Load Fail");
				}
			}
			else { isLoadSuccess = false; }

			yield break;
		}

		public (MapData, List<MapObjectData>) GetMapData()
		{
			return (mapData, mapObjDatas);
		}

		public bool GetSaveSuccess()
		{
			return isSaveSuccess;
		}

		public bool GetLoadSuccess()
		{
			return isLoadSuccess;
		}
	}
}
