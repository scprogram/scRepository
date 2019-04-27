using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAmmo : MonoBehaviour {
    public int ammoOfWhichWeapon=1;//对应哪个编号的武器子弹
    public GameObject weaponBagObj;
    public int ammoAmount;//提供多少弹药量
    public AudioClip pickUpAmmoSound;
	// Use this for initialization
	public void PickUp()
    {
        Weapon weapon = weaponBagObj.GetComponent<WeaponsInBags>().weaponsCarried[ammoOfWhichWeapon]
            .GetComponent<Weapon>();//得到对应的武器
        if (weapon.ammo + ammoAmount > weapon.maxAmmo)
        {//如果加上提供的弹药量超过最大弹药量的话
            weapon.ammo = weapon.maxAmmo;
        }
        else
        {
            weapon.ammo += ammoAmount;
        }
        AudioSource.PlayClipAtPoint(pickUpAmmoSound, transform.position, 0.75f);
        RemoveAfterPick();//销毁对应对象
    }

    void RemoveAfterPick()//得到枪械后将地上的枪械移除
    {
        Destroy(gameObject);
    }
}
