using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                // 리로드를 요청하지 않았을 경우
                if (!GetReLoad())
                {
                    endTask.gltfObj.AddComponents(); // 컴포넌트 추가
                    endTask.gltfObj.gameObject.SetActive(true);

                    AssetObject astObj = endTask.gltfObj;

                    // 동일한 오브젝트 아이디가 존재할 경우
                    if (Managers.Gltf.objInstances.ContainsKey(astObj.object_id))
                    {
                        Destroy(astObj.gameObject); // 오브젝트 삭제
                                                    // Debug.LogError("Exist object Id");
                    }
                    else
                    {
                        Managers.Gltf.objInstances.Add(astObj.object_id, astObj); // 딕셔너리에 오브젝트 추가
                        Debug.Log("Asset Create Success				: ast( " + endTask.astId + " )");
                    }

                    // 모든 작업의 종료되었을 경우
                    if (Managers.Gltf.CheckEndTasks())
                    {
                        Managers.Gltf.ActiveRigidbody(); // 모든 Rigidbody 활성화

                        if (Managers.Map.GetLoadStart())
                        {
                            Debug.Log("Send MapReady");
                            Managers.Map.SendMapReady();
                            Managers.Map.ConvertLoadStart();

                            //DifTimer.Instance.SetEndTime(); // 캐싱 test 종료
                            //DifTimer.Instance.GetTimeDif(); // 캐싱 작업 시간 출력
                        }
                    }
                }
            }
            else
            {
                endTask.TaskInit(); // 작업 초기화
                Managers.Gltf.GetRoutine(0).TaskInsert(endTask); // 작업 재시도
            }
            yield return null;
        }


        protected override void SetTasks()
        {
            Tasks = EndTasks;
        }
    }
}