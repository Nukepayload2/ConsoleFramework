Imports System.Reflection
''' <summary>
''' 表示控制台应用程序
''' </summary>
Public Class App
    ''' <summary>
    ''' 运行控制台应用程序。
    ''' </summary>
    ''' <typeparam name="TMain">包含应用程序入口点的类型</typeparam>
    ''' <param name="args">应用程序的参数</param>
    ''' <exception cref="InvalidOperationException">在包含应用程序入口的程序集内找不到唯一的入口方法</exception>
    Public Sub Run(Of TMain As Class)(args As String())
        Dim asm = GetType(TMain).GetTypeInfo.Assembly
        Dim entryInfo = Aggregate t In asm.GetTypes
                        Let c = t.GetTypeInfo.GetCustomAttribute(Of EntryClassAttribute)
                        Where c IsNot Nothing
                        Select Aggregate main In t.GetRuntimeMethods
                               Let mp = main.GetCustomAttribute(Of EntryMethodAttribute)
                               Where mp IsNot Nothing
                               Select (EntryPoint:=main, Prefix:=mp.Prefix) Into [Single]
                        Into [Single]
        Dim params = entryInfo.EntryPoint.GetParameters

    End Sub
End Class
