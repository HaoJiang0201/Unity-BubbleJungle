using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneJumper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Jump to a specific scene
    public void JumpToScene(int iSceneID)
    {
        SceneManager.LoadScene(iSceneID);
    }

    // Quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
