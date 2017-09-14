Imports System.ComponentModel.DataAnnotations
Imports ConsoleFramework

<EntryClass>
Public Class App
    <EntryMethod("-")>
    Public Shared Sub App_StartUp(
        <Display(Name:="ori", Description:="原始文件目录。")>
        srcA As String,
        <Display(Name:="cmp", Description:="要跟原始文件比较的文件目录。")>
        srcB As String,
        <Display(Name:="dst", Description:="比较输出的目录。")>
        dest As String,
        <Display(Name:="nogpu", Description:="带有这个标签则说明强制使用 OpenCL。")>
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
