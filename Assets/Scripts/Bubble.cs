using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class Bubble : MonoBehaviour
{
    /****** References ******/
    // Serialized Field
    [SerializeField] GameObject[] m_bubbleType;
    [SerializeField] int m_iInitRowNum = 3;
    [SerializeField] GameObject m_floorMiddle;
    [SerializeField] float m_fMoveUpSpeed = 4f;
    [SerializeField] Text m_textPoints;
    [SerializeField] GameObject m_dlgGameOver;
    // Audio Relevant
    [SerializeField] AudioClip[] mySEAudioClip;
    
    /****** Membership Variables ******/
    // Update Time Control
    //int m_iSecond = 0;    // Second counting
    float m_fTimeCount = 0.0f;    // Time counting for each update in the statement machine
    // Bubble Control
    const int c_iBubbleTypeNum = 8;
    const float c_fBubbleSize = 0.7f;
    const int c_iRowNum = 12;
    const int c_iColNum = 8;
    const float c_fBubbleStartX = -2.45f;
    const float c_fGenerateBubbleY = -4.5f;
    const float c_fWallBubbleStartY = -3.8f;
    GameObject[] m_bubbleGenerate;
    GameObject[,] m_bubbleWall;
    int[] m_iGenerateBubbleType;
    int[,] m_iWallBubbleType;
    bool[,] m_bSameColorCheck;
    bool[,] m_bDeleteBubbleList;
    int m_iCurrentGenerate = 0;
    bool m_bBubbleMoving = false;
    bool m_bBubbleDeleting = false;
    public int m_iRowClick = -1;
    public int m_iColClick = -1;
    int m_iClickType = -1;
    int m_iPointGet = 0;
    int m_iPointTotal = 0;
    float m_fSpeedYMonitor = 0.01f;
    const float c_fGameSpeed = 0.2f;
    // For test only
    bool m_bGameStop = false;
    // Data File Write
    public TextAsset fileDataTxt;   //建立TextAsset
    string stringDataTxtPath = @"Assets\Scripts\Data.txt";
    string stringDataText;       //用来存放文本内容
    // Audio Control
    AudioSource mySEAudioSource;
    AudioSource myBGMAudioSource;

    /****** Default Functions ******/
    // Start is called before the first frame update
    void Start()
    {
        GameInit();
    }
    // Update is called once per frame
    void Update()
    {
        // Game Over Stop
        if (m_bGameStop)
            return;
        // Deleting
        if (m_bBubbleDeleting)
        {
            m_bBubbleDeleting = false;
            for (int i = 0; i < c_iRowNum; i++)
            {
                for (int j = 0; j < c_iColNum; j++)
                {
                    if (m_iWallBubbleType[i, j] >= 0)
                    {
                        float fBubbleVelocityY = m_bubbleWall[i, j].GetComponent<Rigidbody2D>().velocity.y;
                        if (fBubbleVelocityY > m_fSpeedYMonitor || fBubbleVelocityY < -1f * m_fSpeedYMonitor)
                        {
                            m_bBubbleDeleting = true;
                        }
                    }
                }
            }
            if (!m_bBubbleDeleting)
            {
                m_bBubbleDeleting = false;
                // 如果需要消除的气泡多于1个，需要刷新气泡墙，并重置相应参数
                if (m_iPointGet > 1)
                {
                    RefreshBubbleWall(false);  // Fall Down
                }
                m_iPointGet = 0;
                m_iRowClick = -1;
                m_iColClick = -1;
            }
            return;
        }
        // Moving Up
        if (m_bBubbleMoving)
        {
            float fCurrentVelocityY = m_bubbleGenerate[0].GetComponent<Rigidbody2D>().velocity.y;
            if(fCurrentVelocityY <= m_fSpeedYMonitor)
            {
                m_floorMiddle.SetActive(true);
                m_fTimeCount += Time.deltaTime;
                if (m_fTimeCount >= 0.25f)
                {
                    m_fTimeCount = 0f;
                    RefreshBubbleWall(true);
                    m_bBubbleMoving = false;
                    m_iCurrentGenerate = 0;
                    GenerateBubble();
                }
            }
            return;
        }
        // Normal Monitoring
        m_fTimeCount += Time.deltaTime;
        if (m_fTimeCount >= c_fGameSpeed)
        {
            m_fTimeCount = 0f;
            ShowGenerate();
        }
        BubbleClick();
    }

    /****** Custom Functions Private ******/
    void GameInit()
    {
        // Varaiable Initialization
        m_iPointGet = 0;
        m_iPointTotal = 0;
        // Sprite Initialization
        m_floorMiddle.SetActive(true);
        m_dlgGameOver.SetActive(false);
        m_textPoints.text = "Points : 0";
        // Audio Init
        mySEAudioSource = GetComponent<AudioSource>();
        myBGMAudioSource = FindObjectOfType<Canvas>().GetComponent<AudioSource>();
        myBGMAudioSource.Play();

        // Initialize "generate" bubbles
        if (!m_bGameStop)
        {
            m_bubbleGenerate = new GameObject[c_iColNum];
            m_iGenerateBubbleType = new int[c_iColNum];
        }
        for (int i = 0; i < c_iColNum; i++)
        {
            float fPosX = c_fBubbleStartX + i * c_fBubbleSize;
            int iType = (int)Random.Range(0f, (float)c_iBubbleTypeNum);
            if (iType < 0 || iType >= c_iBubbleTypeNum)
                iType = c_iBubbleTypeNum - 1;
            m_iGenerateBubbleType[i] = iType;
            m_bubbleGenerate[i] = Instantiate(m_bubbleType[iType]);
            m_bubbleGenerate[i].GetComponent<BubbleButton>().SetID(0, i, iType);
            m_bubbleGenerate[i].name = "BubbleGenerate" + i;
            m_bubbleGenerate[i].transform.position = new Vector3(fPosX, c_fGenerateBubbleY, 1.0f);
            m_bubbleGenerate[i].SetActive(false);
        }
        ShowGenerate();

        // Initialize default bubble rows
        m_bubbleWall = new GameObject[c_iRowNum, c_iColNum];
        m_iWallBubbleType = new int[c_iRowNum, c_iColNum];
        m_bSameColorCheck = new bool[c_iRowNum, c_iColNum];
        m_bDeleteBubbleList = new bool[c_iRowNum, c_iColNum];
        for (int i = 0; i < c_iRowNum; i++)
        {
            float fPosY = c_fWallBubbleStartY + i * c_fBubbleSize;
            for (int j = 0; j < c_iColNum; j++)
            {
                m_iWallBubbleType[i, j] = -1;
                float fPosX = c_fBubbleStartX + j * c_fBubbleSize;
                if (i < m_iInitRowNum)
                {
                    int iType = (int)Random.Range(0f, (float)c_iBubbleTypeNum);
                    if (iType < 0 || iType >= c_iBubbleTypeNum)
                        iType = c_iBubbleTypeNum - 1;
                    WallBubbleInstantiate(i, j, iType, "BubbleWall" + i + j, new Vector3(fPosX, fPosY, 1.0f), true);
                }
            }
        }
    }
    // Generate Bubble Start
    void GenerateBubble()
    {
        for (int i = 0; i < c_iColNum; i++)
        {
            Destroy(m_bubbleGenerate[i]);
            float fPosX = c_fBubbleStartX + i * c_fBubbleSize;
            int iType = (int)Random.Range(0f, (float)c_iBubbleTypeNum);
            if (iType < 0 || iType >= c_iBubbleTypeNum)
                iType = c_iBubbleTypeNum - 1;
            m_iGenerateBubbleType[i] = iType;
            m_bubbleGenerate[i] = Instantiate(m_bubbleType[iType]);
            m_bubbleGenerate[i].GetComponent<BubbleButton>().SetID(0, i, iType);
            m_bubbleGenerate[i].name = "BubbleGenerate" + i;
            m_bubbleGenerate[i].transform.position = new Vector3(fPosX, c_fGenerateBubbleY, 1.0f);
            m_bubbleGenerate[i].SetActive(false);
        }
    }
    // Show Next Generate Bubble
    void ShowGenerate()
    {
        if (m_iCurrentGenerate >= c_iColNum)
        {
            m_fTimeCount = 0f;
            m_bBubbleMoving = true;
            m_floorMiddle.SetActive(false);
            BubbleWallMoveUp();
        }
        else
        {
            m_bubbleGenerate[m_iCurrentGenerate].SetActive(true);
            m_iCurrentGenerate++;
        }
    }
    // Wall Bubble Instantiate
    void WallBubbleInstantiate(int iRow, int iCol, int iType, string strName, Vector3 v3Position, bool bActive)
    {
        m_iWallBubbleType[iRow, iCol] = iType;
        m_bSameColorCheck[iRow, iCol] = false;
        m_bDeleteBubbleList[iRow, iCol] = false;
        m_bubbleWall[iRow, iCol] = Instantiate(m_bubbleType[iType]);
        m_bubbleWall[iRow, iCol].GetComponent<BubbleButton>().SetID(iRow, iCol, iType);
        m_bubbleWall[iRow, iCol].name = strName;
        m_bubbleWall[iRow, iCol].transform.position = v3Position;
        m_bubbleWall[iRow, iCol].SetActive(bActive);
    }
    // Bubble Wall Move Up
    void BubbleWallMoveUp()
    {
        // 从第一列开始
        for (int j = 0; j < c_iColNum; j++)
        {
            // 从最下面一行开始
            for (int i = 0; i < c_iRowNum; i++)
            {
                if (m_iWallBubbleType[i, j] >= 0)
                {
                    m_bubbleWall[i, j].GetComponent<Rigidbody2D>().velocity = new Vector2(0f, m_fMoveUpSpeed);
                }
            }
            m_bubbleGenerate[j].GetComponent<Rigidbody2D>().velocity = new Vector2(0f, m_fMoveUpSpeed);
        }
    }
    // Refresh Bubble Wall
    void RefreshBubbleWall(bool bUpDown)
    {
        // Move Up
        if (bUpDown)
        {
            // 从第一列开始
            for (int j = 0; j < c_iColNum; j++)
            {
                float fPosX = c_fBubbleStartX + j * c_fBubbleSize;
                // 从最上面一行开始
                for (int i = c_iRowNum - 1; i >= 0; i--)
                {
                    float fPosY = c_fWallBubbleStartY + i * c_fBubbleSize;
                    if (i == 0)
                    {
                        // 如果刷新前的类型为有效值，则需要销毁掉改Bubble
                        if (m_iWallBubbleType[i, j] >= 0)
                        {
                            Destroy(m_bubbleWall[i, j]);
                        }
                        m_iWallBubbleType[i, j] = m_iGenerateBubbleType[j];
                        WallBubbleInstantiate(i, j, m_iWallBubbleType[i, j], "BubbleWall" + i + j, new Vector3(fPosX, fPosY, 1.0f), true);
                    }
                    else
                    {
                        // 如果刷新前的类型为有效值，则需要销毁掉该Bubble
                        if (m_iWallBubbleType[i, j] >= 0)
                        {
                            Destroy(m_bubbleWall[i, j]);
                        }
                        m_iWallBubbleType[i, j] = m_iWallBubbleType[i - 1, j];
                        // 如果刷新后的类型为有效值，则需要新生成一个Bubble
                        if (m_iWallBubbleType[i, j] >= 0)
                        {
                            WallBubbleInstantiate(i, j, m_iWallBubbleType[i, j], "BubbleWall" + i + j, new Vector3(fPosX, fPosY, 1.0f), true);
                        }
                    }
                }
            }
        }
        // Fall Down
        else
        {
            // 从第一列开始
            for (int j = 0; j < c_iColNum; j ++)
            {
                float fPosX = c_fBubbleStartX + j * c_fBubbleSize;
                // 从最下面一行开始
                for (int i = 0; i < c_iRowNum; i ++)
                {
                    float fPosY = c_fWallBubbleStartY + i * c_fBubbleSize;
                    if (m_iWallBubbleType[i, j] == -1 && i < c_iRowNum - 1)
                    {
                        // 从当前行（i的位置）一直查到最上面一行
                        for (int k = i + 1; k < c_iRowNum; k ++)
                        {
                            if (m_iWallBubbleType[k, j] >= 0)
                            {
                                m_iWallBubbleType[i, j] = m_iWallBubbleType[k, j];
                                WallBubbleInstantiate(i, j, m_iWallBubbleType[i, j], "BubbleWall" + i + j, new Vector3(fPosX, fPosY, 1.0f), true);
                                Destroy(m_bubbleWall[k, j]);
                                m_iWallBubbleType[k, j] = -1;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    // Bubble Clicked
    void BubbleClick()
    {
        if(m_iRowClick >= 0 && m_iColClick >= 0 && !m_bBubbleMoving && !m_bBubbleDeleting)
        {
            m_iClickType = m_iWallBubbleType[m_iRowClick, m_iColClick];
            m_bDeleteBubbleList[m_iRowClick, m_iColClick] = true;
            m_bSameColorCheck[m_iRowClick, m_iColClick] = true;
            m_iPointGet = 1;
            FindSameColorBubble(m_iRowClick, m_iColClick);
            if (m_iPointGet > 1)
            {
                m_iPointTotal += m_iPointGet * m_iPointGet;
                m_textPoints.text = "Points : " + m_iPointTotal;
                m_bBubbleDeleting = true;
                mySEAudioSource.clip = mySEAudioClip[1];
                mySEAudioSource.Play();
            }
            DeleteCheckReset();
        }
    }
    // Delete target bubble
    void BubbleDelete(int iRow, int iCol)
    {
        if(m_iWallBubbleType[iRow, iCol] >= 0)
        {
            m_bubbleWall[iRow, iCol].GetComponent<BubbleButton>().TriggerSparklesVFX();
            Destroy(m_bubbleWall[iRow, iCol]);
            m_iWallBubbleType[iRow, iCol] = -1;
        }
    }
    // Find Adjant Bubble with same color
    void FindSameColorBubble(int iRow, int iCol)
    {
        // Find Up
        if(iRow > 0)
        {
            if(!m_bSameColorCheck[iRow - 1, iCol] && m_iWallBubbleType[iRow - 1, iCol] == m_iClickType)
            {
                m_iPointGet ++;
                m_bDeleteBubbleList[iRow - 1, iCol] = true;
                m_bSameColorCheck[iRow - 1, iCol] = true;
                FindSameColorBubble(iRow - 1, iCol);
            }
        }
        // Find Down
        if (iRow < c_iRowNum-1)
        {
            if (!m_bSameColorCheck[iRow + 1, iCol] && m_iWallBubbleType[iRow + 1, iCol] == m_iClickType)
            {
                m_iPointGet++;
                m_bDeleteBubbleList[iRow + 1, iCol] = true;
                m_bSameColorCheck[iRow + 1, iCol] = true;
                FindSameColorBubble(iRow + 1, iCol);
            }
        }
        // Find Left
        if (iCol > 0)
        {
            if (!m_bSameColorCheck[iRow, iCol-1] && m_iWallBubbleType[iRow, iCol-1] == m_iClickType)
            {
                m_iPointGet++;
                m_bDeleteBubbleList[iRow, iCol - 1] = true;
                m_bSameColorCheck[iRow, iCol - 1] = true;
                FindSameColorBubble(iRow, iCol - 1);
            }
        }
        // Find Right
        if (iCol < c_iColNum - 1)
        {
            if (!m_bSameColorCheck[iRow, iCol + 1] && m_iWallBubbleType[iRow, iCol + 1] == m_iClickType)
            {
                m_iPointGet++;
                m_bDeleteBubbleList[iRow, iCol + 1] = true;
                m_bSameColorCheck[iRow, iCol + 1] = true;
                FindSameColorBubble(iRow, iCol + 1);
            }
        }
    }
    // Color Check Reset
    void DeleteCheckReset()
    {
        for(int i = 0; i < c_iRowNum; i ++)
        {
            for (int j = 0; j < c_iColNum; j ++)
            {
                // 如果需要消除的气泡多于1个，则启动消除功能
                if(m_iPointGet > 1)
                {
                    if(m_bDeleteBubbleList[i, j])
                    {
                        BubbleDelete(i, j);
                    }
                }
                // 如果只有1个，则不需要做任何处理，在此处重置相应参数
                else
                {
                    m_bSameColorCheck[i, j] = false;
                    m_bDeleteBubbleList[i, j] = false;
                }
            }
        }
    }

    /****** Custom Functions Public ******/
    // Game Over Dialog Show
    public void GameOver()
    {
        if(!m_bGameStop)
        {
            myBGMAudioSource.Stop();
            mySEAudioSource.clip = mySEAudioClip[0];
            mySEAudioSource.Play();
            m_dlgGameOver.SetActive(true);
            m_bGameStop = true;

            if (m_iPointTotal >= GlobalData.g_iFirstPoint)
            {
                GlobalData.g_iThirdPoint = GlobalData.g_iSecondPoint;
                GlobalData.g_iSecondPoint = GlobalData.g_iFirstPoint;
                GlobalData.g_iFirstPoint = m_iPointTotal;
            }
            else
            {
                if (m_iPointTotal >= GlobalData.g_iSecondPoint)
                {
                    GlobalData.g_iThirdPoint = GlobalData.g_iSecondPoint;
                    GlobalData.g_iSecondPoint = m_iPointTotal;
                }
                else
                {
                    if (m_iPointTotal >= GlobalData.g_iThirdPoint)
                    {
                        GlobalData.g_iThirdPoint = m_iPointTotal;
                    }
                }
            }

            stringDataText = GlobalData.g_iFirstPoint + "," + GlobalData.g_iSecondPoint + "," + GlobalData.g_iThirdPoint;
            string[] stringWrite = stringDataText.Split(',');
            File.WriteAllLines(stringDataTxtPath, stringWrite);
        }
    }
    // Restart Game
    public void Restart()
    {
        // Clean Bubble Wall and Generate Bubbles
        for(int j = 0; j < c_iColNum; j ++)
        {
            for (int i = 0; i < c_iRowNum; i++)
            {
                if(m_iWallBubbleType[i, j] >= 0)
                    Destroy(m_bubbleWall[i, j]);
                m_iWallBubbleType[i, j] = -1;
            }
            Destroy(m_bubbleGenerate[j]);
            m_iGenerateBubbleType[j] = -1;
        }
        GameInit();
        m_dlgGameOver.SetActive(false);
        m_bGameStop = false;
    }
    // Go Back to Title
    public void BackToTitle()
    {
        SceneManager.LoadScene(0);
    }

}
