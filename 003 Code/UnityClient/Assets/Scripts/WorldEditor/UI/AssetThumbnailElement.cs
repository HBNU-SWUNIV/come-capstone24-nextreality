using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public class AssetThumbnailElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private RawImage thumbnailImage;
		[SerializeField] private TMP_Text thumbnailText;

		private AssetItem assetItem;
		public AssetItem AssetItem
		{
			get { return assetItem; }
		}

		private bool isPointerDown = false;
		private bool isLongPress = false;
		private float pressTime = 0f;

		private AssetThumbnailScrollView thumbnailScrollView;

		Coroutine dragCoroutine;

		// Start is called before the first frame update
		void Start()
		{

		}

		IEnumerator RunDragAssetItem()
		{
			while (isPointerDown && !isLongPress)
			{
				pressTime += Time.deltaTime;

				if (pressTime >= thumbnailScrollView.thumbnailLongPressDuration && !isLongPress)
				{
					// 길게 누르기 감지
					isLongPress = true;
					OnLongPress();
					break;
				}
				yield return null;
			}

		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isPointerDown = true;
			pressTime = 0f;
			isLongPress = false;
			if (dragCoroutine != null) StopCoroutine(dragCoroutine);
			dragCoroutine = StartCoroutine(RunDragAssetItem());
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			isPointerDown = false;

			// 길게 누르는 도중에 드래그를 중지하면 여기서 처리할 수 있습니다.
			if (isLongPress)
			{
				OnLongPressEnd();
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (isLongPress)
			{
				WorldEditorController.Instance.DragAssetItemCursor(assetItem, eventData.position);
			}
			else
			{
				thumbnailScrollView.ScrollRect.OnDrag(eventData);
				if (eventData.delta.magnitude > 1f)
				{
					if (dragCoroutine != null) StopCoroutine(dragCoroutine);
					dragCoroutine = null;
				}
			}

		}

		public void OnBeginDrag(PointerEventData e)

		{

			thumbnailScrollView.ScrollRect.OnBeginDrag(e);

		}

		public void OnEndDrag(PointerEventData e)

		{
			thumbnailScrollView.ScrollRect.OnEndDrag(e);
			OnLongPressEnd();
		}

		void OnLongPress()
		{
			WorldEditorController.Instance.DragAssetItemCursor(assetItem, Input.mousePosition);
			Debug.Log("Long Pressed!");
		}

		void OnLongPressEnd()
		{
			WorldEditorController.Instance.EndDragAssetItemCursor();
		}

		public AssetThumbnailElement InitScrollView(AssetThumbnailScrollView _thumbnailScrollView)
		{
			thumbnailScrollView = _thumbnailScrollView;
			return this;
		}

		public void SetAsset(AssetItem assetItem)
		{
			this.assetItem = assetItem;
			thumbnailText.text = assetItem.name;
			if (assetItem.thumbnail2D != null) thumbnailImage.texture = assetItem.thumbnail2D;
		}

		public void SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}

		public bool IsActive
		{
			get { return gameObject.activeSelf == this; }
		}
	}
}
