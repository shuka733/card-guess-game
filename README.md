# Silent Ward（廃病棟）

Android向けホラー脱出ゲーム。Unity 2022.3.8f1 製。

## ゲーム概要

廃病棟に閉じ込められた。出口は施錠されている。
**鍵カードを3枚**集めて脱出口を開け、逃げ出せ。
ただし… 何かが徘徊している。

| 項目 | 内容 |
|------|------|
| ジャンル | ホラー探索 / 脱出 |
| プレイ時間 | 約5分 |
| プラットフォーム | Android |
| 操作 | バーチャルジョイスティック（移動）＋右タップ（操作） |

## 操作方法

- **左側ジョイスティック** — キャラクター移動
- **右半分タップ** — 近くのアイテムを調べる / 扉を操作する

## ゲームの流れ

1. 廃病棟内を探索し、**黄色い鍵カード**3枚を集める
2. 鍵カードが揃うと出口扉が**緑色**に変わる
3. 出口扉に近付いて右タップで脱出成功
4. 赤い敵に接触すると即ゲームオーバー

## Android へのインストール

GitHub Releases から最新の `SilentWard.apk` をダウンロードして端末でタップするだけ。

> 初回インストール時は「提供元不明のアプリ」の許可が必要です（設定 > セキュリティ）。

## ローカルビルド（Unity Editor）

1. Unity 2022.3.8f1 でプロジェクトを開く
2. メニュー **[Horror > Build Scene]** を実行してシーンを生成
3. **File > Build Settings** で Android を選択し **Build** を実行

### バッチモードビルド

```bash
Unity.exe -quit -batchmode -projectPath . \
  -executeMethod BuildScript.BuildAndroid \
  -logFile build.log
```

## プロジェクト構成

```
Assets/
├── Editor/
│   ├── HorrorSceneBuilder.cs   # シーン自動生成
│   └── BuildScript.cs          # APKビルドスクリプト
├── Scenes/
│   └── HorrorGame.unity        # ゲームシーン（BuildScene後に生成）
└── Scripts/
    ├── GameManager.cs          # ゲーム状態管理
    ├── PlayerController.cs     # プレイヤー移動・操作
    ├── EnemyAI.cs              # 敵AI（巡回/警戒/追跡）
    ├── KeyCardItem.cs          # 収集アイテム
    ├── ExitDoor.cs             # 出口ドア
    ├── UIManager.cs            # UI管理
    ├── VirtualJoystick.cs      # タッチジョイスティック
    ├── FlashlightEffect.cs     # 懐中電灯（暗闇エフェクト）
    └── CameraFollow.cs         # カメラ追従
```
