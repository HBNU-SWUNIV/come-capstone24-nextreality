using NextReality.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace NextReality.Game.UI {
	public class ModelPreviewPanel : MonoBehaviour, IDragHandler, IScrollHandler
	{
		MaskableGraphic targetGraphic;

		public ModelPreviewer previewer;
		public float previewSensitive = 0.5f;

		private float curZoom = 1.0f;
		public float zoomSensitive = 1.0f;

		public void OnDrag(PointerEventData eventData)
		{
			if (!previewer.IsReadyTarget) return;
			Vector2 delta = eventData.delta * previewSensitive;
			previewer.RotateCamera(delta.x, -delta.y);
		}

		private void Awake()
		{
			targetGraphic = GetComponent<MaskableGraphic>();
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (!previewer.IsReadyTarget) return;
			float zoomDelta = (eventData.scrollDelta.x + eventData.scrollDelta.y)/2;
			previewer.ZoomCamera(zoomDelta);

		}
	}
}


