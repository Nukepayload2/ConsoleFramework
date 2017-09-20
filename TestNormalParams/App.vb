Imports System.ComponentModel.DataAnnotations
Imports ConsoleFramework

Public Class App
    <EntryMethod()>
    Public Sub StartUp(
        <Display(Name:="original", ShortName:="o", Description:="原始文件目录。")>
        srcA As String,
        <Display(Name:="compare", ShortName:="c", Description:="要跟原始文件比较的文件目录。")>
        srcB As String,
        <Display(Name:="destination", ShortName:="d", Description:="比较输出的目录。")>
        dest As String,
        <Display(Name:="nogpu", ShortName:="ng", Description:="带有这个标签则说明强制使用 OpenCL。")>
        Optional OpenCL As Boolean = True
    )
        Console.WriteLine(NameOf(srcA) & " = " & srcA)
        Console.WriteLine(NameOf(srcB) & " = " & srcB)
        Console.WriteLine(NameOf(dest) & " = " & dest)
        If OpenCL Then
            Console.WriteLine(NameOf(OpenCL))
        Else
            Console.WriteLine("Don't " + NameOf(OpenCL))
        End If
    End Sub
End Class
