Module Program
    Sub Main()
        Nukepayload2.ConsoleFramework.Application.Run(
        Function(port As Integer) Async Function()
                                      Console.WriteLine(3)
                                      Await Task.Delay(1000)
                                      Console.WriteLine(2)
                                      Await Task.Delay(1000)
                                      Console.WriteLine(1)
                                      Await Task.Delay(1000)
                                      Console.WriteLine("Started at " & port)
                                  End Function().ConfigureAwait(False))
    End Sub
End Module
