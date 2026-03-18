using System.Collections;
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

    [Header("Card Backgrounds")]
    [SerializeField] private RectTransform playerCardBg;
    [SerializeField] private RectTransform cpuCardBg;

    [Header("Buttons")]
    [SerializeField] private Button higherButton;
    [SerializeField] private Button lowerButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button retryButton;

    [Header("Labels")]
    [SerializeField] private Text resultText;

    [Header("Animation")]
    [SerializeField] private float flipDuration = 0.4f;

    private GameManager.Guess? currentGuess;
    private bool isAnimating;

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
        isAnimating = false;

        // カードを伏せる（スケールもリセット）
        playerCardText.text = "?";
        cpuCardText.text = "?";
        if (playerCardBg != null) playerCardBg.localScale = Vector3.one;
        if (cpuCardBg != null) cpuCardBg.localScale = Vector3.one;
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
        if (!currentGuess.HasValue || isAnimating) return;

        // ボタンを即座に無効化
        SetGuessButtonsInteractable(false);
        confirmButton.interactable = false;

        // カードめくりアニメーション開始
        StartCoroutine(RevealCardsWithFlip());
    }

    private IEnumerator RevealCardsWithFlip()
    {
        isAnimating = true;
        resultText.text = "";

        // 両カードを同時にめくる
        StartCoroutine(FlipCard(playerCardBg, playerCardText, gameManager.PlayerCard.ToString()));
        yield return StartCoroutine(FlipCard(cpuCardBg, cpuCardText, gameManager.CpuCard.ToString()));

        // アニメーション完了後に判定表示
        var result = gameManager.Judge(currentGuess.Value);
        resultText.text = result switch
        {
            GameManager.Result.Win => "勝ち！",
            GameManager.Result.Lose => "負け…",
            GameManager.Result.Draw => "引き分け（同じ数字）",
            _ => ""
        };

        retryButton.gameObject.SetActive(true);
        isAnimating = false;
    }

    /// <summary>
    /// カードを裏返すアニメーション。
    /// スケールXを1→0（裏面が消える）→テキスト変更→0→1（表面が現れる）
    /// </summary>
    private IEnumerator FlipCard(RectTransform cardBg, Text cardText, string newValue)
    {
        float half = flipDuration / 2f;

        // 前半：カードを閉じる（scaleX: 1 → 0）
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float progress = t / half;
            float scaleX = Mathf.Lerp(1f, 0f, progress);
            if (cardBg != null)
                cardBg.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        // 中間：テキストを差し替え
        if (cardBg != null)
            cardBg.localScale = new Vector3(0f, 1f, 1f);
        cardText.text = newValue;

        // 後半：カードを開く（scaleX: 0 → 1）
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float progress = t / half;
            float scaleX = Mathf.Lerp(0f, 1f, progress);
            if (cardBg != null)
                cardBg.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        // 最終値を確定
        if (cardBg != null)
            cardBg.localScale = Vector3.one;
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
