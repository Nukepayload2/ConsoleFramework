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
        <Display(Name:="opencl", Description:="带有这个标签则说明强制使用 OpenCL。")>
        Optional UseOpenCL As Boolean = Nothing
    )
        Console.WriteLine(NameOf(srcA) & " = " & srcA)
        Console.WriteLine(NameOf(srcB) & " = " & srcB)
        Console.WriteLine(NameOf(dest) & " = " & dest)
        If UseOpenCL Then
            Console.WriteLine(NameOf(UseOpenCL))
        Else
            Console.WriteLine("Don't " + NameOf(UseOpenCL))
        End If
    End Sub
End Class
