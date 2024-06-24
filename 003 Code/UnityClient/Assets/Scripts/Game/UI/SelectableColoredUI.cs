using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game.UI
{
	public class SelectableColoredUI : MonoBehaviour
	{

		[SerializeField] private Color selectedBaseColor;
		[SerializeField] private MaskableGraphic targetGraphic;

		private Color originalColor;

		[ReadOnly, SerializeField] private bool isSelected = false;

		protected virtual void Awake()
		{
			if(targetGraphic == null) targetGraphic = GetComponent<MaskableGraphic>();
			originalColor = targetGraphic.color;
		}


		public void Select(bool _isSelect)
		{
			isSelected = _isSelect;
			if (isSelected) targetGraphic.color = selectedBaseColor;
			else targetGraphic.color = originalColor;
		}

		public bool IsSelected { get { return isSelected; } }

	}
}

