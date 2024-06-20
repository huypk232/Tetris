using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Move, Wait
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject level;
    public GameObject gameOverCanvas;
    public GameObject inGameCanvas;
    public GameState currentState = GameState.Move;
    private Spawner _spawner;
    private Shadow _shadow;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _spawner = FindObjectOfType<Spawner>();
        _shadow = FindObjectOfType<Shadow>();
        // currentScore = 0;
        // LoadScore();
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        level.SetActive(true);
        inGameCanvas.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        _spawner.enabled = false;
        _shadow.gameObject.SetActive(false);
        gameOverCanvas.SetActive(true);

    }

    // public void SaveScore()
    // {
    //     if(currentScore > highScore)
    //     {
    //         SaveSystem.SaveScore(currentScore);
    //     }
    // }

    // public void LoadScore()
    // {
    //     GameData data = SaveSystem.LoadScore();
    //     if(data != null) highScore = data.highScore;
    //     else highScore = 0;
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
