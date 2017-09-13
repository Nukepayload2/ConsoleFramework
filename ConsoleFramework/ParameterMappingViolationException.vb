Public Class ParameterMappingViolationException
    Inherits Exception

    Public Sub New()
        MyBase.New("启动参数到实体类或参数列表的映射存在问题。")
    End Sub

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class
