using NextReality.Asset;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace NextReality.Asset.UI
{
    public class ObjectBoundingBox : MonoBehaviour
    {

        public LineRenderer[] boundRendererArray = new LineRenderer[4];

        public TMP_Text nameText;

        private Vector3 v3FrontTopLeft;
        private Vector3 v3FrontTopRight;
        private Vector3 v3FrontBottomLeft;
        private Vector3 v3FrontBottomRight;
        private Vector3 v3BackTopLeft;
        private Vector3 v3BackTopRight;
        private Vector3 v3BackBottomLeft;
        private Vector3 v3BackBottomRight;

        private WorldObject selectedObject;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            DrawBox();
            SetNameTagPosition();
        }

        protected void CalcPositons()
        {
            var bound = selectedObject.BodyBounds;
            var v3Center = bound.center;
            var v3Extents = bound.extents;

            this.v3FrontTopLeft = this.selectedObject.TransformDirection(new Vector3(-v3Extents.x, v3Extents.y, -v3Extents.z));
            this.v3FrontTopRight = this.selectedObject.TransformDirection(new Vector3(v3Extents.x, v3Extents.y, -v3Extents.z));
            this.v3FrontBottomLeft = this.selectedObject.TransformDirection(new Vector3(-v3Extents.x, -v3Extents.y, -v3Extents.z));
            this.v3FrontBottomRight = this.selectedObject.TransformDirection(new Vector3(v3Extents.x, -v3Extents.y, -v3Extents.z));
            this.v3BackTopLeft = this.selectedObject.TransformDirection(new Vector3(-v3Extents.x, v3Extents.y, v3Extents.z));
            this.v3BackTopRight = this.selectedObject.TransformDirection(new Vector3(v3Extents.x, v3Extents.y, v3Extents.z));
            this.v3BackBottomLeft = this.selectedObject.TransformDirection(new Vector3(-v3Extents.x, -v3Extents.y, v3Extents.z));
            this.v3BackBottomRight = this.selectedObject.TransformDirection(new Vector3(v3Extents.x, -v3Extents.y, v3Extents.z));

            this.v3FrontTopLeft += v3Center;
            this.v3FrontTopRight += v3Center;
            this.v3FrontBottomLeft += v3Center;
            this.v3FrontBottomRight += v3Center;
            this.v3BackTopLeft += v3Center;
            this.v3BackTopRight += v3Center;
            this.v3BackBottomLeft += v3Center;
            this.v3BackBottomRight += v3Center;
        }

        protected void DrawBox()
        {
            if (this.selectedObject == null) return;
            this.CalcPositons();

            this.SetLineRenderer(this.boundRendererArray[0], this.v3FrontTopLeft, this.v3FrontTopRight, this.v3FrontBottomRight, this.v3FrontBottomLeft);
            this.SetLineRenderer(this.boundRendererArray[1], this.v3FrontTopRight, this.v3BackTopRight, this.v3BackBottomRight, this.v3FrontBottomRight);
            this.SetLineRenderer(this.boundRendererArray[2], this.v3BackTopRight, this.v3BackTopLeft, this.v3BackBottomLeft, this.v3BackBottomRight);
            this.SetLineRenderer(this.boundRendererArray[3], this.v3BackTopLeft, this.v3FrontTopLeft, this.v3FrontBottomLeft, this.v3BackBottomLeft);
        }

        protected void SetLineRenderer(LineRenderer lineRenderer, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            var vertexPositions = new Vector3[] { p1, p2, p3, p4 };
            lineRenderer.positionCount = 4;
            lineRenderer.loop = false;
            lineRenderer.SetPositions(vertexPositions);
        }

        protected void SetNameTagPosition()
        {
            if (this.selectedObject == null) return;

            this.transform.position = selectedObject.Position;
        }

        void LateUpdate()
        {
            if (Managers.Camera.mainGameCamera)
            {
                nameText.transform.eulerAngles = Managers.Camera.mainGameCamera.transform.eulerAngles;
            }
        }

        public void SetSelectedObject(WorldObject obj)
        {
            selectedObject = obj;
        }
        public void SetPlayerNickName(string userId)
        {
            nameText.text = Managers.Client?.GetPlayer(userId)?.userInfo.nickname ?? "TESTID";
        }
    }

}
