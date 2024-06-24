using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;
using System;

namespace NextReality.Utility.UI
{
	public class InputFieldKeyController : MonoBehaviour
	{
		public KeyCode[] nextKeys;

		private int curPos;

		KeyControlInputField[] keyControlSelectables;

		Coroutine keyCoroutine;
		// Start is called before the first frame update

		private void Awake()
		{
			foreach (InputFieldKeyController controller in GetComponentsInChildren<InputFieldKeyController>())
			{
				if (controller == this) return;
				Destroy(controller);
			}
		}
		void Start()
		{
			keyControlSelectables = GetComponentsInChildren<KeyControlInputField>();

			foreach (KeyControlInputField selectable in GetComponentsInChildren<KeyControlInputField>())
			{
				selectable.controller = this;
			}
		}

		public void SetFocus(int idx = 0)
		{
			if (idx >= 0 && idx < keyControlSelectables.Length)
				keyControlSelectables[idx].Select();
		}

		private void MoveNext()
		{
			GetCurerntPos();
			int tempPos = curPos;
			do
			{
				curPos = (curPos + 1) % keyControlSelectables.Length;
				if ( tempPos == curPos)
				{
					break;
				}
			} while (!keyControlSelectables[curPos].IsActive || !keyControlSelectables[curPos].IsInteractable);

			keyControlSelectables[curPos].Select();

			Debug.LogFormat("[InputField] InputField curPos {0}", curPos);
		}
		private int GetCurerntPos()
		{
			for (int i = 0; i < keyControlSelectables.Length; ++i)
			{
				if (keyControlSelectables[i].isFocused == true)
				{
					curPos = i;
					break;
				}
			}
			return curPos;
		}

		public void OnSelectInputField(BaseEventData eventData, KeyControlInputField input)
		{
			curPos = Array.IndexOf(keyControlSelectables, input);

			if (keyCoroutine != null) { StopCoroutine(keyCoroutine); }
			keyCoroutine = StartCoroutine(StartCheckKey());

			Debug.LogFormat("[InputField] SelectInput: {0}", input.gameObject.name);
		}

		IEnumerator StartCheckKey()
		{
			yield return null;
			while (keyControlSelectables[curPos].isFocused == true)
			{
				for (int i = 0; i < nextKeys.Length; i++)
				{
					if (Input.GetKeyDown(nextKeys[i]))
					{
						MoveNext();
						break;
					}
				}
				yield return null;
			}

			Debug.LogFormat("[InputField] EndFocus: {0}", keyControlSelectables[curPos].gameObject.name);

			keyCoroutine = null;
		}
	}

}
