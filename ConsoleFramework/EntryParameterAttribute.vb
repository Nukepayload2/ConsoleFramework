<AttributeUsage(AttributeTargets.Parameter)>
Public Class EntryParameterAttribute
    Inherits Attribute
    ''' <summary>
    ''' 指定参数的帮助文本。
    ''' </summary>
    ''' <param name="helpText">在帮助中显示的文本。</param>
    Public Sub New(helpText As String)
        Me.HelpText = helpText
    End Sub

    Public ReadOnly Property HelpText As String
    ''' <summary>
    ''' 指定参数名称。如果是空或者长度为 0 则使用参数名作为参数名称。
    ''' </summary>
    Public Property ParamNameOverride As String
End Class
