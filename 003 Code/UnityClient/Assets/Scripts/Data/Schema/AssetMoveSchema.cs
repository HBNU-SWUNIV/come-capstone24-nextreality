using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public class AssetMoveSchema : ZSchema
	{
		public override string SchemaType {
			get
			{
				return "AssetMove";
			}
		}

		public string objectId;
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;

		public AssetMoveSchema(): base() {}
		public AssetMoveSchema(string message) : base(message) { }

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			//Debug.Log(message);
			return prev.Cast(ref objectId)
				.CastVector(ref position)
				.CastVector(ref rotation)
				.CastVector(ref scale);
		}

	}

}
