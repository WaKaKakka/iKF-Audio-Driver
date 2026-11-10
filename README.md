# 项目作用
由于IKF V11max驱动存在bug，开机后如果没连接到设备会卡住界面，因此修复本驱动。

# iKF Audio / XearONE

iKF Audio（XearONE）控制程序，.NET Framework 4.8、x64、WPF 工程。本仓库已包含全部构建引用程序集和安装包运行时，按下面步骤即可从源码编译并打包安装程序。

## 目录结构

```
iKF-Audio-Driver/
├── src/                    # 主程序 XearONE（WPF）源码
├── tools/CmDeviceCheck/    # 设备检查工具 CmDeviceCheck 源码
├── third_party/UI/         # 主程序编译所需的引用 DLL（已随仓库提供）
├── installer/
│   ├── payload/app/        # 完整安装运行时树（DLL、驱动、资源、SDK）
│   └── iKF-Audio-Fixed.iss # Inno Setup 安装脚本
├── patcher/                # 二进制补丁器（无法完整重建时的备用方案）
└── dist/                   # 安装包输出目录
```

## 环境要求

- Windows 10/11 x64；
- 以下任一构建工具链：
  - Visual Studio 2022，勾选「.NET 桌面开发」工作负载；或
  - Build Tools for Visual Studio 2022 + .NET SDK；
- .NET Framework 4.8 Developer Pack（目标框架为 net48 时需要的目标包）；
- Inno Setup 6（仅打包安装包时需要，安装后把其目录加入 PATH，或使用 `ISCC.exe` 完整路径）。

`third_party/UI/` 已包含 OpenTK、Newtonsoft.Json、NAudio、XearONE.Contracts 等引用程序集，无需额外下载或 NuGet 还原第三方包。

## 一、编译主程序 XearONE

在仓库根目录执行（MSBuild）：

```powershell
msbuild .\src\XearONE.csproj /t:Rebuild /p:Configuration=Release /p:Platform=x64
```

或使用 .NET CLI：

```powershell
dotnet build .\src\XearONE.csproj -c Release -p:Platform=x64
```

输出文件：

```
.\src\bin\x64\Release\net48\XearONE.exe
```

## 二、编译设备检查工具 CmDeviceCheck

```powershell
msbuild .\tools\CmDeviceCheck\CmDeviceCheck.csproj /t:Rebuild /p:Configuration=Release /p:Platform=x64
```

输出文件：

```
.\tools\CmDeviceCheck\bin\x64\Release\net48\CmDeviceCheck.exe
```

## 三、更新安装载荷

把新编译的程序集覆盖到 `installer\payload\app` 对应位置：

```powershell
Copy-Item .\src\bin\x64\Release\net48\XearONE.exe        .\installer\payload\app\UI\XearONE.exe -Force
Copy-Item .\tools\CmDeviceCheck\bin\x64\Release\net48\CmDeviceCheck.exe .\installer\payload\app\Program\CmDeviceCheck.exe -Force
```

`installer\payload\app` 已是完整的运行时树（ApoDriver 驱动、UI 资源、SDK 等），只需替换这两个 exe；若不重新编译，也可直接用其中现有的 exe 打包。

## 四、打包安装程序

```powershell
ISCC .\installer\iKF-Audio-Fixed.iss
```

脚本中的相对路径（`payload\app`、输出目录、图标）均相对于 `.iss` 所在目录解析。编译成功后安装包输出到：

```
.\dist\iKF_Audio_修正版_安装包.exe
```

安装包会在安装时部署 ApoDriver、运行原版的 `CmDrvCheck.exe`，将 XearONE 写入系统启动项，并在安装结束时提示重启。重启登录后，XearONE 以 `-s` 参数在后台启动；开始菜单和桌面快捷方式仍会同时创建。

## 附：二进制补丁器（可选）

`patcher\PatchXearONE.cs` 是单文件补丁器，依赖 dnlib 和 WPF 的 WindowsBase，用于在无法完整重建工程时直接修改原厂程序集。可用 C# 编译器引用相关程序集编译，例如：

```powershell
csc .\patcher\PatchXearONE.cs /r:dnlib.dll /r:WindowsBase.dll
```

编译后用法为 `PatchXearONE.exe <原始 XearONE.exe> <输出 exe>`。正常情况下使用前面的源码编译流程即可，无需使用该补丁器。

---
仅作为学习用途，如有版权问题，可联系我进行修改。
