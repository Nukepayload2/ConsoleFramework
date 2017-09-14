Imports System.ComponentModel.DataAnnotations

Public Class ParameterModel
    <Required, Display(Name:="ori", Description:="原始文件目录。")>
    Public Property Original As String
    <Required, Display(Name:="cmp", Description:="要跟原始文件比较的文件目录。")>
    Public Property Compare As String
    <Required, Display(Name:="dst", Description:="比较输出的目录。")>
    Public Property Destination As String
    <Display(Name:="opencl", Description:="带有这个标签则说明强制使用 OpenCL。")>
    Public Property UseOpenCL As Boolean
End Class
