using GLTFast;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace NextReality.Game
{
	public class ObjectCRUD : MonoBehaviour
	{
		public GameObject map;
		public LayerMask instantiateMask; // ����ĳ��Ʈ ���� ����ũ
		public LayerMask destroyMask; // ����ĳ��Ʈ ���� ����ũ
		public GameObject onMyHand; // ����ĳ��Ʈ�� ������ ������Ʈ ������

		public float maxInstantiateRange = 10.0f; // ����ĳ��Ʈ �ִ� ���� 

		string gltfServer = "localhost:2002/"; // gtlf ���� �ּ�
		string localDirectory = "testDatas";



		Camera cam;
		Vector3 ScreenCenter;



		private void Awake()
		{

		}

		// Start is called before the first frame update
		void Start()
		{
			cam = Camera.main;

			string objectDir = Path.Combine(Application.persistentDataPath, localDirectory);
			if (!Directory.Exists(objectDir))
			{
				Directory.CreateDirectory(objectDir);
				Debug.Log("Create Directory");
			}


			ScreenCenter = new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2);
			StartCoroutine(ObjectCreate());
			StartCoroutine(ObjectDelete());
			StartCoroutine(ChangeMyHand());
		}

		// Update is called once per frame
		void Update()
		{

		}

		IEnumerator ObjectCreate()
		{
			while (Application.isPlaying)
			{
				if (Input.GetMouseButton(1)) // ������Ʈ ���� (Create)
				{
					Ray ray = cam.ScreenPointToRay(ScreenCenter);
					bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxInstantiateRange, instantiateMask);

					// hit ���� : hit �� ���� + ��ü�� ����
					// hit ���� : �ִ� raycast ���̸�ŭ
					Vector3 spawn_point = isHit
							? hit.point + new Vector3(0, onMyHand.transform.localScale.y / 2, 0)
							: cam.transform.position + cam.transform.forward * maxInstantiateRange;


					var copyObject = Instantiate(onMyHand, spawn_point, Quaternion.identity);
					copyObject.transform.SetParent(map.transform);
					copyObject.SetActive(true);
					yield return new WaitForSecondsRealtime(0.3f);
				}
				yield return null;
			}
		}
		IEnumerator ObjectDelete()
		{
			while (Application.isPlaying)
			{
				if (Input.GetMouseButton(0))
				{
					Ray ray = cam.ScreenPointToRay(ScreenCenter);
					bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxInstantiateRange, destroyMask);
					if (isHit)
					{
						Destroy(hit.transform.gameObject);
						yield return new WaitForSecondsRealtime(0.3f);
					}
					else
					{
						yield return null;
					}

				}
				yield return null;
			}
		}

		// gltf ���� �տ� ����ִ� �Լ�
		IEnumerator ChangeMyHand()
		{
			while (Application.isPlaying)
			{
				string filename;
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					Debug.Log("Pressed 1");
					filename = "duck.glb";
					LoadGltfModel(filename);
				}

				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					Debug.Log("Pressed 2");
					filename = "wolf.glb";
					LoadGltfModel(filename);
				}
				yield return null;
			}
		}

		async void LoadGltfModel(string filename)
		{
			bool isLoaded = false;
			bool hasOriginal = false;
			string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, filename);

			if (map.transform.Find(filename) != null) // �̹� ���ӿ� �ҷ�����������
			{
				hasOriginal = true; // ���� ������Ʈ�� ����
			}
			else
			{
				if (File.Exists(fullFilePath)) // ���ӿ��� ���µ� ���Ϸδ� ������
				{
					isLoaded = true; // �ε� �Ǿ�����
				}
				else // ���ӿ��� ���� ���Ϸε� ������
				{
					StartCoroutine(DownGltfModel(filename)); // ���� �ٿ� ����
					isLoaded = true;
				}
			}

			if (hasOriginal)
			{
				onMyHand = map.transform.Find(filename).gameObject;
			}
			else if (isLoaded)
			{
				Debug.Log("Loading...");
				byte[] byteData = File.ReadAllBytes(fullFilePath);
				var gltf = new GltfImport();
				bool success = await gltf.LoadGltfBinary(byteData);
				if (success)
				{
					GameObject gltfObj = new GameObject(filename);
					gltfObj.transform.position = new Vector3(0, -200, 0);
					gltfObj.transform.SetParent(map.transform);
					success = await gltf.InstantiateMainSceneAsync(gltfObj.transform);
					if (success)
					{
						MeshCollider collider = gltfObj.AddComponent<MeshCollider>();
						if (collider != null)
							collider.convex = true;

						Rigidbody rb = gltfObj.AddComponent<Rigidbody>();
						if (rb != null)
							rb.mass = 1.0f;

						BoxCollider boxCollider = gltfObj.AddComponent<BoxCollider>();

						gltfObj.layer = 6;
						gltfObj.SetActive(false);

						Debug.Log("Asset Load Success : " + fullFilePath);
						onMyHand = gltfObj;
					}
					else
					{
						Debug.Log("Failed to Load Asset");
					}
				}
				else
				{
					Debug.Log("Failed to Load Asset");
				}
			}
		}

		IEnumerator DownGltfModel(string filename)
		{
			Debug.Log("Download Start. URL : " + gltfServer + filename);
			UnityWebRequest request = UnityWebRequest.Get(gltfServer + filename);
			yield return request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
			{
				string fullPath = Path.Combine(Application.persistentDataPath, localDirectory, filename);

				FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create);
				fs.Write(request.downloadHandler.data, 0, (int)request.downloadedBytes);
				fs.Close();

				Debug.Log("Asset Down Success : " + fullPath);
			}
			else
			{
				Debug.LogError("Failed to Download GLTF File : " + request.error);
			}
			yield return null;
		}
	}

}
