using NextReality.Asset.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace NextReality.Game.UI
{
	[RequireComponent(typeof(Button))]
	public class SelectableColoredButton : SelectableColoredUI, IInteractable
	{

		[SerializeField] private Button targetButton;
		public Button TargetButton { get { return targetButton; } }

		protected override void Awake()
		{
			base.Awake();
			if(targetButton == null) targetButton = GetComponent<Button>();
		}

		public ButtonClickedEvent onClick
		{
			get { return targetButton.onClick; }
			set { targetButton.onClick = value; }
		}

		public bool interactable
		{
			get { return targetButton.interactable;}
			set { targetButton.interactable = value;}
		}
	}
}

