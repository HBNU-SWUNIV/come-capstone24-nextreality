using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NextReality.Networking.Response;
using NextReality.Networking.Request;
using NextReality.Data;

namespace NextReality.Asset
{
    public class MapToJson : MonoBehaviour
    {
        private static string mapServer;
        HttpRequests httpRequests;

        private MapData mapData;
        private List<MapObjectData> mapObjDatas = new List<MapObjectData>();

        private bool isSaveSuccess = false;
        private bool isLoadSuccess = false;

        private void Start()
        {
            httpRequests = Utilities.HttpUtil;
            mapServer = httpRequests.GetServerUrl(HttpRequests.ServerEndpoints.MapUpload);
        }

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
                bool isRequestSuccess = false;


                // 메타 데이터 저장
                yield return StartCoroutine(httpRequests.RequestPost(mapServer, metaData, (result) =>
                {
                    try
                    {
                        ResponseData response = JsonUtility.FromJson<ResponseData>(result);
                        if (response.CheckResult())
                        {
                            Debug.Log("MapData Save in Server");
                            isRequestSuccess = true;
                        }
                        else
                        {
                            Debug.Log("MapData Save Fail");
                            isRequestSuccess = false;
                        }
                    }
                    catch
                    {

                    }
                }));

                if (isRequestSuccess)
                {
                    for (int i = 0; i < mapObjDatas.Count; i++)
                    {
                        while (mapUpLoadFailCount < 5)
                        {
                            string objData = JsonUtility.ToJson(mapObjDatas[i]);
                            isRequestSuccess = false;

                            yield return StartCoroutine(httpRequests.RequestPost(mapServer, objData, (result) =>
                            {
                                try
                                {
                                    ResponseData response = JsonUtility.FromJson<ResponseData>(result);
                                    if (response.CheckResult())
                                    {
                                        Debug.Log("MapData Save in Server");
                                        isRequestSuccess = true;
                                    }
                                    else
                                    {
                                        Debug.Log("MapData Save Fail");
                                        isRequestSuccess = false;
                                    }
                                }
                                catch
                                {

                                }
                            }));

                            if (isRequestSuccess)
                            {
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
                }
            }

            if (mapUpLoadFailCount < 6) { isSaveSuccess = true; }
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
                string command = string.Format("?mapID={0}&version={1}&chunk={2}", map_id, 0, 0);
                Debug.Log("MapData Load from Server:    " + mapServer + command);

                // 메타 데이터 다운
                bool isRequestSuccess = false;

                yield return StartCoroutine(httpRequests.RequestGet(mapServer + command, (result) =>
                {
                    try
                    {
                        MapResponseData response = JsonUtility.FromJson<MapResponseData>(result);
                        if (response.CheckResult())
                        {
                            isRequestSuccess = true;

                            mapData = response.message;
                        }
                        else
                        {
                            isRequestSuccess = false;
                            Debug.Log("Load Fail");
                        }
                    }
                    catch
                    {
                        Debug.Log("Json Fail");
                    }

                }));

                if (isRequestSuccess)
                {
                    // 분할된 오브젝트 데이터 다운
                    for (int i = 0; i < mapData.objCount; i += MapDataController.chunkSize)
                    {
                        while (mapDownFailCount < 5)
                        {
                            command = string.Format("?mapID={0}&version={1}&chunk={2}", map_id, 0, i / MapDataController.chunkSize + 1);
                            Debug.Log("MapData Load from Server:    " + mapServer + command);

                            isRequestSuccess = false;
                            yield return StartCoroutine(httpRequests.RequestGet(mapServer + command, (result) =>
                            {
                                try
                                {
                                    ObjResponseData response = JsonUtility.FromJson<ObjResponseData>(result);
                                    if (response.CheckResult())
                                    {
                                        isRequestSuccess = true;

                                        mapObjDatas.Add(response.message);
                                    }
                                    else
                                    {
                                        isRequestSuccess = false;
                                    }
                                }
                                catch
                                {

                                }

                            }));

                            if (isRequestSuccess)
                            {
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

                // 메타 데이터와 오브젝트 데이터의 개수가 일치 여부 확인
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
