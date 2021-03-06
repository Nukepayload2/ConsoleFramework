# ConsoleFramework
主要针对 .NET Core 设计的控制台应用程序框架。使用 ORM 思想设计。

## 快速上手
__VB__
```vbnet
Module Module1
    Sub Main()
        Nukepayload2.ConsoleFramework.Application.Run(
        Sub(port As Integer)
            Console.WriteLine("Started at " & port)
        End Sub)
    End Sub
End Module
```

__C#__
```csharp
using System;

namespace TestSimpleStartup_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Nukepayload2.ConsoleFramework.Application.Run(
            (Action<int>)(port =>
            {
                Console.WriteLine("Started at " + port);
            }));
        }
    }
}
```

__Cmd__
```cmd
ConsoleApp1.exe --port 12345
```

__运行结果__
```text
Started at 12345
```

## 进度
测试了简单的示例。

## 设计
### 方法映射功能
将控制台应用程序的参数映射到一个方法。
- 唯一包含被映射的方法的类需要有 Attribute 标记
- 唯一被映射的方法的类需要有 Attribute 标记, 指定参数名称的前缀
- 控制台应用的参数映射到带有 Attribute 标记的方法参数, 指定帮助文本
- -? /? --? -help /help --help 用于显示帮助, 不会调用入口方法
- 程序发生未处理的异常则显示帮助

### 实体映射功能
将参数初始化为一个实体类，存储全部的参数。使用 `System.ComponentModel.DataAnnotations` 的 `RequiredAttribute` 和 `DisplayAttribute`，使用属性代替参数。不要使用 DefaultValueAttribute。

### 类型映射关系
必须使用能够映射的类型。

|CLR 类型|是否需要指定值|普通参数的写法示例|作为可选(opt)参数的写法示例|
|-|-|-|-|
|`System.String`|是|value 其中 value 不带空格和双引号。或者是 "value" 其中 value 不带双引号。|-paramName value 其中 value 不带空格和双引号。或者是 -paramName "value" 其中 value 不带双引号。|
|`System.Single`, `System.Double`, `System.Int32`, `System.Int64`|是|value|-paramName value|
|`System.Boolean`|否。如果指定参数名，则值与默认值相反。|无效的情况|-paramName|

### 设计约束
确保一个 C#、VB、F#、C++/CLI 能编写和使用的方法能使用本框架的全部功能, 并且不会让用户感到困惑。因此，主要有以下限制：
- __要__ 让所有参数按值传递。
- __要__ 把参数数组放到最后一个，并且一旦有参数数组就不能指定任何参数的默认值。
- __不要__ 让指针类型, 无符号类型 作为参数或实体成员属性的类型。
- __不要__ 用调用约定、InAttribute、OutAttribute、MarshalAs 等互操作功能。
- __不要__ 利用大小写区分参数。
- __不要__ 使用令人困惑的 Attribute，例如 DefaultValueAttribute。
