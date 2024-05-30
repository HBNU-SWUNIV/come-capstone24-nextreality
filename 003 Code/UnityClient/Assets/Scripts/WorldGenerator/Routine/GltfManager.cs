using NextReality.Asset.Routine;
using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NextReality.Asset
{
    public class GltfManager : MonoBehaviour
    {
        protected static Dictionary<string, byte[]> glb_data;

        protected static string gltfServer; // gtlf 서버 주소
        protected static string localDirectory = "testDatas";
        protected static string objectDir;

        protected static HttpRequests httpRequests;

        private void Awake()
        {
            glb_data = new Dictionary<string, byte[]>();

            objectDir = Path.Combine(Application.persistentDataPath, localDirectory);
            if (!Directory.Exists(objectDir)) // 디렉토리가 존재 여부 확인
            {
                Directory.CreateDirectory(objectDir);
                Debug.Log("Create Directory");
            }
        }

        // 에셋 다운로드 메서드
        protected IEnumerator DownGltf(LoadTask downTask)
        {
            string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");

            if (File.Exists(fullFilePath)) // 에셋 파일이 로컬에 존재하는 경우
            {
                if (!glb_data.ContainsKey(downTask.astId)) // 에셋 파일이 딕셔너리에 존재하지 않는 경우
                {
                    SaveGltf(downTask.astId, File.ReadAllBytes(fullFilePath)); // 에셋 파일을 딕셔너리에 저장
                }
                downTask.isDownSuccess = true;
            }
            else
            {
                int downFailCount = 0;
                while (!downTask.isDownSuccess && !GltfRoutine.GetReLoad()) // 다운로드를 완료하지 않은 경우 반복
                {
                    string command = string.Format("?id={0}", downTask.astId);
                    Debug.Log("AssetData Down from Server	: " + command);

                    // 에셋 다운
                    yield return StartCoroutine(Utilities.HttpUtil.RequestGet(gltfServer + command, (result) =>
                    {
                        AstResponseData response = JsonUtility.FromJson<AstResponseData>(result);
                        if (response.CheckResult())
                        {
                            downFailCount = 0;
                            if (response.data.Count != 0 && response.data[0].id.Equals(downTask.astId)) 
                            {
                                byte[] astData = Convert.FromBase64String(response.data[0].file);
                                SaveGltf(downTask.astId, astData);
                                SaveGltfLocal(downTask.astId, astData);
                                downTask.isDownSuccess = true;
                            }
                            else
                            {
                                downFailCount++;
                            }
                        }
                        else
                        {
                            downFailCount++;
                        }
                    }));

                    if (downFailCount == 5) // 다운로드 5번 실패한 경우
                    {
                        downTask.isFailOrStop = true;
                        yield break;
                    }
                }
            }
        }

        // 에셋 로드 메서드
        protected IEnumerator LoadGltf(LoadTask loadTask, byte[] byteData)
        {
            int loadFailCount = 0;
            while (!loadTask.isLoadSuccess && !GltfRoutine.GetReLoad()) // 에셋 로드를 완료하지 않은 경우 반복
            {
                if (byteData != null) // byte 배열이 null 값이 아닌 경우
                {
                    Task<bool> loadWork =  loadTask.gltfImport.LoadGltfBinary(byteData); // 에셋 로드
					yield return new WaitUntil(() => loadWork.IsCompleted);

                    if (loadWork.IsCompletedSuccessfully) // 에셋 로드 성공한 경우
                    {
                        loadFailCount = 0;
                        loadTask.isLoadSuccess = true;
                    }
                    else
                    {
                        loadFailCount++;

                        if (loadFailCount == 5) // 에셋 로드 5번 실패한 경우
                        {
                            loadTask.isFailOrStop = true;
                            yield break;
                        }
                    }
                }
                else
                {
                    loadTask.isFailOrStop = true;
                    yield break;
                }
            }
        }

        // 에셋 적용 메서드
        protected IEnumerator WearGltf(LoadTask wearTask)
        {
            int wearFailCount = 0;
            while (!wearTask.isWearSuccess && !GltfRoutine.GetReLoad()) // 에셋 적용을 완료하지 않은 경우 반복
            {
                if (wearTask.gltfObj != null) // 적용할 오브젝트가 null 값이 아닌 경우
                {
                    Task<bool> wearWork = wearTask.gltfImport.InstantiateMainSceneAsync(wearTask.gltfObj.transform); // 에셋 적용
					yield return new WaitUntil(() => wearWork.IsCompleted);

					if (wearWork.IsCompletedSuccessfully) // 에셋 적용 성공한 경우
                    {
                        wearFailCount = 0;
                        wearTask.isWearSuccess = true;
                    }
                    else
                    {
                        wearFailCount++;

                        if (wearFailCount == 5) // 에셋 적용 5번 실패한 경우
                        {
                            wearTask.isFailOrStop = true;
                            yield break;
                        }
                    }
                }
                else
                {
                    wearTask.isFailOrStop = true;
                    yield break;
                }
            }
        }

        // 에셋 데이터를 딕셔너리에 추가하는 메서드
        protected void SaveGltf(string filename, byte[] byteData)
        {
            glb_data.Add(filename, byteData);

            Debug.Log("Asset Save in Memory	: " + filename + ".glb");
        }

        // 에셋 데이터를 로컬에 저장하는 메서드
        protected void SaveGltfLocal(string filename, byte[] byteData)
        {
            filename = filename + ".glb";

            string filename_1 = Path.GetFileNameWithoutExtension(filename); // 파일 이름
            string filename_2 = Path.GetExtension(filename);                // 확장자

            string fullPath = Path.Combine(objectDir, filename);

            int fileCounter = 1;
            while (File.Exists(fullPath)) // 파일 이름 중복 처리
            {
                fullPath = Path.Combine(objectDir, filename_1 + " (" + fileCounter++ + ")" + filename_2);
            }

            FileStream fs = new FileStream(fullPath, FileMode.Create);
            fs.Write(byteData, 0, (int)byteData.Length); // 파일 저장
            fs.Close();

            Debug.Log("Asset Save in Local	: " + fullPath);
        }
    }
}