Imports System.Globalization

Friend Class LocalizedStrings
    Shared s_lc$ = CultureInfo.CurrentUICulture.Name
    Public Shared ReadOnly Property NoParamsRequired As String
        Get
            If s_lc = "zh-CN" Then
                Return "不需要任何参数。"
            Else
                Return "No argument(s) required."
            End If
        End Get
    End Property
    Public Shared ReadOnly Property Arguments As String
        Get
            If s_lc = "zh-CN" Then
                Return "参数: "
            Else
                Return "Arguments: "
            End If
        End Get
    End Property
    Public Shared ReadOnly Property [Optional] As String
        Get
            If s_lc = "zh-CN" Then
                Return "可选"
            Else
                Return "Optional"
            End If
        End Get
    End Property
End Class
