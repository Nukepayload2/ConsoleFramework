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

End Module
