using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NextReality.Game
{
	public class NickNameText : MonoBehaviour
	{
		TextMeshPro nicknameText;
		// Start is called before the first frame update
		void Awake()
		{
			if(nicknameText == null) nicknameText = GetComponent<TextMeshPro>();
		}

		// Update is called once per frame
		void LateUpdate()
		{
			if(Managers.Camera.mainGameCamera)
			{
				transform.eulerAngles = Managers.Camera.mainGameCamera.transform.eulerAngles;
			}
		}

		public void SetNickName(string nickname)
		{
			Awake();
			nicknameText.text = nickname;
		}
	}

}
