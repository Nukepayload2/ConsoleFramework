Imports System.Runtime.CompilerServices

Module TaskHelper
    Sub LateWaitTask(lateTask As Object)
        If TypeOf lateTask Is Task Then
            DirectCast(lateTask, Task).GetAwaiter.GetResult()
        Else
            RealLateWaitTask(lateTask)
        End If
    End Sub

End Module
