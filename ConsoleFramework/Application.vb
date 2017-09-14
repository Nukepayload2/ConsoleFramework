Option Strict On
Imports System.Reflection
Imports System.ComponentModel.DataAnnotations

''' <summary>
''' ��ʾ����̨Ӧ�ó���
''' </summary>
Public NotInheritable Class Application
    ''' <summary>
    ''' ����ָ�����������ĳ����еĿ���̨Ӧ�ó���
    ''' </summary>
    ''' <typeparam name="TMain">����Ӧ�ó�����ڵ�ĳ���������һ������</typeparam>
    ''' <param name="args">Ӧ�ó���Ĳ���</param>
    ''' <exception cref="InvalidOperationException">�ڰ���Ӧ�ó�����ڵĳ������Ҳ���Ψһ���ʵ���������ڷ���</exception>
    Public Shared Sub Run(Of TMain As Class)(args As String())
        Dim asm = GetType(TMain).GetTypeInfo.Assembly
        Run(args, asm)
    End Sub

    ''' <summary>
    ''' ����ָ�����������ĳ����еĿ���̨Ӧ�ó���
    ''' </summary>
    ''' <param name="args">Ӧ�ó���Ĳ���</param>
    ''' <param name="typeInAppAssembly">��Ӧ�ó�������е�����һ������</param>
    ''' <exception cref="InvalidOperationException">�ڰ���Ӧ�ó�����ڵĳ������Ҳ���Ψһ���ʵ���������ڷ���</exception>
    Public Shared Sub Run(args As String(), typeInAppAssembly As Type)
        Dim asm = typeInAppAssembly.GetTypeInfo.Assembly
        Run(args, asm)
    End Sub

    ''' <summary>
    ''' ����ָ�������еĿ���̨Ӧ�ó���
    ''' </summary>
    ''' <param name="args">Ӧ�ó���Ĳ���</param>
    ''' <param name="typeInAppAssembly">Ӧ�ó������</param>
    ''' <exception cref="InvalidOperationException">�ڰ���Ӧ�ó�����ڵĳ������Ҳ���Ψһ���ʵ���������ڷ���</exception>
    Public Shared Sub Run(args() As String, asm As Assembly)
        Dim helpCommands = {"-?", "/?", "--?", "-help", "/help", "--help"}
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
#Disable Warning BC42104 ' ��Ϊ������ֵ֮ǰ�������ѱ�ʹ��
        ElseIf entityProp Is Nothing Then
#Enable Warning BC42104 ' ��Ϊ������ֵ֮ǰ�������ѱ�ʹ��
            ' ��Ӧ����
            entityProp = Aggregate p In params
                         Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                         Let name = If(disp?.Name, p.Name)
                         Let help = disp?.Description
                         Select New CommandLineParameterInfo(Not p.IsOptional, name, help, p.ParameterType) With {
                             .Value = If(p.IsOptional, p.DefaultValue, Nothing)
                         }
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
                ' ѡ��һ��Ҫ����Ĳ���
                Dim paraInf As CommandLineParameterInfo = Nothing
                Dim isImplicitParam = False
                If curArg.StartsWith(entryInfo.Prefix) Then
                    paramIndex.TryGetValue(curArg, paraInf)
                Else
                    Dim unhandledParams = From p In entityProp Where p.IsRequired AndAlso Not p.Handled
                    If unhandledParams.Any Then
                        paraInf = unhandledParams.First
                        isImplicitParam = True
                    End If
                End If
                ' �������
                If paraInf IsNot Nothing Then
                    paraInf.Handled = True
                    If paraInf.ParamType.Equals(GetType(Boolean)) Then
                        If paraInf.Value IsNot Nothing Then
                            paraInf.Value = Not CBool(paraInf.Value)
                        End If
                    Else
                        If Not isImplicitParam Then
                            i += 1
                            curArg = args(i)
                        End If
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
            Next
            ' ���ڿ�ѡ������ʹ�� Type.Missing��δָ���Ĳ���ֵ����Ϊ False��
            For Each v In values
                If Not v.Handled Then
                    If v.IsRequired Then
                        ShowHelp(entityProp, entryInfo.Prefix)
                        Return
                    End If
                    v.Value = Type.Missing
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
                        If prop Is Nothing OrElse prop.GetCustomAttribute(Of DisplayAttribute)?.Name IsNot Nothing Then
                            prop = Aggregate p In entityType.GetRuntimeProperties
                                   Let disp = p.GetCustomAttribute(Of DisplayAttribute)?.Name
                                   Where ep.Name = disp
                                   Select p Into [Single]
                        End If
                        If Not ep.Value.Equals(Type.Missing) Then
                            prop.SetValue(entity, ep.Value)
                        End If
                    Next
                End If
                realParams = {entity}
            Else
                If realParams IsNot Nothing Then
                    For i = 0 To realParams.Length - 1
                        realParams(i) = entityProp(i).Value
                    Next
                End If
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

    Private Shared Sub ShowHelp(entityProp As CommandLineParameterInfo(), prefix As String)
        If entityProp Is Nothing OrElse entityProp.Length = 0 Then
            Console.WriteLine("No argument(s) required.")
            Return
        End If
        Dim helpShort = From p In entityProp Select If(p.IsRequired, prefix + p.Name, $"[{prefix}{p.Name}]")
        Dim helpLong = From p In entityProp
                       Select $"{prefix}{p.Name}: {p.ParamType.Name}{Environment.NewLine}{If(p.IsRequired, "", "(Optional) ")}{p.Help}"
        Console.Write("Arguments: ")
        Console.WriteLine(String.Join(" ", helpShort))
        Console.WriteLine(String.Join(Environment.NewLine, helpLong))
    End Sub
End Class
