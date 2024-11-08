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
			UserRoomAuthorityListElement element;
			if(userRoomAuthorityListElements.TryGetValue(userAuthority.user.user_id, out element))
			{
				
			} else
			{
				element = GameObject.Instantiate(UserRoomAuthorityEditor.Instance.listElementPrefab, userListVIewContent);
				userRoomAuthorityListElements.Add(userAuthority.user.user_id, element);
			}
			element.SetUser(userAuthority);

		}

		public void RemoveUserRoomAuthority(string userId)
		{
			if (!userRoomAuthorityListElements.ContainsKey(userId)) return;

			UserRoomAuthorityListElement element;
			if (userRoomAuthorityListElements.TryGetValue(userId, out element))
			{
				userRoomAuthorityListElements.Remove(userId);
				GameObject.Destroy(element.gameObject);
			}
		}

		public void RemoveUserRoomAuthority(UserRoomAuthority userAuthority)
		{
			RemoveUserRoomAuthority(userAuthority.user.user_id);

		}

		public UserRoomAuthorityListElement GetUserRoomAuthorityListElement(UserRoomAuthority userAuthority)
		{
			UserRoomAuthorityListElement element;
			if (userRoomAuthorityListElements.TryGetValue(userAuthority.user.user_id, out element))
			{
				return element;
			}
			else return null;
		}
	}

}
