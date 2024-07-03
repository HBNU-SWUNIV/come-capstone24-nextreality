using NextReality.Asset.Routine;
using NextReality.Data.Schema;
using NextReality.Game;
using NextReality.Networking.Request;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Data;
using NextReality.Asset.UI;

namespace NextReality.Asset
{
    public class GltfRoutineManager : MonoBehaviour
    {
        private readonly List<GltfRoutine> AssetRoutines = new List<GltfRoutine>(); // Routine를 관리하는 리스트
        public Dictionary<int, AssetObject> objInstances = new Dictionary<int, AssetObject>();

        private static GltfRoutineManager instance = null;
        public static GltfRoutineManager Instance { get { return instance; } }
        public ObjectTransformer transformer = null;

        private void Awake()
        {
            if (GltfRoutineManager.instance == null)
            {
                GltfRoutineManager.instance = this;
                DontDestroyOnLoad(this.gameObject);

			}
            else
            {
                Destroy(this.gameObject);
                Debug.Log("Destroy GltfRoutineManager Object");
            }
        }

        //private void OnDestroy()
        //{
        //    if (GltfRoutineManager.instance == this)
        //    {
        //        GltfRoutineManager.instance = null;
        //    }
        //}

        private void Start()
        {
            GltfRoutine.SetEnvironment();

            AssetRoutines.Add(gameObject.AddComponent<AssetDownRoutine>());
            AssetRoutines.Add(gameObject.AddComponent<AssetLoadRoutine>());
            AssetRoutines.Add(gameObject.AddComponent<AssetWearRoutine>());
            AssetRoutines.Add(gameObject.AddComponent<AssetEndRoutine>());

            gameObject.AddComponent<AssetDownTest>(); // 캐싱 Test를 위한 컴포넌트
            gameObject.AddComponent<DifTimer>();

            gameObject.AddComponent<AssetDownTest>(); // 캐싱 Test를 위한 컴포넌트
            gameObject.AddComponent<DifTimer>();

            for (int i = 0; i < AssetRoutines.Count; i++)
            {
                RoutineStart(GetRoutine(i));
            }

            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj.CompareTag("Object"))
                {
                    AssetObject astObj = obj.GetComponent<AssetObject>();
                    objInstances.Add(astObj.object_id, astObj);
                }
            }
        }

        // Routine를 시작하는 메서드
        public void RoutineStart(GltfRoutine Routine)
        {
            Coroutine cor = StartCoroutine(Routine.GltfTaskCoroutine());
            Routine.SetCoroutine(cor);
        }

        // Routine를 멈추는 메서드
        public void RoutineStop(GltfRoutine Routine)
        {
            StopCoroutine(Routine.GetCoroutine());
        }

        public GltfRoutine GetRoutine(int index)
        {
            return AssetRoutines[index];
        }

        // 리로드 메서드
        public IEnumerator RoutineInit()
        {
            GltfRoutine.SetReLoad();

            yield return new WaitUntil(() => CheckEndTasks()); // Routine의 모든 작업이 취소되기를 기다림

            GltfRoutine.SetReLoad();
            Debug.Log("RoutineInit");
        }

        // 모든 작업의 종료 여부 확인 메서드
        public bool CheckEndTasks()
        {
            bool isExistTask = true;
            for (int i = 0; i < AssetRoutines.Count; i++)
            {
                isExistTask &= GetRoutine(i).TaskCount() == 0;
            }
            return isExistTask;
        }

        public void LoadTaskInsert(LoadTask task)
        {
            GetRoutine(0).TaskInsert(task);
        }

        public void CreateObject(string asset_id, Vector3 spawn_point)
        {
            CreateObject(asset_id, spawn_point, Vector3.zero);
        }

        public void CreateObject(string asset_id, Vector3 spawn_point, Vector3 direction)
        {
            ObjectData objData = new ObjectData();
            objData.ast_id = asset_id;
            objData.obj_id = UnityEngine.Random.Range(0, 1000000);
            objData.transform.position = new Position { x = spawn_point.x, y = spawn_point.y, z = spawn_point.z };
            objData.transform.rotation = new Rotation { x = direction.x, y = direction.y, z = direction.z };
            objData.transform.scale = new Scale { x = 1, y = 1, z = 1 };

            if (objData.ast_id != "0")
            {
                objData.type = "Object";
            }
            else
            {
                objData.type = "Tile";
            }
            objData.isMeshCollider = true;
            objData.isRigidbody = false;//(Random.Range(0, 2) == 0);


            if (CheckPosition(spawn_point))
            {
                //CreateObject(objData);

                Managers.Network.SendMessage(assetCreateMessage(objData));
            }
        }

        public bool CheckPosition(Vector3 spawn_point)
        {
            if (transformer != null)
            {
                if (spawn_point.x > transformer.CurMaxPosition.x) return false;
                else if (spawn_point.x < transformer.CurMinPosition.x) return false;

                if (spawn_point.y > transformer.CurMaxPosition.y) return false;
                else if (spawn_point.y < transformer.CurMinPosition.y) return false;

                if (spawn_point.z > transformer.CurMaxPosition.z) return false;
                else if (spawn_point.z < transformer.CurMinPosition.z) return false;
            }
            //Debug.Log(transformer);
            return true;
        }

        public void CreateObject(ObjectData objData)
        {
            if (objInstances.ContainsKey(objData.obj_id))
            {
                Debug.LogError("Exist object Id");
            }
            else
            {
                LoadTaskInsert(new LoadTask("test_user_id", objData.ast_id, objData)); // 에셋 Routine에게 Task 부여
            }
        }

        public void CreateObject(LoadTask loadTask)
        {
            int objKey = CheckExist(loadTask.objData.ast_id); // 동일한 에셋인 오브젝트 확인
            if (objKey != -1)
            {
                loadTask.isExist = true;
            }

            GameObject copyObject;
            if (loadTask.isExist) // 존재하면 오브젝트 복제
            {
                copyObject = Instantiate(objInstances[objKey].gameObject);
                copyObject.name = loadTask.objData.obj_id.ToString();
                //Debug.Log("---Object Copy---");
            }
            else // 존재하지 않으면 오브젝트 생성
            {
                copyObject = new GameObject(loadTask.objData.obj_id.ToString());
            }

            AssetObject asset = SetObjectInfo(copyObject, loadTask.objData); // 오브젝트 정보 설정
            loadTask.gltfObj = asset;
        }

        public string assetCreateMessage(ObjectData objData)
        {
            AssetCreateSchema schema = new AssetCreateSchema();
            schema.userId = Managers.User.Id;
            schema.objData = objData;
            string message = schema.StringifyData();
            Debug.Log("[GltfRoutineManager] Send:    " + message);

            return message;
        }

        // 오브젝트 정보 설정 메서드
        public AssetObject SetObjectInfo(GameObject copyObject, ObjectData objData)
        {
            copyObject.transform.position = new Vector3(objData.transform.position.x, objData.transform.position.y, objData.transform.position.z); ;
            copyObject.transform.eulerAngles = new Vector3(objData.transform.rotation.x, objData.transform.rotation.y, objData.transform.rotation.z);
            copyObject.transform.localScale = new Vector3(objData.transform.scale.x, objData.transform.scale.y, objData.transform.scale.z);

            if (objData.ast_id != "0")
            {
                copyObject.tag = "Object";
            }
            else
            {
                copyObject.tag = "Tile";
            }

            AssetObject asset = copyObject.GetComponent<AssetObject>();
            if (asset == null)
            {
                asset = copyObject.AddComponent<AssetObject>();
                if (asset != null)
                {
                    asset.object_id = objData.obj_id;
                    asset.asset_id = objData.ast_id;

                    asset.isMeshCollider = objData.isMeshCollider;
                    asset.isRigidBody = objData.isRigidbody;
                }
            }
            else
            {
                asset.object_id = objData.obj_id;
                asset.asset_id = objData.ast_id;

                asset.isMeshCollider = objData.isMeshCollider;
                asset.isRigidBody = objData.isRigidbody;
            }

            return asset;
        }

        // 동일한 에셋인 오브젝트 존재 여부 확인 메서드
        public int CheckExist(string ast_id)
        {
            int objKey = -1;
            foreach (KeyValuePair<int, AssetObject> objTuple in objInstances)
            {
                AssetObject astObj = objTuple.Value;

                if (ast_id == astObj.asset_id)
                {
                    objKey = objTuple.Key;
                    break;
                }
            }

            return objKey;
        }

        // Rigidbody 활성화 메서드
        public void ActiveRigidbody()
        {
            foreach (KeyValuePair<int, AssetObject> objTuple in objInstances)
            {
                AssetObject astObj = objTuple.Value;
                GameObject obj = astObj.gameObject;

                if (obj.CompareTag("Object") && astObj.isRigidBody)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                    }
                }
            }
        }

        public AssetObject GetAssetObjectById(int id)
        {
            if (objInstances.ContainsKey(id)) return objInstances[id];
            else return null;
        }

        public AssetObject GetAssetObjectById(string id)
        {
            if (objInstances.ContainsKey(int.Parse(id)))
            {
                return objInstances[int.Parse(id)];
            }
            else
            {
                return null;
            }
        }

        public void RemoveAssetObjectById(string id)
        {
            if (objInstances.ContainsKey(int.Parse(id)))
            {
                objInstances.Remove(int.Parse(id));
            }
        }
    }
}