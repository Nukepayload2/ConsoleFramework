Imports System.ComponentModel.DataAnnotations
Imports Nukepayload2.ConsoleFramework

Public Class App
    <EntryMethod()>
    Public Sub StartUp(
        <Display(Name:="original", ShortName:="o", Description:="原始文件目录。")>
        srcA As String
    )
        Console.WriteLine(NameOf(srcA) & " = " & srcA)
    End Sub
End Class
