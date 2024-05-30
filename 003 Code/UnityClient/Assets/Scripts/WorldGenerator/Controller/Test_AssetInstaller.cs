using System.Collections;
using UnityEngine;

namespace NextReality.Asset
{
	public class Test_AssetInstaller : MonoBehaviour
	{

		public LayerMask instantiateMask;
		public LayerMask destroyMask;

		public float maxInstantiateRange = 10.0f;
			
		Camera cam;
		Vector3 ScreenCenter;
		// Start is called before the first frame update

		private void Start()
		{
			if (WorldEditorController.Instance != null) StartCoroutine(StartController());
		}

		IEnumerator StartController()
		{
			while(true)
			{

				cam = Managers.Camera.mainGameCamera?.mainCam;

				if (cam != null) break;

				yield return null;
			}
			while(true)
			{
				//if (Input.GetMouseButtonDown(0))
				//{
				//	StartCoroutine(DestroyObject());
				//}
				ScreenCenter = new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2);


				if (Input.GetMouseButtonDown(1) && !Managers.Input.IsPointerOverUIObjectAll(1))
				{
					CreateObject();
				}
				yield return null;
			}


		}

		private void CreateObject()
		{
			AssetItem assetItem = WorldEditorController.Instance.SelectedAsset;

			// Debug.Log("[TestAssetInstaller]:	" + assetItem?.id ?? "null");
			if (assetItem == null) return;
			Ray ray = cam.ScreenPointToRay(ScreenCenter);
			bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxInstantiateRange, instantiateMask);

			Vector3 spawn_point = isHit
				? hit.point + new Vector3(0, 0, 0)
				: cam.transform.position + cam.transform.forward * maxInstantiateRange;

			GltfRoutineManager.Instance.CreateObject(assetItem.id, spawn_point, new Vector3(0,cam.transform.eulerAngles.y,0));
		}

		IEnumerator DestroyObject()
		{
			Ray ray = cam.ScreenPointToRay(ScreenCenter);
			bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxInstantiateRange, destroyMask);
			if (isHit)
			{
				AssetCollider targetAsset;
				if(hit.transform.TryGetComponent<AssetCollider>(out targetAsset))
				{
					Destroy(targetAsset.TargetAssetObject.gameObject);
					yield return new WaitForSecondsRealtime(0.3f);
				}
			}
			else
			{
				yield return null;
			}
		}
	}

}
