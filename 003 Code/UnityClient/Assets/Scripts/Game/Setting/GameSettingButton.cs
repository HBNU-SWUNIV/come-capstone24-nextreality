using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game
{
	public class GameSettingButton : MonoBehaviour
	{
		public Button button;

		public TMP_Text text;

		public Image buttonGraphic;
		// Start is called before the first frame update

		public void AddListener(Action action)
		{
			button.onClick.AddListener(() => { action(); });
		}

		public void SetSettingButton()
		{

		}
	}

}
