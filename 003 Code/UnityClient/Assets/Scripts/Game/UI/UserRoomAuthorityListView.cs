using NextReality.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game.UI {
	public class UserRoomAuthorityListView : MonoBehaviour
	{

		Dictionary<UserRoomAuthority,UserRoomAuthorityListElement> userRoomAuthorityListElements = new Dictionary<UserRoomAuthority, UserRoomAuthorityListElement>();

		public RectTransform userListVIewContent;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void AddUserRoomAuthority(UserRoomAuthority userAuthority)
		{
			UserRoomAuthorityListElement element = new UserRoomAuthorityListElement();
			element.SetUser(userAuthority);

			userRoomAuthorityListElements.Add(userAuthority, element);

		}		
		
		public void RemoveUserRoomAuthority(UserRoomAuthority userAuthority)
		{

			userRoomAuthorityListElements.Remove(userAuthority);
		}
	}

}
