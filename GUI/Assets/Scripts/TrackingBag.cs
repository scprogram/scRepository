using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingBag : MonoBehaviour {

    public GameObject playerObj;//实际跟随的玩家对象  
    private Transform myTransform;
    private Transform playerTransform;
    // Use this for initialization
    void Start()
    {
        myTransform = transform;
        playerTransform = playerObj.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0 && Time.deltaTime > 0)
        {
            transform.position =playerObj.transform.position;
        }
    }
}
