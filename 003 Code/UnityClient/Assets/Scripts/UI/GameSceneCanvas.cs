using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

using NextReality.Game.UI;

namespace NextReality.Game
{
	public class GameSceneCanvas : MonoBehaviour
	{

		public UploadPopup uploadPopup;
		public TcpClient socket;

		private string ip = "192.168.50.140";
		private int port = 8000;

		private bool popup = false;

		// Start is called before the first frame update
		void Start()
		{
			Thread connectThread = new Thread(new ThreadStart(ConnectToTcpServer));
			connectThread.IsBackground = false;
			connectThread.Start();
		}

		// Update is called once per frame
		void Update()
		{

			if (Input.GetKeyDown(KeyCode.U))
			{
				this.uploadPopup.closeBtn.onClick.AddListener(() =>
				{
					this.uploadPopup.Close();
				});
				this.uploadPopup.Open();
			}

		}

		public void ConnectToTcpServer()
		{

			try
			{
				socket = new TcpClient(ip, port);
				Debug.Log("Connected!");

			}
			catch (Exception e)
			{
				Debug.Log("Error : " + e);
			}
		}
	}

}
