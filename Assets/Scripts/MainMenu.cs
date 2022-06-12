using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using VanguardEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button ConnectOnlineButton;
    public Button ReplayButton;
    public Button OpenLog;
    public Dropdown ReplayDropdown;
    public string replayDirectory;
    public string SQLPath;
    public string selectedReplay = "";

    // Start is called before the first frame update
    void Start()
    {
        GameObject.DontDestroyOnLoad(this);
        replayDirectory = Application.dataPath + "/../Replays/";
        SQLPath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LogsButtonClicked()
    {
        ConnectOnlineButton.gameObject.SetActive(false);
        ReplayButton.gameObject.SetActive(false);
        ReplayDropdown.gameObject.SetActive(true);
        OpenLog.gameObject.SetActive(true);
        OpenLog.interactable = false;
        string[] filePaths = Directory.GetFiles(replayDirectory);
        foreach (string filePath in filePaths)
        {
            //if (LoadCards.GenerateList(filePath, LoadCode.WithRideDeck, -1) != null)
            //{
            //    ReplayDropdown.options.Add(new Dropdown.OptionData(filePath.Substring(replayDirectory.Length)));
            //}
            ReplayDropdown.options.Add(new Dropdown.OptionData(filePath.Substring(replayDirectory.Length)));
        }
        if (ReplayDropdown.options.Count > 0)
            OpenLog.interactable = true;
    }

    public void OpenLogButtonClicked()
    {
        selectedReplay = replayDirectory + ReplayDropdown.options[ReplayDropdown.value].text;
        IEnumerator Dialog()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Fight");
            while (SceneManager.GetActiveScene() == currentScene)
                yield return null;
            CardFightManager cardFightManager = GameObject.Instantiate(Globals.Instance.cardFightManagerPrefab).GetComponent<CardFightManager>();
            Globals.Instance.cardFightManager = cardFightManager;
            List<string> player1 = LoadCards.GenerateList(selectedReplay, LoadCode.WithRideDeck, 1);
            List<string> player2 = LoadCards.GenerateList(selectedReplay, LoadCode.WithRideDeck, 2);
            yield return new WaitForEndOfFrame();
            cardFightManager.InitializeCardFight(player1, player2, -1, -1, selectedReplay);
            yield return null;
        }
        StartCoroutine(Dialog());
    }

    public void ConnectOnlineButtonClicked()
    {
        SceneManager.LoadSceneAsync("Lobby");
    }
}
