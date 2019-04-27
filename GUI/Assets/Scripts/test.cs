using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {
    public GameObject m_zidan;

	// Use this for initialization
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) //按下鼠标左键        
        {
            //按下鼠标左键之后获取鼠标射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //得到鼠标位置      
            RaycastHit hit; //定义一个用于记录碰撞信息结构体的对象        
            Physics.Raycast(ray, out hit); //物理类下射线检查方法，如果发生物理碰撞，返回值为真，并将碰撞信息存储到hit中
            if (hit.collider != null) //如果碰撞信息中的碰撞体不为空            
            {
                //Debug.Log("yes");
             
                Vector3 start = Camera.main.gameObject.GetComponent<Transform>().position; //定义三维向量类型的start对象，并且赋值为主摄像机的位置               
                Vector3 end = hit.collider.gameObject.GetComponent<Transform>().position; //定义三维向量类型的end对象，并且赋值为碰撞信息中的碰撞体位置                
                Vector3 dir = end - start; //dir是对direction的简写，表示方向，轨迹  
                
                GameObject game_zidan = Instantiate(m_zidan, start, Camera.main.gameObject.GetComponent<Transform>().rotation); 
                //在主摄像机位置实例化无旋转的对象m_zidan，并命名为game_zidan游戏对象                
                game_zidan.GetComponent<Rigidbody>().AddForce(dir * 150); //为game_zidan游戏对象刚体组件添加一个dir方向的力            
            }
        }
	}
}
