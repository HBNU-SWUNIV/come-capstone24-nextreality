using UnityEngine;

namespace NextReality.Asset
{
	public class AssetCollider : MonoBehaviour, IDeformableObjectCollider
	{
		AssetObject targetAssetObject;

		public void SetTargetAssetObject(AssetObject targetAssetObject)
		{
			this.targetAssetObject = targetAssetObject;
		}

		public AssetObject TargetAssetObject { get { return targetAssetObject; } }

		IDeformableObject IDeformableObjectCollider.TargetObject { get { return targetAssetObject; } }

	}
}

