using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWeapon : MonoBehaviour {
    public int weaponNum;//对应武器编号，对应WeaponsCarried中哪一维
    public GameObject weaponBag;
    public GameObject pickUpWeaponObj;//对应捡起来的武器预设体
    public AudioClip pickUpSound;

    
    public int leftAmmo;//弹夹中的子弹量
    public int backAmmo;//储备子弹

	void Start () {
        weaponBag = GameObject.Find("WeaponBag");//为了丢下武器后对应脚本再次初始化，此时不能用拖动了嘛
	}

    public void PickUp()
    {
        WeaponsInBags weaponsInBags = weaponBag.GetComponent<WeaponsInBags>();

        if (!weaponsInBags.weaponsCarried[weaponNum] && weaponsInBags.totalWeapons < weaponsInBags.maxWeapon)
        ////如果本来为null表示对应位置没有枪械如果对应位置不为null则已经捡起过了对应枪械，除非将其销毁掉
        ////此外想要捡起武器必须当前武器数小于最大可携带武器数
        {

            weaponsInBags.weaponsCarried[weaponNum] =
                Instantiate(pickUpWeaponObj,weaponBag.transform.position, weaponBag.transform.rotation);
            weaponsInBags.weaponsCarried[weaponNum].transform.parent
                = weaponBag.transform;
            weaponsInBags.weaponsCarried[weaponNum].GetComponent<Weapon>().setAmmo(leftAmmo, backAmmo);//设置子弹
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position, 0.75f);
            RemoveAfterPick();

            weaponsInBags.AddTotalWeapons();//总武器数增加1
            if (weaponsInBags.totalWeapons==1)//如果只有一把武器说明是刚刚捡的
            {               
                //注意要先开启对应脚本才能开启协程，因为协程要使用对应脚本中的变量
                weaponsInBags.StartCoroutine(weaponsInBags.PickUpFirstWeapon(weaponNum));
                weaponsInBags.OpenShootAndGunScript();
            }
            else//否则是在背包里有武器的情况下捡起来的，隐藏后捡起来的武器
            {               
                weaponsInBags.weaponsCarried[weaponNum].SetActive(false);
            }
        }
        //else
        //{//否则背包已满
        //    AudioSource.PlayClipAtPoint(bagFullSound, weaponBag.transform.position, 0.75f);
        //}
    }

    void RemoveAfterPick()//得到枪械后将地上的枪械移除
    {
        Destroy(gameObject);
    }

    public void setAmmo(int leftammo,int backammo)
    {
        leftAmmo = leftammo;
        backAmmo = backammo;
    }


}
