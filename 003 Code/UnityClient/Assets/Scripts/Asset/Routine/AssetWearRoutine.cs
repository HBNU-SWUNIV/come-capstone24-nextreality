using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace NextReality.Asset.Routine
{
	public class AssetWearRoutine : GltfRoutine
	{
		protected Queue<LoadTask> WearTasks = new Queue<LoadTask>();

		// 작업을 처리하는 메서드
		protected override IEnumerator ProceedTask(LoadTask wearTask)
		{
			// Task가 에셋 적용 과정을 거치지 않은 경우
			if (!wearTask.isWearSuccess)
			{
				// 동일한 에셋인 오브젝트가 존재하지 않을 경우
				if (!wearTask.isExist)
				{
					// Debug.Log("Wearing...	" + wearTask.astId);
					yield return StartCoroutine(WearGltf(wearTask)); // 에셋 적용

					if (wearTask.isFailOrStop) // 에셋 적용 실패 혹은 취소한 경우
					{
						Debug.LogError("Asset Wear Fail or Cancel	: " + wearTask.astId);
					}
				}
				else
				{
					wearTask.isWearSuccess = true; // 적용 과정 스킵
				}
			}

			// 리로드를 요청하지 않았을 경우
			if (!GetReLoad())
			{
				if (wearTask.isWearSuccess) // 적용 성공한 경우
				{
					wearTask.isAllSuccess = true;

					Managers.Gltf.GetRoutine(3).TaskInsert(wearTask); // End Routine에 해당 작업 부여

					Debug.Log("Asset Wear Success && Push EndTasks	: ast( " + wearTask.astId + " )");
				}
			}
		}

		protected override void SetTasks()
		{
			Tasks = WearTasks;
		}
	}
}