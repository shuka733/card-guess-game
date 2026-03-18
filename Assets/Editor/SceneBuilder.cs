using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// バッチモードからシーンを自動構築するエディタスクリプト。
/// 実行後は不要なので削除可。
/// </summary>
public class SceneBuilder
{
    [MenuItem("Tools/Build Game Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- EventSystem ---
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // --- Background Panel ---
        var bg = CreatePanel(canvasObj.transform, "Background", new Color(0.15f, 0.15f, 0.2f));
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // --- Title ---
        var title = CreateText(canvasObj.transform, "TitleText", "Card Guess Game",
            new Vector2(0, 800), new Vector2(800, 100), 60, Color.white, FontStyle.Bold);

        // --- Player Card Area ---
        var playerLabel = CreateText(canvasObj.transform, "PlayerLabel", "あなたのカード",
            new Vector2(0, 500), new Vector2(400, 60), 36, Color.white, FontStyle.Normal);

        var playerCardBg = CreatePanel(canvasObj.transform, "PlayerCardBg", new Color(0.3f, 0.5f, 0.8f));
        SetRect(playerCardBg, new Vector2(0, 350), new Vector2(200, 280));
        var playerCardText = CreateText(playerCardBg.transform, "PlayerCardText", "?",
            Vector2.zero, new Vector2(200, 280), 80, Color.white, FontStyle.Bold);
        playerCardText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        playerCardText.GetComponent<RectTransform>().anchorMax = Vector2.one;
        playerCardText.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        playerCardText.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // --- VS ---
        CreateText(canvasObj.transform, "VSText", "VS",
            new Vector2(0, 100), new Vector2(200, 80), 48, new Color(1f, 0.8f, 0.2f), FontStyle.Bold);

        // --- CPU Card Area ---
        var cpuLabel = CreateText(canvasObj.transform, "CpuLabel", "CPUのカード",
            new Vector2(0, -50), new Vector2(400, 60), 36, Color.white, FontStyle.Normal);

        var cpuCardBg = CreatePanel(canvasObj.transform, "CpuCardBg", new Color(0.8f, 0.3f, 0.3f));
        SetRect(cpuCardBg, new Vector2(0, -200), new Vector2(200, 280));
        var cpuCardText = CreateText(cpuCardBg.transform, "CpuCardText", "?",
            Vector2.zero, new Vector2(200, 280), 80, Color.white, FontStyle.Bold);
        cpuCardText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        cpuCardText.GetComponent<RectTransform>().anchorMax = Vector2.one;
        cpuCardText.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        cpuCardText.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // --- Guess Buttons ---
        var higherBtn = CreateButton(canvasObj.transform, "HigherButton", "高い",
            new Vector2(-150, -450), new Vector2(250, 80), 36);
        var lowerBtn = CreateButton(canvasObj.transform, "LowerButton", "低い",
            new Vector2(150, -450), new Vector2(250, 80), 36);

        // --- Confirm Button ---
        var confirmBtn = CreateButton(canvasObj.transform, "ConfirmButton", "確認",
            new Vector2(0, -570), new Vector2(350, 80), 40);
        confirmBtn.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);

        // --- Result Text ---
        var resultText = CreateText(canvasObj.transform, "ResultText", "",
            new Vector2(0, -690), new Vector2(800, 80), 40, new Color(1f, 1f, 0.5f), FontStyle.Bold);

        // --- Retry Button ---
        var retryBtn = CreateButton(canvasObj.transform, "RetryButton", "もう一度",
            new Vector2(0, -800), new Vector2(350, 80), 40);
        retryBtn.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.8f);

        // --- GameManager ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();

        // --- UIController ---
        var uiObj = new GameObject("UIController");
        var ui = uiObj.AddComponent<UIController>();

        // Set serialized fields via SerializedObject
        var so = new SerializedObject(ui);
        so.FindProperty("gameManager").objectReferenceValue = gm;
        so.FindProperty("playerCardText").objectReferenceValue = playerCardText.GetComponent<Text>();
        so.FindProperty("cpuCardText").objectReferenceValue = cpuCardText.GetComponent<Text>();
        so.FindProperty("playerCardBg").objectReferenceValue = playerCardBg.GetComponent<RectTransform>();
        so.FindProperty("cpuCardBg").objectReferenceValue = cpuCardBg.GetComponent<RectTransform>();
        so.FindProperty("higherButton").objectReferenceValue = higherBtn.GetComponent<Button>();
        so.FindProperty("lowerButton").objectReferenceValue = lowerBtn.GetComponent<Button>();
        so.FindProperty("confirmButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
        so.FindProperty("retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        so.FindProperty("resultText").objectReferenceValue = resultText.GetComponent<Text>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");

        // Set as build scene
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };

        AssetDatabase.SaveAssets();
        Debug.Log("=== Scene built successfully ===");
    }

    private static GameObject CreateText(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, int fontSize, Color color, FontStyle style)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var t = obj.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, int fontSize)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        obj.AddComponent<Image>().color = Color.white;
        obj.AddComponent<Button>();

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        var trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var t = textObj.AddComponent<Text>();
        t.text = label;
        t.fontSize = fontSize;
        t.color = Color.black;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return obj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<Image>().color = color;
        return obj;
    }

    private static void SetRect(GameObject obj, Vector2 pos, Vector2 size)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
