using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NextReality.Asset.UI
{
	public abstract class QuickSlotLayoutBase : MonoBehaviour
	{
		[SerializeField] protected float _space;
		public float space
		{
			get { return _space; }
			set { _space = value; AlignLayout(); }
		}

		[SerializeField] protected Vector2 _slotSize;
		public Vector2 slotSize
		{
			get { return _slotSize; }
			set { _slotSize = value; AlignLayout(); }
		}

		[SerializeField] protected RectTransform slotContainer;
		public RectTransform SlotContainer { get { return slotContainer; } }


		public abstract void AlignLayout();

		protected virtual float GetPrefeWidth(float prevWidth, int curIndex)
		{
			return prevWidth + _slotSize.x + _space;
		}
		protected virtual float GetSlotX(float prevX, int curIndex)
		{
			return prevX + slotSize.x + _space;
		}
	}

	[ExecuteInEditMode]
	public class QuickSlotLayout<SlotType> : QuickSlotLayoutBase where SlotType : QuickSlot
	{

		[SerializeField] protected SlotType[] quickSlots;

		public SlotType[] QuickSlots { get { return quickSlots; } }


		public override void AlignLayout()
		{
			float preferWidth = 0;
			float slotX = 0;
			float preferHeight = _slotSize.y;

			int childCount = slotContainer.transform.childCount;

			if (quickSlots == null || quickSlots.Length != childCount)
			{
				quickSlots = new SlotType[childCount];
				for (int i = 0; i < childCount; i++)
				{
					quickSlots[i] = slotContainer.transform.GetChild(i).GetComponent<SlotType>();
				}
			}

			for (int i = 0; i < quickSlots.Length; i++)
			{
				quickSlots[i].SetRect();
				quickSlots[i].rectTransform.sizeDelta = _slotSize;
				quickSlots[i].rectTransform.anchoredPosition = new Vector2(slotX, 0);

				preferWidth = GetPrefeWidth(preferWidth, i);
				slotX = GetSlotX(slotX, i);
			}
			preferWidth -= _space;

			slotContainer.sizeDelta = new Vector2(preferWidth, preferHeight);

		}

#if UNITY_EDITOR
		private void Update()
		{
			AlignLayout();
		}
#endif

	}
}
