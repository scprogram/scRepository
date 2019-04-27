using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用于管理玩家携带武器的脚本
public class WeaponsInBags : MonoBehaviour
{
    //public int firstWeaponID;//手上现在拿的武器的编号
    //public int backWeaponID;//身上放着的武器编号

    [HideInInspector]
    public int maxWeapon = 3;//最大武器携带数量
    [HideInInspector]
    public int totalWeapons;//当前总武器数
    [HideInInspector]
    public GameObject[] weaponsCarried;//携带的武器列表，初始时为空
    //[HideInInspector]
    //public bool[] myWeaponList=new bool[7];//用来实际标识是否含有某一个编号的武器，防止直接访问weaponsCarried出现空指针异常
    public AudioClip changeWeaponSound;

    public bool haveWeapon = false;//是否有武器在背包中，初始时为false
    private int currentWeaponNum;//现在携带的武器编号

    //换武器的时间要长于播放动画的时间
    [HideInInspector]
    public float switchTime = 0.0f;//开始换武器
    [HideInInspector]
    public float startPlayingSwitchAnimation = 0.0f;//开始播放换武器动画的时间
    [HideInInspector]
    public bool switching;//是否正在更换武器，初始时为false
    [HideInInspector]
    public bool playingSwitchAnimation = false;//是否正在播放更换武器动画，初始为false
    [HideInInspector]
    public float sprintSwitchTime = 0.0f;

    [HideInInspector]
    public bool dead_HaveWeapon;//死亡丢掉武器但身上仍然有武器

    public GameObject playerObj;//指向实际的player对象，用于读取角色控制键位
    [HideInInspector]
    private FPSRigidBodyWalker fpsBodyWalker;
    PlayerControl playerControl;
    ShootSight shootSight;//换枪时修改脚本中的枪支引用

    public GameObject ammoGUI;//子弹容量的GUI显示组件
    private float changeWeaponTimer;//防止极短间隔切换武器，武器切换需要有最小间隔时间
    // Use this for initialization
    void Start()
    {
        playerControl = playerObj.GetComponent<PlayerControl>();//得到控制键位
        fpsBodyWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
        shootSight = playerObj.GetComponent<ShootSight>();
        weaponsCarried = new GameObject[7];//七维都是null
        totalWeapons = 0;
        haveWeapon = false;
        ammoGUI.SetActive(false);//隐藏掉子弹容量GUI
    }

    // Update is called once per frame
    void Update()
    {
        //换弹期间不允许切换武器
        //鼠标中键下滚切换
        if ((Input.GetKeyDown(playerControl.selectNextWeapon) || Input.GetAxis("Mouse ScrollWheel") < 0)
            && totalWeapons > 1 && !playerObj.GetComponent<ShootSight>().reloading
            && !fpsBodyWalker.sprintActive
            && changeWeaponTimer + 0.5f < Time.time)//如果武器数只有1，那么切换不了
        {
            int i = (currentWeaponNum + 1) % weaponsCarried.Length;
            for (; !weaponsCarried[i]; i = (i + 1) % weaponsCarried.Length) ;
            changeWeaponTimer = Time.time;
            StartCoroutine(SelectWeapon(i));
        }

        //鼠标中键上滚切换,小键盘上键切换
        else if ((Input.GetKeyDown(playerControl.selectPreviousWeapon) || Input.GetAxis("Mouse ScrollWheel") > 0)
            && totalWeapons > 1 && !playerObj.GetComponent<ShootSight>().reloading
            && !fpsBodyWalker.sprintActive
            && changeWeaponTimer + 0.5f < Time.time)//如果武器数只有1，那么切换不了
        {
            int i = (currentWeaponNum - 1) % weaponsCarried.Length;
            for (; !weaponsCarried[i]; i = (i + weaponsCarried.Length - 1) % weaponsCarried.Length) ;
            changeWeaponTimer = Time.time;
            StartCoroutine(SelectWeapon(i));
        }
        if (Input.GetKeyDown(playerControl.selectWeapon0) && changeWeaponTimer + 0.5f < Time.time)
        {
            //    if (currentWeapon != 0) { StartCoroutine(SelectWeapon(0)); }
        }
        else if (Input.GetKeyDown(playerControl.selectWeapon1) &&
            !playerObj.GetComponent<ShootSight>().reloading && changeWeaponTimer + 0.5f < Time.time
            && !fpsBodyWalker.sprintActive)
        {//换武器1
            if (currentWeaponNum != 1)//只有当前武器不是武器1时才换
            {
                changeWeaponTimer = Time.time;
                StartCoroutine(SelectWeapon(1));
            }
        }
        else if (Input.GetKeyDown(playerControl.selectWeapon2) &&
            !playerObj.GetComponent<ShootSight>().reloading
            && !fpsBodyWalker.sprintActive && changeWeaponTimer + 0.5f < Time.time)
        {
            if (currentWeaponNum != 2)
            {
                changeWeaponTimer = Time.time;
                StartCoroutine(SelectWeapon(2));
            }
        }
        else if (Input.GetKeyDown(playerControl.selectWeapon3)
            && !playerObj.GetComponent<ShootSight>().reloading
            && !fpsBodyWalker.sprintActive && changeWeaponTimer + 0.4 < Time.time)
        {
            if (currentWeaponNum != 3)
            {
                changeWeaponTimer = Time.time;
                StartCoroutine(SelectWeapon(3));
            }
        }
        else if (Input.GetKeyDown(playerControl.selectWeapon4) &&
            !playerObj.GetComponent<ShootSight>().reloading
            && !fpsBodyWalker.sprintActive && changeWeaponTimer + 0.5f < Time.time)
        {
            if (currentWeaponNum != 4)
            {
                changeWeaponTimer = Time.time;
                StartCoroutine(SelectWeapon(4));
            }
        }

        else if (Input.GetKeyDown(playerControl.selectWeapon5) &&
            !playerObj.GetComponent<ShootSight>().reloading
            && !fpsBodyWalker.sprintActive && changeWeaponTimer + 0.5f < Time.time)
        {
            if (currentWeaponNum != 5)
            {
                changeWeaponTimer = Time.time;
                StartCoroutine(SelectWeapon(5));
            }
        }
        else if (Input.GetKeyDown(playerControl.selectWeapon6) &&
          !playerObj.GetComponent<ShootSight>().reloading
          && !fpsBodyWalker.sprintActive && changeWeaponTimer + 0.5f < Time.time)
        {
            if (currentWeaponNum != 6)
            {
                changeWeaponTimer = Time.time;
                StartCoroutine(SelectWeapon(6));
            }
        }
        if (Input.GetKeyDown(playerControl.dropWeapon))
            DropWeapon(true);



        //如果开始换弹未经过0.87f(长于动画时间)则将状态设置为正在换弹
        if (switchTime + 0.87f > Time.time)
            switching = true;
        else
            switching = false;


        //动画播放时间要短于总换弹时间
        if (startPlayingSwitchAnimation + 0.44f > Time.time)
            playingSwitchAnimation = true;
        else
            playingSwitchAnimation = false;

    }


