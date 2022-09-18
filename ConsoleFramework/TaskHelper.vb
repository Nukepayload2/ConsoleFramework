Imports System.Reflection

Module TaskHelper
    Sub LateWaitTask(lateTask As Object)
        If TypeOf lateTask Is Task Then
            DirectCast(lateTask, Task).GetAwaiter.GetResult()
        ElseIf TypeOf lateTask Is ValueTask Then
            ' ValueTaskAwaiter 有时候不会阻塞
            DirectCast(lateTask, ValueTask).AsTask.GetAwaiter.GetResult()
        Else
            RealLateWaitTask(lateTask)
        End If
    End Sub

    Private Sub RealLateWaitTask(lateTask As Object)
        Dim tskTp = lateTask.GetType
        Dim getAwaiter = tskTp.GetRuntimeMethod("GetAwaiter", Type.EmptyTypes)
        Dim awaiter = getAwaiter.Invoke(lateTask, Nothing)
        Dim awaiterTp = awaiter.GetType
        Dim getResult = awaiterTp.GetRuntimeMethod("GetResult", Type.EmptyTypes)
        getResult.Invoke(awaiter, Nothing)
    End Sub
End Module
