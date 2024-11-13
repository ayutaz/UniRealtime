# UniRealtime

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blueviolet?logo=unity)](https://unity.com/releases/editor/archive)
[![Releases](https://img.shields.io/github/release/ayutaz/UniRealtime.svg)](https://github.com/ayutaz/UniRealtime/releases)

UniRealtime is a library that makes it easy to integrate OpenAI's Realtime API into your Unity project, enabling interactive voice and text-based conversations. With this library, you can smoothly implement low-latency, multimodal character interactions and real-time AI responses.

The repository includes sample scenes that can be cloned and run immediately, and the library can also be integrated into your project as a standalone component.

<!-- TOC -->
* [UniRealtime](#unirealtime)
* [Features Overview](#features-overview)
* [Setting Up the Sample Project](#setting-up-the-sample-project)
* [Supported OpenAI API Features](#supported-openai-api-features)
* [Usage as a Library](#usage-as-a-library)
  * [Installing Dependencies](#installing-dependencies)
  * [Package Manager](#package-manager)
  * [Unity Package](#unity-package)
* [System Requirements](#system-requirements)
* [Dependencies](#dependencies)
* [How to Use the Sample](#how-to-use-the-sample)
* [License](#license)
<!-- TOC -->

# Features Overview
* Low-latency voice interaction: Harness the power of the OpenAI Realtime API to enable real-time voice interactions with characters.
* Multimodal support: Supports bi-directional input/output for both text and voice.
* Speech synthesis: Provides natural, emotionally enriched voice responses.
* WebSocket communication: Achieves efficient, low-latency bi-directional communication using WebSockets.

# Setting Up the Sample Project
* The following are steps to set up the sample scene for using OpenAI's Realtime API in a Unity project:

1. Access OpenAI API and obtain an API key.
2. Open your project in Unity.
3. Open the RealtimeAIDemo scene located at `Assets/UniRealtime/Sample/Scenes`.
4. Set your OpenAI API key within the project.

Once you run the project, it should perform similar to the image below.

![](Docs/sampleSceneImage.png)

# Supported OpenAI API Features
* Text & Audio Input/Output: Supports input and output of both text and voice, enabling interactive voice conversations.
* Function Calls: Enables real-time execution of functions by the AI models.
* Natural voice output: Provides tonally rich and natural voice interactions, delivering seamless, low-latency conversations.

# Usage as a Library

## Installing Dependencies

## Package Manager
1. Open the Package Manager.
2. Click the `+` button, then select `Add package from git URL`.
3. Enter `https://github.com/ayutaz/UniRealtime.git?path=/Assets/UniRealtime/Scripts`.

## Unity Package
1. Install UniTask and unity-websocket.
2. Download the latest Unity package from the release page.
3. Import the downloaded Unity package into your project.

# System Requirements
* Unity: Version 2021.3.14f1 or higher
* OpenAI Realtime API: Supports the latest API versions
* WebSocket: Supports high-speed WebSocket communication

# Dependencies
* [UniTask](https://github.com/Cysharp/UniTask)
* [unity-websocket](https://github.com/mikerochip/unity-websocket)

# How to Use the Sample
* The sample includes a simple character interaction system that handles from voice input, AI response, to real-time speech synthesis. You can follow these steps to use it:
    * Use a microphone or another input device to capture voice in real-time.
    * Send the captured voice or text to the OpenAI Realtime API and receive the response.
    * Have in-game characters respond to the interaction, enabling seamless conversations.

# License
This project is provided under the terms of the Apache-2.0 License. See the LICENSE file for more details.

However, the font used in the sample scene is [Noto Sans JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) from [Google Fonts](https://fonts.google.com/), which is distributed under the [SIL Open Font License, Version 1.1](https://openfontlicense.org/open-font-license-official-text/).