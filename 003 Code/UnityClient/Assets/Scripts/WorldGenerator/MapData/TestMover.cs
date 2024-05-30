using NextReality.Asset.Routine;
using NextReality.Networking.Request;
using System;
using System.Collections;
using UnityEngine;

namespace NextReality.Asset
{
	public class TestMover : MonoBehaviour
	{
		public GameObject map;
		public GameObject main;
		public GameObject copyObject;

		public LayerMask instantiateMask;
		public LayerMask destroyMask;
		public int onMyHand;

		public float maxInstantiateRange = 10.0f;

		Camera cam;
		Vector3 ScreenCenter;

		void Start()
		{
			main = GameObject.FindWithTag("Player");

			cam = Camera.main;
			ScreenCenter = new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2);
			onMyHand = 1;
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				onMyHand++;
				onMyHand %= 10;
				if (onMyHand == 0) onMyHand = 1;
			}
			if (Input.GetMouseButtonDown(0))
			{
				StartCoroutine(DestroyObject());
			}
			if (Input.GetMouseButtonDown(1))
			{
				StartCoroutine(CreateObject());
			}
			if(Input.GetKeyDown(KeyCode.K))
			{
				Debug.Log("Player Join");
				DateTime now = DateTime.UtcNow;
				now = now.AddHours(9);
				Debug.Log(now.ToString());
				Debug.Log(now.ToString("O"));
                // UdpRequests.Instance.RequestsUdp(UdpRequests.Instance.gameServerUrl, UdpRequests.Instance.gameServerPort, "PlayerJoin$testId;638506752265295630;testNickName;125");
            }
			if (Input.GetKeyDown(KeyCode.L))
			{
                Debug.Log("Player Move");
                // UdpRequests.Instance.RequestsUdp(UdpRequests.Instance.gameServerUrl, UdpRequests.Instance.gameServerPort, "PlayerMove$testId;638506755265295630;1.5/0/0;0/0/0");
            }
			if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                Debug.Log("Player Leave");
                // UdpRequests.Instance.RequestsUdp(UdpRequests.Instance.gameServerUrl, UdpRequests.Instance.gameServerPort, "PlayerLeave$testId;638506752265295630");
            }
        }

		IEnumerator CreateObject()
		{
			Ray ray = cam.ScreenPointToRay(ScreenCenter);
			bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxInstantiateRange, instantiateMask);

			Vector3 spawn_point = isHit
				? hit.point + new Vector3(0, 30, 0)
				: cam.transform.position + cam.transform.forward * maxInstantiateRange;

            //GltfRoutineManager.Instance.CreateObject("663dd3624fe104b4551ca873", spawn_point); // 에셋 Routine에게 Task 부여
            GltfRoutineManager.Instance.CreateObject(onMyHand.ToString(), spawn_point); // 에셋 Routine에게 Task 부여
            yield return null;
		}

		IEnumerator DestroyObject()
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
	}
}
