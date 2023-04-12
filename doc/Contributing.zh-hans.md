# 如何贡献

你可以帮助 能源之星X 做得更好。


# 💡 报告问题与建议新功能

- 浏览 [现有的 GitHub issues](https://github.com/JasonWei512/EnergyStarX/issues)。

- [创建 GitHub issue](https://github.com/JasonWei512/EnergyStarX/issues/new/choose) 来报告问题和建议新功能。


# 🗣️ 翻译

有两种方法可以把这个应用翻译到你的语言。

## 使用 Crowdin（推荐）

1. 前往 [能源之星X 的 Crowdin 项目](https://crowdin.com/project/EnergyStarX)。Crowdin 是一个本地化管理平台，帮助个人对项目进行翻译，无需熟悉其代码库。

2. 登录或创建一个账号。加入 EnergyStarX 项目。

3. 在已支持的语言列表中选择一个语言。遵循网站的向导进行翻译。

4. 如果你想要添加一个语言，请在 GitHub 创建一个 issue 或在 Crowdin 创建一个讨论。我会把你的语言添加到列表。

5. 当你完成翻译后，Crowdin 会在一小时内把更改同步到 GitHub 并创建一个 pull request。

## 手工编辑 `.resw` 语言资源文件

1. 复制粘贴 `src\EnergyStarX\Strings\zh-hans` 或 `src\EnergyStarX\Strings\en-us` 文件夹并将其重命名为 [你想要翻译到的语言的语言代码](https://learn.microsoft.com/windows/apps/publish/publish-your-app/supported-languages?pivots=store-installer-msix)。

   例如，如果你想要把这个应用翻译为繁体中文，把 `src\EnergyStarX\Strings\zh-hans` 复制到 `src\EnergyStarX\Strings\zh-Hant`。

2. 编辑粘贴的文件夹中的 `Resource.resw` 文件。

-  如果你更喜欢 VSCode 之类的文本编辑器并且熟悉 XML，在你最喜欢的文本编辑器中打开 `Resources.resw`，然后修改每个 `<data>...</data>` 对象的 `<value>...</value>` 属性。

-  如果你使用 Visual Studio，打开解决方案，双击 `Resources.resw`，然后编辑 `值` 行。

   作为参考，你可以对比 `src\EnergyStarX\Strings\en-us\Resource.resw` 和 `src\EnergyStarX\Strings\zh-hans\Resource.resw`。

3. 完成后，提交更改，推送至 GitHub，然后创建一个 pull request。


# 💻 贡献代码

这个应用使用 C#，Windows App SDK (WinUI 3) 和 Template Studio 开发。

在贡献之前，你需要安装工具链：

1. 根据 [微软的教程](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment) 安装 Visual Studio 和 Windows App SDK 工作负载。

2. （可选）安装这些 Visual Studio 扩展:

   - [Template Studio for WinUI (C#)](https://marketplace.visualstudio.com/items?itemName=TemplateStudio.TemplateStudioForWinUICs)

   - [XAML Styler](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler)

3. 在 Visual Studio 中打开 `src\EnergyStarX.sln`。

文档:

-  [Windows App SDK (WinUI 3)](https://learn.microsoft.com/windows/apps/winui/winui3/)

-  [WinUI 3 Template Studio](https://learn.microsoft.com/windows/apps/winui/winui3/winui-project-templates-in-visual-studio)

-  [Microsoft MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)