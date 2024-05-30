using NextReality.Data.Schema;
using System.Collections;
using UnityEngine;

namespace NextReality.Data.Schema
{
    public class MapReadySchema : ZSchema
    {
        public override string SchemaType
        {
            get
            {
                return "MapReady";

            }
        }

        public MapReadySchema() : base() { }
        public MapReadySchema(string message) : base(message)
        {

        }

        protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
        {
            //Debug.Log("MapReadySchema : " + message);
            return prev;
        }
    }
}