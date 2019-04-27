using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMenu : MonoBehaviour {
    public Canvas startMenu;
    public GameObject playerGroup;
    public void ExitGamePress()
    {//退出游戏
        Application.Quit();
    }

    public void Again()
    {
        gameObject.SetActive(false);//隐藏死亡UI
        playerGroup.SetActive(false);
        startMenu.gameObject.SetActive(true);
        startMenu.enabled = true;
    }
}
