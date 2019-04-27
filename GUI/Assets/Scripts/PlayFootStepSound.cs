using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFootStepSound : MonoBehaviour {
    public AudioClip[] landFootStepSounds;//地面上的脚步声
    public AudioClip[] landWoodStepSounds;//木制地板上的脚步声
    [HideInInspector]
    public bool playWaterSound=false;//播放水中音效
    [HideInInspector]
    public bool playWoodSound=false;//播放踩在木制地板上的音效
    private FPSRigidBodyWalker fpsWalker;
    private float playGapTimer=0.0f;//播放间隔
	void Start () {
        fpsWalker = GetComponent<FPSRigidBodyWalker>();//根据当前移动状态来确定音效
	}
	
	// Update is called once per frame
	void Update () {
        if ((Mathf.Abs(fpsWalker.inputX) > 0 || Mathf.Abs(fpsWalker.inputY) > 0))
        {
            if (fpsWalker.grounded && !fpsWalker.jumping)
            {//注意这个grounded不是在地面上的意思，跳跃蹲下都是地面活动

                if (playWoodSound)//踩踏在木制地板上
                {
                    
                    if (fpsWalker.crouched)//蹲下状态，脚步间隔最低
                    {
                        if (playGapTimer + 0.8f < Time.time)
                        {
                            AudioSource.PlayClipAtPoint(landWoodStepSounds[Random.Range(0, landFootStepSounds.Length)],
                                Camera.main.transform.position, 1.0f);
                            playGapTimer = Time.time;
                        }
                    }
                    else
                    {
                        if (fpsWalker.sprintActive && playGapTimer + 0.3f < Time.time)
                        {//跑步移动频率较高
                            AudioSource.PlayClipAtPoint(landWoodStepSounds[Random.Range(0, landFootStepSounds.Length)],
                            Camera.main.transform.position, 1.0f);
                            playGapTimer = Time.time;
                        }
                        if (!fpsWalker.sprintActive && playGapTimer + 0.6f < Time.time)
                        {//正常移动的话播放声音频率较低,0.6s间隔播放一次脚步声                       
                            AudioSource.PlayClipAtPoint(landWoodStepSounds[Random.Range(0, landFootStepSounds.Length)],
                            Camera.main.transform.position, 1.0f);
                            playGapTimer = Time.time;
                        }
                    }
                }
                else//一般地面上
                {
                    if (fpsWalker.crouched)//蹲下状态，脚步间隔最低
                    {
                        if (playGapTimer + 0.8f < Time.time)
                        {
                            AudioSource.PlayClipAtPoint(landFootStepSounds[Random.Range(0, landFootStepSounds.Length)],
                                Camera.main.transform.position, 1.0f);
                            playGapTimer = Time.time;
                        }
                    }
                    else
                    {
                        if (fpsWalker.sprintActive && playGapTimer + 0.3f < Time.time)
                        {//跑步移动频率较高
                            AudioSource.PlayClipAtPoint(landFootStepSounds[Random.Range(0, landFootStepSounds.Length)],
                            Camera.main.transform.position, 1.0f);
                            playGapTimer = Time.time;
                        }
                        if (!fpsWalker.sprintActive && playGapTimer + 0.6f < Time.time)
                        {//正常移动的话播放声音频率较低,0.6s间隔播放一次脚步声                       
                            AudioSource.PlayClipAtPoint(landFootStepSounds[Random.Range(0, landFootStepSounds.Length)],
                            Camera.main.transform.position, 1.0f);
                            playGapTimer = Time.time;
                        }
                    }
                }
            }  
        }       
	}
}
