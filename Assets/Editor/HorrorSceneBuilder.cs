using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using System.Collections.Generic;

/// <summary>
/// "Silent Ward" ホラーゲームのシーンをコードから自動構築するエディタスクリプト。
/// Unity メニュー [Horror/Build Scene] またはバッチモードから呼び出す。
/// </summary>
public class HorrorSceneBuilder
{
    // マップ定義
    // # = 壁, . = 床, S = プレイヤー開始, M = 敵開始
    // K = 鍵カード (3枚), E = 出口扉
    static readonly string[] Map =
    {
        "####################",
        "#S.................#",
        "#..................#",
        "#..######..........#",
        "#..#K....#.........#",
        "#..#.....#.........#",
        "#..#.....#....M....#",
        "#..######..........#",
        "#..................#",
        "#......K...........#",
        "#..................#",
        "#...........K......#",
        "#.....######.......#",
        "#.....#E...#.......#",
        "#.....######.......#",
        "####################",
    };

    const float Cell = 1f;

    // ─────────────────────────────────────────────────────────
    //  エントリポイント（メニュー / バッチモード共通）
    // ─────────────────────────────────────────────────────────

    [MenuItem("Horror/Build Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        int rows = Map.Length;
        int cols  = Map[0].Length;

        // レイヤーのルートオブジェクト
        var wallsRoot  = new GameObject("Walls");
        var floorsRoot = new GameObject("Floors");
        var itemsRoot  = new GameObject("Items");

        // ───── スプライト素材生成 ─────
        Sprite wallSprite     = MakeSquareSprite();
        Sprite floorSprite    = MakeSquareSprite();
        Sprite playerSprite   = MakeCircleSprite(32);
        Sprite enemySprite    = MakeSquareSprite();
        Sprite keycardSprite  = MakeSquareSprite();
        Sprite exitSprite     = MakeSquareSprite();
        Sprite joystickSprite = MakeSoftCircleSprite(64);

        // 位置収集
        Vector3 playerStart  = Vector3.zero;
        Vector3 enemyStart   = new Vector3(10, -8, 0);
        var keycardPositions = new List<Vector3>();
        Vector3 exitPos      = new Vector3(7, -13, 0);

        // ───── マップ構築 ─────
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < Map[r].Length; c++)
            {
                char ch = Map[r][c];
                Vector3 pos = new Vector3(c * Cell, -(r * Cell), 0);

                if (ch == '#')
                {
                    var w = MakeSpriteObj("W_" + r + "_" + c, wallSprite,
                        new Color(0.13f, 0.10f, 0.13f), pos, wallsRoot.transform, 0);
                    w.AddComponent<BoxCollider2D>();
                }
                else
                {
                    // 床
                    MakeSpriteObj("F_" + r + "_" + c, floorSprite,
                        new Color(0.07f, 0.06f, 0.07f), pos + Vector3.forward * 0.1f,
                        floorsRoot.transform, -1);

                    switch (ch)
                    {
                        case 'S': playerStart = pos; break;
                        case 'M': enemyStart  = pos; break;
                        case 'K': keycardPositions.Add(pos); break;
                        case 'E': exitPos = pos; break;
                    }
                }
            }
        }

        // ───── カメラ ─────
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.depth = -1;
        camObj.transform.position = new Vector3(playerStart.x, playerStart.y, -10f);

        var follow = camObj.AddComponent<CameraFollow>();
        follow.Cam = cam;
        follow.SetBounds(0f, (cols - 1) * Cell, -(rows - 1) * Cell, 0f);

        // ───── プレイヤー ─────
        var playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.transform.position = playerStart;
        var playerSR = playerObj.AddComponent<SpriteRenderer>();
        playerSR.sprite = playerSprite;
        playerSR.color  = new Color(0.9f, 0.9f, 1f);
        playerSR.sortingOrder = 2;
        var playerRB = playerObj.AddComponent<Rigidbody2D>();
        playerRB.gravityScale = 0;
        playerRB.constraints  = RigidbodyConstraints2D.FreezeRotation;
        playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var playerCol = playerObj.AddComponent<CircleCollider2D>();
        playerCol.radius = 0.35f;
        playerObj.AddComponent<PlayerController>();
        follow.Target = playerObj.transform;