    //计算当前携带的总武器数

    public void AddTotalWeapons()//总武器数加1
    {
        if (totalWeapons <= maxWeapon - 1)
        {
            totalWeapons++;
            haveWeapon = true;
            ammoGUI.SetActive(true);//开启子弹数量GUI
        }
    }
    public void SubtractTotalWeapons()
    {
        totalWeapons--;
        if (totalWeapons == 0)
        {
            haveWeapon = false;
            ammoGUI.SetActive(false);//关闭子弹数量GUI

            //防止丢弃武器时正在换弹，因为这影响到是否能再次捡起武器
            if (shootSight.reloading)
                shootSight.reloading = false;
        }
    }
    public int GetCurrentWeaponNum()
    {
        return currentWeaponNum;
    }
    public void SetCurrentWeaponNum(int num)
    {
        currentWeaponNum = num;
    }


    public IEnumerator SelectWeapon(int index)//根据编号选武器，协程
    {
        if (!weaponsCarried[index])//如果换的武器没有那退出
            yield break;//退出协程


        if (weaponsCarried[currentWeaponNum])
        {//防止丢枪导致的换枪操作引发空异常
            Weapon currentWeapon = weaponsCarried[currentWeaponNum].GetComponent<Weapon>();
        }
        //得到当前手持武器编号
        PlayerControl playerControl = playerObj.GetComponent<PlayerControl>();
        ShootSight shootSight = playerControl.GetComponent<ShootSight>();

        playerControl.zoomed = false;//更换武器时取消瞄准动作
        shootSight.reloading = false;//更换武器时取消换弹操作

        //currentWeapon.StopCoroutine("Reload");//停止协程换弹
        switchTime = Time.time - Time.deltaTime;//开始更换武器，将开始换武器的时间设置上
        Camera.main.GetComponent<Animation>().Rewind("CameraSwitch");//播放更换武器的相机动画
        Camera.main.GetComponent<Animation>().CrossFade("CameraSwitch", 0.35f, PlayMode.StopAll);

        for (int i = 0; i < weaponsCarried.Length; i++)
        {
            if (weaponsCarried[i])
            {
                if (i == index)
                    weaponsCarried[i].SetActive(true);
                else
                    weaponsCarried[i].SetActive(false);
            }
        }
        weaponsCarried[index].GetComponent<Weapon>().weaponReady();//播放对应编号武器动画
        currentWeaponNum = index;//成功转换后使得当前武器编号为index

        //修改瞄准射击脚本中的枪械引用，否则会出现初始捡起来的枪能瞄准，切换后的枪不能瞄准（因为引用
        //还指向被隐藏的初始枪械)
        shootSight.gunObj = weaponsCarried[index];
        shootSight.gunTransform = weaponsCarried[index].transform;
    }

