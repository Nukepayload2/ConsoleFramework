Module Module1

    Sub Main()
        Nukepayload2.ConsoleFramework.Application.Run(
        Sub(port As Integer)
            Console.WriteLine("Started at " & port)
        End Sub)
    End Sub

End Module
