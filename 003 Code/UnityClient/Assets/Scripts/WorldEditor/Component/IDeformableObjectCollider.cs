using NextReality.Asset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Asset
{
	public interface IDeformableObjectCollider
	{
		public IDeformableObject TargetObject { get; }
		public Collider TargetCollider { get; }
	}

}
