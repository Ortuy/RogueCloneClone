using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenMenu : MonoBehaviour
{
    public Button startButton;
    public float waitSeconds;
    public float unfadeSeconds;

    private bool buttonUnFade;
    private Text startButtonText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowStartButton());
        startButtonText = startButton.GetComponentInChildren<Text>();
    }

    IEnumerator ShowStartButton()
    {
        yield return new WaitForSeconds(waitSeconds);
        startButton.gameObject.SetActive(true);
        buttonUnFade = true;
    }

    private void Update()
    {
        if(buttonUnFade)
        {
            startButtonText.color = new Color(1, 1, 1, startButtonText.color.a + (Time.deltaTime / unfadeSeconds));
        }
        if(startButtonText.color.a >= 1)
        {
            startButtonText.color = new Color(1, 1, 1, 1);
            buttonUnFade = false;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
