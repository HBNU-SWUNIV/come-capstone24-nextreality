using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NextReality.Asset.UI
{
	public class AssetQuickSlot : QuickSlot, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
	{
		AssetItem targetAssetItem;
		public AssetItem TargetAssetItem { get { return targetAssetItem; } }

		private AssetQuickSlotLayout layout;

		public void OnDrop(PointerEventData eventData)
		{
			Debug.Log("QuickSlot DropDown");
			var item = WorldEditorController.Instance.AssetItemCursor.PopAssetItem();
			if (item != null && item != targetAssetItem)
			{

				var prevQuickSlot = Array.Find(layout.QuickSlots, element => element != this && element.targetAssetItem != null && element.targetAssetItem == item);

				bool anySlotEmpty = !Array.Exists(layout.QuickSlots, element => element.targetAssetItem != null);

				if(prevQuickSlot != null)
				{
					prevQuickSlot.SetAsset(targetAssetItem);
				}

				SetAsset(item);

				if(anySlotEmpty)
				{
					layout.SetSelectedSlot(this);
				}
			}
		}
		public void OnDrag(PointerEventData eventData)
		{
			if (targetAssetItem == null) return;
			WorldEditorController.Instance.DragAssetItemCursor(targetAssetItem, eventData.position);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (targetAssetItem == null) return;
			WorldEditorController.Instance.DragAssetItemCursor(targetAssetItem, eventData.position);

		}

		public void OnEndDrag(PointerEventData eventData)

		{
			WorldEditorController.Instance.EndDragAssetItemCursor();
		}

		void Start()
		{
			SetAsset(null);
		}

		public void SetAsset(AssetItem asset)
		{
			targetAssetItem = asset;
			SetItemImage(asset?.thumbnail2D ?? null);
		}

		public void SetSlotLayout(AssetQuickSlotLayout _layout)
		{
			layout = _layout;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			layout.SetSelectedSlot(this);
		}
	}
}

