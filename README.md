# UniRealtime

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blueviolet?logo=unity)](https://unity.com/releases/editor/archive)
[![Releases](https://img.shields.io/github/release/ayutaz/UniRealtime.svg)](https://github.com/ayutaz/UniRealtime/releases)

日本語 | [English](README_EN.md)

UniRealtimeは、OpenAIのRealtime APIをUnityプロジェクトに簡単に統合し、インタラクティブな音声およびテキストベースの対話を実現するためのライブラリです。このライブラリを使えば、低遅延でマルチモーダルなキャラクター対話や、リアルタイムでのAI応答をスムーズに実装できます。

サンプルシーンを含んでいるため、クローンしてすぐに動作を確認できます。また、ライブラリとしてプロジェクトに組み込むことも可能です。

<!-- TOC -->
* [UniRealtime](#unirealtime)
* [サンプルプロジェクトのセットアップ](#サンプルプロジェクトのセットアップ)
* [ライブラリとしての使用方法](#ライブラリとしての使用方法)
  * [依存ライブラリのインストール](#依存ライブラリのインストール)
  * [Package Manager](#package-manager)
  * [Unity Package](#unity-package)
* [動作環境](#動作環境)
* [サンプルの使用方法](#サンプルの使用方法)
* [License](#license)
<!-- TOC -->

# サンプルプロジェクトのセットアップ
* 以下は、Unityを使ったプロジェクトでOpenAIのRealtime APIを利用するためのサンプルシーンのセットアップ手順です。

1. OpenAI APIにアクセスして、APIキーを取得。
2. Unityでプロジェクトを開きます。
3. `Assets/UniRealtime/Sample/Scenes` にあるRealtimeAIDemoシーンを開きます。
4. OpenAIのAPIキーをプロジェクト内で設定します。

実行することで以下の画像のように動作します。

![](Docs/sampleSceneImage.png)

# ライブラリとしての使用方法

## 依存ライブラリのインストール

1. [unity-websocket](https://github.com/mikerochip/unity-websocket)をインストールします

2. 任意で[UniTask](https://github.com/Cysharp/UniTask)をインストールします。こちらをしようしない場合は，内部処理はTaskを使用します。

## Package Manager
1. Package Managerを開きます。
2. `+` ボタンをクリックし、`Add package from git URL` を選択します。
3. `https://github.com/ayutaz/UniRealtime.git?path=/Assets/UniRealtime/Scripts`

## Unity Package
1. UniTaskおよびunity-websocketをインストールします。
2. リリースページから最新のUnityパッケージをダウンロードします。
3. ダウンロードしたUnityパッケージをプロジェクトにインポートします。

# 動作環境
* Unity: 2021.3.14f1 以降
* OpenAI Realtime API: 最新のAPIバージョンに対応

# サンプルの使用方法
* サンプルには、音声入力からAI応答、さらにリアルタイムでの音声合成までを行うシンプルなキャラクター対話システムが含まれています。以下のステップで利用可能です。
* マイクや他の入力デバイスを用いて音声をリアルタイムで取得。
* 音声やテキストをOpenAI Realtime APIに送り、応答を受信。
* Unity内のキャラクターに応答させることで、インタラクティブな対話が可能。

# License
このプロジェクトはApache-2.0ライセンスの条件下で提供されています。詳細はLICENSEファイルを参照してください。

ただしサンプルシーンに使用しているFontは、[Google Fonts](https://fonts.google.com/)の[NotoSansJP](https://fonts.google.com/noto/specimen/Noto+Sans+JP)を使用しています。Noto Sans JPは[ASIL Open Font License, Version 1.1](https://openfontlicense.org/open-font-license-official-text/)の下で提供されています。