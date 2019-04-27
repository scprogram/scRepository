using UnityEngine;
using System.Collections;
using System;


[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {
    public float sensitivity = 4.0f;

	public float minimumX = -360F;//最小和最大X方向旋转角度
	public float maximumX = 360F;

	public float minimumY = -85F;
	public float maximumY = 80F;
    public float smoothSpeed = 0.35f;

    [HideInInspector]//保证会在属性面板上隐藏
    public float rotationX = 0.0f;
    [HideInInspector]
    public float rotationY = 0.0f;
    private Quaternion originalRotation;
    void Start()
    {
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
        originalRotation = transform.rotation;//保存挂在对象原旋转位置
        originalRotation.eulerAngles = new Vector3(0,Camera.main.transform.eulerAngles.y,0);
        //将原三轴旋转角设置为0度，主摄像机的y对应的角度，0度
    }

    void Update ()
	{
        Cursor.lockState = CursorLockMode.Locked;
        rotationX +=  Input.GetAxisRaw("Mouse X") * sensitivity*Time.timeScale;
		rotationY += Input.GetAxisRaw("Mouse Y") * sensitivity*Time.timeScale;//deltaTime为上一帧时间，timeScale则为时间流逝速度比例
        rotationX = ClampAngle(rotationX, minimumX, maximumX);
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

        //smooth the mouse input
        transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation * xQuaternion * yQuaternion, smoothSpeed * Time.smoothDeltaTime * 60 / Time.timeScale);
        //lock mouselook roll to prevent gun rotating with fast mouse movements
        //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0.0f);
        
	}

    private float Clamp(float rotationX, float minimumX, float maximumX)
    {
        throw new NotImplementedException();
    }

    public static float ClampAngle(float angle, float min, float max){
        angle = angle % 360;       
        return Mathf.Clamp(angle, min, max);
    }
}