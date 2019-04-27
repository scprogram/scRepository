using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedByPlayer : MonoBehaviour
{
    public int hitPoints;//僵尸的生命值
    public AudioClip zombieDieSound;//僵尸死亡音效
    public Transform deadZombie;
    public void HitZombie(int damage, Vector3 attackerDirection, Vector3 attackerPosition)
    {
       
        hitPoints -= damage;//扣血
        GetComponent<ZombieAI>().attackRangeAmt *= 2;//僵尸的搜索范围变为2倍
        if (hitPoints < 1)
        {
            ZombieDie();
        }

    }
    public void ZombieDie()
    {
        AudioSource.PlayClipAtPoint(zombieDieSound,transform.position,0.75f);//僵尸死亡        
        Instantiate(deadZombie, transform.position, transform.rotation);

        Destroy(gameObject);//销毁掉当前对象剩下尸体

        if (gameObject.GetComponent<ZombieAI>().playerObj.GetComponent<PlayerControl>().leftZombie-- == 1)//剩余僵尸减一
            gameObject.GetComponent<ZombieAI>().playerObj.GetComponent<PlayerControl>().gameSuccess();
    }
}
