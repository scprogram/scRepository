using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Make sure there is always a character controller
[RequireComponent(typeof(CharacterController))]

//借鉴的脚本
public class ZombieAI : MonoBehaviour {
    public Transform objectWithAnims;//the object with the Animation component automatically created by the character mesh's import settings
    public float randomSpawnChance = 1.0f;

    //NPC movement speeds
    private float minimumRunSpeed = 3.0f;
    public float walkAnimSpeed = 1.0f;
    public float runAnimSpeed = 1.0f;
    public float speed = 6.0f;//movement speed of the NPC
    private float speedAmt = 1.0f;
    public float pushPower = 5.0f;//physics force to apply to rigidbodies blocking NPC path
    public float rotationSpeed = 5.0f;
    public float shootRange = 15.0f;//minimum range to target for attack
    public float attackRange = 30.0f;//range that NPC will start chasing target until they are within shootRange
    [HideInInspector]
    public float attackRangeAmt = 30.0f;//increased by character damage script if NPC is damaged by player
    public float sneakRangeMod = 0.4f;//reduce NPC's attack range by sneakRangeMod amount when player is sneaking
    private float shootAngle = 10.0f;
    public float dontComeCloserRange = 5.0f;
    public float delayShootTime = 0.35f;
    public float eyeHeight = 0.4f;//height of rayCast starting point/origin which detects player (can be raised if NPC origin is at their feet)
    private float pickNextWaypointDistance = 2.0f;
    [HideInInspector]
    public Transform target;//目标，扫描到玩家后会将目标调整为玩家
    private float lastSearchTime;//delay between NPC checks for target, for efficiency
    [HideInInspector]
    public GameObject playerObj;

    //waypoints and patrolling
    private Transform myTransform;
    public LayerMask searchMask = 0;
    private bool countBackwards = false;

    [HideInInspector]
    public Vector3 backPosition;//返回位置，初始化设定为一开始站的位置

    void OnEnable()
    {

        myTransform = transform;
        backPosition = transform.position;
        Mathf.Clamp01(randomSpawnChance);
        CharacterController controller = GetComponent<CharacterController>();

        //如果不在地面上就加力掉到地面上
        if (!controller.isGrounded)
        {
            Vector3 down = myTransform.TransformDirection(-Vector3.up);
            controller.SimpleMove(down);
        }

        //动画附加在僵尸模型上
        if (objectWithAnims == null) { objectWithAnims = transform; }

            //所有动画播放模式都是循环播放
            objectWithAnims.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            objectWithAnims.GetComponent<Animation>()["shoot"].wrapMode = WrapMode.Once;          
            objectWithAnims.GetComponent<Animation>()["idle"].layer = -1;
            objectWithAnims.GetComponent<Animation>()["walk"].layer = -1;
            objectWithAnims.GetComponent<Animation>()["run"].layer = -1;

            objectWithAnims.GetComponent<Animation>()["walk"].speed = walkAnimSpeed;//走动和跑动动画速度设置
            objectWithAnims.GetComponent<Animation>()["run"].speed = runAnimSpeed;

            objectWithAnims.GetComponent<Animation>().Stop();

            //得到玩家引用
            playerObj = Camera.main.transform.GetComponent<CameraAnimMove>().playerObj;
            attackRangeAmt = attackRange;
            objectWithAnims.GetComponent<Animation>().CrossFade("idle", 0.3f);

            // 设置玩家对象为搜寻对象
            if (target == null && GameObject.FindWithTag("Player"))
            {
                target = GameObject.FindWithTag("Player").transform;
            }
      
           // StartCoroutine(StandWatch());//类似于开始Update()函数
    }

    void Update()
    {
        //能看到玩家且距离小于攻击范围
        
        //if (CanSeeTarget() &&(target.transform.position-transform.position).magnitude>shootRange)
        //    GetComponent<NavMeshAgent>().destination = playerObj.transform.position;

        //播放站立动画
        //objectWithAnims.GetComponent<Animation>().CrossFade("idle", 0.3f);
       
        
        
            if (lastSearchTime + 0.75f < Time.time)
            {
                lastSearchTime = Time.time;
                if (CanSeeTarget())
                {//如果站立时看到目标就开始攻击           
                    if ((target.transform.position - transform.position).magnitude > shootRange)
                    {
                        GetComponent<NavMeshAgent>().destination = playerObj.transform.position;
                        //objectWithAnims.GetComponent<Animation>().Rewind("walk");//重新回到动画开头
                        //objectWithAnims.GetComponent<Animation>()["walk"].speed = 1.0f;
                        objectWithAnims.GetComponent<Animation>().CrossFade("walk", 1.0f, PlayMode.StopAll);
                    }
                    else
                        StartCoroutine(AttackPlayer());
                }
            //else
            //{
            //    objectWithAnims.GetComponent<Animation>().CrossFade("idle", 0.3f);
            //}

            }

        if (!CanSeeTarget() || playerObj.GetComponent<PlayerControl>().getDead())
        {
            GetComponent<NavMeshAgent>().destination = backPosition;
            if ((transform.position - backPosition).sqrMagnitude <= 1)
                objectWithAnims.GetComponent<Animation>().CrossFade("idle", 0.3f);
        }

        
    }

