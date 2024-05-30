using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NextReality.Game.UI
{
    public class MessageSetter : MonoBehaviour
    {

        private static MessageSetter instance = null;

        public static MessageSetter Instance
        {
            get
            {
                if (instance == null)
                    return null;
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }


        public void SetText(TMP_Text textObj, string text)
        {
            textObj.SetText(text);
            textObj.gameObject.SetActive(true);
        }
        public void SetText(TMP_Text textObj, string text, Color color)
        {
            textObj.SetText(text);
            textObj.color = color;
            textObj.gameObject.SetActive(true);
        }
        public void ResetText(TMP_Text textObj)
        {
            textObj.SetText(string.Empty);
            textObj.gameObject.SetActive(false);
        }
    }

}
