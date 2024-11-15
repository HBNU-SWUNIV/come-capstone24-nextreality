using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.IO;
using NextReality.Asset;
using NextReality;
using Unity.VisualScripting.FullSerializer;

namespace Assets.Scripts.Networking.P2P
{
	public class FileServer : MonoBehaviour
	{
		private static FileServer instance = null;
		public static FileServer Instance { get { return instance; } }

		private TcpListener listener;
		public IPAddress localAddress;
		public int port;
		protected static string localDirectory = "testDatas";

		private void Awake()
		{
			if (FileServer.instance == null)
			{
				FileServer.instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
				Debug.Log("Destroy FileServer Object");
			}
		}

		// Use this for initialization
		void Start()
		{
			Managers.Network.SetP2PServer();
			listener = new TcpListener(localAddress, port);
			listener.Start();
			Debug.Log("Server Start: " + localAddress +" " + port);

			listener.BeginAcceptTcpClient(HandleClientConnected, null);
		}

		private void HandleClientConnected(IAsyncResult result)
		{
			TcpClient client = listener.EndAcceptTcpClient(result);
			Debug.Log("Client connected.");

			// Start receiving the client's request
			NetworkStream stream = client.GetStream();
			byte[] lengthPrefix = new byte[4];
			stream.Read(lengthPrefix, 0, lengthPrefix.Length);
			int requestDataLength = BitConverter.ToInt32(lengthPrefix, 0);

			byte[] requestData = new byte[requestDataLength];
			stream.Read(requestData, 0, requestData.Length);

			string jsonRequest = Encoding.UTF8.GetString(requestData);
			Debug.Log(jsonRequest);
			FileRequest request = JsonUtility.FromJson<FileRequest>(jsonRequest);
			Debug.Log($"Requested file: {request.assetId}");

			// Send the requested file
			SendFileToClient(stream, request.assetId, request.mode);
			client.Close();

			// Continue accepting clients
			listener.BeginAcceptTcpClient(HandleClientConnected, listener);
		}

		private void SendFileToClient(NetworkStream stream, string filename, int mode)
		{
			if (GltfManager.GetGlbData().ContainsKey(filename))
			{
				byte[] fileData = GltfManager.GetGlbData()[filename];
				if (mode == 0)
				{
					fileData = fileData[..(fileData.Length / 2)];
				}
				byte[] fileSizePrefix = BitConverter.GetBytes(fileData.Length);

				stream.Write(fileSizePrefix, 0, fileSizePrefix.Length);
				stream.Write(fileData, 0, fileData.Length);
				Debug.Log("File sent successfully.");
			}
			else
			{
				Debug.Log("File not found.");
			}
		}

		private void OnApplicationQuit()
		{
			listener.Stop();
			Debug.Log("Server stopped.");
		}
	}
}