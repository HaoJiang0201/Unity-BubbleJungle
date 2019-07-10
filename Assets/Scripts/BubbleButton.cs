using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleButton : MonoBehaviour
{
    int m_iRowID = -1;
    int m_iColID = -1;
    int m_iType = -1;

    Bubble myBubble;

    [SerializeField] GameObject blockSparklesVFX;

    // Start is called before the first frame update
    void Start()
    {
        myBubble = FindObjectOfType<Bubble>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        if(!name.Contains("Generate"))
        {
            myBubble.m_iRowClick = m_iRowID;
            myBubble.m_iColClick = m_iColID;
        }
    }

    public void TriggerSparklesVFX()
    {
        GameObject sparkles = Instantiate(blockSparklesVFX, transform.position, transform.rotation);
        Destroy(sparkles, 1f);  // 延迟1s销毁该特效对象
    }

    public void SetID(int iRow, int iCol, int iType)
    {
        m_iRowID = iRow;
        m_iColID = iCol;
        m_iType = iType;
    }

}
