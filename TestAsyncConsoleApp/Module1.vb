Module Module1

    Sub Main()
        Nukepayload2.ConsoleFramework.Application.Run(
        Async Function(port As Integer) As Task
            Console.WriteLine(3)
            Await Task.Delay(1000)
            Console.WriteLine(2)
            Await Task.Delay(1000)
            Console.WriteLine(1)
            Await Task.Delay(1000)
            Console.WriteLine("Started at " & port)
        End Function)
    End Sub

End Module
