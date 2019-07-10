using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetResolution : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(540, 960, false, 30);
    }

    // Update is called once per frame
    void Update()
    {
        //Screen.SetResolution(540, 960, false, 30);
    }
}
