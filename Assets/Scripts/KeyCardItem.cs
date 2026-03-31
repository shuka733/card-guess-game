using UnityEngine;

public class KeyCardItem : MonoBehaviour
{
    public bool IsCollected { get; private set; }

    private SpriteRenderer sr;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (IsCollected) return;
        timer += Time.deltaTime * 2.5f;
        float pulse = Mathf.Sin(timer) * 0.15f + 0.85f;
        if (sr != null) sr.color = new Color(1f, 0.75f * pulse, 0f, 1f);
    }

    public void Collect()
    {
        if (IsCollected) return;
        IsCollected = true;
        GameManager.Instance.CollectKeyCard();
        gameObject.SetActive(false);
    }
}
