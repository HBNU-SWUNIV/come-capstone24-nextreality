using NextReality.Asset;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
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

		public byte[] RequestFile(string filename, int mode)
		{
			TcpClient client = new TcpClient();
			client.Connect(serverIP, port);
			NetworkStream stream = client.GetStream();

			// 파일 요청 생성 및 전송
			FileRequest request = new FileRequest { assetId = filename, mode = mode };
			string jsonRequest = JsonUtility.ToJson(request);
			Debug.Log(jsonRequest);
			byte[] requestData = Encoding.UTF8.GetBytes(jsonRequest);
			byte[] lengthPrefix = BitConverter.GetBytes(requestData.Length);

			stream.Write(lengthPrefix, 0, lengthPrefix.Length);
			stream.Write(requestData, 0, requestData.Length);

			// 파일 수신
			byte[] fileData = ReceiveFile(stream, filename, mode);
			client.Close();
			return fileData;
		}

		private byte[] ReceiveFile(NetworkStream stream, string filename, int mode)
		{
			byte[] fileSizePrefix = new byte[4];
			stream.Read(fileSizePrefix, 0, fileSizePrefix.Length);
			int fileSize = BitConverter.ToInt32(fileSizePrefix, 0);

			byte[] fileData = new byte[fileSize];
			int bytesRead = 0;
			while (bytesRead < fileSize)
			{
				bytesRead += stream.Read(fileData, bytesRead, fileSize - bytesRead);
			}

			Debug.Log("File received and saved to " + filename);
			return fileData;
		}
	}
}