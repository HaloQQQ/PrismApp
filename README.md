

# PrismApp

## 介绍

PrismApp 是一个基于 Prism 框架的 WPF 应用程序，包含了多个模块：

- **CustomControlsDemoModule**: 演示了自定义控件的用法，包含按钮、文本框、面板等 UI 控件示例
- **MusicPlayerModule**: 实现了音乐播放器功能，支持播放控制、歌词显示、桌面歌词等特性
- **SqlCreatorModule**: 提供数据库结构导出功能，可以将数据库表结构导出为 C# 模型类

## 软件架构

项目采用模块化设计，使用 Prism 桌面框架进行模块管理和导航：

- **MyApp.Prisms**: 主应用程序，包含主窗口和全局设置
- **CustomControlsDemoModule**: 自定义控件演示模块
- **MusicPlayerModule**: 音乐播放器模块，包含播放控制、播放列表、歌词显示等
- **SqlCreatorModule**: 数据库结构导出模块，支持多种数据库类型

使用的技术和模式：
- Prism 框架（用于模块化、导航、事件聚合）
- MVVM 模式（分离视图和视图模型）
- .NET 6（目标框架）
   