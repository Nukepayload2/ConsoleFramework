Friend Class CommandLineParameterInfo
    Public Sub New(isRequired As Boolean, name As String, shortName As String, help As String, paramType As Type)
        If String.IsNullOrEmpty(name) Then
            Throw New ArgumentException("Name can not be null.", NameOf(name))
        End If

        If shortName Is Nothing Then
            shortName = String.Empty
        End If

        If help Is Nothing Then
            help = String.Empty
        End If

        Me.IsRequired = isRequired
        Me.Name = name
        Me.ShortName = shortName
        Me.Help = help
        Me.ParamType = paramType
    End Sub

    Public ReadOnly Property IsRequired As Boolean
    Public ReadOnly Property Name As String
    Public ReadOnly Property ShortName As String
    Public ReadOnly Property Help As String
    Public ReadOnly Property ParamType As Type
    Public Property Value As Object
    Public Property Handled As Boolean
End Class
