using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System.IO;
using System;
using System.Linq;
using NextReality.Asset.UI;


namespace NextReality.Asset.Routine
{
    public class AssetDownTest : MonoBehaviour
    {
        private Queue<byte[]> AssetData = new Queue<byte[]>();
        public Dictionary<string, byte[]> glb_data;

        protected static string gltfServer; // gtlf 서버 주소
        protected static string localDirectory = "testDatas";
        protected static string objectDir;
        public int mode = 5;

        protected static HttpRequests httpRequests;

        private static AssetDownTest instance = null;
        public static AssetDownTest Instance { get { return instance; } }

        private void OnDestroy()
        {
            if (AssetDownTest.instance == this)
            {
                AssetDownTest.instance = null;
            }
        }

        private void Awake()
        {
            if (AssetDownTest.instance == null)
            {
                AssetDownTest.instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(AssetDownTest.instance.gameObject);
                Debug.Log("Destroy AssetDownTest Object");
            }

            httpRequests = Utilities.HttpUtil;
            glb_data = new Dictionary<string, byte[]>();
            gltfServer = httpRequests.GetServerUrl(HttpRequests.ServerEndpoints.AssetDownload);

            objectDir = Path.Combine(Application.persistentDataPath, localDirectory);
            if (!Directory.Exists(objectDir)) // 디렉토리가 존재 여부 확인
            {
                Directory.CreateDirectory(objectDir);
                Debug.Log("Create Directory");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public IEnumerator AssetSecurity(int mode, LoadTask downTask)
        {
            string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");

            if (mode == 0) // 다운
            {
                yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
            }
            else if (mode == 1) // 메모리
            {
                if (!glb_data.ContainsKey(downTask.astId)) // 에셋 파일이 딕셔너리에 존재하지 않는 경우
                {
                    yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
                }
                else
                {
                    downTask.isDownSuccess = true;
                }
            }
            else if (mode == 2) // 캐시
            {
                if (File.Exists(fullFilePath)) // 에셋 파일이 로컬에 존재하는 경우
                {
                    AssetData.Enqueue(File.ReadAllBytes(fullFilePath));
                    downTask.isDownSuccess = true;
                }
                else
                {
                    yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
                }
            }
            else if (mode == 3) // 캐시 + 메모리
            {
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
                    yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
                }
            }
            else if (mode == 4) // 반 캐시 + 반 다운
            {
                yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
            }
            else if (mode == 5) // 반 캐시 + 반 메모리
            {
                if (!glb_data.ContainsKey(downTask.astId)) // 에셋 파일이 딕셔너리에 존재하지 않는 경우
                {
                    if (File.Exists(fullFilePath))
                    {
                        yield return StartCoroutine(AssetDown(downTask, mode, httpRequests.GetServerUrl(HttpRequests.ServerEndpoints.AssetDownPart)));
                    }
                    else
                    {
                        yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
                    }
                }
                else
                {
                    downTask.isDownSuccess = true;
                }
            }
            else if (mode == 6) // 반 캐시 + 반 캐시
            {
                fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString());
                if (File.Exists(fullFilePath + "_1" + ".glb") && File.Exists(fullFilePath + "_2" + ".glb")) // 에셋 파일이 로컬에 존재하는 경우
                {
                    downTask.isDownSuccess = true;
                }
                else
                {
                    yield return StartCoroutine(AssetDown(downTask, mode, gltfServer));
                }
            }
        }

        protected IEnumerator AssetDown(LoadTask downTask, int mode, string gltfServer)
        {
            string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");

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
                            if (mode == 0) // 다운
                            {
                                AssetData.Enqueue(astData);
                            }
                            else if (mode == 1) // 메모리
                            {
                                SaveGltf(downTask.astId, astData);
                            }
                            else if (mode == 2) // 캐시
                            {
                                SaveGltfLocal(downTask.astId, astData);
                            }
                            else if (mode == 3) // 캐시 + 메모리
                            {
                                SaveGltf(downTask.astId, astData);
                                SaveGltfLocal(downTask.astId, astData);
                            }
                            else if (mode == 4) // 반 캐시 + 반 다운
                            {
                                AssetData.Enqueue(astData[..(astData.Length / 2)]);
                                if (!File.Exists(fullFilePath))
                                {
                                    SaveGltfLocal(downTask.astId, astData[(astData.Length / 2)..]);
                                }
                            }
                            else if (mode == 5) // 반 캐시 + 반 메모리
                            {
                                if (!File.Exists(fullFilePath))
                                {
                                    SaveGltf(downTask.astId, astData[..(astData.Length / 2)]);
                                    SaveGltfLocal(downTask.astId, astData[(astData.Length / 2)..]);
                                }
                                else
                                {
                                    SaveGltf(downTask.astId, astData);
                                }
                            }
                            else if (mode == 6) // 반 캐시 + 반 캐시
                            {
                                SaveGltfLocal(downTask.astId + "_1", astData[..(astData.Length / 2)]);
                                SaveGltfLocal(downTask.astId + "_2", astData[(astData.Length / 2)..]);
                            }
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

        public byte[] GetAsset(int mode, LoadTask downTask)
        {
            string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");

            if (mode == 0) // 다운
            {
                return AssetData.Dequeue();
            }
            else if (mode == 1) // 메모리
            {
                return glb_data[downTask.astId];
            }
            else if (mode == 2) // 캐시
            {
                return File.ReadAllBytes(fullFilePath);
            }
            else if (mode == 3) // 캐시 + 메모리
            {
                return glb_data[downTask.astId];
            }
            else if (mode == 4) // 반 캐시 + 반 다운
            {
                return AssetData.Dequeue().Concat(File.ReadAllBytes(fullFilePath)).ToArray();
            }
            else if (mode == 5) // 반 캐시 + 반 메모리
            {
                return glb_data[downTask.astId].Concat(File.ReadAllBytes(fullFilePath)).ToArray();
            }
            else if (mode == 6) // 반 캐시 + 반 캐시
            {
                fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString());
                return File.ReadAllBytes(fullFilePath + "_1" + ".glb").Concat(File.ReadAllBytes(fullFilePath + "_2" + ".glb")).ToArray();
            }

            return null;
        }

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