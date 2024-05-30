using System;
using System.Collections;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public class MapChangeSchema : ZSchema
	{
		public int map_id;

		public override string SchemaType
		{
			get
			{
				return "MapChange";

			}
		}

		public MapChangeSchema() : base() { }
		public MapChangeSchema(string message) : base(message)
		{

		}

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			Debug.Log("MapChangeSchema : " + message);
			return prev.Cast(ref map_id);
		}
	}
}