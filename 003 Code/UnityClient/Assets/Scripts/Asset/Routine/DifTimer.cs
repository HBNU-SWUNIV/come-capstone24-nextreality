using NextReality.Asset.UI;
using NextReality.Asset;
using System;
using System.Collections;
using UnityEngine;

namespace NextReality.Asset.Routine
{
	public class DifTimer : MonoBehaviour
	{
		private DateTime startTime;
		private DateTime endTime;

		private static DifTimer instance = null;
		public static DifTimer Instance { get { return instance; } }

		private void Awake()
		{
			if (DifTimer.instance == null)
			{
				DifTimer.instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(DifTimer.instance.gameObject);
				Debug.Log("Destroy DifTimer Object");
			}
		}

		private void OnDestroy()
		{
			if (DifTimer.instance == this)
			{
				DifTimer.instance = null;
			}
		}

		public void SetStartTime()
		{
			startTime = DateTime.Now;
		}

		public void SetEndTime()
		{
			endTime = DateTime.Now;
		}

		public void GetTimeDif()
		{
			TimeSpan duration = endTime - startTime;
			Debug.Log("작업에 걸린 시간: " + duration.TotalSeconds.ToString("F3") + "초");
		}
	}
}