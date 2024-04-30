using NextReality.Asset.Routine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextReality.Asset
{
	public class GltfRoutineManager : MonoBehaviour
	{
		private static readonly List<GltfRoutine> AssetRoutines = new List<GltfRoutine>(); // Routine를 관리하는 리스트

		private static GltfRoutineManager instance = null;

		private void Awake()
		{
			if (GltfRoutineManager.instance == null)
			{
				GltfRoutineManager.instance = this;
			}
			else
			{
				GameObject.Destroy(GltfRoutineManager.instance.gameObject);
				Debug.Log("Destroy GltfRoutineManager Object");
			}
		}

		private void OnDestroy()
		{
			if (GltfRoutineManager.instance == this)
			{
				GltfRoutineManager.instance = null;
			}
		}

		private void Start()
		{

			GltfRoutine.SetEnvironment();

			AssetRoutines.Add(gameObject.AddComponent<AssetDownRoutine>());
			AssetRoutines.Add(gameObject.AddComponent<AssetLoadRoutine>());
			AssetRoutines.Add(gameObject.AddComponent<AssetWearRoutine>());
			AssetRoutines.Add(gameObject.AddComponent<AssetEndRoutine>());

			for (int i = 0; i < AssetRoutines.Count; i++)
			{
				RoutineStart(Get_Routine(i));
			}
		}

		public static GltfRoutineManager Instance { get { return instance; } }

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

		public GltfRoutine Get_Routine(int index)
		{
			return AssetRoutines[index];
		}

		// 리로드 메서드
		public IEnumerator RoutineInit()
		{
			GltfRoutine.SetReLoad();
			yield return new WaitUntil(() => AssetRoutines[0].TaskCount() == 0
											&& AssetRoutines[0].TaskCount() == 0
											&& AssetRoutines[0].TaskCount() == 0); // Routine의 모든 작업이 취소되기를 기다림
		}

		public void LoadTaskInsert(LoadTask task)
		{
			Get_Routine(0).TaskInsert(task);
		}

		public void CreateObject(int asset_id, Vector3 spawn_point)
		{
            CreateObject(asset_id, spawn_point, Vector3.zero);
		}

		public void CreateObject(int asset_id, Vector3 spawn_point, Vector3 direction)
		{
            ObjectData objData = new ObjectData();
            objData.ast_id = asset_id;
            objData.transform.position = new Position { x = spawn_point.x, y = spawn_point.y, z = spawn_point.z };
            objData.transform.rotation = new Rotation { x = direction.x, y = direction.y, z = direction.z };
            objData.transform.scale = new Scale { x = 1, y = 1, z = 1 };

            objData.type = "Object";
            objData.isMeshCollider = true;
            objData.isRigidbody = false;

            CreateObject(objData);
        }

		public void CreateObject(ObjectData objData)
		{
            var copyObject = new GameObject();
            copyObject.transform.position = new Vector3(objData.transform.position.x, objData.transform.position.y, objData.transform.position.z); ;
            copyObject.transform.eulerAngles = new Vector3(objData.transform.rotation.x, objData.transform.rotation.y, objData.transform.rotation.z);
            copyObject.transform.localScale = new Vector3(objData.transform.scale.x, objData.transform.scale.y, objData.transform.scale.z);

            copyObject.tag = tag;

            AssetObject asset = copyObject.AddComponent<AssetObject>();
            if (asset != null)
            {
                asset.object_id = objData.obj_id;
                asset.asset_id = objData.ast_id;

                asset.isMeshCollider = objData.isMeshCollider;
                asset.isRigidBody = objData.isRigidbody;
            }

            copyObject.SetActive(true);

            LoadTaskInsert(new LoadTask("test_user_id", asset.asset_id, asset)); // 에셋 Routine에게 Task 부여
        }
	}
}