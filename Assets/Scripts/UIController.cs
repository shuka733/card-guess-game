using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームUIの表示と入力を管理する。
/// GameManagerに処理を委譲し、結果を画面に反映する。
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Card Display")]
    [SerializeField] private Text playerCardText;
    [SerializeField] private Text cpuCardText;

    [Header("Buttons")]
    [SerializeField] private Button higherButton;
    [SerializeField] private Button lowerButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button retryButton;

    [Header("Labels")]
    [SerializeField] private Text resultText;

    private GameManager.Guess? currentGuess;

    private void Start()
    {
        higherButton.onClick.AddListener(OnHigherSelected);
        lowerButton.onClick.AddListener(OnLowerSelected);
        confirmButton.onClick.AddListener(OnConfirm);
        retryButton.onClick.AddListener(OnRetry);

        StartNewGame();
    }

    private void StartNewGame()
    {
        gameManager.StartNewRound();
        currentGuess = null;

        // カードを伏せる
        playerCardText.text = "?";
        cpuCardText.text = "?";
        resultText.text = "「高い」か「低い」を選んでください";

        // ボタン状態リセット
        SetGuessButtonsInteractable(true);
        confirmButton.interactable = false;
        retryButton.gameObject.SetActive(false);

        ResetGuessButtonColors();
    }

    private void OnHigherSelected()
    {
        currentGuess = GameManager.Guess.Higher;
        confirmButton.interactable = true;
        HighlightGuessButton(isHigher: true);
        resultText.text = "予想：自分のカードが高い";
    }

    private void OnLowerSelected()
    {
        currentGuess = GameManager.Guess.Lower;
        confirmButton.interactable = true;
        HighlightGuessButton(isHigher: false);
        resultText.text = "予想：自分のカードが低い";
    }

    private void OnConfirm()
    {
        if (!currentGuess.HasValue) return;

        // カードを公開
        playerCardText.text = gameManager.PlayerCard.ToString();
        cpuCardText.text = gameManager.CpuCard.ToString();

        // 判定
        var result = gameManager.Judge(currentGuess.Value);
        resultText.text = result switch
        {
            GameManager.Result.Win => "勝ち！",
            GameManager.Result.Lose => "負け…",
            GameManager.Result.Draw => "引き分け（同じ数字）",
            _ => ""
        };

        // ボタン状態を結果表示用に切り替え
        SetGuessButtonsInteractable(false);
        confirmButton.interactable = false;
        retryButton.gameObject.SetActive(true);
    }

    private void OnRetry()
    {
        StartNewGame();
    }

    private void SetGuessButtonsInteractable(bool interactable)
    {
        higherButton.interactable = interactable;
        lowerButton.interactable = interactable;
    }

    private void HighlightGuessButton(bool isHigher)
    {
        var selectedColor = new Color(0.6f, 0.8f, 1f);
        var normalColor = Color.white;

        higherButton.GetComponent<Image>().color = isHigher ? selectedColor : normalColor;
        lowerButton.GetComponent<Image>().color = isHigher ? normalColor : selectedColor;
    }

    private void ResetGuessButtonColors()
    {
        higherButton.GetComponent<Image>().color = Color.white;
        lowerButton.GetComponent<Image>().color = Color.white;
    }
}
