Friend Class CommandLineParameterInfo
    Public Sub New(isRequired As Boolean, name As String, help As String, paramType As Type)
        Me.IsRequired = isRequired
        Me.Name = name
        Me.Help = help
        Me.ParamType = paramType
    End Sub

    Public ReadOnly Property IsRequired As Boolean
    Public ReadOnly Property Name As String
    Public ReadOnly Property Help As String
    Public ReadOnly Property ParamType As Type
    Public Property Value As Object
    Public Property Handled As Boolean
End Class
