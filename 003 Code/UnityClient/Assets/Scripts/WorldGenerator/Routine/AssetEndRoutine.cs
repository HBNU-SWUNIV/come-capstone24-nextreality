using System.Collections;
using System.Collections.Generic;

namespace NextReality.Asset.Routine
{
	public class AssetEndRoutine : GltfRoutine
	{
		protected Queue<LoadTask> EndTasks = new Queue<LoadTask>();
	
		// 작업을 처리하는 메서드
		protected override IEnumerator ProceedTask(LoadTask endTask)
		{
			// 작업이 모두 완료되었을 경우
			if (endTask.isAllSuccess)
			{
				// EndTasks.Dequeue(); // End Routine에서 해당 작업 삭제
			}
			else
			{
				endTask.TaskInit(); // 작업 초기화
                GltfRoutineManager.Instance.Get_Routine(0).TaskInsert(endTask); // 작업 재시도
			}
			yield return null;
		}

		protected override void SetTasks()
		{
			Tasks = EndTasks;
		}
	}
}