Option Strict On
Imports System.Reflection
Imports System.ComponentModel.DataAnnotations

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
        Dim params = entryPoint.GetParameters
        Dim entityProp As CommandLineParameterInfo()
        Dim isEntityModel = False
        Dim entity As Object = Nothing
        ' 解析形参
        If params.Length = 1 Then
            Dim param = params(0)
            Dim parameterType As Type = param.ParameterType
            If Not AllowedParamTypes.Contains(parameterType) Then
                ' 实体类
                entity = Activator.CreateInstance(parameterType)
                isEntityModel = True
                entityProp = Aggregate p In parameterType.GetRuntimeProperties
                             Where p.CanRead AndAlso p.CanWrite AndAlso p.GetAccessors.All(Function(ac) ac.IsPublic)
                             Let req = p.GetCustomAttribute(Of RequiredAttribute)
                             Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                             Let name = If(disp?.Name, p.Name)
                             Let sname = disp?.ShortName
                             Let help = disp?.Description
                             Select New CommandLineParameterInfo(req IsNot Nothing, name, sname, help, p.PropertyType) With {
                                 .Value = If(req Is Nothing, p.GetValue(entity), Nothing)
                             }
                             Into ToArray
            End If
        End If
        If params.Length = 0 Then
            ' 没参数
            entityProp = Nothing
#Disable Warning BC42104 ' 在为变量赋值之前，变量已被使用
        ElseIf entityProp Is Nothing Then
#Enable Warning BC42104 ' 在为变量赋值之前，变量已被使用
            ' 对应参数
            entityProp = Aggregate p In params
                         Let disp = p.GetCustomAttribute(Of DisplayAttribute)
                         Let name = If(disp?.Name, p.Name)
                         Let help = disp?.Description
                         Let sname = disp?.ShortName
                         Select New CommandLineParameterInfo(Not p.IsOptional, name, sname, help, p.ParameterType) With {
                             .Value = If(p.IsOptional, p.DefaultValue, Nothing)
                         }
                         Into ToArray
        End If
        ' 识别帮助命令
        If args.Length = 1 AndAlso HelpCommands.Contains(args(0).ToLowerInvariant) Then
            ShowHelp(entityProp)
            Return
        End If
        ' 将实际传递的参数填入形参。
        Dim realParams As Object()
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
                                    ShowHelp(entityProp)
                                    Return
                                End If
                            Case GetType(Long).FullName
                                Dim value As Long
                                If Long.TryParse(curArg, value) Then
                                    paraInf.Value = value
                                Else
                                    ShowHelp(entityProp)
                                    Return
                                End If
                            Case GetType(Double).FullName
                                Dim value As Double
                                If Double.TryParse(curArg, value) Then
                                    paraInf.Value = value
                                Else
                                    ShowHelp(entityProp)
                                    Return
                                End If
                            Case GetType(Single).FullName
                                Dim value As Single
                                If Single.TryParse(curArg, value) Then
                                    paraInf.Value = value
                                Else
                                    ShowHelp(entityProp)
                                    Return
                                End If
                        End Select
                    End If
                Else
                    ShowHelp(entityProp)
                    Return
                End If
            Next
            ' 对于可选参数，使用 Type.Missing。未指定的布尔值设置为 False。
            For Each v In values
                If Not v.Handled Then
                    If v.IsRequired Then
                        ShowHelp(entityProp)
                        Return
                    End If
                    v.Value = Type.Missing
                End If
            Next
        Else
            realParams = Nothing
        End If
        ' 调用入口
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
        Try
            entryPoint.Invoke(New TApp, realParams)
        Catch ex As TargetInvocationException
            Throw
        Catch ex As Exception
            ShowHelp(entityProp)
            Return
        End Try
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
