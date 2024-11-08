using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NextReality.Data.Schema
{
    public class PlayerJoinSchema : ZSchema
    {
        public string joinPlayerId
        {
            get
            {
                return this.userId;
            }
        }
        public string joinPlayerNickname;
		public int mapId;

		public string targetIP_Port = "";
		public override string SchemaType
		{
			get
			{
				return "PlayerJoin";

			}
		}

		public PlayerJoinSchema() : base() { }

		public PlayerJoinSchema(string message) : base(message)
		{

        }

		public PlayerJoinSchema(string joinPlayerNickname, int mapId) : this()
		{
			this.joinPlayerNickname = joinPlayerNickname;
			this.mapId = mapId;
		}

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			//Debug.Log("PlayerJoinSchema : " + message);
			return prev.Cast(ref joinPlayerNickname).Cast(ref mapId).Cast(ref targetIP_Port);
		}
    }
}

