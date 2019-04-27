//FPSRigidBodyWalker.cs by Azuline Studios© All Rights Reserved
//Manages player movement controls, sets player movement speed, plays certain sound effects 
//determines player movement state, and sets player's rigidbody velocity.
using UnityEngine;
using System.Collections;

public class FPSRigidBodyWalker : MonoBehaviour
{

    //objects accessed by this script
    
    public GameObject mainObj;
    [HideInInspector]
    public GameObject weaponObj;
    [HideInInspector]
    public GameObject CameraObj;
    [HideInInspector]
    public Transform myTransform;
    private Transform mainCamTransform;

    //track player input
    [HideInInspector]
    public float inputXSmoothed = 0.0f;//binary inputs smoothed using lerps
    [HideInInspector]
    public float inputYSmoothed = 0.0f;
    [HideInInspector]
    public float inputX = 0;//为0表示左右方向无按键按下，按左时为-1，按右时为1
    [HideInInspector]
    public float inputY = 0;//为0表示前后方向无按键按下，按前时为1，按后时为-1
    private float InputYLerpSpeed;//to allow quick deceleration when running into walls
    private float InputXLerpSpeed;

    //player movement speed amounts
    public float walkSpeed = 4.0f;
    public float sprintSpeed = 9.0f;//奔跑速度

    //sprinting
    public enum sprintType
    {//Sprint mode
        hold,
        toggle,
        both
    }
    public sprintType sprintMode = sprintType.both;
    private float sprintDelay = 0.4f;
    public bool limitedSprint = true;//true if player should run only while staminaForSprint > 0 
    public bool sprintRegenWait = true;//true if player should wait for stamina to fully regenerate before sprinting
    public float sprintRegenTime = 3.0f;//time it takes to fully regenerate stamina if sprintRegenWait is true
    private bool breathFxState;
    public float staminaForSprint = 5.0f;//duration allowed for sprinting when limitedSprint is true
    private float staminaForSprintAmt;//actual duration amt allowed for sprinting modified by scripts
    public bool catchBreathSound = true;//true if the catch breath sound effect should be played when staminaForSprint is depleted
    private bool staminaDepleted;//true when player has run out of stamina and must wait for it to regenerate if sprintRegenWait is also true

    public float jumpSpeed = 3.0f;//vertical speed of player jump
    public float climbSpeed = 4.0f;//speed that player moves vertically when climbing
    public bool lowerGunForClimb = true;//if true, gun will be lowered when climbing surfaces
    public bool lowerGunForSwim = true;//if true, gun will be lowered when swimming
    public bool lowerGunForHold = true;//if true, gun will be lowered when holding object
    [HideInInspector]
    public bool holdingObject;
    [HideInInspector]
    public bool hideWeapon;//true when weapon should be hidden from view and deactivated

    //swimming customization
    public float swimSpeed = 2.0f;//speed that player moves vertically when swimming
    public float holdBreathDuration = 15.0f;//amount of time before player starts drowning
    public float drownDamage = 7.0f;//rate of damage to player while drowning

    //player speed limits

    private float limitStrafeSpeed = 0.0f;
    public float backwardSpeedPercentage = 0.6f;//后退时速度减慢百分之60
    public float crouchSpeedPercentage = 0.5f;//蹲下时速度减慢百分之50
    private float crouchSpeedAmt = 1.0f;
    public float strafeSpeedPercentage = 0.8f; //percentage to decrease movement speed while strafing directly left or right


    private float speedAmtY = 1.0f;//X,Y方向移动输入对应权值，前进时为1，后退时为0.6，蹲下时为0.5，依次类推
    private float speedAmtX = 1.0f;//说白了就是等于后面的那些Percentage数值
    [HideInInspector]
    public bool zoomSpeed;//to control speed of movement while zoomed, handled by Ironsights script and true when zooming
    public float zoomSpeedPercentage = 0.6f;//percentage to decrease movement speed while zooming
    private float zoomSpeedAmt = 1.0f;
    private float speed;//结合按键和speedAmtX权值等信息最终得到的速度值


    //预设值，后面都要进行调整
    [HideInInspector]
    public float standingCamHeight = 0.9f;//站起时摄像机的高度
    [HideInInspector]
    public float crouchingCamHeight = 0.45f;//蹲下时摄像机的高度

    private float standingCapsuleheight = 2.0f;//预设的站起时胶囊体的高度
    private float crouchingCapsuleHeight = 1.25f;//当蹲下时胶囊体的高度要变矮
    private float crouchingHeightChange = 5.0f;//蹲下和站起的速度
    private float standingHeightChange = 2.25f;
    private float capsuleCastHeight = 0.75f;//height of capsule cast above player to check for obstacles before standing from crouch
    private float deadCapsuleHeight = 0.4f;

    public float playerHeightMod;//amount to add to player height (proportionately increases player height, radius, and capsule cast/raycast heights)


    public float crouchHeightPercentage = 0.5f;//percent of standing height to move camera to when crouching
    public int gravity = 20;//additional gravity that is manually applied to the player rigidbody
    private int maxVelocityChange = 5;//maximum rate that player velocity can change
    private Vector3 moveDirection = Vector3.zero;//最终实际的移动方向，由按键组合决定


