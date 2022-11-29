# 如何贡献

你可以帮助 能源之星X 做得更好。


## 🗣️ 翻译

这个应用的语言资源文件位于  `EnergyStarX\Strings\`。

如果你想要帮助把这个应用翻译至你的语言，你需要：

1. 复制粘贴 `EnergyStarX\Strings\zh-hans` 或 `EnergyStarX\Strings\en-us` 文件夹并将其重命名为 [你想要翻译到的语言的语言代码](https://learn.microsoft.com/windows/apps/publish/publish-your-app/supported-languages?pivots=store-installer-msix)。

   例如，如果你想要把这个应用翻译为繁体中文，把 `EnergyStarX\Strings\zh-hans` 复制到 `EnergyStarX\Strings\zh-hant`。

2. 编辑粘贴的文件夹中的 `Resource.resw` 文件。

-  如果你更喜欢 VSCode 之类的文本编辑器并且熟悉 XML，在你最喜欢的文本编辑器中打开 `Resources.resw`，然后修改每个 `<data>...</data>` 对象的 `<value>...</value>` 属性。

-  如果你使用 Visual Studio，打开解决方案，双击 `Resources.resw`，然后编辑 `值` 行。

   作为参考，你可以对比 `EnergyStarX\Strings\en-us\Resource.resw` 和 `EnergyStarX\Strings\zh-hans\Resource.resw`。

3. 完成后，提交更改，推送至 GitHub，然后创建一个 pull request。


## 💻 贡献代码

这个应用使用 C#，Windows App SDK (WinUI 3) 和 Template Studio 开发。

在贡献之前，你需要安装工具链：

1. 根据 [微软的教程](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment) 安装 Visual Studio 和 Windows App SDK 工作负载。

2. （可选）安装这些 Visual Studio 扩展:

   - [Template Studio for WinUI (C#)](https://marketplace.visualstudio.com/items?itemName=TemplateStudio.TemplateStudioForWinUICs)

   - [XAML Styler](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler)

3. 在 Visual Studio 中打开 `EnergyStarX.sln`。

文档:

-  [Windows App SDK (WinUI 3)](https://learn.microsoft.com/windows/apps/winui/winui3/)

-  [WinUI 3 Template Studio](https://learn.microsoft.com/windows/apps/winui/winui3/winui-project-templates-in-visual-studio)

-  [Microsoft MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)