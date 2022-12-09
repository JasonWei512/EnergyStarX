🌏 [简体中文](./Contributing.zh-hans.md)


# How to contribute

You can help Energy Star X get better.


# 💡 Bug report and feature request

- Browse [existing GitHub issues](https://github.com/JasonWei512/EnergyStarX/issues).

- [Create a new GitHub issue](https://github.com/JasonWei512/EnergyStarX/issues/new/choose) if you want to report a bug or request a new feature.


# 🗣️ Translation

There are 2 ways to translate this app to your language.

## Use Crowdin (Recommended)

1. Go to [Energy Star X's Crowdin project](https://crowdin.com/project/EnergyStarX). Crowdin is a localization management platform that helps individuals to translate a project without having to be familiar with its repository.

2. Log in or create an account. Join the EnergyStarX project.

3. Select the language of your choice in the list of existing supported language and let yourself guided by the website to translate the app.

4. If you want to add a new language, please create a issue on GitHub or create a discussion on Crowdin. I will be happy to add your language to the list.

5. When your translation is done, it will be synchronized with this GitHub repository within 1 hour and create a pull request.


## Edit the `.resw` language resource file manually

1. Copy and paste `EnergyStarX\Strings\en-us` or `EnergyStarX\Strings\zh-hans` folder and rename it to [the language code of the language you want to translate to](https://learn.microsoft.com/windows/apps/publish/publish-your-app/supported-languages?pivots=store-installer-msix).

   For example, if you want to translate this app to German, copy `EnergyStarX\Strings\en-us` folder to `EnergyStarX\Strings\de-DE`.

2. Edit `Resource.resw` file in the pasted folder.
   
-  If you prefer text editors like VSCode and are familiar with XML, open `Resources.resw` file in your favorite text editor, and modify the `<value>...</value>` property of each `<data>...</data>` object.

-  If you are using Visual Studio, open the solution, double click `Resources.resw`, and modify the `value` column.
  
   For reference, you can compare `EnergyStarX\Strings\en-us\Resource.resw` with `EnergyStarX\Strings\zh-hans\Resource.resw`.

3. Once you're done, commit your changes, push to GitHub, and make a pull request.


# 💻 Contribute code

This app is developed with C#, Windows App SDK (WinUI 3) and Template Studio.

Before contributing, you need to install the toolchain:

1. Follow [Microsoft's guide](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment) to install Visual Studio and Windows App SDK workloads.
   
2. (Optional) Install these Visual Studio extensions:
   
   - [Template Studio for WinUI (C#)](https://marketplace.visualstudio.com/items?itemName=TemplateStudio.TemplateStudioForWinUICs)

   - [XAML Styler](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler)

3. Open `EnergyStarX.sln` in Visual Studio.

Documents:

-  [Windows App SDK (WinUI 3)](https://learn.microsoft.com/windows/apps/winui/winui3/)

-  [WinUI 3 Template Studio](https://learn.microsoft.com/windows/apps/winui/winui3/winui-project-templates-in-visual-studio)

-  [Microsoft MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)