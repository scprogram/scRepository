using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
    public int currentHitPoints;//当前血量
    private float calculateCurrentHP;
    public float fullHitPoints;//满血量
    public int recoverMultiple;//血量恢复速率倍数，正常为1倍
    public Canvas endCanvas;//死亡时显示出结束画布
    public Canvas successCanvas;

    public Vector3 birthPosition;//出生点

    [HideInInspector]
    public bool buffState;//是否处在生命值恢复加速的状态，捡到苹果的话进入增益状态
    [HideInInspector]
    public float buffRemainDuration;//buff剩余持续时间
    private float buffStartTime;//buff开始的时间
    [HideInInspector]
    public int point;//分数


    public GUIText HPText;

    public GameObject weaponBagObj;
    public GUITexture sightBead;

    public Texture canPickUpTexture;//对应能捡起和不能捡起
    public Texture cannotPickUpTexture;
    public Texture normalSightTexture;//一般准星
    public Texture pickUpAmmoTexture;//捡起弹药的样式

    public AudioClip painSound;//被攻击时的受伤音效
    public AudioClip dieSound;//死亡音效
    public AudioClip breathingSound;//残血喘息声
    [HideInInspector]
    public float breathingTimer;//喘息计时器
    public GameObject painFade;
    public Color painColor;//受伤颜色
    public int leftZombie;//剩余的僵尸
    //换子弹
    public KeyCode reload;
    public KeyCode selectWeapon0;
    public KeyCode selectWeapon1;
    public KeyCode selectWeapon2;
    public KeyCode selectWeapon3;
    public KeyCode selectWeapon4;
    public KeyCode selectWeapon5;
    public KeyCode selectWeapon6;
    public KeyCode selectNextWeapon;
    public KeyCode selectPreviousWeapon;
    public KeyCode pickUp;
    public KeyCode moveForward;
    public KeyCode moveBack;
    public KeyCode strafeLeft;
    public KeyCode strafeRight;
    public KeyCode fire;
    public KeyCode zoom;//右键瞄准
    public KeyCode crouch;//动作蹲下
    public KeyCode jump;//动作跳跃
    public KeyCode sprint;//动作冲刺
    public KeyCode fireMode;//射击模式
    public KeyCode dropWeapon;//丢弃武器

   

    private bool zoomBtnState = true;
    private float zoomStopTime = 0.0f;//track time that zoom stopped to delay making aim reticle visible again
    [HideInInspector]
    public bool zoomed = false;
    private float zoomStartTime = -2.0f;//瞄准开始的时间
    private bool zoomStartState = false;
    private float zoomEndTime = 0.0f;
    private bool zoomEndState = false;
    private float zoomDelay = 0.4f;

    private bool danger;//血量少于30进入danger状态
    private bool dead=false;//是否死亡状态
    private bool showSightBeadVisible=true;//是否开启准星，在右键瞄准和换弹时禁用
    private bool pickUpButtonEnabled=true;//禁止按下e不弹起的方式来捡武器
    private FPSRigidBodyWalker fpsRigidWalker;
    private Transform mainCameraTransform;

    public LayerMask rayMask;
    // Update is called once per frame
  
    void Start()
    {
        fpsRigidWalker = GetComponent<FPSRigidBodyWalker>();
        mainCameraTransform = Camera.main.transform;
        calculateCurrentHP = currentHitPoints;
        point = 0;
    }
    void Update() {       
        RecoverHitPoints();
        ChangeHPTextColor();
        PlayBreathingSound();
        HPText.text = "生命值: " + currentHitPoints;
    }

    void OnEnable()
    {
        if (dead)
        {//死亡状态重新复活
            transform.position = birthPosition;//注意这个位置向量一定得是世界坐标，取父对象世界坐标
            dead = false;
            currentHitPoints = 50;//重新复活只有30滴血
            calculateCurrentHP = currentHitPoints;//一定注意，否则死了之后复活中间计算量还是0，还会死

            fpsRigidWalker.enabled = true;//开启移动脚本
            fpsRigidWalker.cancelSprint = false;     
            weaponBagObj.GetComponent<WeaponsInBags>().ammoGUI.SetActive(false);//关闭弹药显示脚本
            HPText.gameObject.SetActive(true);//隐藏掉血量，子弹和准星
            sightBead.gameObject.SetActive(true);
            showSightBeadVisible = true;

            Camera.main.transform.parent.GetComponent<MouseLook>().enabled = true;//关闭鼠标脚本
            Cursor.lockState = CursorLockMode.None;//解锁鼠标可以点击退出
            Cursor.visible = true;
        }
        if (weaponBagObj.GetComponent<WeaponsInBags>().dead_HaveWeapon)
            weaponBagObj.GetComponent<WeaponsInBags>().reviveChange();
    }

    void FixedUpdate()
    {
        ShootSight shootSight = GetComponent<ShootSight>();
        WeaponsInBags weaponBag = weaponBagObj.GetComponent<WeaponsInBags>();
        Weapon currentWeapon = null;

        if (weaponBag.haveWeapon&&!dead)//有武器时才得到引用
            currentWeapon = weaponBag.weaponsCarried[weaponBag.GetCurrentWeaponNum()].GetComponent<Weapon>();

        RaycastHit hit;

        //换弹，冲刺换枪期间都无法捡起武器
        if (!shootSight.reloading && !fpsRigidWalker.sprintActive && !weaponBag.switching)
        {
            //参数分别为起点,方向（主摄像机的前方向),结果存放变量，长度和检测掩码

            //注意后续要捡起来的物体的层掩码要设置对哦
            //同时注意光标要可见状态才能捡东西
            if (showSightBeadVisible && Physics.Raycast(mainCameraTransform.position,
                mainCameraTransform.forward.normalized, out hit, 2.0f, rayMask))
            {
                if (hit.collider.gameObject.tag == "Usable")
                {
                    PickUpWeapon pickUpWeapon = hit.collider.gameObject.GetComponent<PickUpWeapon>();
                    //背包还有容量而且对应格子没有枪,显示为可捡起
                    if (weaponBag.totalWeapons < weaponBag.maxWeapon &&
                        !weaponBag.weaponsCarried[pickUpWeapon.weaponNum])
                    {
                        sightBead.texture = canPickUpTexture;
                        if (Input.GetKey(pickUp) && pickUpButtonEnabled)
                        {
                            pickUpButtonEnabled = false;//此时禁用捡起键防止按下连续捡起
                            sightBead.texture = normalSightTexture;
                            pickUpWeapon.PickUp();
                        }
                    }
                    else
                    {//否则不能捡
                        sightBead.texture = cannotPickUpTexture;
                    }
                    //如果检测到的物体的tag为可用并且E键抬起过                   
                }
                else if (hit.collider.gameObject.tag == "Ammo")
                { //捡起弹药

                    PickUpAmmo pickUpAmmo = hit.collider.gameObject.GetComponent<PickUpAmmo>();
                    if (weaponBag.weaponsCarried[pickUpAmmo.ammoOfWhichWeapon])//有对应武器而且对应武器有弹药空余
                    {
                        if (weaponBag.weaponsCarried[pickUpAmmo.ammoOfWhichWeapon].GetComponent<Weapon>().haveAmmoRoom())
                        {//有对应武器的情况下才能看是否有子弹空余否则会空引用
                            sightBead.texture = pickUpAmmoTexture;
                            if (Input.GetKey(pickUp) && pickUpButtonEnabled)
                            {
                                pickUpButtonEnabled = false;//此时禁用捡起键防止按下连续捡起
                                sightBead.texture = normalSightTexture;
                                pickUpAmmo.PickUp();
                            }
                        }
                        else
                        {//没有弹药容量了
                            sightBead.texture = cannotPickUpTexture;
                        }
                    }
                    else
                    {//否则不能捡
                        sightBead.texture = cannotPickUpTexture;
                    }
                }
                else if (hit.collider.gameObject.tag == "Edible")
                {
                    //食物
                    sightBead.texture = canPickUpTexture;
                    if (Input.GetKey(pickUp) && pickUpButtonEnabled)
                    {
                        pickUpButtonEnabled = false;//此时禁用捡起键防止按下连续捡起
                        sightBead.texture = normalSightTexture;
                        //捡起食物
                        hit.collider.gameObject.GetComponent<PickUpFood>().PickUp();
                    }
                }
                else
                    sightBead.texture = normalSightTexture;
            }
            else
                sightBead.texture = normalSightTexture;
          
        }

        if (Input.GetKey(pickUp))
        {
            pickUpButtonEnabled = false;//必须拾起键抬起过才能再次捡起武器
        }
        else
        {
            pickUpButtonEnabled = true;
        }

        //按下鼠标右键瞄准时必须对应枪械有瞄准功能且不处于换弹状态，注意要有武器
        if (Input.GetKey(zoom) && weaponBag.haveWeapon && currentWeapon.canZoom && !shootSight.reloading)
        {
            if (!zoomStartState)
            {//不是瞄准状态则进入瞄准状态
                zoomEndState = false;
                zoomStartState = true;
                zoomStartTime = Time.time;
                if (zoomEndTime - zoomStartTime < zoomDelay * Time.timeScale)
                {//如果连续点鼠标右键瞄准，那么瞄准状态直接切换
                    zoomed = !zoomed;
                }
            }
        }
        else//其他情况都要结束瞄准
        {
            zoomEndTime = Time.time;
            zoomStartState = false;
            zoomEndState = true;
            if (zoomEndTime - zoomStartTime > zoomDelay * Time.timeScale)
                zoomed = false;
        }

        //换弹时候或者瞄准时隐藏准星
        if (weaponBag.haveWeapon&&shootSight.reloading || zoomed)
        {
            if (currentWeapon.meleeSwingDelay == 0)
            {
                if (showSightBeadVisible)
                {
                    showSightBeadVisible = false;
                    sightBead.enabled = false;//隐藏对应纹理
                }
            }
        }
        else
        {
            //瞄准结束后延迟一小会儿再显示准星
            if (!showSightBeadVisible && zoomStopTime + 0.2f < Time.time)
            {
                showSightBeadVisible = true;
                sightBead.enabled = true;
            }
        }
    }

    public void RecoverHitPoints()
    {
        //保证死亡时只执行一次die函数，否则会有多次死亡音效
        if (currentHitPoints<1&&!dead)//生命值为0死亡
        {
            dead = true;
            Die();//StartCoroutine("Die");
            return;
        }
        //非死亡状态下才回血
        else if(currentHitPoints<fullHitPoints&&!dead)
        {
            if (buffState)
            {
                if (buffStartTime + buffRemainDuration > Time.time)
                { //还在buff持续时间内
                    calculateCurrentHP = Mathf.MoveTowards(calculateCurrentHP, fullHitPoints, -recoverMultiple * Time.deltaTime);
                    currentHitPoints = Mathf.RoundToInt(calculateCurrentHP);//四舍五入到整数
                }
                else
                {
                    buffState = false;
                    recoverMultiple = 1;//倍数回到1倍
                }
            }
            else
            {
                calculateCurrentHP = Mathf.MoveTowards(calculateCurrentHP, fullHitPoints, recoverMultiple * Time.deltaTime);
                currentHitPoints = Mathf.RoundToInt(calculateCurrentHP);
            }
        }
    }

    public void setFoodBuff(int recoverMul,float duration)
    {
        if (buffState)
        {
            buffRemainDuration += duration;//如果还在buff时间内，那么buff时间顺延
        }
        else
        {
            buffRemainDuration = duration;
            buffStartTime = Time.time;//第一次吃的话开启buff时间
        }
        recoverMultiple = recoverMul;       
        buffState = true;       
    }

    public void PlayBreathingSound()//生命值较低时播放喘息声音效
    {
        if (currentHitPoints < 1)
            return;//已经死了，不用播放

        else if (currentHitPoints <= 20)
            if (breathingTimer + 4.0f < Time.time) {
                AudioSource.PlayClipAtPoint(breathingSound,Camera.main.transform.position, 1.0f);
                breathingTimer = Time.time;
            }
    }
    public void SubtractHitPoints(float damage)
    {
        if (currentHitPoints < 1)
            return;//已经死了

        calculateCurrentHP -= damage;
        currentHitPoints = Mathf.RoundToInt(calculateCurrentHP);
        point -= 10 *(int) damage;//受到僵尸伤害要扣分
        AudioSource.PlayClipAtPoint(painSound, mainCameraTransform.position, 1.0f);

        GameObject painFadeObj = Instantiate(painFade) as GameObject;
        painFadeObj.GetComponent<PainFade>().FadeIn(painColor, 0.75f);
    }

    void ChangeHPTextColor()
    {
        if (currentHitPoints <= 30 && !danger)
        {
            HPText.material.color = Color.red;
            danger = true;
        }
        if (currentHitPoints > 30 && danger)
        {
            HPText.material.color = Color.green;
            danger = false;
        }
    }
    void Die()
    {
        //播放死亡音效
        AudioSource.PlayClipAtPoint(dieSound, Camera.main.transform.position, 1.0f);

        fpsRigidWalker.inputX = 0;
        fpsRigidWalker.inputY = 0;
        fpsRigidWalker.cancelSprint = true;
        weaponBagObj.GetComponent<WeaponsInBags>().DropWeapon(false);//不替换武器的方式丢弃当前武器       
        weaponBagObj.GetComponent<WeaponsInBags>().ammoGUI.SetActive(false);//关闭弹药显示脚本
        HPText.gameObject.SetActive(false);//隐藏掉血量，子弹和准星
        sightBead.gameObject.SetActive(false);
        showSightBeadVisible = false;
        fpsRigidWalker.deadCapsule();//将脑囊体高度变矮模拟角色死亡

        //死亡后关闭移动和控制脚本
        fpsRigidWalker.enabled=false;//关闭掉移动脚本
        enabled = false;//关闭玩家控制脚本

        Camera.main.transform.parent.GetComponent<MouseLook>().enabled = false;//关闭鼠标脚本
        Cursor.lockState = CursorLockMode.None;//解锁鼠标可以点击退出
        Cursor.visible = true;
        endCanvas.gameObject.SetActive(true);
    }

    public bool getDead()
    {
        return dead;
    }

    public void gameSuccess()
    {
        fpsRigidWalker.inputX = 0;
        fpsRigidWalker.inputY = 0;
        fpsRigidWalker.cancelSprint = true;
        sightBead.gameObject.SetActive(false);//关闭准星
        showSightBeadVisible = false;
        //死亡后关闭移动和控制脚本
        fpsRigidWalker.enabled = false;//关闭掉移动脚本
        enabled = false;//关闭玩家控制脚本

        Camera.main.transform.parent.GetComponent<MouseLook>().enabled = false;//关闭鼠标脚本
        Cursor.lockState = CursorLockMode.None;//解锁鼠标可以点击退出
        Cursor.visible = true;
        successCanvas.gameObject.SetActive(true);

        point += currentHitPoints * 10;//没点剩余血量10分
        if (point < 0)
            point = 0;//如果没有分数就设为零分
        successCanvas.transform.Find("point").GetComponent<UnityEngine.UI.Text>().text = point.ToString();
    }
}
