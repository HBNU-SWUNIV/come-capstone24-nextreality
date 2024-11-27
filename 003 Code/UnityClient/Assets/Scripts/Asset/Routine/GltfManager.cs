using Assets.Scripts.Networking.P2P;
using NextReality.Asset.Routine;
using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		protected static string gltfPart;
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

		protected IEnumerator DownGltf(LoadTask downTask)
		{
			string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");
			if (!glb_data.ContainsKey(downTask.astId))
			{
				if (!Managers.Map.isMapReady && FileClient.Instance.serverIP != "")
				{
					Debug.Log("Start AssetDown at P2P");
					bool P2P_success = false;
					yield return StartCoroutine(DownGltfAtPlayer(downTask, (result) =>
					{
						P2P_success = result;
					}));

					if (!P2P_success)
					{
						yield return StartCoroutine(DownGltfAtServer(downTask));
					}
				}
				else
				{
					Debug.Log("Start AssetDown at Server");
					yield return StartCoroutine(DownGltfAtServer(downTask));
				}
			}
			else
			{
				downTask.isDownSuccess = true;
			}
		}

		// 에셋 다운로드 메서드
		protected IEnumerator DownGltfAtServer(LoadTask downTask)
		{
			string serverUrl = gltfServer;
			string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");

			if (File.Exists(fullFilePath))
				serverUrl = gltfPart;
			else
				serverUrl = gltfServer;

			int downFailCount = 0;
			while (!downTask.isDownSuccess && !GltfRoutine.GetReLoad()) // 다운로드를 완료하지 않은 경우 반복
			{
				string command = string.Format("?id={0}", downTask.astId);
				Debug.Log("AssetData Down from Server	: " + serverUrl + command);

				// 에셋 다운
				yield return StartCoroutine(Utilities.HttpUtil.RequestGet(serverUrl + command, (result) =>
				{
					AstResponseData response = JsonUtility.FromJson<AstResponseData>(result);
					if (response.CheckResult())
					{
						downFailCount = 0;
						if (response.data.Count != 0 && response.data[0].id.Equals(downTask.astId))
						{
							string data = response.data[0].file;
							int len_data = data.Length;
							int padding = len_data % 4;
							Debug.Log("padding" +  padding);
							if (padding > 0)
							{
								data = data[..(len_data-2)];
							}
							byte[] astData = Convert.FromBase64String(data);
							if (!File.Exists(fullFilePath))
							{
								int ast_offset = astData.Length / 2;
								while (ast_offset % 3 != 0)
								{
									ast_offset -= 1;
								}
								SaveGltf(downTask.astId, astData);
								SaveGltfLocal(downTask.astId, astData[ast_offset..]);
							}
							else
							{
								SaveGltf(downTask.astId, astData.Concat(File.ReadAllBytes(fullFilePath)).ToArray());
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

		protected IEnumerator DownGltfAtPlayer(LoadTask downTask, Action<bool> onComplete)
		{
			string fullFilePath = Path.Combine(Application.persistentDataPath, localDirectory, downTask.astId.ToString() + ".glb");
			int mode = 0;
			if (File.Exists(fullFilePath))
				mode = 0;
			else
				mode = 1;

			byte[] astData = null;
			yield return FileClient.Instance.RequestFile(downTask.astId, mode, (result) =>
			{
				if (result != null)
				{
					astData = result;
					downTask.isDownSuccess = true;
				}
				else
				{
					onComplete?.Invoke(false);
				}
			});

			if (mode == 0)
			{
				SaveGltf(downTask.astId, astData.Concat(File.ReadAllBytes(fullFilePath)).ToArray());
			}
			else if (mode == 1)
			{
                int ast_offset = astData.Length / 2;
                while (ast_offset % 3 != 0)
                {
                    ast_offset -= 1;
                }
                SaveGltf(downTask.astId, astData);
                SaveGltfLocal(downTask.astId, astData[ast_offset..]);
            }

			onComplete?.Invoke(true);
		}

		// 에셋 로드 메서드
		protected IEnumerator LoadGltf(LoadTask loadTask, byte[] byteData)
		{
			int loadFailCount = 0;
			while (!loadTask.isLoadSuccess && !GltfRoutine.GetReLoad()) // 에셋 로드를 완료하지 않은 경우 반복
			{
				if (byteData != null) // byte 배열이 null 값이 아닌 경우
				{
					Task<bool> loadWork = loadTask.gltfImport.LoadGltfBinary(byteData); // 에셋 로드
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

		public static Dictionary<string, byte[]> GetGlbData()
		{
			return glb_data;
		}
	}
}