using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetScore : MonoBehaviour
{
    // Data File Write
    public Text[] textRecord;

    // Start is called before the first frame update
    void Start()
    {
        textRecord[0].text = GlobalData.g_iFirstPoint.ToString();
        textRecord[1].text = GlobalData.g_iSecondPoint.ToString();
        textRecord[2].text = GlobalData.g_iThirdPoint.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        textRecord[0].text = GlobalData.g_iFirstPoint.ToString();
        textRecord[1].text = GlobalData.g_iSecondPoint.ToString();
        textRecord[2].text = GlobalData.g_iThirdPoint.ToString();
    }
}
