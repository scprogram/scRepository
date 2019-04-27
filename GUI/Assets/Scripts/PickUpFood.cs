using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpFood : MonoBehaviour {
    public int recoverBuff;//生命恢复速率倍数
    public float buffDuration;//buff持续时间
    public AudioClip pickUpFoodSound;
    public GameObject playerObj;
	// Use this for initialization

    public void PickUp()
    {
        playerObj.GetComponent<PlayerControl>().setFoodBuff(recoverBuff, buffDuration);
        AudioSource.PlayClipAtPoint(pickUpFoodSound, transform.position, 0.75f);

        RemoveAfterPick();//销毁苹果
    }

    void RemoveAfterPick()//得到枪械后将地上的枪械移除
    {
        Destroy(gameObject);
    }
}
