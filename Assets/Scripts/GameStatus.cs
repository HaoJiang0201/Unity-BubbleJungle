using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus : MonoBehaviour
{
    public TextAsset fileDataTxt;   //建立TextAsset
    string stringDataText;       //用来存放文本内容

    // Start is called before the first frame update
    void Start()
    {
        if(!GlobalData.g_bGameStart)
        {
            GlobalData.g_bGameStart = true;
            stringDataText = fileDataTxt.text;
            string[] stringPoints = stringDataText.Split('\n');
            GlobalData.g_iFirstPoint = int.Parse(stringPoints[0]);
            GlobalData.g_iSecondPoint = int.Parse(stringPoints[1]);
            GlobalData.g_iThirdPoint = int.Parse(stringPoints[2]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
