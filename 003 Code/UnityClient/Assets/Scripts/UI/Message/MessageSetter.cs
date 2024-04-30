using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NextReality.Game.UI
{
    public class MessageSetter
    {
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
    }

}