    bool CanSeeTarget()
    {
        FPSRigidBodyWalker FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
        if (FPSWalker.crouched)
        {
            attackRangeAmt = attackRange * sneakRangeMod;
        }
        else
        {
            attackRangeAmt = attackRange;
        }
        if (Vector3.Distance(myTransform.position, target.position) > attackRangeAmt)
        {//如果距离大于搜寻范围，就搜寻不到玩家目标
            return false;
        }
        RaycastHit hit;
        //线检测，用于检测起始点到终结点的首个碰撞体
        if (Physics.Linecast(myTransform.position + myTransform.up * (1.0f + eyeHeight), target.position, out hit, searchMask))
        {
            return hit.transform == target;
        }
        return false;
    }


    //真正启用attackplayer脚本中扣除玩家生命值的函数
    IEnumerator Attack()
    {
        //播放攻击动画
        objectWithAnims.GetComponent<Animation>().CrossFade("shoot", 0.3f);
        speedAmt = 0.0f;
        SetSpeed(0.0f);
        yield return new WaitForSeconds(delayShootTime);
        //启用攻击玩家的函数
        GetComponent<AttackPlayer>().Attack();

        yield return new WaitForSeconds(objectWithAnims.GetComponent<Animation>()["shoot"].length - delayShootTime + Random.Range(0.0f, 0.75f));
    }

    IEnumerator AttackPlayer()
    {
        Vector3 lastVisiblePlayerPosition = target.position;
        while (true)
        {
            if (CanSeeTarget())
            {
                
                if (target.GetComponent<PlayerControl>().getDead())//如果死了就不攻击了
                {//玩家死了就不用攻击了
                    speedAmt = 1.0f;
                    yield break;
                }
                //如果距离大于最大搜索距离就放弃
                float distance = Vector3.Distance(myTransform.position, target.position);
                if (distance > attackRangeAmt)
                {
                    
                    speedAmt = 1.0f;
                  
                    yield break;
                }
                speedAmt = speed;
                lastVisiblePlayerPosition = target.position;
                if (distance > dontComeCloserRange)
                {
                    MoveTowards(lastVisiblePlayerPosition);
                    //gameObject.GetComponent<NavMeshAgent>().destination = target.position;
                }
                else
                {
                    RotateTowards(lastVisiblePlayerPosition);
                }
                Vector3 forward = myTransform.TransformDirection(Vector3.forward);
                Vector3 targetDirection = lastVisiblePlayerPosition - myTransform.position;
                targetDirection.y = 0;

                float angle = Vector3.Angle(targetDirection, forward);

                // 开始攻击玩家，如果离得近
                if (distance < shootRange && angle < shootAngle)
                {
                    yield return StartCoroutine(Attack());
                }
            }
            else
            {
                speedAmt = speed;
                yield return StartCoroutine(SearchPlayer(lastVisiblePlayerPosition));
                // 看不见玩家就离开
                if (!CanSeeTarget())
                {
                    speedAmt = 1.0f;         
                    yield break;
                }
            }

            yield return 0;
        }
    }

    IEnumerator SearchPlayer(Vector3 position)
    {
        float timeout = 3.0f;
        while (timeout > 0.0f)
        {
            MoveTowards(position);
            //GetComponent<NavMeshAgent>().destination = position;
            //找到了玩家
            if (CanSeeTarget())
            {
                yield return false;
            }
            timeout -= Time.deltaTime;//只搜索三秒钟时间

            yield break;
        }
    }

    void RotateTowards(Vector3 position)
    {

        SetSpeed(0.0f);

        Vector3 direction = position - myTransform.position;
        direction.y = 0;
        if (direction.magnitude < 0.1f)
        {
            return;
        }
        // 转向目标
        myTransform.rotation = Quaternion.Slerp(myTransform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime * 8);
        myTransform.eulerAngles = new Vector3(0, myTransform.eulerAngles.y, 0);
    }

    void MoveTowards(Vector3 position)
    {
        Vector3 direction = position - myTransform.position;
        direction.y = 0;//忽略掉玩家的跳跃动作
        if (direction.magnitude < 0.5f)
        {
            SetSpeed(0.0f);
            return;
        }

        //转向目标
        myTransform.rotation = Quaternion.Slerp(myTransform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        myTransform.eulerAngles = new Vector3(0, myTransform.eulerAngles.y, 0);
        Vector3 forward = myTransform.TransformDirection(Vector3.forward);
        float speedModifier = Vector3.Dot(forward, direction.normalized);
        speedModifier = Mathf.Clamp01(speedModifier);
        // 移动僵尸
        direction = forward * speedAmt * speedModifier;
        myTransform.GetComponent<CharacterController>().SimpleMove(direction);//使用角色控制器的移动功能

        SetSpeed(speedAmt * speedModifier);

    }

   
    //僵尸可以推开刚体阻拦物
    //void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    Rigidbody body = hit.collider.attachedRigidbody;
    //    //没有刚体
    //    if (body == null || body.isKinematic || body.gameObject.tag == "Player")
    //        return;

    //    // 在后面的刚体不用管
    //    if (hit.moveDirection.y < -0.3f)
    //        return;

    //    // 只在地面方向推不能往空中推
    //    Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
    //   //对碰撞的刚体施加推力
    //    body.velocity = pushDir * pushPower;
    //}

    void SetSpeed(float speed)
    {
        if (speed > minimumRunSpeed)
        {
            objectWithAnims.GetComponent<Animation>().CrossFade("run");
        }
        else
        {
            if (speed > 0)
            {
                objectWithAnims.GetComponent<Animation>().CrossFade("walk");
            }
            else//没有速度的话就站在原地不动
            {
                objectWithAnims.GetComponent<Animation>().CrossFade("idle");
            }
        }
    }
}