    //grounded and slopelimit checks
    public int slopeLimit = 50;//the maximum allowed ground surface/normal angle that the player is allowed to climb
    [HideInInspector]
    public bool grounded;//胶囊体是否着陆
    private bool rayTooSteep;//true when ray from capsule origin hits surface/normal angle greater than slopeLimit, compared with capsuleTooSteep
    private bool capsuleTooSteep;//true when capsule cast hits surface/normal angle greater than slopeLimit, compared with rayTooSteep

    //player movement states
    [HideInInspector]
    public Vector3 velocity = Vector3.zero;//最终受力向量，要根据按键和所处状态为胶囊碰撞体加力
    [HideInInspector]
    public CapsuleCollider capsule;//胶囊体引用

    private Vector3 sweepPos;
    private Vector3 sweepHeight;
    private bool parentState;//only set parent once to prevent rapid parenting and de-parenting that breaks functionality
    [HideInInspector]
    public bool inWater;//当进入水体碰撞体时为true
    [HideInInspector]
    public bool holdingBreath;//true when player view/camera is under the waterline
    [HideInInspector]
    public bool belowWater;//true when player is below water movement threshold and is not treading water (camera/view slightly above waterline)
    [HideInInspector]
    public bool swimming;//是否在水中，根据水体脚本来更改
    [HideInInspector]
    public bool canWaterJump = true;//to make player release and press jump button again to jump if surfacing from water by holding jump button
    private float swimmingVerticalSpeed;
    [HideInInspector]
    public float swimStartTime;
    [HideInInspector]
    public float diveStartTime;
    [HideInInspector]
    public bool drowning;//true when player has stayed under water for longer than holdBreathDuration
    private float drownStartTime = 0.0f;

    //falling
    [HideInInspector]
    public float airTime = 0.0f;//玩家总浮空时间
    private bool airTimeState;
    public float fallingDamageThreshold = 5.5f;//Units that player can fall before taking damage
    private float fallStartLevel;//the y coordinate that the player lost grounding and started to fall
    [HideInInspector]
    public float fallingDistance;//total distance that player has fallen
    private bool falling;//true when player is losing altitude

    //climbing (ladders or other climbable surfaces)
    [HideInInspector]
    public bool climbing;//true when playing is in contact with ladder trigger or edge climbing trigger
    [HideInInspector]
    public bool noClimbingSfx;//true when playing is in contact with edge climbing trigger or ladder with false Play Climbing Audio value
    [HideInInspector]
    public float verticalSpeedAmt = 4.0f;//actual rate that player is climbing



    public float timeBetweenTwoJump = 0.35f;//两次跳跃之间必须间隔的时间
    [HideInInspector]
    public bool jumping;//是否正在跳跃
    private float jumpTimer = 0.0f;//记录跳跃时间
    private bool jumpfxstate = true;
    private bool jumpButtonEnabled = true;//当蹲下时跳跃键禁用
    [HideInInspector]
    public float landStartTime = 0.0f;//time that player landed from jump

    //sprinting
    [HideInInspector]
    public bool canRun = true;//是否可以加速跑，在水中时为false
    [HideInInspector]
    public bool sprintActive;//冲刺键按下状态

    private bool sprintButtonEnabled = true;//冲刺键使能变量
    private float sprintStartTime;//开始冲刺的时间
    private float sprintStart = -2.0f;
    private float sprintEnd;

    [HideInInspector]
    public bool getSprintSignal;//是否接收到冲刺信号，由冲刺键来调控
    [HideInInspector]
    public bool cancelSprint;//当特殊状态打断冲刺状态时
    [HideInInspector]
    public float sprintStopTime = 0.0f;

    //crouching	
    [HideInInspector]
    public float midPos = 0.9f;//camera vertical position which is passed to VerticalBob.cs and HorizontalBob.cs
    [HideInInspector]
    public bool crouched;//当蹲下时为true,站起来时为false
    

    //音效
    public AudioClip jumpSound;//起跳音效
    public AudioClip landfx;//跳跃着陆音效
    private bool playingLandFx=false;//正在播放着陆音效

    public LayerMask clipMask;//mask for reducing the amount of objects that ray and capsule casts have to check
    
    void Start()
    {
        GetComponent<Rigidbody>().freezeRotation = true;
        GetComponent<Rigidbody>().useGravity = true;//启用重力
        capsule = GetComponent<CapsuleCollider>();//得到胶囊碰撞体
          //mainObj = gameObject;
        myTransform = transform;
        mainCamTransform = Camera.main.transform;//得到主摄像机的transform组件


        //clamp movement modifier percentages
        Mathf.Clamp01(backwardSpeedPercentage);
        Mathf.Clamp01(crouchSpeedPercentage);
        Mathf.Clamp01(strafeSpeedPercentage);
        Mathf.Clamp01(zoomSpeedPercentage);

        staminaForSprintAmt = staminaForSprint;//initialize sprint duration counter


        //对胶囊体的实际高度进行调整
        capsule.height = 1.2f+ playerHeightMod;
        capsule.radius = capsule.height * 0.25f;


        //initilize capsule heights
        standingCapsuleheight = 1.2f + playerHeightMod;
        crouchingCapsuleHeight = crouchHeightPercentage * standingCapsuleheight;
        deadCapsuleHeight += standingCapsuleheight/5;
        //站起的相机高度等于胶囊体高度
        standingCamHeight = standingCapsuleheight;
        //蹲下时的相机高度等于站起时的相机高度乘以百分比
        crouchingCamHeight = crouchHeightPercentage * standingCamHeight;

        crouchingHeightChange = (5.0f + playerHeightMod);//蹲下和站起的速度
        standingHeightChange = (2.25f + playerHeightMod);
        //initialize rayCast and capsule cast heights
        if (playerHeightMod > 2.0f)
        {
            //adjust capsule cast height for playerHeightMod amount for better grounded detection and jump timing
            capsuleCastHeight = 0.25f + playerHeightMod / 2;
        }
        else
        {
            capsuleCastHeight = 0.75f + playerHeightMod / 2;
        }

        //scale up jump speed to height addition made by playerHeightMod
        jumpSpeed = jumpSpeed / (1 - (playerHeightMod / capsule.height));

    }

