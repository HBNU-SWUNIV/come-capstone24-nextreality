using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using NextReality;
using static UnityEngine.Rendering.DebugUI.Table;
using System;
using NextReality.Data.Schema;
using NextReality.Data;

namespace NextReality.Asset
{
    public class MapDataController : MonoBehaviour
    {
        private static MapDataController instance = null;
        public static MapDataController Instance { get { return instance; } }

        private MapToJson mapToJson;
        public MapData mapInfo = new MapData();
        public int map_id;
        public bool isLoadStart = false;
        // public string map_name = "";


        public GameObject main; // 메인 오브젝트를 저장할 변수
        public static int chunkSize = 20;

        private void Awake()
        {
            if (MapDataController.instance == null)
            {
                MapDataController.instance = this;
            }
            else
            {
                Destroy(MapDataController.instance.gameObject);
                Debug.Log("Destroy MapDataController Object");
            }

            map_id = -1; // map_id 테스트

            if (Managers.Scene)
            {
                Managers.Scene.GetSceneParameter("MapId", out map_id);
            }
            mapInfo.map_id = map_id;
        }

        private void OnDestroy()
        {
            if (MapDataController.instance == this)
            {
                MapDataController.instance = null;
            }
        }

        private void Start()
        {
            //main = GameObject.FindWithTag("Player"); // "Player" 태그를 가진 오브젝트를 찾아 main 변수에 할당
            mapToJson = gameObject.AddComponent<MapToJson>(); // MapToJson 컴포넌트를 가져옴

            Managers.Client.AddJoinLocalPlayerEvent((player, userId) =>
            {
                StartCoroutine(MapLoad(mapInfo.map_id));
            });
        }


        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.N))
        //    {
        //        StartCoroutine(MapSave()); // 맵 저장
        //    }
        //    else if (Input.GetKeyDown(KeyCode.M))
        //    {

        //        StartCoroutine(MapLoad(mapInfo.map_id)); // 맵 불러오기
        //    }
        //}

        // 맵을 저장하는 메서드
        public IEnumerator MapSave()
        {
            //mapInfo.map_id = UnityEngine.Random.Range(0, 100000);
            if (Managers.Gltf.CheckEndTasks())
            {
                // 맵 데이터 생성
                MapData mapData = new MapData();
                List<MapObjectData> mapObjDatas = new List<MapObjectData>();

                int objCount = ObjectJsonFormat(mapObjDatas); // 맵 오브젝트 데이터 저장
                MapJsonFormat(mapData, objCount); // 맵 메타 데이터 저장

                Debug.Log("Map Save Start:  " + mapInfo.map_id);
                // 맵 데이터 저장
                yield return mapToJson.SaveMapData((mapData, mapObjDatas));

                //Debug.Log("MapSave");
            }
        }

        // 맵 데이터의 다양한 속성을 설정하는 메서드
        public void MapJsonFormat(MapData mapData, int objCount)
        {
            mapData.map_id = mapInfo.map_id;
            mapData.user_id = mapInfo.user_id;
            if (mapInfo.mapName.Equals(""))
            {
                mapData.mapName = mapData.map_id.ToString();
                //mapData.mapName = map_name;
            }
            else
            {
                mapData.mapName = mapInfo.mapName;
            }
            // mapData.mapCTime = DateTime.UtcNow.AddHours(9).ToString("O");
            mapData.mapCTime = DateTime.UtcNow.ToString("O");
            mapData.version = mapInfo.version;
            Bounds bounds = GetBounds("Floor");
            mapData.mapSize.horizontal = bounds.size.x;
            mapData.mapSize.height = bounds.size.y;
            mapData.mapSize.vertical = bounds.size.z;
            mapData.Tags = new List<string>(mapInfo.Tags);
            mapData.chunkSize = chunkSize;
            mapData.chunkNum = 0;
            mapData.objCount = objCount;
        }

        public Bounds GetBounds(string tag)
        {
            GameObject obj = GameObject.FindWithTag(tag);
            Bounds bounds = new Bounds();

            if (obj != null)
            {
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                bounds = meshRenderer.bounds;
            }
            return bounds;
        }

        // 모든 오브젝트의 정보를 Json 포맷에 맞추어 저장하는 메서드
        public int ObjectJsonFormat(List<MapObjectData> mapObjDatas)
        {

            List<ObjectData> objDatas = new List<ObjectData>();

            foreach (KeyValuePair<int, AssetObject> objTuple in Managers.Gltf.objInstances)
            {
                AssetObject astObj = objTuple.Value;
                GameObject obj = astObj.gameObject;

                if (obj.CompareTag("Object"))
                {
                    ObjectData objData = new ObjectData();
                    AssetObject asset = obj.GetComponent<AssetObject>();
                    if (asset != null)
                    {
                        objData.obj_id = asset.object_id;
                        objData.ast_id = asset.asset_id;
                        objData.isRigidbody = asset.isRigidBody;
                        objData.isMeshCollider = asset.isMeshCollider;
                    }

                    objData.transform.position = new Position { x = obj.transform.position.x, y = obj.transform.position.y, z = obj.transform.position.z };
                    objData.transform.rotation = new Rotation { x = obj.transform.eulerAngles.x, y = obj.transform.eulerAngles.y, z = obj.transform.eulerAngles.z };
                    objData.transform.scale = new Scale { x = obj.transform.localScale.x, y = obj.transform.localScale.y, z = obj.transform.localScale.z };

                    objData.type = obj.tag;

                    objDatas.Add(objData);
                }
            }

            // 맵에 오브젝트가 하나도 존재하지 않은 경우
            if (objDatas.Count == 0)
            {
                ObjectData objData = new ObjectData(); // Null 오브젝트 추가
                objData.type = "Null";
                objDatas.Add(objData);
            }

            MapObjectData mapObjData = new MapObjectData();
            mapObjData.map_id = mapInfo.map_id;
            mapObjData.version = 0;
            mapObjData.chunkNum = 1;

            // 맵 오브젝트 Json 분할
            for (int i = 0; i < objDatas.Count; i++)
            {
                if ((i + 1) % chunkSize == 0)
                {
                    mapObjData.objList.Add(objDatas[i]);
                    mapObjDatas.Add(mapObjData);
                    mapObjData = new MapObjectData();
                    mapObjData.map_id = mapInfo.map_id;
                    mapObjData.chunkNum = (i + 1) / chunkSize + 1;
                }
                else
                {
                    mapObjData.objList.Add(objDatas[i]);
                }
            }
            mapObjDatas.Add(mapObjData);

            return objDatas.Count;
        }

        public bool GetLoadStart()
        {
            return isLoadStart;
        }

        public void ConvertLoadStart()
        {
            isLoadStart = !isLoadStart;
        }

        // 저장된 맵을 불러오는 메서드
        public IEnumerator MapLoad(int map_id)
        {
            Debug.Log("MapLoad: " + map_id);
            yield return Managers.Gltf.RoutineInit(); // 에셋 Routine의 모든 작업을 취소
            yield return StartCoroutine(mapToJson.LoadMapData(map_id)); // 저장된 맵 데이터 불러오기

            if (mapToJson.GetLoadSuccess())
            {
                (MapData, List<MapObjectData>) mapDataTuple = mapToJson.GetMapData();

                mapInfo = mapDataTuple.Item1;

                //main.GetComponent<CharacterController>().enabled = false; // 메인 오브젝트(Character)의 중력을 해제

                DestroyAllGameObjects(); // 모든 게임 오브젝트를 파괴

                SendMapReady();
                StartCoroutine(ObjectLoad(mapDataTuple.Item2)); // 오브젝트 로드

                //main.transform.position = Vector3.zero; // 메인 오브젝트 위치를 초기화
                // 다른 스크립트에서 오브젝트의 위치 데이터에 영향을 주고 있어 적용이 되지 않아 수정 예정
                //Debug.Log("MapLoad");
                //main.GetComponent<CharacterController>().enabled = true; // 메인 오브젝트의 중력을 활성화
            }
        }

        // 저장된 오브젝트를 로드하는 메서드
        public IEnumerator ObjectLoad(List<MapObjectData> mapObjDatas)
        {
            Managers.Gltf.objInstances.Clear();

            // 불러온 맵 데이터를 기반으로 게임 오브젝트 생성
            foreach (MapObjectData mapObjData in mapObjDatas)
            {
                foreach (ObjectData objData in mapObjData.objList)
                {
                    if (objData.type == "Null") // Null 키워드는 오브젝트가 하나도 존재하지 않음을 뜻함
                    {
                        continue;
                    }

                    Managers.Gltf.CreateObject(objData);
                }
                yield return null;
            }

            //if (Managers.Gltf.CheckEndTasks())
            //{
            //    if (GetLoadStart())
            //    {
            //        Debug.Log("Send 11MapReady");
            //        SendMapReady();
            //        ConvertLoadStart();
            //    }
            //}
        }

        public void SendMapReady()
        {
            Managers.Network.SendMessage(mapReadyMessage);
        }

        public string mapReadyMessage
        {
            get
            {
                MapReadySchema schema = new MapReadySchema();
                string message = schema.StringifyData();

                Debug.Log("[MapDataController] Send:    " + message);

                return message;
            }
        }

        public void SendMapInit()
        {
            Managers.Network.SendMessage(mapInitMessage);
        }

        public string mapInitMessage
        {
            get
            {
                MapInitSchema schema = new MapInitSchema();
                string message = schema.StringifyData();

                Debug.Log("[MapDataController] Send:    " + message);

                return message;
            }
        }

        public IEnumerator MapInit(string userId)
        {
            yield return Managers.Gltf.RoutineInit(); // 에셋 Routine의 모든 작업을 취소
            Managers.Gltf.objInstances.Clear();

            DestroyAllGameObjects();

            if (Managers.User.Id == userId)
            {
                StartCoroutine(Managers.Map.MapSave());
            }
        }

        // 플레이어, 메인 카메라, 조명을 제외한 모든 게임 오브젝트를 파괴하는 메서드
        private void DestroyAllGameObjects()
        {

            GameObject[] gameObjects = FindObjectsOfType<GameObject>(); // 모든 게임 오브젝트를 찾음

            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.CompareTag("Object"))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}