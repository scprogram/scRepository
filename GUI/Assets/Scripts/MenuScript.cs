using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuScript : MonoBehaviour {
    public Canvas quitCanvas;
    public Canvas startCanvas;
    public Button startText;
    public Button quitText;
    public GameObject playerGroup;
    public Camera menuCamera;
    
	// Use this for initialization
	void Start () {
        //quitCanvas.enabled = false;//关闭退出UI使能
        playerGroup.SetActive(false);//禁用玩家组件
	}

    void OnEnable()
    {
        menuCamera.enabled = true;
    }
    public void ExitPress()
    {
        gameObject.SetActive(false);//隐藏当前画布
        quitCanvas.gameObject.SetActive(true);//显示退出菜单画布

        quitCanvas.enabled = true;//退出菜单开启使能
        startText.enabled = false;
        quitText.enabled = false;
    }

    public void NoPress()
    {
        quitCanvas.enabled = false;
        gameObject.SetActive(true);//再次激活主菜单
        startText.enabled = true;//同时也要激活按键
        quitText.enabled = true;
    }

    public void YesPress()
    {
        Application.Quit();
    }
    public void PlayPress()
    {
        gameObject.SetActive(false);//隐藏当前带有此脚本的对象
        quitCanvas.enabled = false;
        playerGroup.SetActive(true);//激活第一人称控制器，开始移动
        //关闭菜单摄像机上的音频监听器       
        menuCamera.enabled = false;
    }
}

