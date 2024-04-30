using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality;
using System;
using TMPro;
using NextReality.Data;
using UnityEditor.Experimental.GraphView;


namespace NextReality.Game
{
    public class ClientManager : MonoBehaviour
    {

        private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

        private static ClientManager instance = null;

        public GameObject otherPlayers;
        GameObject otherPlayerPrefab;
        


        private void Awake()
        {
            if (null == instance)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public static ClientManager Instance
        {
            get
            {
                if (null == instance)
                {
                    return null;
                }
                return instance;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            // Send PlayerJoin Message
            // players.Add(Managers.User.Id, Managers.User.Nickname);
            otherPlayerPrefab = Resources.Load("Prefabs/OtherPlayer") as GameObject;
            
        }

        // Update is called once per frame
        void Update()
        {
            // check with server player list
            // send my transform
        }

        public IEnumerator PlayerJoin(string playerId, string playerNickname)
        {
            if (!players.ContainsKey(playerId))
            {
                try
                {
                    GameObject joinPlayer = Instantiate(otherPlayerPrefab);

                    // in hierarchy : player ID
                    joinPlayer.name = playerId;

                    // in game : player nickname
                    joinPlayer.GetComponentInChildren<TMP_Text>().text = playerNickname;

                    joinPlayer.transform.position = Vector3.zero;
                    joinPlayer.transform.rotation = Quaternion.identity;
                    joinPlayer.SetActive(true);

                    joinPlayer.transform.SetParent(otherPlayers.transform);

                    players.Add(playerId, joinPlayer);
                    Debug.Log("Players : " + players);
                }
                catch (Exception e)
                {
                    Debug.Log("Player Join Failed.\n Error Message : " + e);
                    if (players.ContainsKey(playerId))
                    {
                        players.Remove(playerId);
                    }
                }
                
                yield return null;
            }
        }

        
        public IEnumerator PlayerLeave(string playerId)
        {

            // check player dictionary 
            bool isRegistered = players.ContainsKey(playerId);

            // check hirarchy object
            GameObject leavePlayer = otherPlayers.transform.Find(playerId).gameObject;

            if (isRegistered) players.Remove(playerId);
            if (leavePlayer != null) Destroy(leavePlayer);

            yield return null;
        }

        public IEnumerator PlayerMove(string playerId, Vector3 position, Vector3 rotation)
        {
            if (players.TryGetValue(playerId, out GameObject player))
            {
                position = player.transform.TransformDirection(position);
                player.GetComponent<CharacterController>().Move(position);

                player.transform.eulerAngles = rotation;
            }

            // if player list has no playerid 5 times -> request new player list to server

            yield return null;
        }
        
    }

}
