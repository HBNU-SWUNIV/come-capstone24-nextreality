using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NextReality.Asset.UI
{
	public class AssetQuickSlotLayout : QuickSlotLayout<AssetQuickSlot>
	{

		protected AssetQuickSlot selectedSlot = null;

		[SerializeField] protected RectTransform highlightFrame;
		[SerializeField] protected Vector2 additionalFrameSize;

		public AssetItem SelectedAsset
		{
			get
			{
				return selectedSlot?.TargetAssetItem ?? null;
			}
		}

		private void InitSlots()
		{
			for(int i =0;i<quickSlots.Length;i++)
			{
				quickSlots[i].SetSlotLayout(this);
			}
			SetSelectedSlot(selectedSlot);
			InitHighlightFrame();
		}

		public override void AlignLayout()
		{
			base.AlignLayout();
			InitSlots();
		}

		protected void InitHighlightFrame()
		{
			highlightFrame.sizeDelta = slotSize + additionalFrameSize;
		}

		public void SetSelectedSlot(AssetQuickSlot _selectedSlot)
		{
			selectedSlot = _selectedSlot;

			highlightFrame.gameObject.SetActive(selectedSlot != null);
			if (selectedSlot != null)
			{
				highlightFrame.SetParent(selectedSlot.transform);
				highlightFrame.anchoredPosition = Vector2.zero;
			}
		}




	}
}

