using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace NextReality.Utility.UI
{
	[RequireComponent(typeof(RawImage))]
	public class FlexibleGridLayout : CustomLayout<FlexibleGridElement>
	{
		public int columnCount = 2;
		public float elementMinHeight = 140f;
		public Vector2 space;
		public bool isOverflow = false;

		protected int preferCount;

		protected RawImage viewer;

		private void Awake()
		{
			viewer = GetComponent<RawImage>();
			viewer.maskable = true;
		}
		// Start is called before the first frame update
		void Start()
		{

		}

		public override void AlignLayout()
		{
			if (columnCount <= 0) columnCount = 1;
			RectTransform tmpRect;
			float curX = padding.left;
			float curY = -padding.top;
			for(int i = 0;i<container.childCount;i++)
			{
				
				if(container.GetChild(i).TryGetComponent<RectTransform>(out tmpRect))
				{
					tmpRect.pivot = new Vector2(0,1);
					tmpRect.anchorMin = new Vector2(0,1);
					tmpRect.anchorMax = new Vector2(0,1);
					tmpRect.anchoredPosition = new Vector2(curX, curY);
					tmpRect.sizeDelta = PreferElementSize;

					tmpRect.gameObject.SetActive(true);
					if (Mathf.Abs(curY) + elementMinHeight > ContainerInnerSize.y) {
						if (!isOverflow)
						{
							tmpRect.gameObject.SetActive(false);
						}
					}

					if (i % columnCount == columnCount - 1)
					{
						curY -= elementMinHeight + space.y;
						curX = padding.left;
					}
					else
					{
						curX += PreferElementSize.x + space.x;
					}
				}
			}

			if(elementMinHeight > 0)
			{
				float calY = 0;
				int preferColumnCount = 0;
				while (calY <= ContainerInnerSize.y)
				{
					calY += elementMinHeight + space.y;
					preferColumnCount++;
				}

				if (calY - space.y > ContainerInnerSize.y )
				{
					preferColumnCount--;
				}

				preferCount = preferColumnCount*columnCount;
			}



		}

		public int PreferCount
		{
			get
			{
				return preferCount;
			}
		}

		public Vector2 PreferElementSize
		{
			get
			{
				float totalSpaceX = space.x * (columnCount - 1);
				return new Vector2(ContainerInnerSize.x / columnCount - totalSpaceX, elementMinHeight);
			}
		}
	}

}
