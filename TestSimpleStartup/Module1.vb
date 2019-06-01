Module Module1

    Sub Main()
        Nukepayload2.ConsoleFramework.Application.Run(
        Sub(port As Integer)
            MsgBox("Started at " & port)
        End Sub)
    End Sub

End Module
