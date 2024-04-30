using Unity.VisualScripting;
using UnityEngine;

namespace NextReality.Asset
{
	public class AssetObject : WorldObject
	{
		public int object_id = 0;
		public int asset_id = 0;

        public bool isMeshCollider = false;
        public bool isRigidBody = false;

        public void MakeObjectSolid()
        {
            try
            {
                foreach (var item in gameObject.GetComponentsInChildren<MeshFilter>())
                {
                    var col = item.AddComponent<MeshCollider>();
                    col.sharedMesh = item.sharedMesh;

                    item.AddComponent<AssetCollider>().SetTargetAssetObject(this);

                }
            }
            catch
            {

            }
        }

        public void AddComponents()
        {
            if (isMeshCollider) { MakeObjectSolid(); }
            if (isRigidBody)
            {
                Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                if (rb != null) rb.mass = 1.0f;
            }
        }
    }
}