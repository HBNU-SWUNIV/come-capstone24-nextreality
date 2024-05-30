using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public class AssetSelectSchema : ZSchema
	{
		public override string SchemaType {
			get
			{
				return "AssetSelect";
			}
		}

		public string objectId;

		public AssetSelectSchema(): base() {}
		public AssetSelectSchema(string message) : base(message) { }

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			return prev.Cast(ref objectId);
		}

	}

}
