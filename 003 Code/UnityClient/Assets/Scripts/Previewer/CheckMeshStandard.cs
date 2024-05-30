using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace NextReality.Game
{
	public class CheckMeshStandard : MonoBehaviour
	{
		public MeshFilter meshStandrard;
		MeshRenderer meshStandardRenderer;
		Collider meshStandardCollider;
		public Material wireMaterial;
		public GameObject targetObject;

		public Color containedColor = Color.green;
		public Color uncontainedColor = Color.red;

		private MeshFilter[] meshFilters;
		private SkinnedMeshRenderer[] skinnedMeshRenderers;

		bool? isContained = null;

		private void Awake()
		{
			meshStandardRenderer = meshStandrard.GetComponent<MeshRenderer>();
			meshStandardCollider = meshStandrard.GetComponent<Collider>();
			meshStandardRenderer.material = wireMaterial;
			meshStandardRenderer.material.SetFloat("_WireThickness", 3.2f);
		}
		// Start is called before the first frame update
		void Start()
		{
			if (targetObject != null) SetTargetObject(targetObject);
		}

		// Update is called once per frame
		void Update()
		{
			DisplayTargetObjectContained();
		}

		public void SetTargetObject(GameObject targetObject)
		{
			if (targetObject == null)
			{
				targetObject = null;
				meshFilters = null;
				skinnedMeshRenderers = null;
				return;
			}

			this.targetObject = targetObject;
			meshFilters = targetObject.GetComponentsInChildren<MeshFilter>();
			skinnedMeshRenderers = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		}

		void DisplayTargetObjectContained()
		{
			if (targetObject == null || meshStandardRenderer == null)
			{
				isContained = null;
				return;
			}

			if (CheckTargetObjectContained())
			{
				if (isContained != true)
				{
					meshStandardRenderer.material.SetColor("_WireColor", containedColor);
				}
				isContained = true;
			}
			else
			{
				if (isContained != false)
				{
					meshStandardRenderer.material.SetColor("_WireColor", uncontainedColor);
				}
				isContained = false;
			}
		}

		bool CheckTargetObjectContained()
		{
			if (meshFilters == null && skinnedMeshRenderers == null) return false;
			if (meshFilters.Length == 0 && skinnedMeshRenderers.Length == 0) return false;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				if (!IsMeshContainedInAnother(meshFilters[i].transform, meshFilters[i].mesh, meshStandardCollider))
				{
					return false;
				}
			}

			for (int i = 0; i < skinnedMeshRenderers.Length; i++)
			{
				if (!IsMeshContainedInAnother(skinnedMeshRenderers[i].transform, skinnedMeshRenderers[i].sharedMesh, meshStandardCollider))
				{
					return false;
				}
			}

			return true;
		}

		// meshFilter: 포함 여부를 확인할 메쉬, meshFilterB:
		bool IsMeshContainedInAnother(Transform meshTargetObject, Mesh targetMesh, Collider meshCollider)
		{

			if (targetMesh == null)
			{
				Debug.LogError("One of the meshes is null");
				return false;
			}

			foreach (Vector3 vertex in targetMesh.vertices)
			{
				Vector3 worldVertex = meshTargetObject.TransformPoint(vertex);
				Vector3 closestPoint = Physics.ClosestPoint(worldVertex, meshCollider, meshCollider.transform.position, meshCollider.transform.rotation);

				// If the closest point on B is not equal to the vertex on A, it's outside
				if (closestPoint != worldVertex)
				{
					return false;
				}
			}

			return true;
		}

		public bool IsContained
		{
			get
			{
				return isContained?? false;
			}
		}
	}

}