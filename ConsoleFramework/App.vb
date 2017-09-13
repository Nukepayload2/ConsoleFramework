Option Strict On
Imports System.Reflection
Imports System.ComponentModel.DataAnnotations

''' <summary>
''' ��ʾ����̨Ӧ�ó���
''' </summary>
Public Class App
    ''' <summary>
    ''' ���п���̨Ӧ�ó���
    ''' </summary>
    ''' <typeparam name="TMain">����Ӧ�ó�����ڵ������</typeparam>
    ''' <param name="args">Ӧ�ó���Ĳ���</param>
    ''' <exception cref="InvalidOperationException">�ڰ���Ӧ�ó�����ڵĳ������Ҳ���Ψһ���ʵ���������ڷ���</exception>
    Public Sub Run(Of TMain As Class)(args As String())
        Dim helpCommands = {"-?", "/?", "--?", "-help", "/help", "--help"}
        Dim asm = GetType(TMain).GetTypeInfo.Assembly
        Dim entryInfo = Aggregate t In asm.GetTypes
                        Let c = t.GetTypeInfo.GetCustomAttribute(Of EntryClassAttribute)
                        Where c IsNot Nothing
                        Select Aggregate main In t.GetRuntimeMethods
                               Let mp = main.GetCustomAttribute(Of EntryMethodAttribute)
                               Where mp IsNot Nothing
                               Select (entryPoint:=main, Prefix:=mp.Prefix) Into [Single]
                        Into [Single]
        Dim entryPoint As MethodInfo = entryInfo.entryPoint
        Dim params = entryPoint.GetParameters
        Dim entityProp As CommandLineParameterInfo()
        Dim isEntityModel = False
        ' �����β�
        If params.Length = 1 Then
            Dim param = params(0)
            Dim parameterType As Type = param.ParameterType
            If Not parameterType.IsPrimitive Then
                ' ʵ����
                isEntityModel = True
                entityProp = Aggregate p In parameterType.GetRuntimeProperties
                             Where p.CanRead AndAlso p.CanWrite AndAlso p.GetAccessors.All(Function(ac) ac.IsPublic)
                             Let req = p.GetCustomAttribute(Of RequiredAttribute)
                             Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                             Let name = If(disp?.Name, p.Name)
                             Let help = disp?.Description
                             Select New CommandLineParameterInfo(req IsNot Nothing, name, help, p.PropertyType)
                             Into ToArray
            End If
        End If
        If params.Length = 0 Then
            ' û����
            entityProp = Nothing
        Else
            ' ��Ӧ����
            entityProp = Aggregate p In params
                         Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                         Let name = If(disp?.Name, p.Name)
                         Let help = disp?.Description
                         Select New CommandLineParameterInfo(Not p.IsOptional, name, help, p.ParameterType)
                         Into ToArray
        End If
        ' ʶ���������
        If args.Length = 1 AndAlso helpCommands.Contains(args(0).ToLowerInvariant) Then
            ShowHelp(entityProp, entryInfo.Prefix)
            Return
        End If
        ' ��ʵ�ʴ��ݵĲ��������βΡ�
        Dim realParams As Object()
        If entityProp IsNot Nothing Then
            Dim paramIndex = entityProp.ToDictionary(Function(o) entryInfo.Prefix + o.Name.ToLowerInvariant, Function(o) o)
            Dim values = paramIndex.Values
            ReDim realParams(paramIndex.Count - 1)
            ' �������Ƿ����Ҫ��
            Dim allowedParamTypes = {
                GetType(String),
                GetType(Integer), GetType(Long),
                GetType(Double), GetType(Single),
                GetType(Boolean)
            }
            For Each v In values
                ' ���������ͼ��
                If Not allowedParamTypes.Contains(v.ParamType) Then
                    Throw New ParameterMappingViolationException("�������Ͳ���Ԥ�ڵġ���鿴���� https://github.com/Nukepayload2/ConsoleFramework/blob/master/README.md��")
                End If
                ' ����ֵ�����ǿ�ѡ����
                If v.IsRequired AndAlso v.ParamType.Equals(GetType(Boolean)) Then
                    Throw New ParameterMappingViolationException("����ֵ�����ǿ�ѡ������")
                End If
            Next
            For i = 0 To args.Length - 1
                Dim curArg = args(i)
                If curArg.StartsWith(entryInfo.Prefix) Then
                    Dim paraInf As CommandLineParameterInfo = Nothing
                    If paramIndex.TryGetValue(curArg, paraInf) Then
                        paraInf.Handled = True
                        If paraInf.ParamType.Equals(GetType(Boolean)) Then
                            paraInf.Value = True
                        Else
                            i += 1
                            curArg = args(i)
                            Select Case paraInf.ParamType.FullName
                                Case GetType(String).FullName
                                    paraInf.Value = curArg
                                Case GetType(Integer).FullName
                                    Dim value As Integer
                                    If Integer.TryParse(curArg, value) Then
                                        paraInf.Value = value
                                    Else
                                        ShowHelp(entityProp, entryInfo.Prefix)
                                        Return
                                    End If
                                Case GetType(Long).FullName
                                    Dim value As Long
                                    If Long.TryParse(curArg, value) Then
                                        paraInf.Value = value
                                    Else
                                        ShowHelp(entityProp, entryInfo.Prefix)
                                        Return
                                    End If
                                Case GetType(Double).FullName
                                    Dim value As Double
                                    If Double.TryParse(curArg, value) Then
                                        paraInf.Value = value
                                    Else
                                        ShowHelp(entityProp, entryInfo.Prefix)
                                        Return
                                    End If
                                Case GetType(Single).FullName
                                    Dim value As Single
                                    If Single.TryParse(curArg, value) Then
                                        paraInf.Value = value
                                    Else
                                        ShowHelp(entityProp, entryInfo.Prefix)
                                        Return
                                    End If
                            End Select
                        End If
                    Else
                        ShowHelp(entityProp, entryInfo.Prefix)
                        Return
                    End If
                End If
            Next
            ' ���ڿ�ѡ������ʹ�� Type.Missing��δָ���Ĳ���ֵ����Ϊ False��
            For Each v In values
                If Not v.Handled Then
                    If v.IsRequired Then
                        ShowHelp(entityProp, entryInfo.Prefix)
                        Return
                    End If
                    If v.ParamType.Equals(GetType(Boolean)) Then
                        v.Value = False
                    Else
                        v.Value = Type.Missing
                    End If
                End If
            Next
        Else
            realParams = Nothing
        End If
        ' �������
        If entryPoint.IsStatic Then
            If isEntityModel Then
                Dim entityType = params(0).ParameterType
                Dim entity = Activator.CreateInstance(entityType)
                If realParams IsNot Nothing Then
                    For Each ep In entityProp
                        Dim prop = entityType.GetRuntimeProperty(ep.Name)
                        If Not ep.Value.Equals(Type.Missing) Then
                            prop.SetValue(entity, ep.Value)
                        End If
                    Next
                End If
                realParams = {entity}
            End If
            Try
                entryPoint.Invoke(Nothing, realParams)
            Catch ex As Exception
                ShowHelp(entityProp, entryInfo.Prefix)
                Return
            End Try
        Else
            Throw New InvalidOperationException("��ڷ������벻��ʵ��������")
        End If
    End Sub

    Private Sub ShowHelp(entityProp As CommandLineParameterInfo(), prefix As String)
        Dim helpShort = From p In entityProp Select If(p.IsRequired, prefix + p.Name, $"[{prefix}{p.Name}]")
        Dim helpLong = From p In entityProp
                       Select $"{If(p.IsRequired, "", "opt ")}{prefix}{p.Name}: {p.ParamType.Name}{Environment.NewLine}{p.Help}"
        Console.Write("Args: ")
        Console.WriteLine(String.Join(" ", helpShort))
        Console.WriteLine(String.Join(Environment.NewLine, helpLong))
    End Sub
End Class
