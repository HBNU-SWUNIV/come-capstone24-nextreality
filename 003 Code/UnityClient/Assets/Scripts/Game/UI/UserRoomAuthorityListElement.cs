using NextReality.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game.UI
{
	public class UserRoomAuthorityListElement : MonoBehaviour
	{
		[SerializeField]
		TMP_Text userNameText;

		[SerializeField]
		TMP_Text buttonText;

		[SerializeField]
		Button actionButton;
		[SerializeField]
		Image actionButtonRenderer;

		public UserRoomAuthority userRoomAuthority;

		private UserRoomAuthorityListView listView;


		// Start is called before the first frame update
		void Start()
		{
			actionButton.onClick.AddListener(() =>
			{
				ListenerEvent();
			});
		}

		public void SetButton(UserRoomAuthority userData)
		{
			userRoomAuthority = userData;
			userNameText.text = string.Format("{0}({1})", userData.user.nickname, userData.user.user_id);

			SetActionButton(userData.room_authority);
		}

		void SetActionButton(RoomAuthority authority)
		{
			actionButton.enabled = true;
			switch(authority)
			{
				case RoomAuthority.Normal:
					buttonText.text = "추가";
					actionButtonRenderer.color = UserRoomAuthorityEditor.AddAuthorityColor;
					break;
				case RoomAuthority.Manager:
					buttonText.text = "삭제";
					actionButtonRenderer.color = UserRoomAuthorityEditor.RemoveAuthorityColor;
					break;
				case RoomAuthority.Master:
					buttonText.text = "마스터";
					actionButton.enabled = false;
					actionButtonRenderer.color = UserRoomAuthorityEditor.MasterAuthorityColor;
					break;
				default:
					buttonText.text = "---";
					actionButtonRenderer.color = UserRoomAuthorityEditor.UnabledAuthorityColor;
					actionButton.enabled = false;
					break;
			}
		}

		private void ListenerEvent()
		{
			RoomAuthority targetAuthority = RoomAuthority.Error;
			if (authority == RoomAuthority.Manager)
			{
				targetAuthority = RoomAuthority.Normal;
			}
			else if (authority == RoomAuthority.Normal)
			{
				targetAuthority = RoomAuthority.Manager;
			}

			UserRoomAuthorityEditor.Instance.SendConvertAuthority(userRoomAuthority, targetAuthority);
		}

		public RoomAuthority authority
		{
			get
			{
				return userRoomAuthority?.room_authority ?? RoomAuthority.Error;
			}
		}

	}

}
