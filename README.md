# ConsoleFramework
主要针对 .NET Core 设计的控制台应用程序框架

## 进度
仍在设计中。不可用。

## 设计
将控制台应用程序的参数映射到一个方法。
- 唯一包含被映射的方法的类需要有 Attribute 标记
- 唯一被映射的方法的类需要有 Attribute 标记, 指定参数名称的前缀
- 控制台应用的参数映射到带有 Attribute 标记的方法参数, 指定帮助文本
- -? /? --? /help -help --help 用于显示帮助, 不会调用入口方法

### 类型映射关系

|CLR 类型|是否需要指定值|普通参数的写法示例|作为可选(opt)参数的写法示例|
|-|-|-|-|
|`System.String`|是|value 其中 value 不带空格和双引号。或者是 "value" 其中 value 不带双引号。|-paramName value 其中 value 不带空格和双引号。或者是 -paramName "value" 其中 value 不带双引号。|
|`System.Single`, `System.Double`|是|value|-paramName value|
|`System.Boolean`|否|无效的情况|-paramName|
