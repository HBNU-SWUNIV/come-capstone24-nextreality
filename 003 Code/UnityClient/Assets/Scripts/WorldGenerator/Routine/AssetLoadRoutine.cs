using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace NextReality.Asset.Routine
{
	public class AssetLoadRoutine : GltfRoutine
	{
		protected Queue<LoadTask> LoadTasks = new Queue<LoadTask>();


		// 작업을 처리하는 메서드
		protected override IEnumerator ProceedTask(LoadTask loadTask)
		{
			// Task가 로드 과정을 거치지 않은 경우
			if (!loadTask.isLoadSuccess)
			{
				Debug.Log("Loading...	" + loadTask.astId);
				yield return StartCoroutine(LoadGltf(loadTask, glb_data[loadTask.astId])); // 에셋 로드
				
				// 로드 실패 혹은 취소한 경우
				if (loadTask.isFailOrStop)
				{
					Debug.LogError("Asset Load Fail or Cancel");
				}
			}

			
			if (!GetReLoad()) // 리로드를 요청하지 않았을 경우
			{
				// 로드 성공한 경우
				if (loadTask.isLoadSuccess)
				{
                    GltfRoutineManager.Instance.Get_Routine(2).TaskInsert(loadTask); // Wear Routine에 해당 작업 부여

					Debug.Log("Asset Load Success && Push WearTasks	:" + loadTask.astId);
				}
			}
			else // 리로드를 요청한 경우
			{
				LoadTasks.Clear(); // Load Routine의 작업 초기화
			}
		}

		protected override void SetTasks()
		{
			Tasks = LoadTasks;
		}
	}
}