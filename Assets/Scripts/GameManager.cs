using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, GameOver, Win }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public int KeyCardsCollected { get; private set; }
    public const int TotalKeyCards = 3;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()
    {
        KeyCardsCollected = 0;
        CurrentState = GameState.Playing;
        UIManager.Instance.OnGameStart();
    }

    public void CollectKeyCard()
    {
        if (CurrentState != GameState.Playing) return;
        KeyCardsCollected++;
        UIManager.Instance.OnKeyCardCollected(KeyCardsCollected, TotalKeyCards);
    }

    public bool HasAllKeyCards() => KeyCardsCollected >= TotalKeyCards;

    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.GameOver;
        UIManager.Instance.OnGameOver();
    }

    public void Win()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Win;
        UIManager.Instance.OnWin();
    }

    public bool IsPlaying() => CurrentState == GameState.Playing;

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
