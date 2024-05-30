using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NextReality.Utility.UI
{
	public class KeyControlInputField : MonoBehaviour, ISelectHandler
	{
		public Selectable selectable;

		public InputFieldKeyController controller;

		private void Awake()
		{
			Button button = GetComponent<Button>();
			selectable = GetComponent<Selectable>();
		}

		public void OnSelect(BaseEventData eventData)
		{
			//selectable.OnSelect(eventData);
			controller.OnSelectInputField(eventData, this);
		}

		public void Select()
		{
			selectable.Select();
		}

		public bool isFocused
		{
			get
			{
				return EventSystem.current.isFocused && EventSystem.current.currentSelectedGameObject == selectable.gameObject;
			}
		}

		public bool IsActive
		{
			get
			{
				return gameObject.activeInHierarchy;
			}
		}

		public bool IsInteractable
		{
			get
			{
				return selectable.interactable;
			}
		}
	}

}
