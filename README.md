🌏 [简体中文](README.zh-hans.md)


<p align="center">
  <img width="128" align="center" src="src/EnergyStarX/Assets/Icon.png" />
</p>

<h1 align="center" style="font-weight: bold">
  Energy Star X
</h1>

<p align="center">
  <a title="Get it from Microsoft" href="https://apps.microsoft.com/store/detail/9NF7JTB3B17P?launch=true&mode=full" target="_blank">
    <picture>
      <source srcset="https://get.microsoft.com/images/en-US%20light.svg" media="(prefers-color-scheme: dark)" />
      <source srcset="https://get.microsoft.com/images/en-US%20dark.svg" media="(prefers-color-scheme: light), (prefers-color-scheme: no-preference)" />
      <img src="https://get.microsoft.com/images/en-US%20dark.svg" width=144 />
    </picture>
  </a>
</p>

<p align="center">
  Improve your Windows 11 device's battery life
</p>

<p align="center">
  <a title="GitHub Release" href="https://github.com/JasonWei512/EnergyStarX/releases" target="_blank">
    <img src="https://img.shields.io/github/v/release/JasonWei512/EnergyStarX?label=Release&color=red" />
  </a>
  <a title="Microsoft Store Rating" href="https://www.microsoft.com/store/productId/9NF7JTB3B17P" target="_blank">
    <img src="https://img.shields.io/endpoint?color=blue&label=Microsoft%20Store%20Rating&url=https%3A%2F%2Fmicrosoft-store-badge.fly.dev%2Fapi%2Frating%3FstoreId%3D9NF7JTB3B17P" />
  </a>
  <a title="Crowdin" href="https://crowdin.com/project/energystarx" target="_blank">
    <img src="https://badges.crowdin.net/energystarx/localized.svg" />
  </a>
</p>

![Screenshot](.msstore/images/2_Screenshot.png)


# Donate

## Donate on [Buy Me a Coffee](https://www.buymeacoffee.com/nickjohn):

(Open on your phone to pay with Apple Pay or Google Pay)

[![Buy me a coffee](src/EnergyStarX/Assets/InApp/Buy_me_a_coffee.png)](https://www.buymeacoffee.com/nickjohn)

## For Chinese users, scan with WeChat:

![WeChat Donation QR Code](src/EnergyStarX/Assets/InApp/WeChat_Donation_QR_Code.png)


# Introduction

Energy Star X leverages Windows 11's [EcoQoS API](https://devblogs.microsoft.com/performance-diagnostics/introducing-ecoqos/) (aka "Efficiency Mode") to throttle background applications to improve battery life and system thermal. It will not throttle foreground application to ensure user experience.

This app is a GUI version of the open source software [EnergyStar](https://github.com/imbushuo/EnergyStar/), developed with Windows App SDK (WinUI 3).


# Requirements

For the best result, you need:

## Software

- Windows 11 22H2 (Build 22621) or above

## Hardware

- Intel 10th gen or newer mobile processors
- AMD Ryzen 5000 or newer mobile processors
- Qualcomm mobile processors

This app can work on Windows 11 21H2 (Build 22000) and older hardware, but may not get the best result.


# Usage

Let it run in background in the system tray. You can choose to run it automatically at startup in settings page.

You can see a green leaf icon next to throttled background process in Task Manager's "Status" column.

![Task Manager Leaf](src/EnergyStarX/Assets/InApp/Task_Manager_Leaf.jpg)


# Known Limitations

- If you are using some taskbar enhancement software such as [StartAllBack](https://www.startallback.com/), this app may crash when you hover over system tray icon.
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