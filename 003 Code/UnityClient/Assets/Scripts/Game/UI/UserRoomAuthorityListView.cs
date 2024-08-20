using NextReality.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game.UI {
	public class UserRoomAuthorityListView : MonoBehaviour
	{

		Dictionary<string,UserRoomAuthorityListElement> userRoomAuthorityListElements = new Dictionary<string, UserRoomAuthorityListElement>();

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
			UserRoomAuthorityListElement element = GameObject.Instantiate(UserRoomAuthorityEditor.Instance.listElementPrefab, userListVIewContent);
			element.SetUser(userAuthority);

			

			userRoomAuthorityListElements.Add(userAuthority.user_id, element);

		}		
		
		public void RemoveUserRoomAuthority(UserRoomAuthority userAuthority)
		{
			UserRoomAuthorityListElement element;
			if(userRoomAuthorityListElements.TryGetValue(userAuthority.user_id, out element))
			{
				userRoomAuthorityListElements.Remove(userAuthority.user_id);
				GameObject.Destroy(element.gameObject);
			}

		}

		public UserRoomAuthorityListElement GetUserRoomAuthorityListElement(UserRoomAuthority userAuthority)
		{
			UserRoomAuthorityListElement element;
			if (userRoomAuthorityListElements.TryGetValue(userAuthority.user_id, out element))
			{
				return element;
			}
			else return null;
		}
	}

}
