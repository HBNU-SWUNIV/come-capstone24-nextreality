using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NicknameSetting : MonoBehaviour
{

    public GameObject mainCam;

    Vector3 startScale;
    public float distance = 10;

    void Start()
    {
        startScale = transform.localScale;    
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCam == null)
        {
            mainCam = GameObject.FindWithTag("MainCamera");
        }


        float dist = Vector3.Distance(mainCam.transform.position, transform.position);
        Vector3 newScale = startScale * dist / distance;
        // Debug.Log("newScale : " + newScale);

        transform.localScale = (newScale.x < 4 && newScale.y < 4 && newScale.z < 4) ? newScale : new Vector3(4, 4, 4);

        transform.rotation = mainCam.transform.rotation;
    }
}
