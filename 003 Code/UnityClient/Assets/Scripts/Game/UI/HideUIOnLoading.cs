using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game.UI
{
	public class HideUIOnLoading : MonoBehaviour
	{
		public Canvas canvas;
		private CanvasGroup canvasGroup;
		private GraphicRaycaster raycaster;

		public float showNextTime = 2f;

		// Start is called before the first frame update
		void Start()
		{
			if(LoadingPage.Instance != null)
			{
				if (canvas == null) canvas = GetComponent<Canvas>();

				if (canvas != null)
				{
					if (canvas.TryGetComponent(out canvasGroup))
					{

					}
					else
					{
						canvasGroup = canvas.AddComponent<CanvasGroup>();
					}
					canvasGroup.alpha = 0f;

					if (canvas.TryGetComponent(out raycaster))
					{

					}
					else
					{
						raycaster = canvas.AddComponent<GraphicRaycaster>();
					}
					raycaster.enabled = true;
				}

				LoadingPage.Instance.AddEndLoadingAction(() =>
				{
					StartCoroutine(StartShowUI());
				});
			}

		}

		IEnumerator StartShowUI()
		{
			float curTime = 0;
			raycaster.enabled = true;
			while (gameObject.activeInHierarchy)
			{
				if (curTime > showNextTime)
				{
					yield break;
				}
				canvasGroup.alpha = curTime / showNextTime;
				curTime += Time.deltaTime;
				yield return null;
			}
		}
	}

}
