using UnityEngine;

namespace NextReality.Asset
{
	public class AssetCollider : MonoBehaviour, IDeformableObjectCollider
	{
		public AssetObject targetAssetObject;

		public void SetTargetAssetObject(AssetObject targetAssetObject)
		{
			this.targetAssetObject = targetAssetObject;
			targetAssetObject.colliders.Add(this);
		}

		public AssetObject TargetAssetObject { get { return targetAssetObject; } }

		IDeformableObject IDeformableObjectCollider.TargetObject { get { return targetAssetObject; } }

		Collider collider;
        Collider IDeformableObjectCollider.TargetCollider
		{
			get
			{
                if (collider == null)
                    collider = GetComponent<Collider>();
				return collider;

            }
		}
    }
}