    void Update()
    {
        //得到角色控制组件
        PlayerControl playerControl = GetComponent<PlayerControl>();


        Vector3 p1 = myTransform.position;//胶囊体底部的位置
        Vector3 p2 = p1 + Vector3.up * capsule.height / 2;
        //得到胶囊体顶部的位置
        //如果蹲下键被按下，且没在游泳和攀爬
        if (Input.GetKeyDown(playerControl.crouch) && !swimming && !climbing)
        {
            //此时不在蹲下状态
            //说白了只能使用蹲键起身之后才能跳跃，蹲下状态中不能跳跃
            if (!crouched)//如果不处在已蹲下状态
            {
                crouched = true;//蹲下
                sprintButtonEnabled = false;//禁用冲刺
                jumpButtonEnabled = false;//禁用跳跃键
            }
            else//如果crouched为true表示已经蹲下，那么再按一次crounch键要站起来，当然要有位置站起来
            {//查看此位置是否有位置能容纳胶囊碰撞体,返回值为false表示没有物体和站起来的胶囊体碰撞，则可以
             //取消蹲姿
                if (!Physics.CheckCapsule(p1, p2 + Vector3.up * (standingCapsuleheight - crouchingCapsuleHeight),
                    capsule.radius, clipMask.value))
                {//如果则不能蹲下
                    crouched = false;
                    jumpButtonEnabled = true;//启用跳跃键
                    sprintButtonEnabled = true;//启用冲刺键
                }
            }
        }

        else
        {//攀爬或者游泳都不能蹲下
            if ((climbing || swimming) && !Physics.CheckCapsule(p1, p2 + (Vector3.up * 0.3f), capsule.radius, clipMask.value))
            {//取消蹲下状态如果没有位置蹲下
                crouched = false;
                sprintButtonEnabled = true;//启用冲刺键
                jumpButtonEnabled = true;//启用跳跃键
            }
        }
    }

