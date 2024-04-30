using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace NextReality.Networking
{
	public class TcpTest : MonoBehaviour
	{
		private TcpClient socket;
		private string host = "192.168.50.88";
		private int port = 8000;

		// TCP ���� �κ��� ������� �ؼ� ���� �����尡 ������ �ʵ��� ��
		void Start()
		{
			Thread connectThread = new Thread(new ThreadStart(ConnectToTcpServer));
			connectThread.IsBackground = true;
			connectThread.Start();
		}

		// = Ű ������ test test ���ڿ��� ������ ���� (�̰� ���Ŀ� ���� ����)
		void Update()
		{
			if (socket == null) { return; }
			if (Input.GetKeyDown(KeyCode.Equals))
			{
				Thread sendThread = new Thread(() => sendMessage("test test"));
				sendThread.Start();
			}

		}

		// ������ �� ������ ȭ �ؼ� �ҷ����� TCP ���� ���� �Լ�
		private void ConnectToTcpServer()
		{
			Debug.Log("Try to Connect...");
			try
			{
				socket = new TcpClient(host, port);
				Debug.Log("Connected!");
				recvMessage(); // �޽��� ���� �κ��� �ɾ����

			}
			catch (Exception e)
			{
				Debug.Log("On client connect exception : " + e);
			}
		}

		private void sendMessage(string message)
		{
			if (socket == null)
			{
				return;
			}
			try
			{
				NetworkStream stream = socket.GetStream();
				if (stream.CanWrite)
				{
					// ���ڿ� -> ����Ʈ�� ����
					byte[] buffer = Encoding.UTF8.GetBytes(message);
					stream.Write(buffer, 0, buffer.Length);

					Debug.Log("[Client] : " + message);
				}
			}
			catch (SocketException e)
			{
				Debug.Log("send message exception : " + e);
			}
		}

		private void recvMessage()
		{
			while (true)
			{
				try
				{
					NetworkStream stream = socket.GetStream();
					if (stream.CanRead)
					{
						byte[] buffer = new byte[4098];
						int bytes = stream.Read(buffer, 0, buffer.Length);
						if (bytes <= 0) { continue; }
						string message = Encoding.UTF8.GetString(buffer, 0, bytes);
						if (message != null)
						{
							Debug.Log("[Server] : " + message);
						}
					}
				}
				catch (Exception e)
				{
					Debug.Log("recv message exception : " + e);
				}
			}
		}
	}

}

