using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject level;
    public GameObject gameOverCanvas;
    public GameObject ingameCanvas;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        // currentscore = 0;
        // LoadScore();
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        level.SetActive(true);
        ingameCanvas.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        GameObject spawnerGO = level.transform.Find("Spawner").gameObject;
        if(spawnerGO.TryGetComponent<Spawner>(out Spawner spawner))
        {
            spawner.enabled = false;
        }
        gameOverCanvas.SetActive(true);

    }

    // public void SaveScore()
    // {
    //     if(currentscore > highscore)
    //     {
    //         SaveSystem.SaveScore(currentscore);
    //     }
    // }

    // public void LoadScore()
    // {
    //     GameData data = SaveSystem.LoadScore();
    //     if(data != null) highscore = data.highScore;
    //     else highscore = 0;
    // }
 
    public void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void MainMenuAndRestart()
    {
        SceneManager.LoadScene("Menu");
    }

}
