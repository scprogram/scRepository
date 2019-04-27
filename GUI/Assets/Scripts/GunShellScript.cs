using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//子弹壳脚本，在开枪射击时需要使用
public class GunShellScript : MonoBehaviour { 
    [HideInInspector] 
    public GameObject playerObj;    
    [HideInInspector]
    public GameObject gunObj;
    [HideInInspector]
    public Transform shellPosition;

    private Vector3 tempPosition;
    private Vector3 tempRotation;

    private Transform playerTransform;

    [HideInInspector]
    public float shellRotateUp = 0.0f;//垂直方向旋转角
    [HideInInspector]
    public float shellRotateSide = 0.0f;//水平方向旋转角
    [HideInInspector]
    public int shellDuration = 0;

    public AudioClip[] shellSounds;
    private bool soundState=true;
    private float shellRemoveTime = 0.0f;


    // Use this for initialization
    void Start () {
        Weapon weapon = gunObj.GetComponent<Weapon>();
        playerTransform = playerObj.transform;

        shellRotateUp = weapon.shellRotateUp / (Time.fixedDeltaTime * 100.0f);
        shellRotateSide = weapon.shellRotateSide / (Time.fixedDeltaTime * 100.0f);
        shellDuration = weapon.shellDuration;

        transform.parent = gunObj.transform;
        shellPosition.parent = gunObj.transform;

        tempPosition = transform.position;
        shellPosition.position = tempPosition;

        shellRemoveTime = Time.time + shellDuration;
    }
	
	// Update is called once per frame
	void Update () {
        //用线性插值将当前位置调整到transform的位置,tempposition的位置实际上已经落后于transform的位置了
        tempPosition = Vector3.Lerp(tempPosition, transform.position, Time.deltaTime * 64.0f);
        shellPosition.position = tempPosition;
        //同样的道理对角度进行平滑处理
        tempRotation.x = Mathf.LerpAngle(tempRotation.x, transform.eulerAngles.x, Time.deltaTime * 64.0f);
        tempRotation.y = Mathf.LerpAngle(tempRotation.y, transform.eulerAngles.y, Time.deltaTime * 64.0f);
        tempRotation.z = Mathf.LerpAngle(tempRotation.z, transform.eulerAngles.z, Time.deltaTime * 64.0f);
        shellPosition.eulerAngles = tempRotation;
    }

    void FixedUpdate()
    {
        if (Time.time > shellRemoveTime)//子弹出现一小段时间后就被销毁
        {
            Destroy(shellPosition.gameObject);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //play a bounce sound when shell object collides with a surface
        if (soundState)
        {
            if (shellSounds.Length > 0)
            {//播放子弹落地的音效
                AudioSource.PlayClipAtPoint(shellSounds[(int)Random.Range(0, (shellSounds.Length))], 
                    transform.position, 0.75f);
            }
            soundState = false;
        }
        
        if (collision.gameObject.layer ==8)
        {//碰到地形之后移除子弹对象
            Destroy(shellPosition.gameObject);
            Destroy(gameObject);
        }
    }
}
