Imports System.Reflection

Partial Module TaskHelper
    ' .NET Standard 2.0
    Private Sub RealLateWaitTask(lateTask As Object)
        Dim tskTp = lateTask.GetType
        Dim getAwaiter = tskTp.GetRuntimeMethod("GetAwaiter", Type.EmptyTypes)
        Dim awaiter = getAwaiter.Invoke(lateTask, Nothing)
        Dim awaiterTp = awaiter.GetType
        Dim getResult = awaiterTp.GetRuntimeMethod("GetResult", Type.EmptyTypes)
        getResult.Invoke(awaiter, Nothing)
    End Sub
End Module
