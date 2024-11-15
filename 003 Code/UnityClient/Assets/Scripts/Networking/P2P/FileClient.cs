using NextReality.Asset;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Networking.P2P
{
	public class FileClient : MonoBehaviour
	{
		private static FileClient instance = null;
		public static FileClient Instance { get { return instance; } }

		public string serverIP = "";
		public int port;

		private void Awake()
		{
			if (FileClient.instance == null)
			{
				FileClient.instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
				Debug.Log("Destroy FileClient Object");
			}
		}

		public IEnumerator RequestFile(string filename, int mode, Action<byte[]> onComplete)
		{
			TcpClient client = null;
			NetworkStream stream = null;
			Debug.Log("serverIP: " + serverIP + " serverPort: " + port);
			client = new TcpClient();
			client.Connect(serverIP, port);
			stream = client.GetStream();

			// 파일 요청 생성 및 전송
			FileRequest request = new FileRequest { assetId = filename, mode = mode };
			string jsonRequest = JsonUtility.ToJson(request);
			byte[] requestData = Encoding.UTF8.GetBytes(jsonRequest);
			byte[] lengthPrefix = BitConverter.GetBytes(requestData.Length);

			stream.Write(lengthPrefix, 0, lengthPrefix.Length);
			stream.Write(requestData, 0, requestData.Length);

			// 파일 수신 및 저장
			yield return StartCoroutine(ReceiveFile(stream, onComplete));
			client.Close();
		}

		private IEnumerator ReceiveFile(NetworkStream stream, Action<byte[]> onComplete)
		{
			byte[] fileData = null;

			// 파일 크기 정보 읽기
			byte[] fileSizePrefix = new byte[4];
			stream.Read(fileSizePrefix, 0, fileSizePrefix.Length);
			int fileSize = BitConverter.ToInt32(fileSizePrefix, 0);

			fileData = new byte[fileSize];
			int totalBytesRead = 0;
			byte[] buffer = new byte[fileSize];

			while (totalBytesRead < fileSize)
			{
				// 현재 읽어야 할 크기 계산
				int bytesToRead = Math.Min(buffer.Length, fileSize - totalBytesRead);
				int bytesRead = stream.Read(buffer, 0, bytesToRead);
				if (bytesRead == 0) break;

				Array.Copy(buffer, 0, fileData, totalBytesRead, bytesRead);
				totalBytesRead += bytesRead;

				yield return null;
			}

			Debug.Log("File received successfully into memory.");

			// 결과 반환
			onComplete?.Invoke(fileData);
		}
	}
}