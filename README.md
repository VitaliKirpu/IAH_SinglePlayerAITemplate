u<p align="center">  <img src="iah_logo.png" width="350" title="IAH: INTERNET WAR logo"/> </p>

IAH: INTERNET WAR is a Turn-based strategy that meets real-time battles. Explore the internet, fight enemies as a hacker, and create combat bots. Play your way with a cursor, controller, or code.

This repository contains starter C# code to help you start writing your own IAH AI Code in C#.

<p align="center"><img src="GIF_1.gif" title="combat bot robots shooting"/> </p>

IAH: INTERNET WAR Steam Page: **https://store.steampowered.com/app/304770/IAH_INTERNET_WAR/**

Main Website: **https://iamhacker.cc/**

Online API Documentation: **https://iamhacker.cc/documentation**

Singleplayer API Documentation **https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/wiki**

<p align="center"><img src="GIF_2.gif" title="combat bot robots shooting"/> </p>


### Quick Start Guide

1. Obtain the API Key from the Main Website and paste your own here: https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/blob/cc9d2fe329d34d837b78799a853de09222ec799a/IAH_SinglePlayerAutomation/Program.cs#L133
2. Launch IAH: INTERNET WAR Game. Default AI Port is 6800, but you can change this with -apiPort 6900 launch parameter
3. Launch this Example API Template Project; automation should occur now. From the main menu, the AI will navigate the UI, select Hacker, enter the game, perform all necessary actions, create units, and battle enemies if they are present.

### Troubleshooting
- AI Template Crashes on Launch, or nothing happens. -> Make sure to do port forwarding and ensure the game runs. https://github.com/VitaliKirpu/IAH_SinglePlayerAITemplate/blob/016dd4caf0547f001e7ef03a33c421afb9412fb2/IAH_SinglePlayerAutomation/Program.cs#L29