    void FixedUpdate()
    {
        RaycastHit rayHit;
        RaycastHit capHit;
        RaycastHit hit2;


        MouseLook MouseLook = CameraObj.GetComponent<MouseLook>();
        PlayerControl playerControl = GetComponent<PlayerControl>();
        //Footsteps FootstepsComponent = GetComponent<Footsteps>();
        WeaponsInBags weaponsInBags = weaponObj.GetComponent<WeaponsInBags>();
        //WeaponBehavior WeaponBehaviorComponent = PlayerWeaponsComponent.weaponOrder[PlayerWeaponsComponent.currentWeapon].GetComponent<WeaponBehavior>();

        if (Time.timeScale > 0)//不是处在暂停状态
        {

            //同理玩家胶囊碰撞体的底部和顶部
            Vector3 p1 = myTransform.position;
            Vector3 p2 = p1 + Vector3.up * capsule.height / 2;
            //如果在地面上
            if (grounded)
            {
                //move bottom of frontal CapsuleCast higher than bottom of player capsule to allow climbing up stairs
                sweepPos = myTransform.position + Vector3.up * (0.1f + (playerHeightMod * 0.05f));//keep sweepPos in proportion with playerHeightMod (original capsule height must be 2.0f)
                sweepHeight = myTransform.position + Vector3.up * (0.75f + (playerHeightMod * 0.375f));
            }
            else
            {
                sweepPos = myTransform.position - Vector3.up * (0.2f + (playerHeightMod * 0.1f));
                sweepHeight = myTransform.position + Vector3.up * (capsule.height * 1.2f + (playerHeightMod * 0.6f));
            }


            velocity = GetComponent<Rigidbody>().velocity;//胶囊体碰撞体的速度
            ////////////////////////////////////////////////////////////////////////////////
            //玩家输入调整
            ////////////////////////////////////////////////////////////////////////////////////////////////////


            //血量大于1.0，还活着
            if (!playerControl.getDead())
            {
                //在玩家前面一点设置一个胶囊体来进行碰撞检测，如果前面有碰撞体则阻止玩家前进
                //Sweep a capsule in front of player to detect walls or other obstacles and stop Y input if there is a detected
                //collision to prevent player capsule from overlapping into world collision geometry in between fixed updates. 
                //This allows smoother jumping over obstacles when the player is walking into them. 
                if (!Physics.CapsuleCast(sweepPos, sweepHeight, capsule.radius * 0.5f, myTransform.forward,
                    out hit2, 0.4f + (playerHeightMod * 0.2f), clipMask.value) || climbing)
                {//前面没有阻拦的碰撞体

                    //decrease y input lerp speed to allow the player to slowly come to rest when forward button is pressed
                    if (!swimming)//不是游泳状态
                    {
                        InputYLerpSpeed = 6.0f;
                        InputXLerpSpeed = 6.0f;
                    }
                    else//是游泳状态则使得X,Y方向移动速度变慢
                    {
                        InputYLerpSpeed = 3.0f;
                        InputXLerpSpeed = 3.0f;
                    }
                }
                else
                {
                    if ((!hit2.rigidbody && grounded))
                    {//如果前面有碰撞体并且在地面上，那么使得角色停止
                        inputY = 0;

                        InputYLerpSpeed = 128.0f;
                    }
                }

                //按前没按后表示向前
                if (Input.GetKey(playerControl.moveForward) && !Input.GetKey(playerControl.moveBack)) { inputY = 1; }
                //按后没按前表示向后
                if (Input.GetKey(playerControl.moveBack) && !Input.GetKey(playerControl.moveForward)) { inputY = -1; }
                //同时按或者同时没按都表示前后方向上没动
                if (!Input.GetKey(playerControl.moveBack) && !Input.GetKey(playerControl.moveForward)) { inputY = 0; }
                if (Input.GetKey(playerControl.moveBack) && Input.GetKey(playerControl.moveForward)) { inputY = 0; }


                if (Input.GetKey(playerControl.strafeLeft) && !Input.GetKey(playerControl.strafeRight)) { inputX = -1; }
                if (Input.GetKey(playerControl.strafeRight) && !Input.GetKey(playerControl.strafeLeft)) { inputX = 1; }
                if (!Input.GetKey(playerControl.strafeLeft) && !Input.GetKey(playerControl.strafeRight)) { inputX = 0; }
                if (Input.GetKey(playerControl.strafeLeft) && Input.GetKey(playerControl.strafeRight)) { inputX = 0; }

                //Lerp是线性插值函数，从inputXSmoothed到inputX之间插值,每次插入的值为deltaTime*InputXLerpSpeed
                //显然inputXSmoothed应该为0
                inputXSmoothed = Mathf.Lerp(inputXSmoothed, inputX, Time.deltaTime * InputXLerpSpeed);
                inputYSmoothed = Mathf.Lerp(inputYSmoothed, inputY, Time.deltaTime * InputYLerpSpeed);


            }


            //看是否在游泳，攀爬使得武器被收起
            if ((holdingBreath && lowerGunForSwim) || (climbing && lowerGunForClimb) || (holdingObject && lowerGunForHold))
            {
                hideWeapon = true;
            }
            else
            {
                hideWeapon = false;//否则拿出武器
            }





            //所有地面活动分类
            if (grounded)
            {
                //reset airTimeState var so that airTime will only be set once when player looses grounding
                airTimeState = true;

                if (falling)
                {//正在下落

                    fallingDistance = 0;//下落距离设置为0
                    landStartTime = Time.time;//track the time when player landed
                    falling = false;

                    if ((fallStartLevel - myTransform.position.y) > 2.0f)
                    {
                        //play landing sound effect when falling and not landing from jump
                        if (!jumping)
                        {
                            if (!inWater&&!playingLandFx)
                            {
                                //播放着陆音效
                                AudioSource.PlayClipAtPoint(landfx, mainCamTransform.position);
                                playingLandFx = true;
                            }

                            //相机也要播放相应动画
                            //make camera jump when landing for better feeling of player weight	
                            if (Camera.main.GetComponent<Animation>().IsPlaying("CameraLand"))
                            {
                                Debug.Log("land");
                                //rewind animation if already playing to allow overlapping playback
                                Camera.main.GetComponent<Animation>().Rewind("CameraLand");
                            }
                            Camera.main.GetComponent<Animation>().CrossFade("CameraLand", 0.35f, PlayMode.StopAll);
                        }
                    }

                    //跳得太高掉下来要计算扣血
                    if (myTransform.position.y < fallStartLevel - fallingDamageThreshold && !inWater)
                    {
                        CalculateFallingDamage(fallStartLevel - myTransform.position.y);
                    }
                }
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Sprinting
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //冲刺模式有三种，不用管
                switch (sprintMode)
                {
                    case sprintType.both://两种模式都可以冲刺
                        sprintDelay = 0.4f;
                        break;
                    case sprintType.hold://按住冲刺
                        sprintDelay = 0.0f;
                        break;
                    case sprintType.toggle://重复点击冲刺
                        sprintDelay = 999.0f;
                        break;
                }

                //前进时按下冲刺键即可冲刺，除非再按一次冲刺键取消冲刺或者松开前进键
                if (inputY> 0)//必须前进才能冲刺后退不能冲刺的
                {//冲刺键生效的前提是有向前方向上的位移量才能进一步冲刺，否则只按冲刺键是无效操作
                    if (Input.GetKey(playerControl.sprint))//冲刺键按下
                    {
                        //没有得到冲刺信号
                        if (!getSprintSignal && sprintButtonEnabled)//不在冲刺状态且冲刺键可用
                        {//冲刺信号要配合冲刺按键使能使用
                            sprintStart = Time.time;//开始冲刺的时间
                            getSprintSignal = true;//得到冲刺信号变量为true
                            if (sprintEnd - sprintStart < sprintDelay * Time.timeScale)
                            //间隔小于0.4s的冲刺键
                            {//冲刺键反复按下的时候要切换冲刺状态
                                if (!sprintActive)
                                {
                                    sprintActive = true;
                                }
                                else
                                {
                                    sprintActive = false;
                                }
                            }
                        }
                    }
                    else//冲刺键松开
                    {
                        if (getSprintSignal)//在冲刺则需要取消冲刺
                        {
                            sprintEnd = Time.time;//记录冲刺结束的时间
                            getSprintSignal = false;//松开冲刺键时没有接受到冲刺信号了
                            if (sprintEnd - sprintStart > sprintDelay * Time.timeScale)
                            {
                                sprintActive = false;//松开冲刺键之后维持delay时间之后
                            }
                        }
                    }
                }
                else
                {
                    if (!Input.GetKey(playerControl.sprint))//冲刺键未按下
                    {//未冲刺
                        sprintActive = false;
                    }
                }


                //某些场合要取消冲刺状态

                //reset cancelSprint var so it has to pressed again to sprint
                if (!sprintActive && cancelSprint)
                {
                    if (!Input.GetKey(playerControl.zoom))
                    {
                        cancelSprint = false;
                    }
                }

                //determine if stamina has been fully depleted and set staminaDepleted to true
                //to disable sprinting until stamina fully regenerates, if sprintRegenWait is true
                if (limitedSprint && staminaForSprintAmt <= 0.0f)
                {
                    staminaDepleted = true;
                }


                if (((Input.GetKey(playerControl.moveForward)) || (Mathf.Abs(inputY) > 0.1f))
                && sprintActive
                && !crouched
                && !cancelSprint
                && grounded)
                {
                    canRun = true;
                    playerControl.zoomed = false;//cancel zooming when sprinting

                    if (staminaForSprintAmt > 0.0f && limitedSprint)
                    {
                        staminaForSprintAmt -= Time.deltaTime;//reduce stamina when sprinting
                    }
                }
                else
                {
                    canRun = false;
                    if (limitedSprint)
                    {
                        if (sprintRegenWait)
                        {//determine if player should not be allowed to run unless they have full stamina
                            if (!staminaDepleted)
                            {
                                if (staminaForSprintAmt < staminaForSprint)
                                {
                                    staminaForSprintAmt += Time.deltaTime/* * 1.1f*/;//recover stamina when not sprinting (multiply this by a value to increase recover rate) 
                                }
                            }
                            else
                            {//stamina fully depleted, wait for it to regenerate before allowing player to sprint again
                                if (sprintStopTime + sprintRegenTime < Time.time)
                                {
                                    staminaForSprintAmt = staminaForSprint;//recover full stamina when not sprinting and sprintRegenTime has elapsed
                                    staminaDepleted = false;
                                }
                            }
                        }
                        else
                        {//option to allow player to run as soon as any stamina amount has regenerated 
                            if (staminaForSprintAmt < staminaForSprint)
                            {
                                staminaForSprintAmt += Time.deltaTime/* * 1.1f*/;//recover stamina when not sprinting (multiply this by a value to increase recover rate) 
                            }
                        }
                        breathFxState = false;
                    }
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Player Movement Speeds
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //均是将速度设定稳定在对应状态下的速度值，比如游泳速度，奔跑速度
                if (canRun)
                {//如果跟该速度有偏差就进行调整
                    if (speed < sprintSpeed - 0.1f)
                    {
                        speed += 16 * Time.deltaTime;
                    }
                    else if (speed > sprintSpeed + 0.1f)
                    {
                        speed -= 16 * Time.deltaTime;
                    }
                }
                else
                {
                    if (!swimming)
                    {
                        if (speed > walkSpeed + 0.1f)
                        {
                            speed -= 16 * Time.deltaTime;
                        }
                        else if (speed < walkSpeed - 0.1f)
                        {
                            speed += 16 * Time.deltaTime;
                        }
                    }
                    else//在游泳状态
                    {
                        if (speed > swimSpeed + 0.1f)
                        {
                            speed -= 16 * Time.deltaTime;//将速度稳定到标准游泳速度
                        }
                        else if (speed < swimSpeed - 0.1f)
                        {
                            speed += 16 * Time.deltaTime;//同理
                        }
                    }
                }

                //check if player is zooming and set speed 
                if (zoomSpeed)
                {
                    if (zoomSpeedAmt > zoomSpeedPercentage)
                    {
                        zoomSpeedAmt -= Time.deltaTime;//gradually decrease zoomSpeedAmt to zooming limit value
                    }
                }
                else
                {
                    if (zoomSpeedAmt < 1.0f)
                    {
                        zoomSpeedAmt += Time.deltaTime;//gradually increase zoomSpeedAmt to neutral
                    }
                }

                //check that player can crouch and set speed
                //also check midpos because player can still be under obstacle when crouch button is released 
                if (crouched || midPos < standingCamHeight)
                {
                    if (crouchSpeedAmt > crouchSpeedPercentage)
                    {
                        crouchSpeedAmt -= Time.deltaTime;//gradually decrease crouchSpeedAmt to crouch limit value
                    }
                }
                else
                {
                    if (crouchSpeedAmt < 1.0f)
                    {
                        crouchSpeedAmt += Time.deltaTime;//gradually increase crouchSpeedAmt to neutral
                    }
                }





                //同理，将speedAmtY稳定住，当为前进时，权值应为1，后退时应该等于backwardSpeedPercentage的值
                if (inputY >= 0)
                {
                    if (speedAmtY < 1.0f)
                    {
                        speedAmtY += Time.deltaTime;//逐渐加到1
                    }
                }
                //后退
                else
                {
                    if (speedAmtY > backwardSpeedPercentage)
                    {
                        speedAmtY -= Time.deltaTime;//逐渐减到backwardSpeedPercentage的值
                    }
                }


                //仅仅左右移动
                if (inputX != 0 && inputY == 0)
                {
                    if (speedAmtX < 1.0f)//正常移动速度
                    {
                        speedAmtX += Time.deltaTime;
                    }
                }

                //前后方向之一和左右方向之一一起按,此时必定最开始的稳定speedAmtY已经调整过了调整speedAmtX即可
                if (inputX != 0 && inputY != 0)
                {
                    if (inputY >= 0)//前进方向按下，此时左右一起按不影响速度（和正常一样)
                    {
                        if (speedAmtX < 1.0f)
                            speedAmtX += Time.deltaTime;
                    }
                    else
                    {//后退时候就要减速啦
                        if (speedAmtX > strafeSpeedPercentage)
                            speedAmtX -= Time.deltaTime;
                    }
                }



                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //跳跃控制
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                if (jumping)//处在跳跃状态中
                {
                    //play landing sound effect after landing from jump and reset jumpfxstate

                    if (jumpTimer + 0.75f < Time.time)//说明已经跳了0.05f的时间
                    {
                        //play landing sound
                        if (!inWater&&!playingLandFx)
                        {
                            AudioSource.PlayClipAtPoint(landfx, mainCamTransform.position, 0.75f);
                            playingLandFx = true;//已经开始播放音效，只播放一次
                        }
                        if (Camera.main.GetComponent<Animation>().IsPlaying("CameraLand"))
                        {              
                            //rewind animation if already playing to allow overlapping playback
                            Camera.main.GetComponent<Animation>().Rewind("CameraLand");
                        }
                        Camera.main.GetComponent<Animation>().CrossFade("CameraLand", 0.35f, PlayMode.StopAll);

                        jumpfxstate = true;
                    }
                    if (jumpTimer + 0.8f < Time.time)
                    {//跳跃已经浮空0.8f，跳跃状态已经结束       
                        jumping = false;
                        playingLandFx = false;
                    }
                }


                //首先不是在跳跃状态，才监测跳跃键是否按下
                if (!jumping && Input.GetKey(playerControl.jump) && jumpButtonEnabled
                //&& !FPSPlayerComponent.zoomed 
                && !crouched//非蹲下状态
                && !belowWater
                && canWaterJump
                && !climbing//非攀爬状态
                && jumpTimer + 0.8f + timeBetweenTwoJump < Time.time//还要检查最小跳跃时间间隔是否满足

                && (!rayTooSteep || inWater))
                {
                    jumping = true;//跳跃状态变量设为true
                    AudioSource.PlayClipAtPoint(jumpSound, mainCamTransform.position, 0.75f);
                    jumpTimer = Time.time;//记录开始跳跃的时间
                    //施加一个向上的跳跃力
                    GetComponent<Rigidbody>().velocity = new Vector3(velocity.x, Mathf.Sqrt(1.5f * jumpSpeed * gravity), velocity.z);
                }




                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Crouching
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                if (Time.timeSinceLevelLoad > 0.5f)
                {
                    //crouch
                    if (crouched)
                    {//also lower to crouch position if player dies
                        if (midPos > crouchingCamHeight) { midPos -= crouchingHeightChange * Time.deltaTime; }//decrease camera height to crouch height
                        if (capsule.height > crouchingCapsuleHeight) { capsule.height -= crouchingHeightChange * Time.deltaTime; }//decrease capsule height to crouch height
                    }
                    else
                    {
                        if (!Input.GetKey(playerControl.jump))
                        {
                            if (midPos < standingCamHeight) { midPos += standingHeightChange * Time.deltaTime; }//increase camera height to standing height
                            if (capsule.height < standingCapsuleheight) { capsule.height += standingHeightChange * Time.deltaTime; }//increase capsule height to standing height
                        }
                    }
                }

            }



            //地面行动终于完了，接下来是非地面行为，包括空中行为，游泳和攀爬


            else
            {//Player is airborn////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //keep track of the time that player lost grounding for air manipulation and moving gun while jumping
                if (airTimeState)
                {
                    airTime = Time.time;
                    airTimeState = false;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Falling
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //subtract height we began falling from current position to get falling distance
                fallingDistance = fallStartLevel - myTransform.position.y;//this value referenced in other scripts

                if (!falling)
                {
                    falling = true;
                    //start tracking altitude (y position) for fall check
                    fallStartLevel = myTransform.position.y;

                    //check jumpfxstate var to play jumping sound only once
                    if (jumping && jumpfxstate)
                    {
                        //play jumping sound
                        //AudioSource.PlayClipAtPoint(FPSPlayerComponent.jumpfx, mainCamTransform.position);
                        jumpfxstate = false;
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Holding Breath
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (holdingBreath)
            {
                //determine if player will gasp for air when surfacing
                if (Time.time - diveStartTime > holdBreathDuration / 1.5f)
                {
                    drowning = true;
                }
                //determine if player is drowning
                if (Time.time - diveStartTime > holdBreathDuration)
                {
                    if (drownStartTime < Time.time)
                    {
                        //FPSPlayerComponent.ApplyDamage(drownDamage);
                        drownStartTime = Time.time + 1.75f;
                    }
                }

            }
            else
            {
                if (drowning)
                {//play gasping sound if player needed air when surfacing
                    //AudioSource.PlayClipAtPoint(FPSPlayerComponent.gasp, mainCamTransform.position, 0.75f);
                    drowning = false;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Player Ground Check
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            ////cast capsule shape down to see if player is about to hit anything or is resting on the ground
            if (Physics.CapsuleCast(p1, p2, capsule.radius, -myTransform.up, out capHit, capsuleCastHeight, clipMask.value)
            || climbing
            || swimming)
            {

                grounded = true;
                //用于决定到底是踩在什么材质的物体上，如果是一般的地面材质就发出缺省的音效
                if (!climbing)
                {
                    if (!inWater && !swimming)
                    {
                        switch (capHit.collider.gameObject.tag)
                        {
                            
                            case "Water":
                                
                                GetComponent<PlayFootStepSound>().playWaterSound = true;
                                break;                          
                            case "Wood":
                                
                                GetComponent<PlayFootStepSound>().playWoodSound = true;
                                break;
                            default:
                                
                                GetComponent<PlayFootStepSound>().playWoodSound = false;
                                GetComponent<PlayFootStepSound>().playWaterSound =false;
                                break;
                        }
                    }
                }
                //    else
                //    {
                //        landfx = FootstepsComponent.dirtLand;
                //    }
            }
            else
            {
                grounded = false;
            }

            ////check that angle of the normal directly below the capsule center point is less than the movement slope limit 
            //if (Physics.Raycast(myTransform.position, -myTransform.up, out rayHit, rayCastHeight, clipMask.value))
            //{
            //    if (Vector3.Angle(rayHit.normal, Vector3.up) > 60.0f && !inWater)
            //    {
            //        rayTooSteep = true;
            //    }
            //    else
            //    {
            //        rayTooSteep = false;
            //    }
            //    //pass the material/surface type tag player is on to the Footsteps.cs script
            //    FootstepsComponent.materialType = rayHit.collider.gameObject.tag;

            //}

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Player Velocity
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //limit speed if strafing diagonally
            limitStrafeSpeed = (inputX != 0.0f && inputY != 0.0f) ? .7071f : 1.0f;

            //让当前碰撞体冲着摄像机的欧拉角度前进
            Vector3 tempLocalEulerAngles = new Vector3(0.0f, CameraObj.transform.localEulerAngles.y, 0.0f);//store angles in temporary vector
            myTransform.localEulerAngles = tempLocalEulerAngles;//apply angles from temporary vector to player object
            Vector3 tempEulerAngles = new Vector3(0.0f, CameraObj.transform.eulerAngles.y, 0.0f);//store angles in temporary vector
            myTransform.eulerAngles = tempEulerAngles;//apply angles from temporary vector to player object

            grounded = true;
            if ((grounded || climbing || swimming || ((airTime + 0.3f) > Time.time)) &&!playerControl.getDead())
            {
                //Check both capsule center point and capsule base slope angles to determine if the slope is too high to climb.
                //If so, bypass player control and apply some extra downward velocity to help capsule return to more level ground.
                if (!capsuleTooSteep || climbing || swimming || (capsuleTooSteep && !rayTooSteep))
                {

                    // We are grounded, so recalculate movedirection directly from axes	
                    moveDirection = new Vector3(inputXSmoothed * limitStrafeSpeed, 0.0f, inputYSmoothed * limitStrafeSpeed);
                    //realign moveDirection vector to world space
                    moveDirection = myTransform.TransformDirection(moveDirection);
                    //apply speed limits to moveDirection vector
                    moveDirection = moveDirection * speed * speedAmtX * speedAmtY * crouchSpeedAmt * zoomSpeedAmt;

                    //apply a force that attempts to reach target velocity
                    Vector3 velocityChange = moveDirection - velocity;
                    //limit max speed
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

                    //apply ladder climbing speed to velocityChange vector and set y velocity to zero if not climbing ladder
                    if (climbing)
                    {
                        if ((Input.GetKey(playerControl.moveForward)) || (Mathf.Abs(inputY) > 0.1f))
                        {//move player up climbable surface if pressing forward button
                            velocityChange.y = verticalSpeedAmt;
                        }
                        else if (Input.GetKey(playerControl.jump))
                        {//move player up climbable surface if pressing jump button
                            inputY = 1;//to cycle bobbing effects

                            velocityChange.y = climbSpeed * 0.75f;
                        }
                        else if (Input.GetKey(playerControl.crouch))
                        {//move player down climbable surface if pressing crouch button
                            inputY = -1;//to cycle bobbing effects

                            velocityChange.y = -climbSpeed * 0.75f;
                        }
                        else
                        {
                            velocityChange.y = 0;
                        }

                    }
                    else
                    {
                        velocityChange.y = 0;
                    }

                    //finally, add movement velocity to player rigidbody velocity
                    GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);
                }
                else
                {
                    //If slope is too high below both the center and base contact point of capsule, apply some downward velocity to help
                    //the capsule fall to more level ground. Check the slope angle at two points on the collider to prevent it from 
                    //getting stuck when player control is bypassed and to have more control over the slope angle limit.                   
                    GetComponent<Rigidbody>().AddForce(new Vector3(0, -2, 0), ForceMode.VelocityChange);
                }
            }
        
            if (!climbing)
            {
                if (!swimming)
                {
                    //apply gravity manually for more tuning control except when climbing a ladder to avoid unwanted downward movement
                    //Debug.Log("third addforce,1109");
                    GetComponent<Rigidbody>().AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
                    GetComponent<Rigidbody>().useGravity = true;
                }
                else
                {
                    if (swimStartTime + 0.2f > Time.time)
                    {//make player sink under surface for a short time if they jumped in deep water 
                     //dont make player sink if they are close to bottom
                        if (landStartTime + 0.3f > Time.time)
                        {//make sure that player doesn't try to sink into the ground if wading into water
                            if (!Physics.CapsuleCast(p1, p2, capsule.radius * 0.9f, -myTransform.up, out capHit, capsuleCastHeight, clipMask.value))
                            {
                                //Debug.Log("fourth addforce,1122");
                                GetComponent<Rigidbody>().AddForce(new Vector3(0, -6.0f, 0), ForceMode.VelocityChange);//make player sink into water after jump
                            }
                        }
                    }
                    else
                    {
                        //make player rise to water surface if they hold the jump button
                        if (Input.GetKey(playerControl.jump))
                        {

                            if (belowWater)
                            {
                                swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, 3.0f, Time.deltaTime * 4);
                                if (holdingBreath)
                                {
                                    canWaterJump = false;//don't also jump if player just surfaced by holding jump button
                                }
                            }
                            else
                            {
                                swimmingVerticalSpeed = 0.0f;
                            }
                            //make player dive downwards if they hold the crouch button
                        }
                        else if (Input.GetKey(playerControl.crouch))
                        {

                            swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -3.0f, Time.deltaTime * 4);

                        }
                        else
                        {
                            //make player sink slowly when underwater due to the weight of their gear
                            if (belowWater)
                            {
                                swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -0.2f, Time.deltaTime * 4);
                            }
                            else
                            {
                                swimmingVerticalSpeed = 0.0f;
                            }

                        }
                        //allow jumping when treading water if player has released the jump button after surfacing 
                        //by holding jump button down to prevent player from surfacing and immediately jumping
                        if (!belowWater && !Input.GetKey(playerControl.jump))
                        {
                            canWaterJump = true;
                        }
                       
                        GetComponent<Rigidbody>().AddForce(new Vector3(0, swimmingVerticalSpeed, 0), ForceMode.VelocityChange);

                    }
                    GetComponent<Rigidbody>().useGravity = false;//don't use gravity when swimming	
                }
            }
            else
            {
                GetComponent<Rigidbody>().useGravity = false;
            }
        }

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Rigidbody Collisions
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void TrackCollision(Collision col)
    {
        //define a height of about a fourth of the capsule height to check for collisions with platforms
        float maximumHeight = (capsule.bounds.min.y + capsule.radius);
        //check the collision points within our predefined height range  
        foreach (ContactPoint c in col.contacts)
        {
            if (c.point.y < maximumHeight)
            {
                //check that we want to collide with this object (check for "Moving Platforms" layer) and that its surface is not too steep 
                if (!parentState && col.gameObject.layer == 15 && Vector3.Angle(c.normal, Vector3.up) < 70)
                {
                    //set player object parent to platform transform to inherit it's movement
                    myTransform.parent = col.transform;
                    parentState = true;//only set parent once to prevent rapid parenting and de-parenting that breaks functionality
                }
                //check that angle of the surface that the capsule base is touching is less than the movement slope limit  
                if (Vector3.Angle(c.normal, Vector3.up) > slopeLimit && !inWater)
                {
                    capsuleTooSteep = true;
                }
                else
                {
                    capsuleTooSteep = false;
                }
            }
        }

    }

    void OnCollisionExit(Collision col)
    {
        parentState = false;
        capsuleTooSteep = false;
        inWater = false;
    }

    void OnCollisionStay(Collision col)
    {
        TrackCollision(col);
    }

    void OnCollisionEnter(Collision col)
    {
        TrackCollision(col);
    }

    void CalculateFallingDamage(float fallDistance)
    {
        //GetComponent<FPSPlayer>().ApplyDamage(fallDistance * 2);
    }

    public void deadCapsule()
    {//当死亡时胶囊体变矮
        capsule.height=deadCapsuleHeight;
    }
}