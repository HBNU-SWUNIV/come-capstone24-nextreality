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

		// TCP 연결 부분은 스레드로 해서 메인 스레드가 멈추지 않도록 함
		void Start()
		{
			Thread connectThread = new Thread(new ThreadStart(ConnectToTcpServer));
			connectThread.IsBackground = true;
			connectThread.Start();
		}

		// = 키 누르면 test test 문자열이 가도록 했음 (이건 추후에 수정 가능)
		void Update()
		{
			if (socket == null) { return; }
			if (Input.GetKeyDown(KeyCode.Equals))
			{
				Thread sendThread = new Thread(() => sendMessage("test test"));
				sendThread.Start();
			}

		}

		// 시작할 때 스레드 화 해서 불러지는 TCP 서버 연결 함수
		private void ConnectToTcpServer()
		{
			Debug.Log("Try to Connect...");
			try
			{
				socket = new TcpClient(host, port);
				Debug.Log("Connected!");
				recvMessage(); // 메시지 수신 부분을 심어놨음

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
					// 문자열 -> 바이트로 변경
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

