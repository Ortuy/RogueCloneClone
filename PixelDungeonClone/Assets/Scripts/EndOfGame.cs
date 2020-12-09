using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(Player.instance != null)
        {
            Player.instance.gameObject.SetActive(false);
        }
        
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
