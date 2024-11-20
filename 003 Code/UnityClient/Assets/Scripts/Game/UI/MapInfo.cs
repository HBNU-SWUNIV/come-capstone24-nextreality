using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Asset;
using TMPro;
using UnityEngine.UI;
using NextReality.Data;


namespace NextReality.Game.UI
{
    public class MapInfo : MonoBehaviour
    {

        public MapPopup mapPopup;
        public MapListData mapListData;

        public TMP_Text mapName;
        public TMP_Text mapMaker;
        public int myIndex;

        public CanvasGroup canvasGroup;

        public void SetData(MapListData data, int index)
        {
            myIndex = index;
            mapListData = data;
            mapName.text = mapListData.mapName;
            mapMaker.text = mapListData.user_id?? "admin";
			canvasGroup.alpha = 1;

			Debug.Log("mapInfos[" + myIndex + "] :  name - " + mapListData.mapName + "   id - " + mapListData.map_id);
        }

        public void ResetData()
        {
            mapName.text = null;
            mapMaker.text = null;
			canvasGroup.alpha = 0;
		}

		public void OnClickThisMap()
        {
            // this.GetComponent<Image>().color = new Color(255f, 222f, 222f); // light pink color

            Debug.LogFormat("[MapInfo] {0} {1}", mapListData?.map_id ?? -1, myIndex);
            mapPopup.ClickMapInfo(this.mapListData?.map_id ?? 0, this.myIndex);
        }
    }
}
