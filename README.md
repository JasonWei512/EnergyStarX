<p align="center">
  <img width="128" align="center" src="EnergyStarX/Assets/Icon.png">
</p>

<h1 align="center" style="font-weight: bold">
  Energy Star X
</h1>

<p align="center">
  🌏
  <a href="README.zh-hans.md">简体中文</a>
</p>

<p align="center">
  Improve your Windows device's battery life
</p>

<p align="center">
  <a href="https://www.microsoft.com/store/productId/9NF7JTB3B17P" target="_blank">
    <img src="https://img.shields.io/badge/Microsoft%20Store-Download-orange" alt="Microsoft Store Download" />
  </a>
  <a href="https://github.com/JasonWei512/EnergyStarX/releases" target="_blank">
    <img src="https://img.shields.io/github/v/release/JasonWei512/EnergyStarX?label=GitHub%20Release" alt="GitHub Release" />
  </a>
</p>

![Screenshot](.github/images/Screenshot.png)


# Introduction

Energy Star X is a GUI version of the open source software [Energy Star](https://github.com/imbushuo/EnergyStar/), developed with Windows App SDK (WinUI 3).

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

- If you use some taskbar enhancement software such as [StartAllBack](https://www.startallback.com/), this app may crash when you hover over system tray icon.
- Child processes do not get boosted when the parent process receives input focus.
- System processes (which is Session 0) do not get throttled. Currently there are some assumption that non-user processes know what they are doing.


# Acknowledgements

- imbushuo: https://github.com/imbushuo/
- App Icon: 
  - https://www.flaticon.com/free-icon/star_3103390/
  - https://www.flaticon.com/free-icon/accept_4303945
  - https://www.flaticon.com/free-icon/pause-button_561920


# How to contribute

See [Contributing.md](./doc/Contributing.md).