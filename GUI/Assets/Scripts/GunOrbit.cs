using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunOrbit : MonoBehaviour {    
    public GameObject cameraObj;
    public GameObject playerObj;
    [HideInInspector]
    public GameObject weaponObj;
    private Transform myTransform;
    
    private float dampSpeed = 0.01f;
    private float dampVelocity1 = 0.0f;
    private float dampVelocity2 = 0.0f;
    private float dampVelocity6 = 0.0f;
    //angle target vectors
    private Vector3 targetRotation;
    private Vector3 targetRotation2;
    private float targetRotationRoll = 0.0f;
    //to manage Z axis
    private float zAxis1 = 0.0f;
    private float zAxis2 = 0.0f;
    //passed on to ironsights script to sway gun
    private float localSide = 0.0f;
    private float localRaise = 0.0f;
    private float swingAmt = 0.035f;
    private float swingSpeed = 9.0f;
    //rolling of weapon
    private float localRoll = 0.0f;
    private float rollSpeed = 0.0f;
    //枪械摆动角度
    private float gunBobRoll = 10.0f;
    private float gunBobYaw = 16.0f;
    //面板上用于调整枪械摆动的变量，规范在0-1之间
    public float swayAmount = 1.0f;
    public float rollSwayAmount = 1.0f;
    public float walkBobYawAmount = 1.0f;
    public float walkBobRollAmount = 1.0f;
    public float sprintBobYawAmount = 1.0f;
    public float sprintBobRollAmount = 1.0f;

    void Start()
    {
        myTransform = transform;
        //将数据规范化在0-1之间
        walkBobYawAmount = Mathf.Clamp01(walkBobYawAmount);
        walkBobRollAmount = Mathf.Clamp01(walkBobRollAmount);
        sprintBobYawAmount = Mathf.Clamp01(sprintBobYawAmount);
        sprintBobRollAmount = Mathf.Clamp01(sprintBobRollAmount);
        gunBobRoll *= walkBobRollAmount;
        gunBobYaw *= walkBobYawAmount;
    }

    void Update()
    {

        if (Time.timeScale > 0 && Time.deltaTime > 0)
        {//非暂停状态
            FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
            ShootSight shootSight = playerObj.GetComponent<ShootSight>();
            Weapon weapon = shootSight.gunObj.GetComponent<Weapon>();
            //HorizontalBob HorizontalBob = playerObj.GetComponent<HorizontalBob>();
            PlayerControl FPSPlayerComponent = playerObj.GetComponent<PlayerControl>();

            if (FPSWalkerComponent.sprintActive)
            {//冲刺状态

                swingAmt = 0.02f * swayAmount * weapon.swayAmountUnzoomed;
                swingSpeed = 0.000025f * swayAmount * weapon.swayAmountUnzoomed;
                rollSpeed = 0.0f;

                //进行平滑调整，因为状态可能在冲刺和行走之间进行切换
                if (gunBobYaw < 12.0f * sprintBobYawAmount) { gunBobYaw += 60.0f * Time.deltaTime; }
                if (gunBobRoll < 15.0f * sprintBobRollAmount) { gunBobRoll += 60.0f * Time.deltaTime; }

            }
            else
            {//正常行走状态
             //smoothly change bobbing amounts for walking
                if (gunBobYaw > -16.0f * walkBobYawAmount) { gunBobYaw -= 60.0f * Time.deltaTime; }
                if (gunBobRoll > 10.0f * walkBobRollAmount) { gunBobRoll -= 60.0f * Time.deltaTime; }
                if (!FPSPlayerComponent.zoomed || shootSight.reloading || weapon.meleeSwingDelay != 0)
                {
                    swingAmt = 0.035f * swayAmount * weapon.swayAmountUnzoomed;
                    swingSpeed = 0.00015f * swayAmount * weapon.swayAmountUnzoomed;
                    rollSpeed = 0.025f * rollSwayAmount * weapon.swayAmountUnzoomed;
                }
                else
                {
                    swingAmt = 0.025f * swayAmount * weapon.swayAmountZoomed;
                    swingSpeed = 0.0001f * swayAmount * weapon.swayAmountZoomed;
                    rollSpeed = 0.075f * rollSwayAmount * weapon.swayAmountZoomed;
                }
            }
            zAxis1 = Camera.main.transform.localEulerAngles.z;
            zAxis2 = cameraObj.transform.localEulerAngles.z;
            //枪械目标角度要根据相机组对象的角度进行调整
            targetRotation.x = cameraObj.transform.localEulerAngles.x;
            targetRotation.y = cameraObj.transform.localEulerAngles.y;
            targetRotation.z = cameraObj.transform.localEulerAngles.z;

            //找到小的差异角乘积值
            localSide = Mathf.DeltaAngle(targetRotation2.y, targetRotation.y) * -(swingSpeed / Time.deltaTime);
            //将计算结果正规化后传递给shootsight脚本
            shootSight.side = Mathf.Clamp(localSide, -swingAmt, swingAmt);

            //同理
            localRaise = Mathf.DeltaAngle(targetRotation2.x, targetRotation.x) * (swingSpeed / Time.deltaTime);
            //将计算结果正规化后传递给shootsight脚本
            shootSight.raise = Mathf.Clamp(localRaise, -swingAmt, swingAmt);

            //逐渐插值达到指定角度
            localRoll = Mathf.LerpAngle(localRoll, Mathf.DeltaAngle(targetRotationRoll, targetRotation.y) * -rollSpeed * 3.0f, Time.deltaTime * 5.0f);

            //角度上平滑阻尼插值到达
            targetRotation2.x = Mathf.SmoothDampAngle(targetRotation2.x, targetRotation.x, ref dampVelocity1, dampSpeed, Mathf.Infinity, Time.deltaTime);
            targetRotation2.y = Mathf.SmoothDampAngle(targetRotation2.y, targetRotation.y, ref dampVelocity2, dampSpeed, Mathf.Infinity, Time.deltaTime);
            targetRotationRoll = Mathf.SmoothDampAngle(targetRotationRoll, targetRotation.y, ref dampVelocity6, 0.075f, Mathf.Infinity, Time.deltaTime);

            //枪械旋转角设定为计算结果
            myTransform.localEulerAngles = 
                new Vector3(targetRotation2.x, targetRotation2.y, targetRotation.z+localRoll);
        }
    }
}
