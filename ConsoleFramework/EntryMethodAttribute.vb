''' <summary>
''' 表示入口点对应的方法。如果参数没有默认值，用户可以省略参数名称，直接写参数。如果参数有默认值（是可选参数），用户需要输入 前缀+参数名 [参数内容]。例如，前缀是 "-", 参数名称是 "src", 类型是 <see cref="String"/>, 那么用户输入的参数可以是 -src xxx。如果用户没有输入参数的值并且类型不是 <see cref="Boolean"/>，则使用默认值。<see cref="Boolean"/> 类型的参数没有值，带有这种参数的开关则认为用户为这种参数输入了真。
''' </summary>
<AttributeUsage(AttributeTargets.Method)>
Public Class EntryMethodAttribute
    Inherits Attribute

    Public Sub New(prefix As String)
        Me.Prefix = prefix
    End Sub

    Public ReadOnly Property Prefix As String
End Class
