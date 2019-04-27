using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimMove : MonoBehaviour
{
    [HideInInspector]
    public GameObject gun;//this variable updated by PlayerWeapons script
    public GameObject playerObj;
    [HideInInspector]
    public GameObject weaponObj;
    private Transform myTransform;
    private FPSRigidBodyWalker FPSWalkerComponent;
    private PlayerControl FPSPlayerComponent;

    //camera angles
    [HideInInspector]
    public float CameraYawAmt = 0.0f;//this value is modified by animations and added to camera angles
    [HideInInspector]
    public float CameraPitchAmt = 0.0f;//this value is modified by animations and added to camera angles
    [HideInInspector]
    public float CameraRollAmt = 0.0f;//this value is modified by animations and added to camera angles
                                      //	private float timer;
                                      //	private float waveslice;
    [HideInInspector]
    public Vector3 bobAngles = new Vector3(0, 0, 0);//view bobbing angles are sent here from the HeadBob script
    private float returnSpeed = 4.0f;//speed that camera angles return to neutral
                                     //to move gun and view down slightly on contact with ground
    private bool landState = false;
    private float landStartTime = 0.0f;
    private float landElapsedTime = 0.0f;
    private float landTime = 0.35f;
    private float landAmt = 20.0f;
    private float landValue = 0.0f;
    //weapon position
    private float gunDown = 0.0f;
    [HideInInspector]
    public float dampOriginX = 0.0f;//Player X position is passed from the GunBob script
    [HideInInspector]
    public float dampOriginY = 0.0f;//Player Y position is passed from the HeadBob script
                                    //camera position vars
    private Vector3 tempLerpPos;
    private Vector3 dampVel;
    private float lerpSpeed;
    private Transform playerObjTransform;
    private Transform mainCameraTransform;

    void Start()
    {
        myTransform = transform;//store this object's transform for optimization
        playerObjTransform = playerObj.transform;
        mainCameraTransform = Camera.main.transform;
        //define external script references
        FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
        FPSPlayerComponent = playerObj.GetComponent<PlayerControl>();
    }

    void LateUpdate()
    {

        if (Time.timeScale > 0 && Time.deltaTime > 0)
        {//allow pausing by setting timescale to 0
            if (!GetComponent<Animation>().isPlaying)//如果没有播放动画则不用调整相机角度
            {
                CameraPitchAmt = 0.0f;
                CameraYawAmt = 0.0f;
                CameraRollAmt = 0.0f;
            }

            Vector3 tempPosition = tempLerpPos + (playerObjTransform.right * dampOriginX) + new Vector3(0.0f, dampOriginY, 0.0f);

            if (Time.timeSinceLevelLoad < 1) { returnSpeed = 32.0f; } else { returnSpeed = 4.0f; };
            //apply a force to the camera that returns it to neutral angles (Quaternion.identity) over time after being changed by code or by animations
            myTransform.localRotation = Quaternion.Slerp(myTransform.localRotation, Quaternion.identity, Time.deltaTime * returnSpeed);

            //根据动画计算相机的实际动态转角
            Vector3 tempCamAngles = new Vector3(mainCameraTransform.localEulerAngles.x - bobAngles.x + (CameraPitchAmt * Time.deltaTime * 75.0f),
                                                mainCameraTransform.localEulerAngles.y + bobAngles.y + (CameraYawAmt * Time.deltaTime * 75.0f),
                                                mainCameraTransform.localEulerAngles.z - bobAngles.z + (CameraRollAmt * Time.deltaTime * 75.0f));

            //应用实际相机转角，根据动画进行转动
            mainCameraTransform.localEulerAngles = tempCamAngles;
        }
    }
}
