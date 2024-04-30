﻿using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace NextReality.Data
{
	public class testListener : MonoBehaviour
	{
		BroadcastHandler broHandler;

		// Use this for initialization
		void Start()
		{
			if (broHandler == null)
			{
				broHandler = gameObject.GetComponent<BroadcastHandler>();
			}

			StartCoroutine(LogReplay());
		}


		public IEnumerator LogReplay()
		{
			for (int j = 0; j < 1; j++)
			{
				// jointest = player join command test file
                byte[] bytes = File.ReadAllBytes("Assets/Resources/EventListener/jointest.txt");

                // byte[] bytes = File.ReadAllBytes("Assets/Resources/EventListener/test.txt");

                string logData = Encoding.UTF8.GetString(bytes);
				string[] logList = logData.Split("\r\n"); // Linux 서버라면 \r\n 을 \n으로 변경해야 함

				for (int i = 0; i < logList.Length; i++)
				{
					yield return new WaitForSeconds(0.5f);
					string[] log = logList[i].Split("$");
					if (log.Length >= 2)
					{
						broHandler.InvokeEvent(log[0], log[1]);
					}
				}
			}
		}
	}
}