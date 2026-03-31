using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null || GameManager.Instance == null) return;
        sr.color = GameManager.Instance.HasAllKeyCards()
            ? new Color(0.2f, 1f, 0.3f)
            : new Color(0.25f, 0.25f, 0.25f);
    }

    public void TryOpen()
    {
        if (GameManager.Instance.HasAllKeyCards())
            GameManager.Instance.Win();
        else
        {
            int need = GameManager.TotalKeyCards - GameManager.Instance.KeyCardsCollected;
            UIManager.Instance.ShowMessage("あと " + need + " 枚必要です");
        }
    }
}
