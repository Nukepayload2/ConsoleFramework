Module Module1

    Sub Main(args As String())
        ' Test 1
        Nukepayload2.ConsoleFramework.Application.Run(
        Sub(port As Integer)
            Console.WriteLine("Started at " & port)
        End Sub)
        ' Test 2
        Nukepayload2.ConsoleFramework.Application.Run(args,
            New Action(Of String)(AddressOf Startup))
    End Sub

    Sub Startup(
        Optional faithful As String = "no")
        Console.WriteLine($"faithful={faithful}")
    End Sub

End Module
