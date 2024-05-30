using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Data;

namespace NextReality.Data.Schema
{
    public class PlayerLeaveSchema : ZSchema
    {
		public string leavePlayerId {
			get
			{
				return this.userId;
			}
			set
			{
				this.userId = value;
			}
		}

		public override string SchemaType
		{
			get
			{
				return "PlayerLeave";

			}
		}

		public PlayerLeaveSchema() : base() { }

		public PlayerLeaveSchema(string message) : base(message)
		{

        }

		public PlayerLeaveSchema(string leavePlayerId, int? mapId) : this()
		{
			this.leavePlayerId = leavePlayerId;
		}

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			//Debug.Log("PlayerLeaveSchema : " + message);
			return prev;
		}

    }
}
