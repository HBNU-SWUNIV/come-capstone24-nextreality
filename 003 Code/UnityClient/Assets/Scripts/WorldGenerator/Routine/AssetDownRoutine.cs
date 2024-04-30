using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace NextReality.Asset.Routine
{ 
	public class AssetDownRoutine : GltfRoutine
	{
		private Queue<LoadTask> DownTasks = new Queue<LoadTask>();

		// 작업을 처리하는 메서드
		protected override IEnumerator ProceedTask(LoadTask downTask)
		{
			// Task가 다운로드 과정을 거치지 않은 경우
			if (!downTask.isDownSuccess)
			{
				Debug.Log("DownLoad...	" + downTask.astId);
				yield return StartCoroutine(DownGltf(downTask)); // 에셋 다운로드

				// 다운로드 실패 혹은 취소한 경우
				if (downTask.isFailOrStop)
				{
					Debug.LogError("Asset Download Fail or Cancel");
				}
			}

			if (!GetReLoad()) // 리로드를 요청하지 않았을 경우
			{
				// 다운로드 성공한 경우
				if (downTask.isDownSuccess)
				{
                    GltfRoutineManager.Instance.Get_Routine(1).TaskInsert(downTask); // Load Routine에 해당 작업 부여

					Debug.Log("Asset Down Success && Push LoadTasks	:" + downTask.astId);
				}
			}
			else // 리로드를 요청한 경우
			{
				DownTasks.Clear(); // Down Routine의 작업 초기화
			}
		}

		protected override void SetTasks()
		{
			Tasks = DownTasks;
		}
	}
}