using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayer : MonoBehaviour {
    public float force;//
    public float damage;//伤害
    public int range;//攻击范围
    public float attackInterval;//伤害间隔时间

    private float attackEndTime;//伤害结束时间，必须超过伤害间隔
    private float attackStartTime;//伤害开始时间

    private bool attacking;//正在攻击状态中
    public AudioClip zombieAttackSound;
	
	// Update is called once per frame
	void Update () {
        if (attacking)
        {
            attackEndTime = Time.time - attackStartTime;
            if (attackEndTime > attackInterval)//大于攻击间隔，攻击状态为false
                attacking = false;
        }
		
	}

    public void Attack()
    {//触发攻击的函数
        if (!attacking)
        {
            attackStartTime = Time.time;
            AudioSource.PlayClipAtPoint(zombieAttackSound, transform.position, 0.75f);//播放攻击音效
            ZombieAttack();
            attacking = true;//进入攻击状态
        }
    }

    public void ZombieAttack()
    {
        RaycastHit hit;

        ZombieAI zombieAI = GetComponent<ZombieAI>();
        //得到AI中存储的攻击目标
        Transform target=GetComponent<ZombieAI>().target;


        //检测方向为目标位置减去僵尸位置
        Vector3 targetDirection =target.position-transform.position;
        //僵尸位置是射线起点，注意僵尸的y值要加上高度
        Vector3 originPosition = new Vector3(transform.position.x, transform.position.y+zombieAI.eyeHeight, transform.position.z);

        //参数为射线起点，方向，输出信息和检测范围
        if(Physics.Raycast(originPosition,targetDirection,out hit, range))
        {
            if (hit.rigidbody)
            {//打到的对象有刚体，为刚体施加力
             //hit.rigidbody.AddForce();
            }
            float tempDamage = Random.Range(damage - 5.0f, damage + 5.0f);
            switch (hit.collider.gameObject.layer)
            {
                case 16://16为玩家的层
                    hit.collider.gameObject.GetComponent<PlayerControl>().SubtractHitPoints(tempDamage);
                    break;
            }


        }
    }
}
