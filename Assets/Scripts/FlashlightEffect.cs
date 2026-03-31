using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤー周辺を明るくし、遠方を暗くする懐中電灯エフェクト。
/// 256x256 のテクスチャをスクリーン全体に重ねることで実現する。
/// </summary>
public class FlashlightEffect : MonoBehaviour
{
    public Transform PlayerTransform;
    public Camera GameCamera;
    public RawImage DarknessOverlay;
    public float LightRadiusFraction = 0.27f; // テクスチャ幅に対する光の半径の割合

    private const int TexSize = 256;
    private Texture2D tex;
    private Color32[] pixels;
    private Vector2 lastScreenPos;
    private const float UpdateThresholdPx = 4f;

    void Start()
    {
        tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        pixels = new Color32[TexSize * TexSize];
        RedrawTexture(new Vector2(TexSize * 0.5f, TexSize * 0.5f));
        if (DarknessOverlay) DarknessOverlay.texture = tex;
    }

    void LateUpdate()
    {
        if (PlayerTransform == null || GameCamera == null || DarknessOverlay == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying()) return;

        Vector3 sp = GameCamera.WorldToScreenPoint(PlayerTransform.position);
        Vector2 screenPos = new Vector2(sp.x, sp.y);
        if (Vector2.Distance(screenPos, lastScreenPos) < UpdateThresholdPx) return;
        lastScreenPos = screenPos;

        float tx = (sp.x / Screen.width)  * TexSize;
        float ty = (sp.y / Screen.height) * TexSize;
        RedrawTexture(new Vector2(tx, ty));
    }

    void RedrawTexture(Vector2 center)
    {
        float r  = LightRadiusFraction * TexSize;
        float ro = r * 1.9f;

        for (int i = 0; i < TexSize; i++)
        {
            for (int j = 0; j < TexSize; j++)
            {
                float dx = j - center.x;
                float dy = i - center.y;
                float d  = Mathf.Sqrt(dx * dx + dy * dy);

                byte a;
                if (d < r)
                    a = 0;
                else if (d < ro)
                    a = (byte)(Mathf.Pow((d - r) / (ro - r), 1.4f) * 230f);
                else
                    a = 230;

                pixels[i * TexSize + j] = new Color32(0, 0, 0, a);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply(false);
    }

    void OnDestroy()
    {
        if (tex != null) Destroy(tex);
    }
}
