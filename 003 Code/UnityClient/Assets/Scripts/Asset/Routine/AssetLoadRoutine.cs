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
                Managers.Gltf.CreateObject(loadTask); // 오브젝트 생성

                // 동일한 에셋인 오브젝트가 존재하지 않을 경우
                if (!loadTask.isExist)
                {
                    // Debug.Log("Loading...	" + loadTask.astId);
                    //yield return StartCoroutine(LoadGltf(loadTask, glb_data[loadTask.astId])); // 에셋 로드
                    yield return StartCoroutine(LoadGltf(loadTask, glb_data[loadTask.astId]));

                    if (loadTask.isFailOrStop) // 로드 실패 혹은 취소한 경우
                    {
                        Debug.LogError("Asset Load Fail or Cancel	: " + loadTask.astId);
                    }
                }
                else
                {
                    loadTask.isLoadSuccess = true; // 로드 과정 스킵
                }
            }

            // 리로드를 요청하지 않았을 경우
            if (!GetReLoad())
            {
                if (loadTask.isLoadSuccess) // 로드 성공한 경우
                {
                    Managers.Gltf.GetRoutine(2).TaskInsert(loadTask); // Wear Routine에 해당 작업 부여

                    Debug.Log("Asset Load Success && Push WearTasks	: ast( " + loadTask.astId + " )");
                }
            }
        }

        protected override void SetTasks()
        {
            Tasks = LoadTasks;
        }
    }
}