using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowShellAmount : MonoBehaviour {

    [HideInInspector]
    public int nowShellAmount;//当前子弹容量
    [HideInInspector]
    public int backShellAmount;

    // Update is called once per frame
    void Update()
    {
        GetComponent<GUIText>().text = "子弹: "+nowShellAmount+"/"+backShellAmount;
        
    }
}
