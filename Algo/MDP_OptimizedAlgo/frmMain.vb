Option Explicit On
Option Strict On

Imports System.Text
Imports System.Net.Sockets

Public Class frmMain
    Private ClientSocket As New TcpClient
    Private ServerStream As NetworkStream
    Private Solver As New clsSolverPath

    Private MessageProc As New MessageProcessor
    Private WithEvents tmrReader As New Windows.Forms.Timer

    Private ServerAddress As String = "192.168.3.1" ' Set the IP address of the server
    Private PortNumber As Integer = 9101         ' Set the port number used by the server

    Private GoalTarget As Point = New Point(13, 18)
    Private FoundGoal As Boolean = False
    Private FastrunMode As Boolean = False

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RobotImage.Add(My.Resources.RobotNORTH)
        RobotImage.Add(My.Resources.RobotEAST)
        RobotImage.Add(My.Resources.RobotSOUTH)
        RobotImage.Add(My.Resources.RobotWEST)
        Me.Width = Me.Width - Me.ClientRectangle.Width + (picMaze.Left + picMaze.Width + txtIncoming.Left)
        Me.Height = Me.Height - Me.ClientRectangle.Height + (picMaze.Top + picMaze.Height + txtIncoming.Left)
        With Screen.PrimaryScreen.WorkingArea
            Me.Location = New Point(CInt(.Width / 2 - Me.Width / 2), CInt(.Height / 2 - Me.Height / 2))
        End With
        cboDisplay.SelectedIndex = 0

        'Solver.FastSolverPath(New Point(1, 1), clsSolverPath.Direction.NORTH, New Point(13, 18))
        'txtFlood.Text = Solver.GetLayoutInfo(1)
        'txtLayout.Text = Solver.GetLayoutInfo(0)

        'Exit Sub
        If (MsgBox("Please ensure robot is ready and click YES to connect or NO to abort.", vbYesNo Or vbInformation, "TCP Ready To Connect") = MsgBoxResult.Yes) Then
            If (Connect() = True) Then
                If (MsgBox("Connection Successful! Ready to send/receive instructions.", vbOKOnly Or vbInformation, "Connected to Robot") = MsgBoxResult.Ok) Then
                    tmrReader.Interval = 50
                    tmrReader.Start()
                End If
                'SendMessage("TZ")
            Else
                If (MsgBox("Failed to connect to robot!", vbExclamation Or vbOKOnly, "Connection Failed") = MsgBoxResult.Ok) Then End
            End If
        Else
            End
        End If

        'Dim TempDir As clsSolverPath.Direction = clsSolverPath.Direction.EAST
        'Solver.PlaceObstacle(New Point(1, 1), TempDir, clsSolverPath.Sensors.SL)
        'Solver.PlaceObstacle(New Point(1, 1), TempDir, clsSolverPath.Sensors.FL)
        'Solver.PlaceObstacle(New Point(1, 1), TempDir, clsSolverPath.Sensors.F)
        'Solver.PlaceObstacle(New Point(1, 1), TempDir, clsSolverPath.Sensors.FR)
        'Solver.PlaceObstacle(New Point(1, 1), TempDir, clsSolverPath.Sensors.SR)
    End Sub

    Private Sub SendMessage(Message As String)
        Try
            Dim OutStream As Byte() = Encoding.ASCII.GetBytes(Message & vbCrLf)
            ServerStream = ClientSocket.GetStream()
            ServerStream.Write(OutStream, 0, OutStream.Length)
            txtOutgoing.Text = txtOutgoing.Text & Message & vbCrLf
            txtOutgoing.SelectionStart = txtOutgoing.Text.Length
            txtOutgoing.SelectionLength = 0
            txtOutgoing.ScrollToCaret()
        Catch ex As Exception
        End Try
    End Sub

    Private Function ReadMessage() As String
        Try
            Dim inStream(100000) As Byte
            Dim ReceivedData As String, ReadCount As Integer
            ServerStream = ClientSocket.GetStream()
            If ServerStream.CanRead Then
                If (ServerStream.DataAvailable) Then
                    ReadCount = ServerStream.Read(inStream, 0, inStream.Length)
                    ReceivedData = Encoding.ASCII.GetString(inStream)
                    ReceivedData = Strings.Left(ReceivedData, ReadCount)
                    If (ReadCount > 0) Then
                        Return ReceivedData
                    Else
                        Return ""
                    End If
                End If
            End If
        Catch ex As Exception
        End Try
        Return ""
    End Function

    Private Function Connect() As Boolean
        Try
            ClientSocket.Connect(ServerAddress, PortNumber)
            Return True
        Catch ex As Exception
        End Try
        Return False
    End Function

    Private MessageQueue As New MessageProcessor

    Private Sub tmrReader_Tick() Handles tmrReader.Tick
        Dim Message As String = ReadMessage()
        Dim ProcMessage As String
        If (Message IsNot Nothing) Then
            ProcMessage = Message
            ProcMessage = Strings.Replace(ProcMessage, vbCr, "")
            ProcMessage = Strings.Replace(ProcMessage, vbLf, "")
            If (ProcMessage IsNot Nothing) Then
                If (ProcMessage.Count > 0) Then MessageQueue.WriteQueue(ProcMessage)
            End If
            ProcMessage = MessageQueue.ReadCommand("Z")
            If (ProcMessage.Count > 0) Then
                txtIncoming.Text = txtIncoming.Text & ProcMessage & vbCrLf
                txtIncoming.SelectionStart = txtIncoming.Text.Length
                txtIncoming.SelectionLength = 0
                txtIncoming.ScrollToCaret()
                InterpretCellMessage(ProcMessage)
            End If
        End If
    End Sub

    Public Sub InterpretCellMessage(ByVal Message As String)
        If (Message.Length > 0) Then
            Dim RobotCoord As Point, RobotDir As clsSolverPath.Direction, Msg As String = ""
            Select Case Message.ToUpper.Trim
                Case "EXP"
                    FoundGoal = False
                    FastrunMode = False
                    GoalTarget = New Point(13, 18)
                    Solver.ResetLayout()
                    SendMessage("TZ")
                Case "SP"
                    GoalTarget = New Point(13, 18)
                    RobotCoord = New Point(1, 1)
                    RobotDir = clsSolverPath.Direction.NORTH
                    Msg = Solver.FastSolverPath(RobotCoord, RobotDir, GoalTarget, False, False)
                    'MsgBox(RobotCoord.X & " " & RobotCoord.Y & " " & RobotDir & " " & Msg)
                    If (Msg.Length > 0) Then SendMessage(Msg)
                Case Else
                    Select Case Val(Strings.Left(Message, 1))
                        Case clsSolverPath.MessageType.CELL_UPDATE
                            'If (Message.Length = 11) And (FastrunMode = False) Then
                            If (Message.Length = 11) Then
                                RobotCoord = New Point(CInt(Val(Strings.Mid(Message, 2, 2))), CInt(Val(Strings.Mid(Message, 4, 2))))
                                RobotDir = CType(Val(Strings.Mid(Message, 6, 1)), clsSolverPath.Direction)
                                Dim SensorDetails As String = Strings.Mid(Message, 7, 5)
                                Solver.UpdateObstacles(RobotCoord, RobotDir, SensorDetails)
                                Solver.UpdateExploration(RobotCoord)
                                'txtLayout.Text = Solver.GetLayoutInfo(0)
                                DisplayLayout()
                                PlotRobot = New Point(RobotCoord.X, RobotCoord.Y)
                                PlotRobotDir = RobotDir
                                picMaze.Refresh()
                            End If
                        Case clsSolverPath.MessageType.REQUEST_PATH
                            If (Message.Length = 11) Then
                                RobotCoord = New Point(CInt(Val(Strings.Mid(Message, 2, 2))), CInt(Val(Strings.Mid(Message, 4, 2))))
                                RobotDir = CType(Val(Strings.Mid(Message, 6, 1)), clsSolverPath.Direction)
                                If (RobotCoord.X = GoalTarget.X) And (RobotCoord.Y = GoalTarget.Y) Then
                                    If (RobotCoord.X = 13) And (RobotCoord.Y = 18) Then
                                        FoundGoal = True
                                        GoalTarget = New Point(1, 1)
                                        If (FastrunMode = True) Then
                                            Exit Sub
                                        End If
                                    Else
                                        If (FoundGoal = True) Then FastrunMode = True
                                        GoalTarget = New Point(13, 18)
                                        Select Case RobotDir
                                            Case clsSolverPath.Direction.EAST : SendMessage("FXL00ZTC01010Z")
                                            Case clsSolverPath.Direction.SOUTH : SendMessage("FXB00ZTC01010Z")
                                            Case clsSolverPath.Direction.WEST : SendMessage("FXR00ZTC01010Z")
                                        End Select
                                        RobotDir = clsSolverPath.Direction.NORTH
                                    End If
                                End If
                                Msg = Solver.FastSolverPath(RobotCoord, RobotDir, GoalTarget, False)
                                PlotCoord = New Point(RobotCoord.X, RobotCoord.Y)
                                PlotDir = RobotDir
                                PathInfo = Msg
                                txtFlood.Text = Solver.GetLayoutInfo(1)
                                picMaze.Refresh()
                                If (FoundGoal = True) And (FastrunMode = True) And (GoalTarget.X = 13) And (GoalTarget.Y = 18) Then Msg = ""
                                If (Msg.Length > 0) Then SendMessage(Msg)
                            End If
                    End Select
            End Select
        End If
    End Sub

    Private PlotRobot As Point = New Point(1, 1), PlotRobotDir As clsSolverPath.Direction = clsSolverPath.Direction.NORTH
    Private PlotCoord As Point = New Point(1, 1), PlotDir As clsSolverPath.Direction = clsSolverPath.Direction.NORTH, PathInfo As String = "FS17R12Z"
    Private RobotImage As New List(Of Bitmap)
    Private PathPen As New Pen(Brushes.Red, 3)
    Private Font8 As New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point)
    Private ShowExplored As Boolean = True

    Private Sub picMaze_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles picMaze.Paint
        With e.Graphics
            Dim ObstaclesCount As Integer = 0
            .PageUnit = GraphicsUnit.Pixel
            .CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            .InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            .TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            .SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            .PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            .Clear(Color.Black)
            .FillRectangle(Brushes.White, New Rectangle(New Point(17, 17 + 20), New Size(334, 444)))
            .FillRectangle(Brushes.DimGray, New Rectangle(New Point(19, 19 + 20), New Size(330, 440)))
            .FillRectangle(Brushes.Gray, New Rectangle(New Point(20, 392 + 20), New Size(22 * 3, 22 * 3)))
            .FillRectangle(Brushes.Gray, New Rectangle(New Point(282, 20 + 20), New Size(22 * 3, 22 * 3)))
            For Y = 0 To 19
                For X = 0 To 14
                    If ((Solver.LayoutMap(X, 19 - Y) And clsSolverPath.CELL_FLAGS.BLOCKED) > 0) Then
                        .FillRectangle(Brushes.Silver, New Rectangle(New Point(X * 22 + 20, Y * 22 + 40), New Size(20, 20)))
                        ObstaclesCount = ObstaclesCount + 1
                    Else
                        If (ShowExplored = True) Then
                            If ((Solver.LayoutMap(X, 19 - Y) And clsSolverPath.CELL_FLAGS.EXPLORED) > 0) Then
                                .FillRectangle(Brushes.DarkSlateGray, New Rectangle(New Point(X * 22 + 20, Y * 22 + 40), New Size(20, 20)))
                            Else
                                '.FillRectangle(Brushes.DimGray, New Rectangle(New Point(X * 22 + 20, Y * 22 + 40), New Size(20, 20)))
                            End If
                        Else
                            '.FillRectangle(Brushes.DimGray, New Rectangle(New Point(X * 22 + 20, Y * 22 + 40), New Size(20, 20)))
                        End If
                    End If
                    If (Y > 0) And (X > 0) Then .FillEllipse(Brushes.Yellow, New Rectangle(X * 22 + 20 - 3, Y * 22 + 40 - 3, 4, 4))
                Next X
            Next Y
            If (PathInfo.Count > 2) Then
                Dim TempDir As Integer = PlotDir, StartX As Integer = PlotCoord.X * 22, StartY As Integer = PlotCoord.Y * 22, Magnitude As Integer
                Dim EndX As Integer, EndY As Integer
                .FillEllipse(Brushes.Red, New Rectangle(New Point(StartX + 20 + 7, 460 - StartY + 5), New Size(6, 6)))
                For Ind = 2 To PathInfo.Count - 2 Step 3
                    Select Case Strings.Mid(PathInfo, Ind, 1).ToUpper
                        Case "S"
                        Case "R" : TempDir = (TempDir + 1) Mod 4
                        Case "L" : TempDir = (TempDir + 3) Mod 4
                        Case "B" : TempDir = (TempDir + 2) Mod 4
                    End Select
                    Magnitude = CInt(Val(Strings.Mid(PathInfo, Ind + 1, 2)))
                    For SubInd = 1 To Magnitude
                        EndX = StartX
                        EndY = StartY
                        Select Case CType(TempDir, clsSolverPath.Direction)
                            Case clsSolverPath.Direction.NORTH : EndY = EndY + 22
                            Case clsSolverPath.Direction.EAST : EndX = EndX + 22
                            Case clsSolverPath.Direction.SOUTH : EndY = EndY - 22
                            Case clsSolverPath.Direction.WEST : EndX = EndX - 22
                        End Select
                        .DrawLine(PathPen, New Point(StartX + 20 + 10, 460 - StartY + 8), New Point(EndX + 20 + 10, 460 - EndY + 8))
                        StartX = EndX
                        StartY = EndY
                    Next SubInd
                Next Ind
                .FillEllipse(Brushes.Red, New Rectangle(New Point(EndX + 20 + 7, 460 - EndY + 5), New Size(6, 6)))
            End If
            .DrawImage(RobotImage(PlotRobotDir), New Rectangle(PlotRobot.X * 22 - 3, 440 - PlotRobot.Y * 22 - 5, 66, 66))
            .DrawString("Obstacles Found: " & ObstaclesCount.ToString(), Font8, Brushes.White, New Point(15, picMaze.ClientRectangle.Height - 25))
        End With
    End Sub

    Private Sub lblTitle_Click(sender As System.Object, e As System.EventArgs) Handles lblTitle.Click

    End Sub

    Private Sub cmdManualStart_Click(sender As System.Object, e As System.EventArgs) Handles cmdManualStart.Click
        InterpretCellMessage("EXP")
    End Sub

    Private Sub cmdFastrun_Click(sender As System.Object, e As System.EventArgs) Handles cmdFastrun.Click
        InterpretCellMessage("SP")
    End Sub

    Private Sub DisplayLayout()
        Dim ExpStr As String = "", BlockStr As String = "", ExpHex As String = "", BlockHex As String = ""
        If (cboDisplay.SelectedIndex > 0) Then Solver.GenerateString(ExpStr, BlockStr, ExpHex, BlockHex)
        Select Case cboDisplay.SelectedIndex
            Case 0 : txtLayout.Text = Solver.GetLayoutInfo(0)
            Case 1 : txtLayout.Text = ExpStr
            Case 2 : txtLayout.Text = ExpHex
            Case 3 : txtLayout.Text = BlockStr
            Case 4 : txtLayout.Text = BlockHex
        End Select
    End Sub

    Private Sub cboDisplay_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cboDisplay.SelectedIndexChanged
        DisplayLayout()
    End Sub

    Private Sub picMaze_Click(sender As System.Object, e As System.EventArgs) Handles picMaze.Click
        'X+20
        'Y+40
    End Sub

    Private Sub picMaze_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles picMaze.MouseDown
        With e.Location
            Dim XStart As Integer, YStart As Integer
            If ((.X > 20) And (.Y > 40)) Then
                If ((.X < (22 * 16 + 20)) And (.Y < (22 * 21 + 40))) Then
                    XStart = CInt((.X - 20) / 22)
                    YStart = CInt((.Y - 40) / 22)
                    Solver.PlaceObstacle(New Point(XStart, 19 - YStart), clsSolverPath.Direction.CENTER, clsSolverPath.Sensors.F)
                    DisplayLayout()
                    picMaze.Refresh()
                End If
            End If
        End With
    End Sub
End Class
