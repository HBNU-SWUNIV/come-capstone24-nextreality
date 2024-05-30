using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public class AssetDeselectSchema : ZSchema
	{
		public override string SchemaType {
			get
			{
				return "AssetDeselect";
			}
		}

		public string objectId;

		public AssetDeselectSchema(): base() {}
		public AssetDeselectSchema(string message) : base(message) { }

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			return prev.Cast(ref objectId);
		}

	}

}
