using NextReality.Data.Schema;
using Unity.VisualScripting;
using UnityEngine;

namespace NextReality.Asset
{
    public class AssetObject : WorldObject
    {
        public int object_id = 0;
        public string asset_id = "0";

        public bool isMeshCollider = false;
        public bool isRigidBody = false;

        // MeshCollider 생성 메서드
        public void MakeObjectSolid()
        {
            try
            {
                bool isSolidThisTime = false;
                foreach (var item in gameObject.GetComponentsInChildren<MeshFilter>())
                {

					if (item.gameObject.layer == (int)Layers.UI) continue;

					MeshCollider collider;

					if (!item.TryGetComponent<MeshCollider>(out collider))
                    {
						collider = item.AddComponent<MeshCollider>();
                        isSolidThisTime = true;
					}
					collider.sharedMesh = item.sharedMesh;
					collider.convex = true;
					collider.gameObject.layer = (int)Layers.EditableObject;

                    AssetCollider assetCollider;
                    if(!item.TryGetComponent<AssetCollider>(out assetCollider))
                    {
                        assetCollider = item.AddComponent<AssetCollider>();
                    }

                    assetCollider.SetTargetAssetObject(this);
                }

                foreach (var item in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
					MeshCollider collider;

					if (!item.TryGetComponent<MeshCollider>(out collider))
					{
						collider = item.AddComponent<MeshCollider>();
						isSolidThisTime = true;
					}
					collider.sharedMesh = item.sharedMesh;
					collider.convex = true;
					collider.gameObject.layer = (int)Layers.EditableObject;

					AssetCollider assetCollider;
					if (!item.TryGetComponent<AssetCollider>(out assetCollider))
					{
						assetCollider = item.AddComponent<AssetCollider>();
					}

					assetCollider.SetTargetAssetObject(this);
				}

                if(isSolidThisTime)
                {
                    InitWireFrame();
                }

			}
            catch
            {

            }

            //if(WireFrame) WireFrame.SetActive(false);

            
        }

        // Component들 생성 메서드
        public void AddComponents()
        {
            try
            {
                {
                    Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = gameObject.AddComponent<Rigidbody>();
                    }

                    if (rb != null)
                    {
                        rb.mass = 1.0f;
                        rb.isKinematic = true;
                    }
                }

                if (isMeshCollider)
                {
					MakeObjectSolid();
				}
            }
            catch
            {

            }
        }

		//protected override void Start()
		//{
		//	base.Start();
		//}

		//protected override void Awake()
		//{
		//	base.Awake();
		//}

		private void OnDrawGizmos()
		{
			if(colliders != null && colliders.Count>0)
            {
                Bounds bounds = BodyBounds;
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			} else
            {
                Gizmos.DrawWireSphere(transform.position, 1);
            }
		}

		public void TransformBySchema(AssetMoveSchema schema)
        {
            if (schema == null && int.Parse(schema.objectId) != object_id) return;

            LocalPosition = schema.position;
            LocalEulerAngles = schema.rotation;
            LocalScale = schema.scale;
        } 
    }
}