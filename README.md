<p align="center">
  <img width="128" align="center" src="EnergyStarX/Assets/Icon.png">
</p>
<h1 align="center">
  Energy Star X　能源之星X
</h1>
<p align="center">
  Improve your device's battery life <br/>
  提升您设备的电池续航
</p>
<p align="center">
  <a href="https://www.microsoft.com/store/productId/9NF7JTB3B17P" target="_blank" rel="noopener noreferrer">
   <img width=128 src="https://getbadgecdn.azureedge.net/images/en-us%20dark.svg" alt="Download" />
  </a>
</p>

# Introduction

Energy Star X is a GUI version of the open source software [Energy Star](https://github.com/imbushuo/EnergyStar/), developed using Windows App SDK (WinUI 3).

It leverages Windows 11's [EcoQoS API](https://devblogs.microsoft.com/performance-diagnostics/introducing-ecoqos/) to throttle background applications to improve system thermal and battery life.


# Requirements

## Hardware

- Intel 10th gen or newer mobile processors
- AMD Ryzen 5000 or newer mobile processors
- Qualcomm mobile processors

## Software

- Works best on Windows 11 22H2 (Build 22621) and above.
- Works on Windows 11 21H2 (Build 22000), but not as well.


# Usage

Let it run in the background in the system tray. You can choose to run it automatically on startup in the settings page.

You can see a green leaf icon in the "Status" column of the Task Manager next to background processes that are throttled.

![Task Manager Leaf](/EnergyStarX/Assets/InApp/Task_Manager_Leaf.jpg)


# Known Limitations

- Child processes do not get boosted when the parent process receives input focus.
- System processes (which is Session 0) do not get throttled. Currently there are some assumption that non-user processes know what they are doing.


# Acknowledgements

- imbushuo: https://github.com/imbushuo/
- App Icon: 
  - https://www.flaticon.com/free-icon/star_3103390/
  - https://www.flaticon.com/free-icon/accept_4303945
  - https://www.flaticon.com/free-icon/pause-button_561920



# 简介

能源之星X 是开源程序 [Energy Star](https://github.com/imbushuo/EnergyStar/) 的图形界面版应用，使用 Windows App SDK (WinUI 3) 开发。

它利用 Windows 11 的 [EcoQos API](https://devblogs.microsoft.com/performance-diagnostics/introducing-ecoqos/) 来限制后台应用的资源占用，从而提高散热表现和电池续航。


# 要求

## 硬件 

- 英特尔 10 代及以上移动处理器
- AMD Ryzen 5000 及以上移动处理器
- 高通移动处理器

## 软件

- 在 Windows 11 22H2 (Build 22621) 以上可完全发挥作用。
- 可在 Windows 11 21H2 (Build 22000) 上工作，但是不会有最佳表现。


# 使用方法

让它在任务栏右下角后台运行即可。你可以在设置页中选择开机启动。

你可以在任务管理器的 “状态” 列中看到被限制资源的后台应用旁会显示一个绿叶图标。

![任务管理器绿叶](/EnergyStarX/Assets/InApp/Task_Manager_Leaf.jpg)


# 已知问题

- 当父进程获得输入焦点时，子进程不会被解除资源限制。
- 系统进程 (Session 0) 将不会被限制资源。目前我们假设非用户进程会自己管理好资源。


# 致谢

- imbushuo：https://github.com/imbushuo/
- 应用图标：
  - https://www.flaticon.com/free-icon/star_3103390/
  - https://www.flaticon.com/free-icon/accept_4303945
  - https://www.flaticon.com/free-icon/pause-button_561920