using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace NextReality.Game.UI
{
    public class SidePopUpUI : MonoBehaviour
    {

        public RectTransform targetRect;

        public Vector3 popUpPos;
        public Vector3 popOutPos;

        public float popUpSpeed;
        public float popOutSpeed;

        [SerializeField] protected bool isPopUp = false;

        [SerializeField, ReadOnly] protected float locationRate;

        private Coroutine curCoroutine;

        protected void Awake()
        {
            locationRate = isPopUp ? 1 : 0;
        }

        protected void Start()
        {
            SetUILocation();
        }

        public bool IsPopUp { get { return isPopUp; } }

        public void PopUp(bool _isPopUp)
        {
            if (popUpPos.x == popOutPos.x && popUpPos.y == popOutPos.y && popUpPos.z == popOutPos.z) return;

            if (curCoroutine != null) { StopCoroutine(curCoroutine); }
            if (this.gameObject.activeSelf)
            {
                curCoroutine = StartCoroutine(StartMoveUI(_isPopUp));
            }

        }

        IEnumerator StartMoveUI(bool _isPopUp)
        {
            isPopUp = _isPopUp;

            while (true)
            {
                yield return null;
                if (targetRect.gameObject.activeSelf)
                {
                    if (isPopUp)
                    {
                        locationRate += Time.deltaTime / popUpSpeed;
                        if (locationRate > 1) locationRate = 1;
                    }
                    else
                    {
                        locationRate -= Time.deltaTime / popOutSpeed;
                        if (locationRate < 0) locationRate = 0;
                    }

                    SetUILocation();

                    if (isPopUp && locationRate == 1) break;
                    else if (!isPopUp && locationRate == 0) break;
                }
            }
            curCoroutine = null;

        }

        protected void SetUILocation()
        {
            targetRect.anchoredPosition = Vector3.Lerp(popOutPos, popUpPos, locationRate);
        }

    }

}
