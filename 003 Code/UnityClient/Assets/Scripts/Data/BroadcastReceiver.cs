using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace NextReality.Data
{
    public class BroadcastReceiver : MonoBehaviour
    {
        private BroadcastHandler broadcastHandler;
        private const int listenPort = 10000;
        private UdpClient listener;


        // Use this for initialization
        void Start()
        {
            if (broadcastHandler == null)
            {
                broadcastHandler = gameObject.GetComponent<BroadcastHandler>();
            }

            listener = new UdpClient(listenPort);
            listener.BeginReceive(ReceiveCallback, null);
        }

        // 게임 서버로부터 데이터를 받으면 이벤트 실행하는 메서드
        private void ReceiveCallback(IAsyncResult result)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
            byte[] receivedBytes = listener.EndReceive(result, ref endPoint);
            string receivedData = Encoding.ASCII.GetString(receivedBytes);


            string command = "test";
            string data = "test data";
            // 받은 데이터를 이벤트로 전달
            broadcastHandler.InvokeEvent(command, data);

            // 다음 데이터 수신 대기
            listener.BeginReceive(ReceiveCallback, null);
        }

        private void OnDestroy()
        {
            listener.Close();
        }
    }
}