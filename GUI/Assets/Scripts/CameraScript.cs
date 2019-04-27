using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    bool CameraState = true;//表示使用角色上的摄像机
    
    public Texture2D shooterCursor;
    // Update is called once per frame
    private void Start()
    {
        Camera.main.enabled = false;//初始情况禁用全局摄像机
        
    }
    void Update () {
	}
}
