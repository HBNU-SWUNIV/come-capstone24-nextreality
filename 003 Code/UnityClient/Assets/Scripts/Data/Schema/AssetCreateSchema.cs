using NextReality.Asset;
using System;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace NextReality.Data.Schema
{
    public class AssetCreateSchema : ZSchema
    {
        public ObjectData objData;

		public override string SchemaType
		{
			get
			{
				return "AssetCreate";

			}
		}

		public AssetCreateSchema(): base() { }
		public AssetCreateSchema(string message) : base(message)
        {

        }

		protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
		{
			//Debug.Log("AssetCreateSchema : " + message);
			if (objData == null) objData = new ObjectData();
			return prev
				.CastAnyFromProperty(objData, nameof(ObjectData.ast_id))
				.CastAnyFromProperty(objData, nameof(ObjectData.obj_id))
				.CastVector(objData.transform.position)
				.CastVector(objData.transform.rotation)
				.CastVector(objData.transform.scale)
				.CastAnyFromProperty(objData, nameof(ObjectData.type))
				.CastAnyFromProperty(objData, nameof(ObjectData.isMeshCollider))
				.CastAnyFromProperty(objData, nameof(ObjectData.isMeshCollider));
		}

	}
}