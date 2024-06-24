using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Utility.UI
{
	[ExecuteAlways]
	public abstract class CustomLayout<T> : MonoBehaviour where T:CustomLayoutElement
	{
		public RectTransform container;

		public RectOffset padding;

		public List<T> elements;
		// Update is called once per frame
		void Update()
		{
			if(container != null)
			{
				AlignLayout();
			}
		}

		public abstract void AlignLayout();

		public Vector2 ContainerSize
		{
			get
			{
				return new Vector2(container.rect.width, container.rect.height);
			}
		}

		public Vector2 ContainerInnerSize
		{
			get
			{
				Vector2 innerPadding = new Vector2(padding.left + padding.right, padding.top + padding.bottom);
				return ContainerSize - innerPadding;
			}
		}

		public int ChildCount
		{
			get
			{
				return transform.childCount;
			}
		}
	}
}

