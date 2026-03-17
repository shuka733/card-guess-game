using UnityEngine;

/// <summary>
/// カード予想ゲームのコアロジックを管理する。
/// UIには依存せず、結果の判定のみを行う。
/// </summary>
public class GameManager : MonoBehaviour
{
    public const int CardMin = 1;
    public const int CardMax = 13;

    public int PlayerCard { get; private set; }
    public int CpuCard { get; private set; }

    public enum Guess { Higher, Lower }
    public enum Result { Win, Lose, Draw }

    /// <summary>
    /// 新しいラウンドを開始し、両者にランダムなカードを配る。
    /// </summary>
    public void StartNewRound()
    {
        PlayerCard = Random.Range(CardMin, CardMax + 1);
        CpuCard = Random.Range(CardMin, CardMax + 1);
    }

    /// <summary>
    /// プレイヤーの予想に基づいて勝敗を判定する。
    /// </summary>
    public Result Judge(Guess guess)
    {
        if (PlayerCard == CpuCard)
            return Result.Draw;

        bool playerIsHigher = PlayerCard > CpuCard;

        if ((guess == Guess.Higher && playerIsHigher) ||
            (guess == Guess.Lower && !playerIsHigher))
            return Result.Win;

        return Result.Lose;
    }
}
