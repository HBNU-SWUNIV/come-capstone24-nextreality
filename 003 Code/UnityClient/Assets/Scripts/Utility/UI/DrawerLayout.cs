using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Utility.UI
{
	public class DrawerLayout : MonoBehaviour
	{

		[SerializeField] private LayoutGroup drawerPanel;

		[SerializeField] private RectTransform[] fixedElement;
		[SerializeField] private RectTransform variableElement;
		// Start is called before the first frame update

		private RectTransform drawerRectTransfrom;
		void Awake()
		{
			drawerRectTransfrom = drawerPanel.GetComponent<RectTransform>();
		}

		public void AlignLayout()
		{
			float innerHeight = drawerRectTransfrom.rect.height - drawerPanel.padding.bottom - drawerPanel.padding.top;
			float fixedHeight = 0;
			for (int i = 0; i < fixedElement.Length; i++) { fixedHeight += fixedElement[i].rect.height; }

			VerticalLayoutGroup horizontal;

			float totalSpace = 0;

			if (drawerRectTransfrom.TryGetComponent(out horizontal))
			{
				totalSpace = horizontal.spacing * (drawerPanel.transform.childCount - 1);
			}

			
			if (variableElement != null) variableElement.sizeDelta = new Vector2(variableElement.sizeDelta.x, innerHeight - fixedHeight - totalSpace);

		}
	}

}
