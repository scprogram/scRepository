//Ironsights.cs by Azuline Studios© All Rights Reserved
//Adjusts weapon position and bobbing speeds and magnitudes 
//for various player states like zooming, sprinting, and crouching.
using UnityEngine;
using System.Collections;

public class ShootSight : MonoBehaviour
{
    //other objects accessed by this script
    [HideInInspector]
    public GameObject playerObj;
    public GameObject weaponsBagObj;
    public GameObject CameraObj;
    //public Camera weaponCameraObj;
    //weapon object (weapon object child) set by PlayerWeapons.cs script
    [HideInInspector]
    public GameObject gunObj;
    //Var set to sprint animation time of weapon
    [HideInInspector]
    public Transform gunTransform;//this set by PlayerWeapons script to active weapon transform
    private Transform mainCameraTransform;
    private WeaponsInBags weaponsInBags;

    //weapon positioning	
    private float nextPosX = 0.0f;//weapon x position that is smoothed using smoothDamp function
    private float nextPosY = 0.0f;//weapon y position that is smoothed using smoothDamp function
    private float nextPosZ = 0.0f;//weapon z position that is smoothed using smoothDamp function
    private float zPosRecNext = 0.0f;//weapon recoil z position that is smoothed using smoothDamp function
    private float newPosX = 0.0f;//target weapon x position that is smoothed using smoothDamp function
    private float newPosY = 0.0f;//target weapon y position that is smoothed using smoothDamp function
    private float newPosZ = 0.0f;//target weapon z position that is smoothed using smoothDamp function
    private float zPosRec = 0.0f;//target weapon recoil z position that is smoothed using smoothDamp function
    private Vector3 dampVel = Vector3.zero;//velocities that are used by smoothDamp function
    private float recZDamp = 0.0f;//velocity that is used by smoothDamp function
    private Vector3 tempGunPos = Vector3.zero;

    //camera FOV handling
    public float defaultFov = 75.0f;//default camera field of view value
    public float sprintFov = 85.0f;//camera field of view value while sprinting
    public float weaponCamFovDiff = 20.0f;//amount to subtract from main camera FOV for weapon camera FOV
    private float nextFov = 75.0f;//camera field of view that is smoothed using smoothDamp
    private float newFov = 75.0f;//camera field of view that is smoothed using smoothDamp
    private float FovSmoothSpeed = 0.15f;//speed that camera FOV is smoothed
    private float dampFOV = 0.0f;//target weapon z position that is smoothed using smoothDamp function

    //zooming
    public enum zoomType
    {
        hold,
        toggle,
        both
    }
    public zoomType zoomMode = zoomType.both;
    public float zoomSensitivity = 0.5f;//percentage to reduce mouse sensitivity when zoomed
    public AudioClip sightsUpSnd;
    public AudioClip sightsDownSnd;
    [HideInInspector]
    public bool zoomSfxState = true;//var for only playing sights sound effects once
    [HideInInspector]
    public bool reloading = false;//this variable true when player is reloading

    public bool cameraIdleBob = true;//true if camera should bob slightly up and down when idle to simulate player breathing 
    public bool cameraSwimBob = true;//true if camera should bob slightly up and down when player is swimming
    //gun X position amount for tweaking ironsights position
    private float horizontalGunPosAmt = -0.02f;
    private float weaponSprintXPositionAmt = 0.0f;

    private float gunup = 0.015f;//amount to move weapon up while sprinting
    private float gunRunSide = 1.0f;//to control horizontal bobbing of weapon during sprinting
    private float gunRunUp = 1.0f;//to control vertical bobbing of weapon during sprinting
    private float sprintBob = 0.0f;//to modify weapon bobbing speeds when sprinting
    private float sprintBobAmtX = 0.0f;//actual horizontal weapon bobbing speed when sprinting
    private float sprintBobAmtY = 0.0f;//actual vertical weapon bobbing speed when sprinting
                                       //weapon positioning
    private float yDampSpeed = 0.0f;//this value used to control speed that weapon Y position is smoothed
    private float zDampSpeed = 0.0f;//this value used to control speed that weapon Z position is smoothed
    private float bobDir = 0.0f;//positive or negative direction of bobbing
    private float bobMove = 0.0f;
    private float sideMove = 0.0f;
    [HideInInspector]
    public float switchMove = 0.0f;//for moving weapon down while switching weapons
    private float jumpmove = 0.0f;//for moving weapon down while jumping
    [HideInInspector]
    public float jumpAmt = 0.0f;
    private float idleX = 0.0f;//amount of weapon movement when idle
    private float idleY = 0.0f;
    [HideInInspector]
    public float side = 0.0f;//amount to sway weapon position horizontally
    [HideInInspector]
    public float raise = 0.0f;//amount to sway weapon position vertically
    [HideInInspector]
    public float gunAnimTime = 0.0f;