        // ───── 敵 ─────
        var enemyObj = new GameObject("Enemy");
        enemyObj.transform.position = enemyStart;
        var enemySR = enemyObj.AddComponent<SpriteRenderer>();
        enemySR.sprite = enemySprite;
        enemySR.color  = new Color(0.75f, 0.05f, 0.05f);
        enemySR.sortingOrder = 2;
        var enemyRB = enemyObj.AddComponent<Rigidbody2D>();
        enemyRB.gravityScale = 0;
        enemyRB.constraints  = RigidbodyConstraints2D.FreezeRotation;
        enemyRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var enemyCol = enemyObj.AddComponent<CircleCollider2D>();
        enemyCol.radius = 0.42f;
        var enemyAI = enemyObj.AddComponent<EnemyAI>();

        // 巡回ポイント（敵スポーン付近を回る）
        var ppRoot = new GameObject("PatrolPoints");
        ppRoot.transform.SetParent(enemyObj.transform);
        var patrolOffsets = new Vector2[]
        {
            new Vector2( 0,  0),
            new Vector2( 4,  0),
            new Vector2( 4, -4),
            new Vector2( 0, -4),
        };
        foreach (var off in patrolOffsets)
        {
            var pp = new GameObject("PP");
            pp.transform.position = enemyStart + new Vector3(off.x, off.y, 0);
            pp.transform.SetParent(ppRoot.transform);
            enemyAI.PatrolPoints.Add(pp.transform);
        }

        // ───── 鍵カード ─────
        foreach (var kp in keycardPositions)
        {
            var kc = MakeSpriteObj("KeyCard", keycardSprite,
                new Color(1f, 0.8f, 0f), kp, itemsRoot.transform, 1);
            kc.transform.localScale = Vector3.one * 0.55f;
            var kcCol = kc.AddComponent<BoxCollider2D>();
            kcCol.isTrigger = true;
            kc.AddComponent<KeyCardItem>();
        }

        // ───── 出口 ─────
        var exit = MakeSpriteObj("ExitDoor", exitSprite,
            new Color(0.25f, 0.25f, 0.25f), exitPos, itemsRoot.transform, 1);
        exit.transform.localScale = Vector3.one * 0.85f;
        var exitCol = exit.AddComponent<BoxCollider2D>();
        exitCol.isTrigger = true;
        exit.AddComponent<ExitDoor>();

        // ───── マネージャ ─────
        new GameObject("GameManager").AddComponent<GameManager>();

        // ───── UI ─────
        BuildUI(canvasCam: null, playerObj: playerObj,
            cam: cam, camObj: camObj, joystickSprite: joystickSprite);

