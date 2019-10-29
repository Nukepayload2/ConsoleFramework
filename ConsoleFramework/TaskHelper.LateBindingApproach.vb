Partial Module TaskHelper
    ' .NET Framework, .NET Core 3.0+
    Private Sub RealLateWaitTask(lateTask As Object)
        lateTask.GetAwaiter().GetResult()
    End Sub
End Module
