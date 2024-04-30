using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public class QuickSlot : MonoBehaviour
	{
		protected RectTransform _rectTransform;
		public RectTransform rectTransform { get { if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>(); return _rectTransform; } }

		[SerializeField] protected RawImage _itemImage;
		public RawImage ItemImage { get { return _itemImage; } }

		private void Start()
		{
			SetItemImage(null);
		}

		public void SetRect()
		{
			rectTransform.anchorMin = new Vector2(0, 1);
			rectTransform.anchorMax = new Vector2(0, 1);
			rectTransform.pivot = new Vector2(0, 1);
		}

		public void SetItemImage(Texture2D tex = null)
		{
			_itemImage.texture = tex;
			_itemImage.color = tex != null ? Color.white : Color.clear;
		}
	}

}
