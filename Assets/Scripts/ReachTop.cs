using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachTop : MonoBehaviour
{
    [SerializeField] GameObject myBubbles;
    [SerializeField] GameObject myFloor;

    bool m_bTopTouch = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // The bubbles reaching the top
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!m_bTopTouch)
        {
            myFloor.SetActive(true);
            myBubbles.GetComponent<Bubble>().GameOver();
        }
    }

}
