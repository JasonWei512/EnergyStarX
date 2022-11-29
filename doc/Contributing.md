🌏 [简体中文](./Contributing.zh-hans.md)


# How to contribute

You can help Energy Star X get better.


## 🗣️ Translation

This app's language resource files are in `EnergyStarX\Strings\`.

If you want to help translate this app to your language, you need to:

1. Copy and paste `EnergyStarX\Strings\en-us` or `EnergyStarX\Strings\zh-hans` folder and rename it to [the language code of the language you want to translate to](https://learn.microsoft.com/windows/apps/publish/publish-your-app/supported-languages?pivots=store-installer-msix).

   For example, if you want to translate this app to German, copy `EnergyStarX\Strings\en-us` folder to `EnergyStarX\Strings\de`.

2. Edit `Resource.resw` file in the pasted folder.
   
-  If you prefer text editors like VSCode and are familiar with XML, open `Resources.resw` file in your favorite text editor, and modify the `<value>...</value>` property of each `<data>...</data>` object.

-  If you are using Visual Studio, open the solution, double click `Resources.resw`, and modify the `value` column.
  
   For reference, you can compare `EnergyStarX\Strings\en-us\Resource.resw` with `EnergyStarX\Strings\zh-hans\Resource.resw`.

3. Once you're done, commit your changes, push to GitHub, and make a pull request.


## 💻 Contribute code

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