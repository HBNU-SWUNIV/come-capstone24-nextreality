using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public class ManagerEditSchema : ZSchema
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

		public string editorUserId;
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

		public string authority
		{
			get
			{
				return action;
			}
		}

		public ManagerEditSchema() : base() { }
		public ManagerEditSchema(string message) : base(message) { }

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			return prev.Cast(ref editorUserId).Cast(ref action);
		}

	}

}
