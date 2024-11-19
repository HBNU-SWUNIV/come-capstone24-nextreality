using Assets.Scripts.Networking.P2P;
using NextReality.Data;
using NextReality.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace NextReality.Networking
{
	public class NetworkManager : MonoBehaviour
	{
		private UdpClient udpClient;
		private IPEndPoint serverEndPoint;
		private Thread receiveThread;

		private static NetworkManager instance = null;

		// ���� IP�� ��Ʈ ����
		public string serverIP = "172.25.17.17";
		public int serverPort = 8080;
		public int random_P2P_Port = 0;

		Action endUDPEvnets;

		private void Awake()
		{
			if (instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
				return;
			}
			else
			{
				//Debug.Log("Network Manager Instantiate");
				// ���� ����
				serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

				// ��Ʈ �ڵ����� ����
				random_P2P_Port = UnityEngine.Random.Range(9000, 9999);
				udpClient = new UdpClient(random_P2P_Port);
				udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				udpClient.EnableBroadcast = true;
				//udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, serverPort));
			}

			instance = this;
		}

		public static NetworkManager Instance
		{
			get
			{
				if (null == instance)
				{
					return null;
				}
				return instance;
			}
		}

		void Start()
		{
			serverIP = Managers.Conf.GetConfigData("serverIP");
			serverPort = int.Parse(Managers.Conf.GetConfigData("udpServerPort"));
			serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

			// ���� ������ ����
			receiveThread = new Thread(new ThreadStart(ReceiveData));
			receiveThread.IsBackground = true;

			receiveThread.Start();
			Thread.Sleep(10);

			// SendMessage("PlayerJoin$testID;638506752265295630;testNickname;125");
		}

		public new void SendMessage(string message)
		{
			byte[] data = Encoding.UTF8.GetBytes(message);
			udpClient.Send(data, data.Length, serverEndPoint);
		}

		void ReceiveData()
		{
			while (receiveThread.IsAlive)
			{
				try
				{
					if (udpClient == null || udpClient.Client == null || udpClient.Client.Connected == false)
					{
						// Debug.Log("udpClient Closed");
						continue;
					}

					byte[] data = udpClient.Receive(ref serverEndPoint);
					string message = Encoding.UTF8.GetString(data);
					// Debug.Log("Received:    " + message);

					// ������ �޽����� ���� �����忡�� ó���� �� �ֵ��� ����
					MainThreadDispatcher.Instance().Enqueue(() => HandleReceivedMessage(message));
				}
				catch (Exception e)
				{
					Debug.LogError("Exception: " + e.ToString());
					//receiveThread.Abort();
				}
			}
		}

		void HandleReceivedMessage(string message)
		{
			// ������ �޽��� ó��
			// Debug.Log("Handle on main thread: " + message);
			Managers.BroadcastHandler.InvokeEvent(message);
		}

		private void OnDestroy()
		{
			endUDPEvnets?.Invoke();

			udpClient?.Close();

			if (receiveThread != null && receiveThread.IsAlive)
			{
				receiveThread.Abort();
			}
		}

		public void AddEndUDPEvnets(Action action)
		{
			if (endUDPEvnets == null) endUDPEvnets = action;
			else endUDPEvnets += action;

		}

		public string SetP2PServer()
		{
			string targetIP_Port = "";
			foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4�� ��ȯ
				{
					FileServer.Instance.localAddress = ip;
					targetIP_Port += ip;
					break;
				}
			}
			IPEndPoint localEndPoint = (IPEndPoint)udpClient.Client.LocalEndPoint;
			FileServer.Instance.port = localEndPoint.Port;
			targetIP_Port += ":" + localEndPoint.Port;
			return targetIP_Port;
		}
	}
}

