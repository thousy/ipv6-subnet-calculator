# IPv6 子网计算器

一个用于 IPv6 子网计算、地址范围查看、IPv6 压缩/扩展转换的小工具。

## 使用方式

### Web 版本

发布到 GitHub Pages 后，可从入口页选择：

- `IPv6新版界面.html`
- `IPv6传统桌面界面.html`

### Windows EXE 版本

下载并双击运行：

```text
IPv6SubnetCalculator.exe
```

当前 EXE 是单文件原生 Windows 窗口版，不需要随附 HTML、WebView2 DLL 或其它资源文件。

## 主要功能

- 输入 IPv6 地址和前缀位，计算当前子网地址。
- 显示当前子网起始地址、结束地址和地址数量。
- 显示下一个子网地址、起始地址、结束地址和地址数量。
- 支持 IPv6 完整地址与压缩地址转换。
- 地址结果支持一键复制。

## 发布建议

- GitHub Pages：使用根目录的 `index.html` 作为 Web 入口。
- GitHub Releases：上传 `IPv6SubnetCalculator.exe` 作为 Windows 下载文件。

## 开发文件

- `index.html`：GitHub Pages 入口页。
- `IPv6新版界面.html`：新版 Web 界面。
- `IPv6 子网计算工具V1.2.HTML`：最新 V1.2 版 Web 界面。
- `IPv6传统桌面界面.html`：传统桌面风格 Web 界面。
- `PortableCalculator.cs`：EXE 原生窗口版源码。
- `IPv6SubnetCalculator.exe`：已编译的 Windows 单文件版本（V1.2）。
