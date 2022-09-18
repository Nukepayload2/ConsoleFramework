Option Strict On
Imports System.Reflection
Imports System.ComponentModel.DataAnnotations
Imports System.Runtime.CompilerServices

''' <summary>
''' 表示控制台应用程序
''' </summary>
Public NotInheritable Class Application
    Public Shared ReadOnly Property HelpCommands As New List(Of String) From {
        "-?", "/?", "-h", "/help", "--help"
    }
    Public Shared ReadOnly Property AllowedParamTypes As Type() = {
        GetType(String),
        GetType(Integer), GetType(Long),
        GetType(Double), GetType(Single),
        GetType(Boolean)
    }

    ''' <summary>
    ''' 运行指定类型所属的程序集中的控制台应用程序。
    ''' </summary>
    ''' <typeparam name="TApp">应用程序类。</typeparam>
    ''' <param name="args">应用程序的参数</param>
    ''' <exception cref="InvalidOperationException">找不到唯一合适的入口方法</exception>
    Public Shared Sub Run(Of TApp As New)(args As String())
        Dim entryPoint = Aggregate main In GetType(TApp).GetRuntimeMethods
                         Let mp = main.GetCustomAttribute(Of EntryMethodAttribute)
                         Where mp IsNot Nothing
                         Select main Into [Single]
        Dim entityProp As CommandLineParameterInfo() = Nothing
        Dim realParams As Object() = Nothing
        If TryParseParameters(args, entryPoint, entityProp, realParams) Then
            Try
                entryPoint.Invoke(New TApp, realParams)
            Catch ex As TargetInvocationException
                Throw
            Catch ex As Exception
                ShowHelp(entityProp)
            End Try
        Else
            ShowHelp(entityProp)
        End If
    End Sub

    ''' <summary>
    ''' 将命令行参数转换成指定方法的参数并调用指定的入口方法。
    ''' </summary>
    ''' <typeparam name="TApp">应用程序类。</typeparam>
    ''' <param name="args">应用程序的参数</param>
    ''' <param name="entryMethod">入口方法的委托</param>
    ''' <exception cref="InvalidOperationException">找不到唯一合适的入口方法</exception>
    Public Shared Sub Run(args As String(), entryMethod As System.Delegate)
        Dim entryPoint = entryMethod.Method
        Dim entityProp As CommandLineParameterInfo() = Nothing
        Dim realParams As Object() = Nothing
        If TryParseParameters(args, entryPoint, entityProp, realParams) Then
            Try
                Dim retVal = entryMethod.DynamicInvoke(realParams)
                Dim retType = entryMethod.Method.ReturnType
                If IsAwaitable(retType) AndAlso retVal IsNot Nothing Then
                    LateWaitTask(retVal)
                End If
            Catch ex As TargetInvocationException
                Throw ex.InnerException
            Catch ex As Exception
                ShowHelp(entityProp)
            End Try
        Else
            ShowHelp(entityProp)
        End If
    End Sub

    Private Shared Function IsAwaitable(returnType As Type) As Boolean
        IsAwaitable = False

        If returnType = GetType(Task) OrElse returnType = GetType(ValueTask) Then
            Return True
        End If

        If returnType IsNot GetType(Void) Then
            Dim getAwaiterMethod As MethodInfo = returnType.GetRuntimeMethod("GetAwaiter", Type.EmptyTypes)
            If getAwaiterMethod IsNot Nothing Then
                Dim awaiterType As Type = getAwaiterMethod.ReturnType
                IsAwaitable = IsAwaiter(awaiterType)
            End If
        End If
    End Function

    Private Shared Function IsAwaiter(awaiterType As Type) As Boolean
        IsAwaiter = False

        Dim isCriticalNotify = GetType(ICriticalNotifyCompletion).IsAssignableFrom(awaiterType)
        Dim isNotify = GetType(INotifyCompletion).IsAssignableFrom(awaiterType)
        If isCriticalNotify OrElse isNotify Then
            Dim isCompletedProperty As PropertyInfo = awaiterType.GetRuntimeProperty("IsCompleted")
            If isCompletedProperty IsNot Nothing AndAlso isCompletedProperty.CanRead Then
                Dim getResultMethod As MethodInfo = awaiterType.GetRuntimeMethod("GetResult", Type.EmptyTypes)
                If getResultMethod IsNot Nothing Then IsAwaiter = True
            End If
        End If
    End Function

    ''' <summary>
    ''' 将当前进程的命令行参数转换成指定方法的参数并调用指定的入口方法。
    ''' </summary>
    ''' <typeparam name="TApp">应用程序类。</typeparam>
    ''' <param name="entryMethod">入口方法的委托</param>
    ''' <exception cref="InvalidOperationException">找不到唯一合适的入口方法</exception>
    Public Shared Sub Run(entryMethod As [Delegate])
        Run(Environment.GetCommandLineArgs.Skip(1).ToArray, entryMethod)
    End Sub

    Private Shared Function TryParseParameters(args() As String,
                                               entryPoint As MethodInfo,
                                               ByRef entityProp() As CommandLineParameterInfo,
                                               ByRef realParams() As Object) As Boolean
        Dim params = entryPoint.GetParameters
        Dim isEntityModel = False
        Dim entity As Object = Nothing
        ' 解析形参
        TryParseEntityParameters(entityProp, params, isEntityModel, entity)

        If params.Length = 0 Then
            ' 没参数
            entityProp = Nothing
        ElseIf entityProp Is Nothing Then
            ' 对应参数
            entityProp = Aggregate p In params
                         Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                         Let name = If(disp?.Name, p.Name)
                         Let help = disp?.Description
                         Let sname = disp?.ShortName
                         Select New CommandLineParameterInfo(Not p.IsOptional, name, sname, help, p.ParameterType) With {
                             .Value = If(p.IsOptional, If(p.HasDefaultValue, p.DefaultValue, Nothing), Nothing)
                         }
                         Into ToArray
        End If

        ' 识别帮助命令
        If args.Length = 1 AndAlso HelpCommands.Contains(args(0).ToLowerInvariant) Then
            Return False
        End If

        If entityProp IsNot Nothing Then
            Dim paramIndex = entityProp.ToDictionary(Function(o) LongPrefix + o.Name.ToLowerInvariant, Function(o) o)
            Dim values = paramIndex.Values
            ReDim realParams(paramIndex.Count - 1)
            For Each prop In entityProp
                If prop.ShortName.Length > 0 Then
                    Dim key As String = ShortPrefix + prop.ShortName.ToLowerInvariant
                    If paramIndex.ContainsKey(key) Then
                        Throw New ParameterMappingViolationException("短名称存在冲突。")
                    End If
                    paramIndex.Add(key, prop)
                End If
            Next
            ' 检查参数是否符合要求
            For Each v In values
                ' 参数的类型检查
                If Not AllowedParamTypes.Contains(v.ParamType) Then
                    Throw New ParameterMappingViolationException("参数类型不是预期的。请查看帮助 https://github.com/Nukepayload2/ConsoleFramework/blob/master/README.md。")
                End If
                ' 布尔值必须是可选参数
                If v.IsRequired AndAlso v.ParamType.Equals(GetType(Boolean)) Then
                    Throw New ParameterMappingViolationException("布尔值必须是可选参数。")
                End If
            Next
            For i = 0 To args.Length - 1
                Dim curArg = args(i)
                ' 选择一个要处理的参数
                Dim paraInf As CommandLineParameterInfo = Nothing
                Dim isImplicitParam = False
                If curArg.StartsWith(ShortPrefix) OrElse curArg.StartsWith(LongPrefix) Then
                    paramIndex.TryGetValue(curArg.ToLowerInvariant, paraInf)
                Else
                    Dim unhandledParams = From p In entityProp Where p.IsRequired AndAlso Not p.Handled
                    If unhandledParams.Any Then
                        paraInf = unhandledParams.First
                        isImplicitParam = True
                    End If
                End If
                ' 处理参数
                If paraInf IsNot Nothing Then
                    paraInf.Handled = True
                    If paraInf.ParamType.Equals(GetType(Boolean)) Then
                        paraInf.Value = Not CBool(paraInf.Value)
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
                                    Return False
                                End If
                            Case GetType(Long).FullName
                                Dim value As Long
                                If Long.TryParse(curArg, value) Then
                                    paraInf.Value = value
                                Else
                                    Return False
                                End If
                            Case GetType(Double).FullName
                                Dim value As Double
                                If Double.TryParse(curArg, value) Then
                                    paraInf.Value = value
                                Else
                                    Return False
                                End If
                            Case GetType(Single).FullName
                                Dim value As Single
                                If Single.TryParse(curArg, value) Then
                                    paraInf.Value = value
                                Else
                                    Return False
                                End If
                        End Select
                    End If
                Else
                    Return False
                End If
            Next
        Else
            realParams = Nothing
        End If

        ' 准备参数，调用入口用
        realParams = ActivateParameters(entityProp, realParams, params, isEntityModel, entity)
        Return True
    End Function

    Private Shared Function ActivateParameters(entityProp() As CommandLineParameterInfo, realParams() As Object, params() As ParameterInfo, isEntityModel As Boolean, entity As Object) As Object()
        If isEntityModel Then
            Dim entityType = params(0).ParameterType
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

        Return realParams
    End Function

    Private Shared Sub TryParseEntityParameters(ByRef entityProp() As CommandLineParameterInfo,
                                                params() As ParameterInfo,
                                                ByRef isEntityModel As Boolean,
                                                ByRef entity As Object)
        If params.Length = 1 Then
            Dim param = params(0)
            Dim parameterType As Type = param.ParameterType
            If Not AllowedParamTypes.Contains(parameterType) Then
                ' 实体类
                entity = Activator.CreateInstance(parameterType)
                Dim entityByVal = entity
                isEntityModel = True
                entityProp = Aggregate p In parameterType.GetRuntimeProperties
                             Where p.CanRead AndAlso p.CanWrite AndAlso p.GetAccessors.All(Function(ac) ac.IsPublic)
                             Let req = p.GetCustomAttribute(Of RequiredAttribute)
                             Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                             Let name = If(disp?.Name, p.Name)
                             Let sname = disp?.ShortName
                             Let help = disp?.Description
                             Select New CommandLineParameterInfo(req IsNot Nothing, name, sname, help, p.PropertyType) With {
                                 .Value = If(req Is Nothing, p.GetValue(entityByVal), Nothing)
                             }
                             Into ToArray
            End If
        End If
    End Sub

    Public Shared Property ShortPrefix As String = "-"
    Public Shared Property LongPrefix As String = "--"

    Private Shared Sub ShowHelp(entityProp As CommandLineParameterInfo())
        If entityProp Is Nothing OrElse entityProp.Length = 0 Then
            Console.WriteLine(LocalizedStrings.NoParamsRequired)
            Return
        End If
        Dim helpShort =
            From p In entityProp
            Select If(p.IsRequired, p.Name, $"[{p.Name}]")
        Dim helpLong = From p In entityProp
                       Select $"{If(Not String.IsNullOrEmpty(p.ShortName), ShortPrefix + p.ShortName + "|", "")}{LongPrefix}{p.Name}: {p.ParamType.Name}{Environment.NewLine}{If(p.IsRequired, "", $"({LocalizedStrings.Optional}) ")}{p.Help}"
        Console.Write(LocalizedStrings.Arguments)
        Console.WriteLine(String.Join(" ", helpShort))
        Console.WriteLine(String.Join(Environment.NewLine, helpLong))
    End Sub
End Class
