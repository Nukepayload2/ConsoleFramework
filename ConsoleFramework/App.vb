Imports System.Reflection
''' <summary>
''' ��ʾ����̨Ӧ�ó���
''' </summary>
Public Class App
    ''' <summary>
    ''' ���п���̨Ӧ�ó���
    ''' </summary>
    ''' <typeparam name="TMain">����Ӧ�ó�����ڵ������</typeparam>
    ''' <param name="args">Ӧ�ó���Ĳ���</param>
    ''' <exception cref="InvalidOperationException">�ڰ���Ӧ�ó�����ڵĳ������Ҳ���Ψһ����ڷ���</exception>
    Public Sub Run(Of TMain As Class)(args As String())
        Dim asm = GetType(TMain).GetTypeInfo.Assembly
        Dim entryInfo = Aggregate t In asm.GetTypes
                        Let c = t.GetTypeInfo.GetCustomAttribute(Of EntryClassAttribute)
                        Where c IsNot Nothing
                        Select Aggregate main In t.GetRuntimeMethods
                               Let mp = main.GetCustomAttribute(Of EntryMethodAttribute)
                               Where mp IsNot Nothing
                               Select (EntryPoint:=main, Prefix:=mp.Prefix) Into [Single]
                        Into [Single]
        Dim params = entryInfo.EntryPoint.GetParameters

    End Sub
End Class