        // ───── EventSystem ─────
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // ───── シーン保存 ─────
        const string scenePath = "Assets/Scenes/HorrorGame.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        AssetDatabase.SaveAssets();
        Debug.Log("[HorrorSceneBuilder] Scene saved to: " + scenePath);
    }

    // ─────────────────────────────────────────────────────────
    //  UI 構築
    // ─────────────────────────────────────────────────────────

    static void BuildUI(Camera canvasCam, GameObject playerObj,
        Camera cam, GameObject camObj, Sprite joystickSprite)
    {
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 暗闇オーバーレイ（FlashlightEffect が書き込む RawImage）
        var darknessObj = new GameObject("DarknessOverlay");
        darknessObj.transform.SetParent(canvasObj.transform, false);
        var darknessRI = darknessObj.AddComponent<RawImage>();
        darknessRI.color = Color.white;
        StretchFull(darknessObj.GetComponent<RectTransform>());
        darknessObj.GetComponent<RectTransform>().SetAsLastSibling(); // 後で整理

        // FlashlightEffect をカメラに付ける
        var fl = camObj.AddComponent<FlashlightEffect>();
        fl.PlayerTransform = playerObj.transform;
        fl.GameCamera = cam;
        fl.DarknessOverlay = darknessRI;

        // UIManager
        var uiMgrObj = new GameObject("UIManager");
        uiMgrObj.transform.SetParent(canvasObj.transform, false);
        var uiMgr = uiMgrObj.AddComponent<UIManager>();

        // ── Main Menu ──
        var mainMenu = MakePanel("MainMenuPanel", canvasObj.transform, new Color(0, 0, 0, 0.92f));
        {
            var title = MakeText("TitleText", mainMenu.transform,
                "SILENT WARD", 72, new Color(0.85f, 0.05f, 0.05f));
            SetAnchoredPos(title, new Vector2(0, 400));

            var sub = MakeText("SubtitleText", mainMenu.transform,
                "廃病棟に閉じ込められた。\n鍵カードを3枚集めて脱出せよ。\n…あれに捕まるな。", 28, Color.gray);
            sub.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            SetSizeDelta(sub, new Vector2(850, 120));
            SetAnchoredPos(sub, new Vector2(0, 150));

            var startBtn = MakeButton("StartButton", mainMenu.transform, "START", new Vector2(0, -100));
            UnityEventTools.AddPersistentListener(startBtn.GetComponent<Button>().onClick, uiMgr.OnStartButton);
        }

        // ── HUD ──
        var hud = MakePanel("HUDPanel", canvasObj.transform, Color.clear);
        hud.SetActive(false);
        {
            // 鍵カード表示（左上）
            var kcText = MakeText("KeyCardText", hud.transform, "鍵カード: 0/3", 30, Color.yellow);
            var kcRect = kcText.GetComponent<RectTransform>();
            kcRect.anchorMin = new Vector2(0, 1);
            kcRect.anchorMax = new Vector2(0, 1);
            kcRect.pivot     = new Vector2(0, 1);
            kcRect.anchoredPosition = new Vector2(20, -20);
            kcRect.sizeDelta = new Vector2(300, 50);

            // メッセージ（中央下寄り）
            var msgText = MakeText("MessageText", hud.transform, "", 32, Color.white);
            msgText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            SetSizeDelta(msgText, new Vector2(700, 70));
            SetAnchoredPos(msgText, new Vector2(0, -250));
            msgText.SetActive(false);

            // 危険警告（上中央）
            var warnText = MakeText("WarningText", hud.transform, "！　危険　！", 48, Color.red);
            warnText.GetComponent<Text>().alignment = TextAnchor.UpperCenter;
            var warnRect = warnText.GetComponent<RectTransform>();
            warnRect.anchorMin = new Vector2(0, 1);
            warnRect.anchorMax = new Vector2(1, 1);
            warnRect.pivot     = new Vector2(0.5f, 1);
            warnRect.anchoredPosition = new Vector2(0, -10);
            warnRect.sizeDelta = new Vector2(0, 60);
            warnText.SetActive(false);

            // バーチャルジョイスティック（左下）
            var jsBg = new GameObject("JoystickBg");
            jsBg.transform.SetParent(hud.transform, false);
            var jsBgImg = jsBg.AddComponent<Image>();
            jsBgImg.sprite = joystickSprite;
            jsBgImg.color  = new Color(1f, 1f, 1f, 0.25f);
            var jsBgRect = jsBg.GetComponent<RectTransform>();
            jsBgRect.anchorMin = new Vector2(0, 0);
            jsBgRect.anchorMax = new Vector2(0, 0);
            jsBgRect.pivot     = new Vector2(0.5f, 0.5f);
            jsBgRect.sizeDelta = new Vector2(220, 220);
            jsBgRect.anchoredPosition = new Vector2(160, 190);

            var jsKnob = new GameObject("JoystickKnob");
            jsKnob.transform.SetParent(jsBg.transform, false);
            var jsKnobImg = jsKnob.AddComponent<Image>();
            jsKnobImg.sprite = joystickSprite;
            jsKnobImg.color  = new Color(1f, 1f, 1f, 0.55f);
            var jsKnobRect = jsKnob.GetComponent<RectTransform>();
            jsKnobRect.sizeDelta = new Vector2(90, 90);
            jsKnobRect.anchoredPosition = Vector2.zero;

            var joystick = jsBg.AddComponent<VirtualJoystick>();
            joystick.Background = jsBgRect;
            joystick.Knob       = jsKnobRect;
            joystick.MaxRadius  = 65f;

            // 操作ヒント（右下）
            var hint = MakeText("InteractHint", hud.transform, "右タップ\nで操作", 22, new Color(1, 1, 1, 0.4f));
            hint.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            var hRect = hint.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(1, 0);
            hRect.anchorMax = new Vector2(1, 0);
            hRect.pivot     = new Vector2(1, 0);
            hRect.sizeDelta = new Vector2(180, 70);
            hRect.anchoredPosition = new Vector2(-30, 30);

            // UIManager にフィールドをセット
            uiMgr.KeyCardText = kcText.GetComponent<Text>();
            uiMgr.MessageText = msgText.GetComponent<Text>();
            uiMgr.WarningText = warnText.GetComponent<Text>();
        }

        // ── Game Over ──
        var gameOver = MakePanel("GameOverPanel", canvasObj.transform, new Color(0, 0, 0, 0.88f));
        gameOver.SetActive(false);
        {
            var t1 = MakeText("GOTitle", gameOver.transform, "捕まった…", 70, Color.red);
            SetAnchoredPos(t1, new Vector2(0, 250));

            var t2 = MakeText("GOSub", gameOver.transform, "暗闇に飲み込まれた。", 30, Color.gray);
            t2.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            SetAnchoredPos(t2, new Vector2(0, 100));

            var btn = MakeButton("RestartBtn", gameOver.transform, "もう一度", new Vector2(0, -120));
            UnityEventTools.AddPersistentListener(btn.GetComponent<Button>().onClick, uiMgr.OnRestartButton);
        }

        // ── Win ──
        var win = MakePanel("WinPanel", canvasObj.transform, new Color(0, 0.08f, 0, 0.88f));
        win.SetActive(false);
        {
            var t1 = MakeText("WinTitle", win.transform, "脱出成功！", 70, Color.green);
            SetAnchoredPos(t1, new Vector2(0, 250));

            var t2 = MakeText("WinSub", win.transform, "廃病棟から生還した。", 30, Color.white);
            t2.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            SetAnchoredPos(t2, new Vector2(0, 100));

            var btn = MakeButton("WinRestartBtn", win.transform, "もう一度", new Vector2(0, -120));
            UnityEventTools.AddPersistentListener(btn.GetComponent<Button>().onClick, uiMgr.OnRestartButton);
        }

        // 暗闇レイヤーを最前面へ（UI の上から暗くする）
        darknessObj.transform.SetSiblingIndex(1); // HUD の直下

        // UIManager のパネル参照をセット
        uiMgr.MainMenuPanel = mainMenu;
        uiMgr.HUDPanel      = hud;
        uiMgr.GameOverPanel = gameOver;
        uiMgr.WinPanel      = win;
    }

    // ─────────────────────────────────────────────────────────
    //  ヘルパー
    // ─────────────────────────────────────────────────────────

    static GameObject MakeSpriteObj(string name, Sprite sprite, Color color,
        Vector3 pos, Transform parent, int sortOrder)
    {
        var obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.SetParent(parent);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color  = color;
        sr.sortingOrder = sortOrder;
        return obj;
    }

    static GameObject MakePanel(string name, Transform parent, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        StretchFull(obj.GetComponent<RectTransform>());
        return obj;
    }

    static GameObject MakeText(string name, Transform parent, string content,
        int fontSize, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var t = obj.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.color     = color;
        t.alignment = TextAnchor.MiddleLeft;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 70);
        return obj;
    }

    static GameObject MakeButton(string name, Transform parent, string label, Vector2 pos)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.color = new Color(0.18f, 0.06f, 0.06f);
        obj.AddComponent<Button>();
        var rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 90);
        rt.anchoredPosition = pos;

        var textObj = MakeText(name + "_Label", obj.transform, label, 38, Color.white);
        textObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        StretchFull(textObj.GetComponent<RectTransform>());
        return obj;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    static void SetAnchoredPos(GameObject obj, Vector2 pos) =>
        obj.GetComponent<RectTransform>().anchoredPosition = pos;

    static void SetSizeDelta(GameObject obj, Vector2 size) =>
        obj.GetComponent<RectTransform>().sizeDelta = size;

    // ─────────────────────────────────────────────────────────
    //  スプライト生成
    // ─────────────────────────────────────────────────────────

    static Sprite MakeSquareSprite()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        var px = new Color[s * s];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        float cx = size * 0.5f, r = size * 0.5f - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cx));
                tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    static Sprite MakeSoftCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = size * 0.5f, r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cx));
                float a = Mathf.Clamp01(1f - Mathf.Max(0f, d - r + 2f) / 2f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
