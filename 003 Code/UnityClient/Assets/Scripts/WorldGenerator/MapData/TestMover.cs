using NextReality.Asset.Routine;
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
		}

		IEnumerator CreateObject()
		{
			Ray ray = cam.ScreenPointToRay(ScreenCenter);
			bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxInstantiateRange, instantiateMask);

			Vector3 spawn_point = isHit
				? hit.point + new Vector3(0, 30, 0)
				: cam.transform.position + cam.transform.forward * maxInstantiateRange;

            GltfRoutineManager.Instance.CreateObject(onMyHand, spawn_point); // 에셋 Routine에게 Task 부여

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
