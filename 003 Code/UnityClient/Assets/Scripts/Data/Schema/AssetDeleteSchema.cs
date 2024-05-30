using NextReality.Asset;
using System;
using System.Collections;
using UnityEngine;

namespace NextReality.Data.Schema
{
    public class AssetDeleteSchema : ZSchema
    {
        public string obj_id;

        public override string SchemaType
        {
            get
            {
                return "AssetDelete";

            }
        }

        public AssetDeleteSchema() : base() { }
        public AssetDeleteSchema(string message) : base(message)
        {

        }

        protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
        {
            //Debug.Log("AssetDeleteSchema : " + message);
            return prev.Cast(ref obj_id);
        }
    }
}