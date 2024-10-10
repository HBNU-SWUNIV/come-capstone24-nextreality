using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public class ManagerEdit : ZSchema
	{
		public enum Action
		{
			ADD, DELETE
		}
		public override string SchemaType
		{
			get
			{
				return "ManagerEdit";
			}
		}

		public string editroUserId;
		private string action = "Add";

		public void SetAction(string action)
		{
			this.action = action;
		}

		public void SetActionAdd()
		{
			SetAction("Add");
		}
		public void SetActionDelete()
		{
			SetAction("Delete");
		}

		public ManagerEdit() : base() { }
		public ManagerEdit(string message) : base(message) { }

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			return prev.Cast(ref editroUserId).Cast(ref action);
		}

	}

}
