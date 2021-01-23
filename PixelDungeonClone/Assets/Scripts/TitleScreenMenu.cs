using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

public class TitleScreenMenu : MonoBehaviour
{
    public Button startButton;
    public float waitSeconds;
    public float unfadeSeconds;

    private bool buttonUnFade;
    private Text startButtonText;

    [SerializeField]
    private GameObject credits;
    [SerializeField]
    private GameObject nameSelect;
    [SerializeField]
    private InputField nameInputField;
    [SerializeField]
    private GameObject scoreboard;
    [SerializeField]
    private Transform scoreList;
    [SerializeField]
    private RunLogDisplay runLogPrefab;
    [SerializeField]
    private Dropdown pronounSelect;

    public List<RunLog> runLogs;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowStartButton());
        startButtonText = startButton.GetComponentInChildren<Text>();
        LoadRunLogs();
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

    public void ToggleCredits()
    {
        if(credits.activeInHierarchy)
        {
            credits.SetActive(false);
        }
        else
        {
            credits.SetActive(true);
        }
    }

    public void ToggleNameSelect()
    {
        if (nameSelect.activeInHierarchy)
        {
            nameSelect.SetActive(false);
        }
        else
        {
            nameSelect.SetActive(true);
            if(!PlayerPrefs.HasKey("lastName"))
            {
                PlayerPrefs.SetString("lastName", "Knight Errant");
            }
            if(!PlayerPrefs.HasKey("pronouns"))
            {
                PlayerPrefs.SetInt("pronouns", 2);
            }
            nameInputField.text = PlayerPrefs.GetString("lastName");
            pronounSelect.value = PlayerPrefs.GetInt("pronouns");
        }
    }

    public void ToggleScoreboard()
    {
        if (scoreboard.activeInHierarchy)
        {
            scoreboard.SetActive(false);
        }
        else
        {
            scoreboard.SetActive(true);            
        }
    }

    public void SetName(string value)
    {
        PlayerPrefs.SetString("lastName", value);
    }

    public void SetPronouns(int value)
    {
        PlayerPrefs.SetInt("pronouns", value);
    }

    public void StartGame()
    {
        StartCoroutine(LoadLevel());
        GetComponent<Animator>().SetTrigger("Load");
    }

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void LoadRunLogs()
    {
        if (File.Exists(Application.persistentDataPath + "/scoreboard.pr"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/scoreboard.pr", FileMode.Open);
            runLogs = (List<RunLog>)formatter.Deserialize(file);
            file.Close();

            runLogs.Sort((l1, l2) => l1.maxFloor.CompareTo(l2.maxFloor));

            for(int i = runLogs.Count-1; i >= 0; i--)
            {
                Instantiate(runLogPrefab, scoreList).SetLog(runLogs[i]);
            }            
        }
        else
        {
            Debug.Log("No scoreboard file!");
        }
    }   
}
