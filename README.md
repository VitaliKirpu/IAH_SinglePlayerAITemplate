<p align="center">  <img src="iah_logo.png" width="350" title="IAH: INTERNET WAR logo"/> </p>

IAH: INTERNET WAR is a unique and strategic turn-based RTS where you immerse yourself in an endless internet conflict. In this game, you'll embark on a thrilling journey to capture internet websites, eliminate malware, battle powerful corporations, resist government authorities, and outwit rival hackers.

**Gameplay Features:**

**Upgrade Your Bots:** Customize and enhance your bots with various parts and power-ups, giving you the edge in battle. Take control of these upgraded bots as you navigate through the digital battlefield.

**Replayable Campaign:** Engage in a replayable run-based hacking campaign, ensuring that no two playthroughs are the same. Adapt your strategies and face new challenges each time you play.
Diverse Bot Arsenal: Command over 100 different bots, each with its unique role and abilities. Some are designed for support, while others excel in direct combat. Explore real-time tactical battles with turn-based pacing, where your actions trigger real-time encounters and responses.


**Sandbox Mode:** Experiment and create your own scenarios in Sandbox Mode, separate from the main campaign. Spawn units, manipulate tiles, and craft custom challenges. This mode is ideal for players interested in programming and participating in the IAH algorithmic multiplayer.


**IAH Algorithmic Multiplayer:** For programmers seeking a competitive challenge, IAH offers algorithmic multiplayer. In this mode, you cannot use a cursor or controller; your interactions are limited to the API Interface. Create or join competitive clubs, write code solo or collaborate in a group, and use your preferred IDE and programming language to wage highly competitive algorithmic wars.


**Storyline:**
The world is in turmoil as ZENDAR CORP, the most influential corporation on the planet, wages an invisible internet cyber war against modern society. Their sinister objective is to install a centralized blackhat government AI. As a skilled hacker, you must rise to the call, battling ZENDAR CORP and defending the free and open internet.
This repository contains starter C# code to help you start writing your own IAH AI Code in C#.

<p align="center"><img src="GIF_1.gif" title="combat bot robots shooting"/> </p>

IAH: INTERNET WAR Steam Page: **https://store.steampowered.com/app/304770/IAH_INTERNET_WAR/**

Main Website: **https://iamhacker.cc/**

Online API Documentation: **https://iamhacker.cc/documentation**

Singleplayer API Documentation **[https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/wiki](https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/wiki/IAH:-INTERNET-WAR-%7C-SINGLEPLAYER-AI-API)**

<p align="center"><img src="GIF_2.gif" title="combat bot robots shooting"/> </p>


### Quick Start Guide

1. Obtain the API Key from the Main Website and paste your own here: https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/blob/cc9d2fe329d34d837b78799a853de09222ec799a/IAH_SinglePlayerAutomation/Program.cs#L133
2. Launch IAH: INTERNET WAR Game. Default AI Port is 6800, but you can change this with -apiPort 6900 launch parameter
3. Launch this Example API Template Project; automation should occur now. From the main menu, the AI will navigate the UI, select Hacker, enter the game, perform all necessary actions, create units, and battle enemies if they are present.

### Troubleshooting
- AI Template Crashes on Launch, or nothing happens. -> Make sure to do port forwarding and ensure the game runs. https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/blob/016dd4caf0547f001e7ef03a33c421afb9412fb2/IAH_SinglePlayerAutomation/Program.cs#L29

### TODO
- Add Support for Multiplayer Co-Op.
- Add Support for Invading Other Players.