    void OnEnable()
    {
        playerObj = gameObject;
        weaponsInBags = weaponsBagObj.GetComponent<WeaponsInBags>();
        gunObj = weaponsInBags.weaponsCarried[weaponsInBags.GetCurrentWeaponNum()];
        gunTransform = gunObj.transform;
        mainCameraTransform = Camera.main.transform;
    }

    void Update()
    {
        MouseLook mouseLook = CameraObj.GetComponent<MouseLook>();
        FPSRigidBodyWalker FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
        Weapon weapon = gunObj.GetComponent<Weapon>();
        PlayerControl playerControl = playerObj.GetComponent<PlayerControl>();

        if (Time.timeScale > 0 && Time.deltaTime > 0)
        {//allow pausing by setting timescale to 0

            //main weapon position smoothing happens here
            newPosX = Mathf.SmoothDamp(newPosX, nextPosX, ref dampVel.x, yDampSpeed, Mathf.Infinity, Time.deltaTime);
            newPosY = Mathf.SmoothDamp(newPosY, nextPosY, ref dampVel.y, yDampSpeed, Mathf.Infinity, Time.deltaTime);
            newPosZ = Mathf.SmoothDamp(newPosZ, nextPosZ, ref dampVel.z, zDampSpeed, Mathf.Infinity, Time.deltaTime);
            zPosRec = Mathf.SmoothDamp(zPosRec, zPosRecNext, ref recZDamp, 0.25f, Mathf.Infinity, Time.deltaTime);//smooth recoil kick back of weapon
            newFov = Mathf.SmoothDamp(Camera.main.fieldOfView, nextFov, ref dampFOV, FovSmoothSpeed, Mathf.Infinity, Time.deltaTime);//smooth camera FOV
            Camera.main.fieldOfView = newFov;
            //Get input from player movement script
            float horizontal = FPSWalker.inputX;//玩家对象移动值
            float vertical = FPSWalker.inputY;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Adjust weapon position and bobbing amounts dynamicly based on movement and player states
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //move weapon back towards camera based on kickBack amount in WeaponBehavior.cs
            					
            if (weapon&&weapon.shootStartTime + 0.1f > Time.time)
            {
                if (playerControl.zoomed)
                {
                    zPosRecNext = weapon.kickBackAmtZoom;
                }
                else
                {
                    zPosRecNext = weapon.kickBackAmtUnzoom;
                }
            }
            else
            {
                zPosRecNext = 0.0f;
            }


            if (Mathf.Abs(horizontal) != 0
            || (( (Input.GetKey(playerControl.moveForward) || Input.GetKey(playerControl.moveBack)))
            ||  Mathf.Abs(vertical) > 0.1f))
            {
                idleY = 0;
                idleX = 0;
                //check for sprinting
                if (FPSWalker.sprintActive
                && !playerControl.zoomed
                && !FPSWalker.crouched
                && FPSWalker.midPos >= FPSWalker.standingCamHeight//player might not have completely stood up yet from crouch
                && !((Mathf.Abs(horizontal) != 0.0f) && (Mathf.Abs(vertical) < 0.75f))
                && !FPSWalker.cancelSprint)
                {

                    sprintBob = 128.0f;

                    if (!FPSWalker.cancelSprint
                    && !reloading
                    && !FPSWalker.jumping
                    && FPSWalker.fallingDistance < 0.75f)
                    {//actually sprinting now
                     //set the camera's fov back to normal if the player has sprinted into a wall, but the sprint is still active
                        if (FPSWalker.inputY != 0)
                        {
                            nextFov = sprintFov;
                        }
                        else
                        {
                            nextFov = defaultFov;
                        }
                        //gradually move weapon more towards center while sprinting
                        weaponSprintXPositionAmt = Mathf.MoveTowards(weaponSprintXPositionAmt, weapon.weaponSprintXPosition, Time.deltaTime * 16);
                        horizontalGunPosAmt = weapon.weaponUnzoomXPosition + weaponSprintXPositionAmt;
                        gunRunSide = 2.0f;
                        if (gunRunUp < 1.4f) { gunRunUp += Time.deltaTime / 4.0f; }//gradually increase for smoother transition
                        bobMove = gunup + weapon.weaponSprintYPosition;//raise weapon while sprinting
                    }
                    else
                    {//not sprinting
                        nextFov = defaultFov;
                        horizontalGunPosAmt = weapon.weaponUnzoomXPosition;
                        gunRunSide = 1.0f;
                        gunRunUp = 1.0f;
                        bobMove = -0.01f;
                        switchMove =-0.4f;
                    }
                }
                else
                {//walking
                    gunRunSide = 1.0f;
                    gunRunUp = 1.0f;
                    //reset horizontal weapon positioning var and make sure it returns to zero when not sprinting to prevent unwanted side movement
                    weaponSprintXPositionAmt = Mathf.MoveTowards(weaponSprintXPositionAmt, 0, Time.deltaTime * 16);
                    horizontalGunPosAmt = weapon.weaponUnzoomXPosition + weaponSprintXPositionAmt;
                    if (reloading)
                    {//move weapon position up when reloading and moving for full view of animation
                        nextFov = defaultFov;
                        sprintBob = 216;
                        bobMove = 0.0F;
                        sideMove = -0.0f;
                    }
                    else
                    {
                        nextFov = defaultFov;
                        if (playerControl.zoomed && weapon.meleeSwingDelay == 0)
                        {//开启瞄准且是非近战武器
                            sprintBob = 96.0f;
                            if (Mathf.Abs(horizontal) != 0 || Mathf.Abs(vertical) > 0.75f)
                            {
                                bobMove = -0.001f;//move weapon down
                            }
                            else
                            {
                                bobMove = 0.0F;//move weapon to idle
                            }
                        }
                        else
                        {//非瞄准状态
                            sprintBob = 216.0f;
                            if (Mathf.Abs(horizontal) != 0 || Mathf.Abs(vertical) > 0.75f)
                            {
                                //move weapon down and left when crouching
                                if (FPSWalker.crouched || FPSWalker.midPos < FPSWalker.standingCamHeight * 0.85f)
                                {
                                    bobMove = -0.01f;
                                    sideMove = -0.0125f;
                                }
                                else
                                {
                                    bobMove = -0.005f;
                                    sideMove = -0.00f;
                                }
                            }
                            else
                            {
                                //move weapon to idle
                                bobMove = 0.0F;
                                sideMove = 0.0F;
                            }
                        }
                    }
                }
            }
            else
            {//没有移动
                nextFov = defaultFov;
                horizontalGunPosAmt = weapon.weaponUnzoomXPosition;
                if (weaponSprintXPositionAmt > 0) { weaponSprintXPositionAmt -= Time.deltaTime / 4; }
                sprintBob = 96.0f;
                if (reloading)
                {
                    nextFov = defaultFov;
                    sprintBob = 96.0f;
                    bobMove = 0.0F;
                    sideMove = -0.0f;
                }
                else
                {
                    //move weapon to idle
                    if ((FPSWalker.crouched || FPSWalker.midPos < FPSWalker.standingCamHeight * 0.85f) && !playerControl.zoomed)
                    {
                        bobMove = -0.005f;
                        sideMove = -0.0125f;
                    }
                    else
                    {
                        bobMove = 0.0f;
                        sideMove = 0.0f;
                    }
                }
                //weapon idle motion
                if (playerControl.zoomed && weapon.meleeSwingDelay == 0)
                {
                    idleX = Mathf.Sin(Time.time * 1.25f) / 4800.0f;
                    idleY = Mathf.Sin(Time.time * 1.5f) / 4800.0f;
                }
                else
                {
                    if (!FPSWalker.swimming)
                    {
                        idleX = Mathf.Sin(Time.time * 1.25f) / 800.0f;
                        idleY = Mathf.Sin(Time.time * 1.5f) / 800.0f;
                    }
                    else
                    {
                        idleX = Mathf.Sin(Time.time * 1.25f) / 400.0f;
                        idleY = Mathf.Sin(Time.time * 1.5f) / 400.0f;
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Weapon Swaying/Bobbing while moving
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (!FPSWalker.canRun)
            {
                sprintBobAmtX = sprintBob / weapon.walkBobAmountX;
                sprintBobAmtY = sprintBob / weapon.walkBobAmountY;
            }
            else
            {
                sprintBobAmtX = sprintBob / weapon.sprintBobAmountX;
                sprintBobAmtY = sprintBob / weapon.sprintBobAmountY;
            }

            //set smoothed weapon position to actual gun position vector
            tempGunPos.x = newPosX;
            tempGunPos.y = newPosY;
            tempGunPos.z = newPosZ + zPosRec;//add weapon z position and recoil kick back
                                             //apply temporary vector to gun's transform position
            gunTransform.localPosition = tempGunPos;

            //lower weapon when jumping, falling, or slipping off ledge
            if (FPSWalker.jumping || FPSWalker.fallingDistance > 1.25f)
            {
                //lower weapon less when zoomed
                if (!playerControl.zoomed)
                {
                    //raise weapon when jump is ascending and lower when descending
                    if ((FPSWalker.airTime + 0.175f) > Time.time)
                    {
                        jumpmove = 0.015f;
                    }
                    else
                    {
                        jumpmove = -0.025f;
                    }
                }
                else
                {
                    jumpmove = -0.01f;
                }
            }
            else
            {
                jumpmove = 0.0f;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Adjust vars for zoom and other states
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            float deltaAmount = Time.deltaTime * 100;//define delta for framerate independence
            float bobDeltaAmount = 0.12f / Time.deltaTime;//define bobbing delta for framerate independence

            if (!weapon.PistolSprintAnim || !FPSWalker.canRun)
            {
                gunAnimTime = gunObj.GetComponent<Animation>()["RifleSprinting"].normalizedTime;//Track playback position of rifle sprinting animation
            }
            else
            {
                gunAnimTime = gunObj.GetComponent<Animation>()["PistolSprinting"].normalizedTime;//Track playback position of pistol sprinting animation	
            }

            //if zoomed
            //check time of weapon sprinting anim to make weapon return to center, then zoom normally 
            if (playerControl.zoomed
            && playerControl.currentHitPoints > 1.0f
            && weaponsInBags.switchTime + weapon.readyTime < Time.time//don't raise sights when readying weapon 
            && !reloading
            && gunAnimTime < 0.35f
            && weapon.meleeSwingDelay == 0//not a melee weapon
            && weaponsInBags.GetCurrentWeaponNum() != 0
            && weapon.reloadLastStartTime + weapon.reloadLastTime < Time.time)
            {
                //adjust FOV and weapon position for zoom
                nextFov = weapon.zoomFOV;
                FovSmoothSpeed = 0.09f;//faster FOV zoom speed when zooming in
                yDampSpeed = 0.09f;
                zDampSpeed = 0.15f;
                //X pos with idle movement
                nextPosX = weapon.weaponZoomXPosition + (side / 1.5f) + idleX;
                nextPosY = weapon.weaponZoomYPosition + (raise / 1.5f) + idleY + (switchMove + jumpAmt + jumpmove);
                
                nextPosZ = weapon.weaponZoomZPosition;
                //slow down turning and movement speed for zoom
                FPSWalker.zoomSpeed = true;
                //If not a melee weapon, play sound effect when raising sights
                if (zoomSfxState && weapon.meleeSwingDelay == 0)// && !weapon.unarmed)
                {
                    AudioSource.PlayClipAtPoint(sightsUpSnd, mainCameraTransform.position);
                    zoomSfxState = false;
                }
            }
            else
            {//非瞄准状态

                FovSmoothSpeed = 0.18f;//slower FOV zoom speed when zooming out

                //adjust weapon Y position smoothing speed for unzoom and switching weapons
                if (!weaponsInBags.switching)
                {
                    yDampSpeed = 0.18f;//weapon swaying speed
                }
                else
                {
                    yDampSpeed = 0.2f;//weapon switch raising speed
                }
                zDampSpeed = 0.1f;
                //X pos with idle movement
                nextPosX = side + idleX + sideMove + horizontalGunPosAmt;

                if (weapon.meleeSwingDelay > 0.0f)
                    switchMove = -0.4f;
                nextPosY = raise + idleY + (switchMove + jumpAmt + jumpmove) + weapon.weaponUnzoomYPosition;
                nextPosZ = weapon.weaponUnzoomZPosition;
                //Set turning and movement speed for unzoom
                FPSWalker.zoomSpeed = false;
                //If not a melee weapon, play sound effect when lowering sights	
                if (!zoomSfxState && weapon.meleeSwingDelay == 0)
                {
                    AudioSource.PlayClipAtPoint(sightsDownSnd, mainCameraTransform.position);
                    zoomSfxState = true;
                }
                    nextPosZ = weapon.weaponSprintZPosition;

                }
            }
        }
    }