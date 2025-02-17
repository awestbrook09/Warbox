# Warbox
Warbox is a modding tool for Kingdom Come: Deliverance II (KCD 2). 

[![GitHub release](https://img.shields.io/github/release/vawser/Warbox.svg)](https://github.com/vawser/Warbox/releases/latest)
[![Github All Releases](https://img.shields.io/github/downloads/vawser/Warbox/total.svg)](https://github.com/vawser/Warbox/releases/latest)

## Key Features
- Table Editor: editor for searching and editing all the configuration table data.
- Text Editor: editor for searching and editing the text localization.

# Basic Usage
You will be prompted to create a project when first starting the program.
* Name the project.
* Select the Data directory. This is the top directory that contains all of the game data.
  * Example: G:\SteamLibrary\steamapps\common\KingdomComeDeliverance2
* Select the Project directory. This is the directory your project files will reside in.
  * Example: G:\SteamLibrary\steamapps\common\KingdomComeDeliverance2\Mods\MyMod

# Mod Making
There are two concepts you will need to understand with Warbox: Save and Package.

Save is the process in which your edits are stored in your project folder. There are three saves options generally:
 - Save File: this will save the current file you have selected in the editor.
   - Found in /Source/
 - Save All Files: this will save all files.
   - Found in /Source/
 - Save Patch File: this will save the current file you have selected in the editor, but only include entries that differ to the base game.
   - Found in /PTF/

Assuming your project directory is in \KingdomComeDeliverance2\Mods\:

Package is the process in which the stored files are packaged into a PAK file ready for the game to load.
 - Package Files: this will package all stored files into the relevant PAK file
   - For Tables, this will be modname.pak
   - For Localization this is language_xml.pak.
 - Package Patched Files: this will package all patch files into the relevant PAK file
   - For Tables, this will be modname.pak
   - For Localization this is language_xml.pak.

In general, you should alway save your edits. But when you want to test them in-game, you should save the patch file (for each file you edit), and then package the patched files.

Warbox will create a mod.manifest if it is missing, and thus you should be able to immediately see your changes in-game.

## Requirements
* Windows 7/8/8.1/10/11 (64-bit only)
* [Visual C++ Redistributable x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)
* For the error message "You must install or update .NET to run this application", use these exact download links. It is not enough to install the default .NET runtime.
  * [Microsoft .NET Core 7.0 Desktop Runtime](https://aka.ms/dotnet/7.0/windowsdesktop-runtime-win-x64.exe)
  * [Microsoft .NET Core 7.0 ASP.NET Core Runtime](https://aka.ms/dotnet/7.0/aspnetcore-runtime-win-x64.exe)
* A Vulkan 1.3 compatible graphics card with up to date graphics drivers:
   * NVIDIA Maxwell (900 series) or newer
   * AMD Polaris (Radeon 400 series) or newer

## Credits
* Vawser
* Katalash, philiquaz, george, thefifthmatt, TKGP, Nordgaren, Pav, Meowmaritus, etc for the DSMapStudio tool, which this tool is based upon.
