using NextReality;
using NextReality.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NextReality.Game.UI
{
	public class LoadingPage : MonoBehaviour
	{
		public TMP_Text loadingText;

		public float loadingDotTiming = 0.1f;
		public int maxDotCount = 3;

		public float delayTime = 1f;

		string initText = "";

		Action endLoading;

		private static LoadingPage instance;
		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Destroy(this);
			}

			initText = loadingText.text;
		}

		public static LoadingPage Instance
		{
			get { return instance; }
		}

		private void OnDestroy()
		{
			if (instance == this) instance = null;
		}

		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(StartLoadingPage());
		}

		public void EndLoading()
		{
			StartCoroutine(StartEndLoading());

		}

		IEnumerator StartEndLoading()
		{
			yield return new WaitForSeconds(delayTime);
			instance = null;
			Managers.Scene.UnLoadScene(SceneName.LoadingScene);
			endLoading?.Invoke();
		}

		IEnumerator StartLoadingPage()
		{
			var wait = new WaitForSeconds(loadingDotTiming);
			int dotCount = 0;
			string dot = "";
			while (instance != null)
			{
				dot = "";
				for (int i = 0; i < dotCount; i++)
				{
					dot += ".";
				}
				loadingText.text = initText + dot;
				if (dotCount > maxDotCount) dotCount = 0;
				else dotCount++;
				yield return wait;
			}
		}

		public void AddEndLoadingAction(Action action)
		{
			if (endLoading == null) endLoading = action;
			else endLoading += action;
		}
	}

}

