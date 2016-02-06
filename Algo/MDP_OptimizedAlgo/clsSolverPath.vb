Imports Microsoft.VisualBasic

Public Class clsSolverPath
    Public Const MAZE_WIDTH = 15
    Public Const MAZE_HEIGHT = 20

    Public Enum MessageType As Byte
        CELL_UPDATE = 1
        REQUEST_PATH = 2
    End Enum

    Public Enum Direction As Byte
        NORTH = 0
        EAST = 1
        SOUTH = 2
        WEST = 3
        CENTER = 4
    End Enum

    Public Enum Sensors As Byte
        SL = 0
        FL = 1
        F = 2
        FR = 3
        SR = 4
    End Enum

    Public Enum CELL_FLAGS As Integer
        BLOCKED = 1
        VIRT_BLOCK = 2
        EXPLORED = 4
        HARD_EXPLORED = 8

        FLOODABLE = -1
        UNFLOODABLE = -2
    End Enum

    Public LayoutMap(0 To MAZE_WIDTH - 1, 0 To MAZE_HEIGHT - 1) As Integer
    Public FloodMap(0 To MAZE_WIDTH - 1, 0 To MAZE_HEIGHT - 1) As Integer
    Private Obstacle(0 To 4) As List(Of Point)
    Private Move As New List(Of Point)

    Public Sub GenerateString(ByRef ExploredStr As String, ByRef BlockedStr As String, ByRef ExploredHex As String, ByRef BlockedHex As String)
        Dim MapStr As String = ""
        Dim X As Integer, Y As Integer
        For Y = 0 To 19
            For X = 0 To 14
                If ((LayoutMap(X, Y) And CELL_FLAGS.HARD_EXPLORED) > 0) Then
                    ExploredStr = ExploredStr & "1"
                    If ((LayoutMap(X, Y) And CELL_FLAGS.BLOCKED) > 0) Then
                        BlockedStr = BlockedStr & "1"
                    Else
                        BlockedStr = BlockedStr & "0"
                    End If
                Else
                    ExploredStr = ExploredStr & "0"
                End If
            Next X
            If (ExploredStr.Length > 1) Then
                If (Strings.Right(ExploredStr, 2) <> vbCrLf) Then ExploredStr = ExploredStr & vbCrLf
            End If
            If (BlockedStr.Length > 1) Then
                If (Strings.Right(BlockedStr, 2) <> vbCrLf) Then BlockedStr = BlockedStr & vbCrLf
            End If
        Next Y
        ExploredStr = "11" & vbCrLf & ExploredStr & "11"
        ExploredHex = StringToHex(ExploredStr)
        BlockedHex = StringToHex(BlockedStr)
    End Sub

    Public Function StringToHex(BinStr As String) As String
        Dim MapStr As String, TempRead As String, ByteVal As Integer, TempHex As String, Ind As Integer
        StringToHex = ""
        MapStr = BinStr.Replace(vbCr, "").Replace(vbLf, "")
        MapStr = MapStr & Space(8 - (MapStr.Length Mod 8)).Replace(" ", "0")
        Do While (MapStr.Length >= 8)
            TempRead = Strings.Left(MapStr, 8)
            MapStr = Strings.Right(MapStr, MapStr.Length - 8)
            ByteVal = 0
            For Ind = 0 To TempRead.Count - 1
                If (Strings.Mid(TempRead, Ind + 1, 1) = "1") Then
                    ByteVal = ByteVal + 2 ^ (7 - Ind)
                End If
            Next Ind
            TempHex = Strings.Right("00" & Hex(ByteVal).Trim, 2)
            StringToHex = StringToHex & TempHex
        Loop
    End Function

    Public Function MoveCoord(ByVal RobotCoord As point, ByVal RobotDir As Direction, Optional ByVal Cells As Byte = 1) As point
        Dim Ind As Byte, X As Integer = RobotCoord.X, Y As Integer = RobotCoord.Y
        If (Cells < 1) Then Cells = 1
        For Ind = 0 To Cells - 1
            With RobotCoord
                Select Case RobotDir
                    Case Direction.NORTH : Y = Y + 1
                    Case Direction.EAST : X = X + 1
                    Case Direction.SOUTH : Y = Y - 1
                    Case Direction.WEST : X = X - 1
                End Select
            End With
        Next Ind
        Return New point(X, Y)
    End Function

    Public Sub New()
        Dim Ind As Integer
        For Ind = 0 To 4
            Obstacle(Ind) = New List(Of Point)
        Next Ind
        With Obstacle(Direction.NORTH)
            .Add(New Point(-2, 1))
            .Add(New Point(-1, 2))
            .Add(New Point(0, 2))
            .Add(New Point(1, 2))
            .Add(New Point(2, 1))
        End With
        With Obstacle(Direction.EAST)
            .Add(New Point(1, 2))
            .Add(New Point(2, 1))
            .Add(New Point(2, 0))
            .Add(New Point(2, -1))
            .Add(New Point(1, -2))
        End With
        With Obstacle(Direction.SOUTH)
            .Add(New Point(2, -1))
            .Add(New Point(1, -2))
            .Add(New Point(0, -2))
            .Add(New Point(-1, -2))
            .Add(New Point(-2, -1))
        End With
        With Obstacle(Direction.WEST)
            .Add(New Point(-1, -2))
            .Add(New Point(-2, -1))
            .Add(New Point(-2, 0))
            .Add(New Point(-2, 1))
            .Add(New Point(-1, 2))
        End With
        With Obstacle(Direction.CENTER)
            .Add(New Point(0, 0))
            .Add(New Point(0, 0))
            .Add(New Point(0, 0))
            .Add(New Point(0, 0))
            .Add(New Point(0, 0))
        End With
        ResetLayout()
    End Sub

    Public Sub ResetLayout()
        Dim X As Integer, Y As Integer
        For X = 0 To MAZE_WIDTH - 1
            For Y = 0 To MAZE_HEIGHT - 1
                If (X = 0) Or (X = MAZE_WIDTH - 1) Or (Y = 0) Or (Y = MAZE_HEIGHT - 1) Then
                    LayoutMap(X, Y) = CELL_FLAGS.VIRT_BLOCK
                Else
                    LayoutMap(X, Y) = 0
                End If
                'LayoutMap(X, Y) = LayoutMap(X, Y) Or CELL_FLAGS.HARD_EXPLORED
            Next Y
        Next X
    End Sub

    Public Function PlaceObstacle(ByVal RobotCoord As Point, ByVal RobotDir As Direction, Sensor As Sensors) As Boolean
        Dim NewPoint As Point
        With RobotCoord
            NewPoint = New Point(Obstacle(RobotDir)(Sensor).X + .X, Obstacle(RobotDir)(Sensor).Y + .Y)
        End With
        If (CheckBoundary(NewPoint) = True) Then
            With NewPoint
                LayoutMap(.X, .Y) = LayoutMap(.X, .Y) Or CELL_FLAGS.BLOCKED Or CELL_FLAGS.EXPLORED Or CELL_FLAGS.HARD_EXPLORED
                For X = -1 To 1
                    For Y = -1 To 1
                        If (CheckBoundary(.X + X, .Y + Y) = True) Then
                            LayoutMap(.X + X, .Y + Y) = LayoutMap(.X + X, .Y + Y) Or CELL_FLAGS.VIRT_BLOCK Or CELL_FLAGS.EXPLORED
                        End If
                    Next Y
                Next X
            End With
            Return True
        End If
        Return False
    End Function

    Public Function CheckBoundary(ByVal RobotCoord As Point) As Boolean
        With RobotCoord
            Return CheckBoundary(.X, .Y)
        End With
    End Function

    Public Function CheckBoundary(ByVal X As Integer, ByVal Y As Integer) As Boolean
        If (X < 0) Or (X > MAZE_WIDTH - 1) Or (Y < 0) Or (Y > MAZE_HEIGHT - 1) Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub InitalizeFloodMap(Optional ByVal BlockUnexplored As Boolean = False)
        Dim X As Integer, Y As Integer
        For X = 0 To MAZE_WIDTH - 1
            For Y = 0 To MAZE_HEIGHT - 1
                If (BlockUnexplored = True) Then
                    If ((LayoutMap(X, Y) And CELL_FLAGS.EXPLORED) = 0) Then
                        FloodMap(X, Y) = CELL_FLAGS.UNFLOODABLE
                    Else
                        FloodMap(X, Y) = CELL_FLAGS.FLOODABLE
                    End If
                Else
                    If ((LayoutMap(X, Y) And (CELL_FLAGS.BLOCKED Or CELL_FLAGS.VIRT_BLOCK)) > 0) Then
                        FloodMap(X, Y) = CELL_FLAGS.UNFLOODABLE
                    Else
                        FloodMap(X, Y) = CELL_FLAGS.FLOODABLE
                    End If
                End If
            Next Y
        Next X
    End Sub

    Public Function GetLayoutInfo(MapType As Integer) As String
        Dim Layout As String = vbCrLf & "  ", CellInfo As String
        For Y = MAZE_HEIGHT - 1 To 0 Step -1
            For X = 0 To MAZE_WIDTH - 1
                If (MapType = 0) Then
                    CellInfo = LayoutMap(X, Y).ToString()
                Else
                    CellInfo = FloodMap(X, Y).ToString()
                End If
                CellInfo = CellInfo & Space(5 - CellInfo.Length)
                Layout = Layout & CellInfo
            Next X
            Layout = Layout & vbCrLf & "  "
        Next Y
        Return Layout
    End Function

    Public Function FastSolverPath(ByVal RobotCoord As Point, ByVal RobotDir As Direction, ByVal DestCoord As Point, Optional ByVal BlockUnexplored As Boolean = False, Optional ByVal FastRun As Boolean = False) As String
        Dim Toggle As Byte = 1, WriteToggle As Byte
        Dim CoordList(0 To 1) As List(Of Point)
        Dim TempCoord As Point
        Dim Iteration As Integer = 0
        Dim Processed As Boolean = False, FoundGoal As Boolean = False
        CoordList(0) = New List(Of Point)
        CoordList(1) = New List(Of Point)
        CoordList(0).Add(New Point(DestCoord.X, DestCoord.Y))
        InitalizeFloodMap(BlockUnexplored)
        FloodMap(DestCoord.X, DestCoord.Y) = 0
        Do
            Processed = False
            Iteration = Iteration + 1
            WriteToggle = Toggle Mod 2 + 1
            CoordList(WriteToggle - 1).Clear()
            For Ind = 0 To CoordList(Toggle - 1).Count - 1
                With CoordList(Toggle - 1)(Ind)
                    For Dirs = 0 To 3
                        TempCoord = MoveCoord(New Point(.X, .Y), Dirs)
                        If (CheckBoundary(TempCoord) = True) Then
                            If (FloodMap(TempCoord.X, TempCoord.Y) = CELL_FLAGS.FLOODABLE) Then
                                FloodMap(TempCoord.X, TempCoord.Y) = Iteration
                                CoordList(WriteToggle - 1).Add(New Point(TempCoord.X, TempCoord.Y))
                                If (TempCoord.X = RobotCoord.X) And (TempCoord.Y = RobotCoord.Y) Then FoundGoal = True
                                Processed = True
                            End If
                        End If
                    Next Dirs
                End With
            Next Ind
            Toggle = Toggle Mod 2 + 1
            If (FoundGoal = True) Then Exit Do
        Loop While (Processed = True)
        Dim PathStr As String = ""
        If (FoundGoal = True) Then
            Dim LastDir As Integer = RobotDir + 1, NextDir = -1
            Dim NewCoord As Point, MoveCount As Integer = 0
            PathStr = "F"
            If (FastRun = True) Then
                PathStr = PathStr & "A"
            Else
                PathStr = PathStr & "X"
            End If
            Iteration = FloodMap(RobotCoord.X, RobotCoord.Y)
            TempCoord = New Point(RobotCoord.X, RobotCoord.Y)
            Do
                For Ind = 0 To 3
                    NewCoord = MoveCoord(TempCoord, LastDir - 1)
                    With NewCoord
                        If (FloodMap(.X, .Y) = Iteration - 1) Then
                            If (NextDir <> LastDir) Then
                                If (MoveCount > 0) Then
                                    PathStr = PathStr & MoveCount.ToString("00")
                                    MoveCount = 1
                                End If
                                Select Case Ind
                                    Case 0 : PathStr = PathStr & "S"
                                    Case 1 : PathStr = PathStr & "R"
                                    Case 2 : PathStr = PathStr & "B"
                                    Case 3 : PathStr = PathStr & "L"
                                End Select
                                NextDir = LastDir
                                MoveCount = 1
                            Else
                                MoveCount = MoveCount + 1
                            End If
                            TempCoord = NewCoord
                            Exit For
                        End If
                    End With
                    LastDir = LastDir Mod 4 + 1
                Next Ind
                Iteration = Iteration - 1
            Loop While (Iteration >= 0)
            PathStr = PathStr & MoveCount.ToString("00") & "Z"
        End If
        Return PathStr
    End Function

    Public Sub UpdateExploration(ByVal RobotCoord As Point)
        Dim X As Integer, Y As Integer, TempCoord As Point
        For Y = -1 To 1
            For X = -1 To 1
                TempCoord = New Point(RobotCoord.X + X, RobotCoord.Y + Y)
                If (CheckBoundary(TempCoord) = True) Then
                    LayoutMap(TempCoord.X, TempCoord.Y) = LayoutMap(TempCoord.X, TempCoord.Y) Or CELL_FLAGS.EXPLORED
                End If
            Next X
        Next Y
    End Sub

    Public Sub UpdateObstacles(ByVal RobotCoord As Point, ByVal RobotDir As Direction, SensorDetails As String)
        Dim Ind As Integer
        With RobotCoord
            For X = -1 To 1
                For Y = -1 To 1
                    If (CheckBoundary(.X + X, .Y + Y) = True) Then
                        LayoutMap(.X + X, .Y + Y) = LayoutMap(.X + X, .Y + Y) Or CELL_FLAGS.HARD_EXPLORED
                    End If
                Next Y
            Next X
            For Ind = 1 To SensorDetails.Length
                If (Val(Strings.Mid(SensorDetails, Ind, 1)) > 0) Then PlaceObstacle(RobotCoord, RobotDir, Ind - 1)
                If (CheckBoundary(.X + Obstacle(RobotDir)(Ind - 1).X, .Y + Obstacle(RobotDir)(Ind - 1).Y) = True) Then
                    LayoutMap(.X + Obstacle(RobotDir)(Ind - 1).X, .Y + Obstacle(RobotDir)(Ind - 1).Y) = LayoutMap(.X + Obstacle(RobotDir)(Ind - 1).X, .Y + Obstacle(RobotDir)(Ind - 1).Y) Or CELL_FLAGS.HARD_EXPLORED
                End If
            Next Ind
        End With
    End Sub
End Class


Public Class MessageProcessor
    Private QueueData As String = ""

    Public Sub New()
        ResetQueue()
    End Sub

    Public Sub WriteQueue(ByVal Message As String)
        QueueData = QueueData & Message
    End Sub

    Public Sub ResetQueue()
        QueueData = ""
    End Sub

    Public Function ReadCommand(DelimitChar As String) As String
        Dim Result() As String = Split(QueueData, DelimitChar, 2, CompareMethod.Binary)
        If (Result.Count = 2) Then
            QueueData = Result(1)
            Return Result(0)
        End If
        Return ""
    End Function
End Class