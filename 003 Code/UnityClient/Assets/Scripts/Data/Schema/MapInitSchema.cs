using NextReality.Data.Schema;
using System.Collections;
using UnityEngine;

namespace NextReality.Data.Schema
{
    public class MapInitSchema : ZSchema
    {

        public override string SchemaType
        {
            get
            {
                return "MapInit";

            }
        }

        public MapInitSchema() : base() { }
        public MapInitSchema(string message) : base(message)
        {

        }

        protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
        {
            //Debug.Log("MapReadySchema : " + message);
            return prev;
        }
    }
}