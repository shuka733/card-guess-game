using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject MainMenuPanel;
    public GameObject HUDPanel;
    public GameObject GameOverPanel;
    public GameObject WinPanel;

    [Header("HUD")]
    public Text KeyCardText;
    public Text MessageText;
    public Text WarningText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ShowPanel(MainMenuPanel);
    }

    void ShowPanel(GameObject target)
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(MainMenuPanel == target);
        if (HUDPanel)      HUDPanel.SetActive(HUDPanel == target);
        if (GameOverPanel) GameOverPanel.SetActive(GameOverPanel == target);
        if (WinPanel)      WinPanel.SetActive(WinPanel == target);
    }

    public void OnGameStart()
    {
        ShowPanel(HUDPanel);
        if (KeyCardText) KeyCardText.text = "鍵カード: 0/" + GameManager.TotalKeyCards;
        if (WarningText) WarningText.gameObject.SetActive(false);
        if (MessageText) MessageText.gameObject.SetActive(false);
    }

    public void OnKeyCardCollected(int collected, int total)
    {
        if (KeyCardText) KeyCardText.text = "鍵カード: " + collected + "/" + total;
        ShowMessage("鍵カード入手! " + collected + "/" + total);
        if (collected >= total) ShowMessage("全て集めた！出口へ急げ！");
    }

    public void OnGameOver() => ShowPanel(GameOverPanel);
    public void OnWin()      => ShowPanel(WinPanel);

    private Coroutine msgCoroutine;

    public void ShowMessage(string msg)
    {
        if (MessageText == null) return;
        MessageText.text = msg;
        MessageText.gameObject.SetActive(true);
        if (msgCoroutine != null) StopCoroutine(msgCoroutine);
        msgCoroutine = StartCoroutine(HideAfter(3f));
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (MessageText) MessageText.gameObject.SetActive(false);
    }

    public void ShowWarning(bool show)
    {
        if (WarningText) WarningText.gameObject.SetActive(show);
    }

    public void OnStartButton()   => GameManager.Instance.StartGame();
    public void OnRestartButton() => GameManager.Instance.RestartGame();
}