    public IEnumerator PickUpFirstWeapon(int index)//捡起第一把武器
    {
        PlayerControl playerControl = playerObj.GetComponent<PlayerControl>();
        ShootSight shootSight = playerControl.GetComponent<ShootSight>();
        Weapon currentWeapon = weaponsCarried[index].GetComponent<Weapon>();//当前武器就是捡到的武器
        shootSight.reloading = false;
        switchTime = Time.time - Time.deltaTime;//开始更换武器，将开始换武器的时间设置上

        currentWeapon.GetComponent<Weapon>().weaponReady();//执行武器预备的函数
        Camera.main.GetComponent<Animation>().Rewind("CameraSwitch");//播放相机更换武器的动画
        Camera.main.GetComponent<Animation>().CrossFade("CameraSwitch", 0.35f, PlayMode.StopAll);

        //播放拿起武器的音效
        AudioSource.PlayClipAtPoint(changeWeaponSound, Camera.main.transform.position);
        currentWeaponNum = index;//成功转换后使得当前武器编号为index
        shootSight.switchMove = -0.4f;
        if (Time.timeSinceLevelLoad > 2)
        {
            if (currentWeapon.meleeSwingDelay == 0)
            {
                shootSight.switchMove = -0.4f;
            }
            else
            {
                shootSight.switchMove = -1.2f;
            }
            yield return new WaitForSeconds(0.2f);

        }
        yield break;
    }

    public bool haveIndexOfWeapon(int index)
    {
        //检测是否有对应编号的武器
        if (weaponsCarried[index])
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public void OpenShootAndGunScript()//打开射击和枪械环绕脚本
    {
        shootSight.enabled = true;
        GetComponent<GunOrbit>().enabled = true;
    }
    public void CloseShootAndGunScript()
    {
        shootSight.enabled = false;//关闭射击脚本
        GetComponent<GunOrbit>().enabled = false;
    }

    public void DropWeapon(bool replace)
    {
        if (totalWeapons == 0)
        {//无武器
            return;
        }

        if (!weaponsCarried[currentWeaponNum])//如果当前编号的武器已经扔了，那么无法扔武器，除非切换到已有的武器上
            return;
        Weapon weapon = weaponsCarried[currentWeaponNum].GetComponent<Weapon>();//需要得到待丢弃武器对应的pickupobj
        AudioSource[] audioSources = GetComponents<AudioSource>();//有两个音效来源，这里是用的是components
        AudioSource otherAudio = audioSources[1] as AudioSource;
        otherAudio.Stop();//停止播放当前音效，比如换弹音效
        Camera.main.GetComponent<Animation>().Stop();//同时停止摄像机上播放的动画

        weapon.StopCoroutine("Reload");

        //记下枪械的剩余子弹
        int leftAmmo = weaponsCarried[currentWeaponNum].GetComponent<Weapon>().bulletsLeft;
        int ammo = weaponsCarried[currentWeaponNum].GetComponent<Weapon>().ammo;
        //销毁对应枪械对象
        GameObject.Destroy(weaponsCarried[currentWeaponNum]);//删除当前位置的枪械
        weaponsCarried[currentWeaponNum] = null;//设置为null，以便能重新捡起武器
        GameObject dropWeapon=Instantiate(weapon.weaponDropObj, 
        Camera.main.transform.position + playerObj.transform.forward * 0.25f+Vector3.up*-0.25f,
            //枪械丢弃时向前向下丢出，同时实例化一个丢弃的枪械实体
            playerObj.transform.rotation);
        dropWeapon.GetComponent<PickUpWeapon>().setAmmo(leftAmmo,ammo);

        
        SubtractTotalWeapons();//总武器数减1
        CloseShootAndGunScript();//关闭脚本

        if (replace)//丢弃武器时是否自动更换枪支
        {
            //如果背包中还有武器，调出来使用同时再次打开脚本，否则脚本关闭
            for (int i = 0; i < weaponsCarried.Length; i++)
                if (weaponsCarried[i])
                {//显示一把可见武器，防止空手
                    StartCoroutine(SelectWeapon(i));//选择一把可用武器
                    OpenShootAndGunScript();//再次打开脚本
                    break;
                }
        }
        else
        {
            if(totalWeapons>0)
                dead_HaveWeapon = true;//以不切换武器的状态丢弃枪械
        }
    }

    public void reviveChange()
    {
        for(int i=0;i<weaponsCarried.Length;i++)
            if (weaponsCarried[i])
            {//显示一把可见武器，防止空手
                StartCoroutine(SelectWeapon(i));//选择一把可用武器
                OpenShootAndGunScript();//再次打开脚本
                ammoGUI.SetActive(true);
                break;
            }
    }
}
