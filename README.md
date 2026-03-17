# Card Guess Game

1対1のプレイヤー vs CPU カード予想ゲーム（Unity / Android）

## ゲームルール

- プレイヤーと CPU がそれぞれ 1〜13 のランダムなカードを1枚持つ
- カードは最初伏せられている
- プレイヤーは自分のカードが CPU より「高い」か「低い」かを予想する
- 「確認」ボタンを押すと両方のカードが公開され、勝敗が決まる
- 同じ数字の場合は引き分け
- 「もう一度」ボタンで何度でも遊べる

## ローカル起動方法

1. Unity Hub で Unity 2022.3.8f1 を使用してプロジェクトを開く
2. `Assets/Scenes/GameScene.unity` を開く
3. Play ボタンで実行

## Android ビルド方法

### ローカルビルド

```bash
"C:\Program Files\Unity\Hub\Editor\2022.3.8f1\Editor\Unity.exe" \
  -batchmode -quit \
  -projectPath "（プロジェクトパス）" \
  -executeMethod BuildScript.BuildAndroid \
  -logFile -
```

APK は `Build/CardGuessGame.apk` に出力される。

### GitHub Actions 自動ビルド（要セットアップ）

1. Unity ライセンスファイルを取得する
2. GitHub リポジトリの Settings > Secrets に以下を設定：
   - `UNITY_LICENSE` : Unity ライセンス XML
   - `UNITY_EMAIL` : Unity アカウントメール
   - `UNITY_PASSWORD` : Unity アカウントパスワード
3. `main` ブランチに push すると自動ビルドが走る
4. ビルド成功時、GitHub Release に APK が自動アップロードされる

## 遠隔テスト配布方式

現在の方式: **GitHub Releases による APK 配布**

### 更新フロー

```
スマホから Claude Code Remote Control で修正を指示
  ↓
PC 側でコード修正
  ↓
git add → git commit → git push
  ↓
ローカルで APK ビルド（または GitHub Actions 自動ビルド）
  ↓
gh release create で APK をアップロード
  ↓
スマホから GitHub Release ページで APK をダウンロード・インストール
```

### クイック更新コマンド（PC側）

```bash
# 1. ビルド
"C:\Program Files\Unity\Hub\Editor\2022.3.8f1\Editor\Unity.exe" \
  -batchmode -quit \
  -projectPath "/c/Users/k2000/Documents/01_app_dev/newgame_temp" \
  -executeMethod BuildScript.BuildAndroid -logFile -

# 2. リリース作成
cd "（プロジェクトフォルダ）"
gh release create v0.x.x Build/CardGuessGame.apk --title "vX.X.X" --notes "更新内容"
```

## ディレクトリ構成

```
.
├── .github/workflows/
│   └── build-android.yml    # GitHub Actions ワークフロー
├── Assets/
│   ├── Editor/
│   │   ├── AndroidBuildConfigurator.cs  # Android設定スクリプト
│   │   ├── BuildScript.cs              # APKビルドスクリプト
│   │   └── SceneBuilder.cs             # シーン自動構築スクリプト
│   ├── Scenes/
│   │   └── GameScene.unity             # メインシーン
│   └── Scripts/
│       ├── GameManager.cs              # ゲームロジック（判定・カード管理）
│       └── UIController.cs             # UI制御（表示・入力）
├── Packages/
├── ProjectSettings/
├── .gitignore
└── README.md
```

## 設計方針

- **GameManager**: ゲームロジックのみ。UIに依存しない
- **UIController**: UI表示と入力を管理。GameManagerに委譲
- **SceneBuilder**: エディタスクリプトでシーンをコードから構築（Claude Code で修正しやすい）
- シーン数は1つ、UI主体の構成

## 今後の拡張候補

- スコア記録（連勝カウント）
- カードのビジュアル強化（画像・アニメーション）
- 効果音・BGM追加
- 複数ラウンド制
- オンライン対戦
- GitHub Actions 自動ビルドの有効化
- Firebase App Distribution への配布切り替え
