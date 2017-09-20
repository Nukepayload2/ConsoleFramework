Imports ConsoleFramework

Public Class App
    <EntryMethod>
    Public Sub StartUp(params As ParameterModel)
        With params
            Console.WriteLine(NameOf(.Original) & " = " & .Original)
            Console.WriteLine(NameOf(.Compare) & " = " & .Compare)
            Console.WriteLine(NameOf(.Destination) & " = " & .Destination)
            If .UseOpenCL Then
                Console.WriteLine(NameOf(.UseOpenCL))
            Else
                Console.WriteLine("Don't " + NameOf(.UseOpenCL))
            End If
        End With
    End Sub
End Class
