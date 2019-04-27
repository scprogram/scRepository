using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainFade : MonoBehaviour {
    [HideInInspector]
    public GameObject painFadeObj;

    public void FadeIn(Color color, float fadeLength)
    {
        Texture2D fadeTexture = new Texture2D(1, 1);
        fadeTexture.SetPixel(0, 0, color);
        fadeTexture.Apply();

        gameObject.layer = 5;//将层设置为UI，让主摄像机可以显示
        gameObject.AddComponent<GUITexture>();
        gameObject.transform.position = new Vector3(0.5f, 0.5f, 1000);
        gameObject.GetComponent<GUITexture>().texture = fadeTexture;
        StartCoroutine(DoFade(fadeLength, true));
    }

    IEnumerator DoFade(float fadeLength, bool destroyTexture)
    {


        Vector4 tempColorVec = GetComponent<GUITexture>().color;
        tempColorVec.w = 0.0f;
        GetComponent<GUITexture>().color = tempColorVec;
        float time = 0.0f;
        while (time < fadeLength)
        {
            time += Time.deltaTime;
            tempColorVec.w = Mathf.InverseLerp(fadeLength, 0.0f, time);
            GetComponent<GUITexture>().color = tempColorVec;
            yield return 0;
        }

        Destroy(gameObject);
        if (destroyTexture)
        {
            Destroy(GetComponent<GUITexture>().texture);
        }
    }
}
