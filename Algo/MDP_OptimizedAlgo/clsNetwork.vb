Option Explicit On
Option Strict On

Imports System.IO
Imports System.IO.Compression
Imports System.Threading
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Runtime.Remoting.Messaging
Imports Microsoft.Win32.SafeHandles

<System.ComponentModel.DesignerCategory("")>
Public Class PrecisionTimers
    Private Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Short
    Private Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Short
    Private sysTimerFrequency As Long = -1

    Private Class TimeKeeper
        Inherits System.Windows.Forms.Form

        Public Event Tick(ByVal ElaspsedSincePrevious As Double)

        Private TimeKeeperThread As Threading.Thread = Nothing
        Private timerFrequency As Long = 0, Terminate As Boolean, TimerPause As Boolean = True, TargetTick As Long, StartTick As Long, ShotTick As Long
        Private PersistShotInterval As Double = 1000, ShotInterval As Double = 1000, DelayInterval As Double = 1, CycleMod As Integer, CycleInd As Integer

        Private Const IdleInterval As Double = 200

        Public ReadOnly Property TimerFreq As Long
            Get
                Return timerFrequency
            End Get
        End Property

        Public ReadOnly Property Running As Boolean
            Get
                Return Not TimerPause
            End Get
        End Property

        Public Property Interval As Double
            Get
                Return PersistShotInterval
            End Get
            Set(ByVal value As Double)
                PersistShotInterval = value
                If (TimerPause = True) Then
                    SetInternalInterval(IdleInterval)
                Else
                    SetInternalInterval(PersistShotInterval)
                End If
            End Set
        End Property

        Private Sub SetInternalInterval(ByVal SetInterval As Double)
            ShotInterval = SetInterval
            If (ShotInterval < 0) Then ShotInterval = 0
            DelayInterval = ShotInterval / 5
            If (DelayInterval < 1) Then
                CycleMod = CInt(1 / DelayInterval)
                DelayInterval = 1
            Else
                CycleMod = 1
            End If
            ShotTick = mSecTickDiff(ShotInterval)
            StartTick = TimeValue()
            TargetTick = StartTick + ShotTick
        End Sub

        Public Sub New()
            Me.ShowInTaskbar = False
            Me.Visible = False
            Dim HandlePtr As IntPtr = Me.Handle
            QueryPerformanceFrequency(timerFrequency)
            Me.Interval = IdleInterval
            TimerPause = True
            TimeKeeperThread = New Threading.Thread(AddressOf TimeKeeperLoop)
            TimeKeeperThread.Priority = ThreadPriority.Highest
            TimeKeeperThread.IsBackground = True
            TimeKeeperThread.Start()
        End Sub

        Private Sub TimeKeeperLoop()
            Dim NowTick As Long, DiffTick As Long
            Do While Terminate = False
                CycleInd = CycleInd Mod CycleMod + 1
                If (CycleInd = 1) Then Threading.Thread.Sleep(CInt(DelayInterval))
                NowTick = TimeValue()
                If (NowTick > TargetTick) Then
                    DiffTick = NowTick - TargetTick
                    TargetTick = TargetTick + ShotTick
                    If (Terminate = True) Then Exit Do
                    If (TimerPause = False) Then
                        Try
                            Me.Invoke(Sub()
                                          RaiseEvent Tick(TickDiffmSec(DiffTick + ShotTick))
                                      End Sub)
                        Catch ex As Exception
                        End Try
                    End If
                End If
            Loop
        End Sub

        Public Sub StopTimer()
            SetInternalInterval(IdleInterval)
            TimerPause = True
        End Sub

        Public Sub StartTimer()
            SetInternalInterval(PersistShotInterval)
            TimerPause = False
        End Sub

        Public Function TimeValue() As Long
            Dim tickCount As Long = 0
            QueryPerformanceCounter(tickCount)
            Return tickCount
        End Function

        Private Function TickDiffmSec(ByVal TickDiff As Long) As Double
            Return TickDiff / timerFrequency * 1000
        End Function

        Private Function mSecTickDiff(ByVal mSec As Double) As Long
            Return CLng(mSec * timerFrequency / 1000)
        End Function

        Public Function Elapsed_mSec(ByVal StartStamp As Long, ByVal EndStamp As Long) As Double
            Return (EndStamp - StartStamp) / timerFrequency * 1000
        End Function

        Public Sub Shutdown()
            Terminate = True
            Application.DoEvents()
            TimeKeeperThread.Join(CInt(ShotInterval * 2))
            Me.Dispose()
        End Sub
    End Class

    Public Class PrecisionTimer
        Private timerFrequency As Long = 0

        Public Sub New()
            QueryPerformanceFrequency(timerFrequency)
        End Sub

        Public ReadOnly Property TickValue() As Long
            Get
                Dim tickCount As Long = 0
                QueryPerformanceCounter(tickCount)
                Return tickCount
            End Get
        End Property

        Public Function Elapsed_mSec(ByVal StartStamp As Long, ByVal EndStamp As Long) As Double
            Return (EndStamp - StartStamp) / timerFrequency * 1000
        End Function
    End Class

    Public Class PrecisionEventTimer
        Public Event Tick(ByVal ElaspsedSincePrevious As Double)

        Private WithEvents tmrShot As New TimeKeeper

        Private Sub tmrShot_Tick(ByVal ElaspsedSincePrevious As Double) Handles tmrShot.Tick
            RaiseEvent Tick(ElaspsedSincePrevious)
        End Sub

        Public ReadOnly Property IsRunning As Boolean
            Get
                Return tmrShot.Running
            End Get
        End Property

        Public Property Interval As Double
            Get
                Return tmrShot.Interval
            End Get
            Set(ByVal value As Double)
                tmrShot.Interval = value
            End Set
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return tmrShot.IsDisposed
            End Get
        End Property

        Public Sub StartTimer()
            tmrShot.StartTimer()
        End Sub

        Public Sub StopTimer()
            tmrShot.StopTimer()
        End Sub

        Public Sub Dispose()
            tmrShot.Shutdown()
        End Sub

        Public Function Elapsed_mSec(ByVal StartStamp As Long, ByVal EndStamp As Long) As Double
            Return (EndStamp - StartStamp) / tmrShot.TimerFreq * 1000
        End Function

        Public ReadOnly Property TickValue() As Long
            Get
                Dim tickCount As Long = 0
                QueryPerformanceCounter(tickCount)
                Return tickCount
            End Get
        End Property
    End Class

    Public Class PrecisionStopWatch
        Private timerFrequency As Long = 0, StartStamp As Long = -1, StopStamp As Long = -1, Started As Boolean = False

        Public Sub New()
            QueryPerformanceFrequency(timerFrequency)
            StartStamp = TickValue()
        End Sub

        Private Function TickValue() As Long
            Dim tickCount As Long = 0
            QueryPerformanceCounter(tickCount)
            Return tickCount
        End Function

        Public ReadOnly Property ElapsedmSec() As Double
            Get
                If (Started = True) And (StartStamp <> -1) Then
                    Return (TickValue() - StartStamp) / timerFrequency * 1000
                Else
                    If (StartStamp = -1) Or (StopStamp = -1) Then
                        Return 0
                    Else
                        Return (StopStamp - StartStamp) / timerFrequency * 1000
                    End If
                End If
            End Get
        End Property

        Public Sub StartTimer()
            StartStamp = TickValue()
            Started = True
        End Sub

        Public Sub StopTimer()
            StopStamp = TickValue()
            Started = False
        End Sub
    End Class
End Class

<System.ComponentModel.DesignerCategory("")>
Public Class clsNetwork
    Private Const HIGH_PRIORITY As Boolean = True

    Public Event Connection(ByVal SlaveIndex As Integer, ByVal SocketName As String, ByVal RemoteEP As System.Net.IPEndPoint)
    Public Event Disconnection(ByVal SlaveIndex As Integer, ByVal SocketName As String)
    Public Event Received(ByVal SlaveIndex As Integer, ByVal SocketName As String, ByVal Packets() As Byte, ByVal RemoteEP As System.Net.IPEndPoint)

    Private Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Short
    Private Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Short

    Private uSecTimer As New PrecisionTimers.PrecisionTimer
    Private WithEvents ResendTimer As New PrecisionTimers.PrecisionEventTimer
    Private Terminate As Boolean = False, TargetTick As Long, ShotTick As Long, timerFrequency As Long = 0
    Private Shared Serializer As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
    Private LockCache As New Object

    Private Const MAX_RESEND_ATTEMPTS As Integer = 3
    Private RESEND_INTERVAL As Double = 50
    Private RESEND_COMP_FACTOR As Double = 1.5

    Public ReadOnly Property AverageRoundTrip_mSec As Double
        Get
            Return RESEND_INTERVAL
        End Get
    End Property

    Private Function TimeValue() As Long
        Dim tickCount As Long = 0
        QueryPerformanceCounter(tickCount)
        Return tickCount
    End Function

    Public Sub New()
        sysSockets = New List(Of SOCKET_ELEMENT)()
        QueryPerformanceFrequency(timerFrequency)
        ResendTimer.Interval = 1
        ResendTimer.StartTimer()
    End Sub

    Public Sub Dispose()
        Dim Ind As Integer
        Terminate = True
        ResendTimer.StopTimer()
        ResendTimer.Dispose()
        For Ind = 0 To sysSockets.Count - 1
            RemoveHandler sysSockets(Ind).Connection, AddressOf sysSockets_Connection
            RemoveHandler sysSockets(Ind).Received, AddressOf sysSockets_Received
            sysSockets(Ind).DisposeSocket()
            Application.DoEvents()
            sysSockets(Ind).Dispose()
        Next Ind
    End Sub

    Public Shared Function ConcatByteArray(ByVal Source1 As Byte(), ByVal Source2 As Byte()) As Byte()
        Dim CombinedArray(0 To Source1.Count + Source2.Count - 1) As Byte
        If (Source1.Count > 0) Then Array.Copy(Source1, 0, CombinedArray, 0, Source1.Count)
        If (Source2.Count > 0) Then Array.Copy(Source2, 0, CombinedArray, Source1.Count, Source2.Count)
        Return CombinedArray
    End Function

    Private Sub ResendTimer_Tick() Handles ResendTimer.Tick
        Dim SendBytes() As Byte = New Byte() {}, SocketID As Integer, RemoveKeys As New List(Of String), NowTick As Long
        SyncLock LockCache
            For Ind = 0 To PacketCache.Count - 1
                With PacketCache(PacketCache.Keys(Ind))
                    If (.ResendCount >= MAX_RESEND_ATTEMPTS) Then
                        RemoveKeys.Add(PacketCache.Keys(Ind))
                    Else
                        NowTick = TimeValue()
                        If (uSecTimer.Elapsed_mSec(CLng(.SenderTick), NowTick) >= RESEND_INTERVAL * RESEND_COMP_FACTOR) Then
                            .SenderTick = CULng(NowTick)
                            .ResendCount = CByte(.ResendCount + 1)
                            SendBytes = SerializeCachePacket(PacketCache(PacketCache.Keys(Ind)))
                            SocketID = FindSocketID(.SocketName)
                            If (SocketID <> -1) Then sysSockets(SocketID).Send(SendBytes, .RemoteIP, .RemotePort, .SlaveIndex) Else RemoveKeys.Add(PacketCache.Keys(Ind))
                        End If
                    End If
                End With
            Next Ind
            For Ind = 0 To RemoveKeys.Count - 1
                PacketCache.Remove(RemoveKeys(Ind))
            Next Ind
        End SyncLock
    End Sub

#Region "Socket Routines"
    Public Class SOCKET_ELEMENT
        Inherits Windows.Forms.Form

        Private POLL_INTERVAL As Integer = 100
        Private POLL_DURATION As Integer = 1

        Public Event Connection(ByVal SlaveIndex As Integer, ByVal SocketName As String, ByVal Connected As Boolean, ByVal RemoteEP As System.Net.IPEndPoint)
        Public Event Received(ByVal SlaveIndex As Integer, ByVal SocketName As String, ByVal ReceivedData() As Byte, ByVal RemoteEP As System.Net.IPEndPoint)

        Private Conn_Thread As New Threading.Thread(AddressOf ConnThread)
        Private Recv_Thread As New Threading.Thread(AddressOf RecvThread)

        Private PollTimer As New PrecisionTimers.PrecisionTimer
        Private DataSocket As Socket, SlaveSockets As New List(Of SlaveSocket)
        Private Adaptor_IP As String, Adaptor_Port As Integer, Adaptor_IPEP As IPEndPoint
        Private Remote_IP As String, Remote_Port As Integer, Remote_IPEP As IPEndPoint
        Private Multicast_IP As String, Multicast_Option As MulticastOption
        Private Role As SOCKET_ROLE, AdaptorSocketName As String, Terminate As Boolean = False, LastConnected As Boolean = False
        Private RemainderBytes() As Byte = New Byte() {}
        Private objLockRemain As New Object

        Public Sub IsConnected()
            Select Case Role
                Case SOCKET_ROLE.TCP_SERVER
                    Dim Ind As Integer, SubInd As Integer
                    For Ind = 0 To SlaveSockets.Count - 1
                        Try
                            If ((Not (SlaveSockets(Ind).DataSocket.Poll(POLL_DURATION, SelectMode.SelectRead) AndAlso SlaveSockets(Ind).DataSocket.Available = 0)) = False) Then
                                SubInd = Ind
                                SlaveSockets(Ind).DataSocket.Disconnect(True)
                                RaiseEvent Connection(SubInd, AdaptorSocketName, False, Nothing)
                            End If
                        Catch ex As Exception
                        End Try
                    Next Ind
                Case SOCKET_ROLE.TCP_CLIENT
                    Try
                        If (DataSocket.Connected = True) Then
                            If ((Not (DataSocket.Poll(POLL_DURATION, SelectMode.SelectRead) AndAlso DataSocket.Available = 0)) = False) Then
                                DataSocket.Disconnect(True)
                                RaiseEvent Connection(-1, AdaptorSocketName, False, Nothing)
                            End If
                        End If
                    Catch ex As Exception
                    End Try
            End Select
        End Sub

        Public Sub SendAlive(ByVal SendPack As Byte())
            Dim SubInd As Integer
            For SubInd = 0 To SlaveSockets.Count - 1
                Try
                    If (SlaveSockets(SubInd).LastConnected = True) Then Send(SendPack, , SubInd)
                Catch ex As Exception
                End Try
            Next SubInd
            Try
                If (DataSocket.Connected = True) Then Send(SendPack)
            Catch ex As Exception
            End Try
        End Sub

        Public Enum SOCKET_ROLE As Byte
            TCP_SERVER = 0
            TCP_CLIENT = 1
            UDP_UNICAST = 2
            UDP_MULTICAST = 3
        End Enum

        Private Class SlaveSocket
            Public OriginHost As IPEndPoint = New IPEndPoint(IPAddress.Any, 0)
            Public DataSocket As Socket = Nothing
            Public LastConnected As Boolean = False
            Public IdleRead As Boolean
            Public Buffer() As Byte = Nothing
            Public DelayCnt As Integer

            Public ReadOnly Property RemoteEP As IPEndPoint
                Get
                    If (DataSocket Is Nothing) Or (DataSocket.LocalEndPoint Is Nothing) Then Return Nothing
                    Return CType(DataSocket.RemoteEndPoint, IPEndPoint)
                End Get
            End Property

            Public Sub New()
                DataSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                DataSocket.ReceiveBufferSize = 65535000
                DataSocket.SendBufferSize = 65535000
                DataSocket.NoDelay = True
                If (HIGH_PRIORITY = True) Then DataSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.BsdUrgent Or SocketOptionName.Expedited, True)
            End Sub
        End Class

        Public ReadOnly Property Port() As Integer
            Get
                Try
                    If (DataSocket Is Nothing) Or (DataSocket.LocalEndPoint Is Nothing) Then Return 0
                    Return CType(DataSocket.LocalEndPoint, IPEndPoint).Port
                Catch ex As Exception
                End Try
                Return 0
            End Get
        End Property

        Public ReadOnly Property Address() As String
            Get
                Try
                    If (DataSocket Is Nothing) Or (DataSocket.LocalEndPoint Is Nothing) Then Return "0.0.0.0"
                    Return CType(DataSocket.LocalEndPoint, IPEndPoint).Address.ToString.Trim
                Catch ex As Exception
                End Try
                Return "0.0.0.0"
            End Get
        End Property

        Public ReadOnly Property Connected() As Boolean
            Get
                Try
                    If (DataSocket Is Nothing) Or (DataSocket.LocalEndPoint Is Nothing) Then Return False
                    Connected = DataSocket.Connected
                Catch ex As Exception
                    Connected = False
                End Try
            End Get
        End Property

        Public ReadOnly Property SocketName() As String
            Get
                SocketName = AdaptorSocketName
            End Get
        End Property

        Public ReadOnly Property RemoteEP() As IPEndPoint
            Get
                If (DataSocket Is Nothing) Or (DataSocket.LocalEndPoint Is Nothing) Then Return Nothing
                Return CType(DataSocket.RemoteEndPoint, IPEndPoint)
            End Get
        End Property

        Public Sub New(ByVal SocketName As String, ByVal SocketRole As SOCKET_ROLE, ByVal AdaptorIP As String, Optional ByVal AdaptorPort As Long = 0, Optional ByVal MulticastAddr As String = "224.0.0.1")
            Me.ShowInTaskbar = False
            Me.Visible = False
            Dim HandlePtr As IntPtr = Me.Handle
            Try
                AdaptorSocketName = SocketName
                Role = SocketRole
                Multicast_IP = MulticastAddr
                Adaptor_IPEP = New IPEndPoint(IPAddress.Parse(AdaptorIP), CInt(AdaptorPort))
                Select Case Role
                    Case SOCKET_ROLE.UDP_UNICAST
                        DataSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                    Case SOCKET_ROLE.UDP_MULTICAST
                        DataSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                    Case SOCKET_ROLE.TCP_SERVER
                        DataSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        DataSocket.LingerState = New LingerOption(False, 0)
                    Case SOCKET_ROLE.TCP_CLIENT
                        DataSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        DataSocket.LingerState = New LingerOption(False, 0)
                End Select
                With DataSocket
                    .Blocking = False
                    .DontFragment = True
                    .ExclusiveAddressUse = False
                    .ReceiveBufferSize = 65535000
                    .SendBufferSize = 65535000
                    .SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
                    .Bind(Adaptor_IPEP)
                    Select Case Role
                        Case SOCKET_ROLE.TCP_SERVER
                            .NoDelay = True
                            If (HIGH_PRIORITY = True) Then
                                .SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.BsdUrgent, True)
                                .SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Expedited, True)
                            End If
                            .Listen(100)
                            Conn_Thread.IsBackground = True
                            Conn_Thread.Start()
                        Case SOCKET_ROLE.TCP_CLIENT
                            .NoDelay = True
                            If (HIGH_PRIORITY = True) Then
                                .SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.BsdUrgent, True)
                                .SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Expedited, True)
                            End If
                        Case SOCKET_ROLE.UDP_MULTICAST
                            .EnableBroadcast = True
                            .MulticastLoopback = True
                            Multicast_Option = New MulticastOption(IPAddress.Parse(Multicast_IP))
                            .SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, Multicast_Option)
                        Case SOCKET_ROLE.UDP_UNICAST
                    End Select
                End With
                Recv_Thread.Priority = ThreadPriority.Highest
                Recv_Thread.IsBackground = True
                Recv_Thread.Start()
            Catch ex As Exception
            End Try
        End Sub

        Public Sub Connect(ByVal RemoteIP As String, ByVal RemotePort As Long)
            Try
                If (DataSocket.Connected = False) Then
                    Dim e As New SocketAsyncEventArgs
                    Remote_IPEP = New IPEndPoint(IPAddress.Parse(RemoteIP), CInt(RemotePort))
                    e.RemoteEndPoint = Remote_IPEP
                    If (DataSocket.Connected = False) Then DataSocket.ConnectAsync(e)
                End If
            Catch ex As Exception
            End Try
        End Sub

        Public Sub DisposeSocket()
            Try
                If (Role = SOCKET_ROLE.UDP_MULTICAST) Then DataSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, Multicast_Option)
            Catch ex As Exception
            End Try
            For Ind = SlaveSockets.Count - 1 To 0 Step -1
                Try
                    SlaveSockets(Ind).DataSocket.Close()
                Catch ex As Exception
                End Try
            Next Ind
            Try
                DataSocket.Close()
            Catch ex As Exception
            End Try
            DataSocket.Dispose()
            Terminate = True
            Debug.Print("Socket_Element disposed...")
            Application.DoEvents()
            Me.Dispose()
        End Sub

        Public Sub Send(ByVal Buffer() As Byte, Optional ByVal RemoteIP As String = "", Optional ByVal RemotePort As Long = 0, Optional ByVal SlaveIndex As Integer = -1)
            Dim e As New Sockets.SocketAsyncEventArgs
            Dim BuffSize As Long = Buffer.Count
            Dim EncapBuffer(0 To Buffer.Count - 1) As Byte
            Array.Copy(Buffer, 0, EncapBuffer, 0, Buffer.Count)
            e.SetBuffer(EncapBuffer, 0, EncapBuffer.Count)
            Select Case Role
                Case SOCKET_ROLE.UDP_MULTICAST
                    If (RemoteIP.Count > 0) Or (RemotePort <> 0) Then Remote_IPEP = New IPEndPoint(IPAddress.Parse(RemoteIP), CInt(RemotePort))
                    e.RemoteEndPoint = Remote_IPEP
                    e.UserToken = DataSocket
                    Try
                        DataSocket.SendToAsync(e)
                    Catch ex As Exception
                    End Try
                Case SOCKET_ROLE.TCP_CLIENT
                    If (DataSocket.Connected = True) Then
                        Remote_IPEP = CType(DataSocket.RemoteEndPoint, IPEndPoint)
                        e.RemoteEndPoint = Remote_IPEP
                        e.UserToken = DataSocket
                        Try
                            DataSocket.SendToAsync(e)
                        Catch ex As Exception
                        End Try
                    End If
                Case SOCKET_ROLE.TCP_SERVER
                    If (SlaveIndex > -1) Then
                        If (SlaveSockets(SlaveIndex).DataSocket.Connected = True) Then
                            Remote_IPEP = CType(SlaveSockets(SlaveIndex).DataSocket.RemoteEndPoint, IPEndPoint)
                            e.RemoteEndPoint = Remote_IPEP
                            e.UserToken = SlaveSockets(SlaveIndex).DataSocket
                            Try
                                SlaveSockets(SlaveIndex).DataSocket.SendToAsync(e)
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                Case SOCKET_ROLE.UDP_UNICAST
                    If (RemoteIP.Count > 0) Or (RemotePort <> 0) Then Remote_IPEP = New IPEndPoint(IPAddress.Parse(RemoteIP), CInt(RemotePort))
                    e.RemoteEndPoint = Remote_IPEP
                    e.UserToken = DataSocket
                    Try
                        DataSocket.SendToAsync(e)
                    Catch ex As Exception
                    End Try
            End Select
            e.Dispose()
        End Sub

        Private Sub ConnThread()
            SlaveSockets.Clear()
            DataSocket.Blocking = True
            Do While Terminate = False
                Try
                    SlaveSockets.Add(New SlaveSocket())
                    SlaveSockets(SlaveSockets.Count - 1).DataSocket = DataSocket.Accept()
                    SlaveSockets(SlaveSockets.Count - 1).OriginHost = CType(SlaveSockets(SlaveSockets.Count - 1).DataSocket.RemoteEndPoint, IPEndPoint)
                    Threading.Thread.Sleep(1)
                Catch ex As Exception
                End Try
            Loop
            Dim Ind As Integer
            If (Role = SOCKET_ROLE.TCP_SERVER) Then
                For Ind = 0 To SlaveSockets.Count - 1
                    Try
                        SlaveSockets(Ind).DataSocket.Close()
                    Catch ex As Exception
                    End Try
                Next Ind
            End If
            Try
                DataSocket.Close()
            Catch ex As Exception
            End Try
        End Sub

        Private Sub RecvThread()
            Dim SenderIPEP As IPEndPoint = New IPEndPoint(DataSocket.AddressFamily, 0), castSenderEndPoint As EndPoint = CType(SenderIPEP, EndPoint)
            Dim RecvBuffer() As Byte, RecvCount As Integer, StartConnected As Boolean, SendInd As Integer, NoSleep As Boolean, ConcatBytes() As Byte, LastPoll As Long, NowPoll As Long
            Do While Terminate = False
                Try
                    NowPoll = PollTimer.TickValue
                    If (PollTimer.Elapsed_mSec(LastPoll, NowPoll) > POLL_INTERVAL) Then
                        IsConnected()
                        LastPoll = NowPoll
                    End If
                    NoSleep = False
                    Select Case Role
                        Case SOCKET_ROLE.TCP_SERVER
                            For Ind = SlaveSockets.Count - 1 To 0 Step -1
                                With SlaveSockets(Ind)
                                    StartConnected = .LastConnected
                                    If (.DataSocket.Connected = True) Then
                                        .LastConnected = True
                                        If (.DataSocket.Available > 0) Then
                                            ReDim RecvBuffer(0 To .DataSocket.Available - 1)
                                            RecvCount = .DataSocket.ReceiveFrom(RecvBuffer, castSenderEndPoint)
                                            ReDim Preserve RecvBuffer(0 To RecvCount - 1)
                                            SendInd = Ind
                                            SyncLock objLockRemain
                                                ConcatBytes = ConcatByteArray(RemainderBytes, RecvBuffer)
                                                RemainderBytes = New Byte() {}
                                            End SyncLock
                                            Me.Invoke(Sub()
                                                          RaiseEvent Received(SendInd, AdaptorSocketName, CType(ConcatBytes.Clone, Byte()), CType(.DataSocket.RemoteEndPoint, IPEndPoint))
                                                      End Sub)
                                            NoSleep = True
                                        End If
                                    Else
                                        .LastConnected = False
                                    End If
                                End With
                                If (StartConnected <> SlaveSockets(Ind).LastConnected) Then
                                    SendInd = Ind
                                    If (SlaveSockets(Ind).LastConnected = True) Then
                                        Me.Invoke(Sub()
                                                      RaiseEvent Connection(SendInd, AdaptorSocketName, True, SlaveSockets(SendInd).RemoteEP)
                                                  End Sub)
                                    Else
                                        SlaveSockets(Ind).DataSocket.Close()
                                        SlaveSockets.RemoveAt(Ind)
                                        Me.Invoke(Sub()
                                                      RaiseEvent Connection(SendInd, AdaptorSocketName, False, Nothing)
                                                  End Sub)
                                    End If
                                End If
                            Next Ind
                        Case SOCKET_ROLE.TCP_CLIENT
                            StartConnected = LastConnected
                            If (DataSocket.Connected = True) Then
                                LastConnected = True
                                If (DataSocket.Available > 0) Then
                                    ReDim RecvBuffer(0 To DataSocket.Available - 1)
                                    RecvCount = DataSocket.ReceiveFrom(RecvBuffer, castSenderEndPoint)
                                    ReDim Preserve RecvBuffer(0 To RecvCount - 1)
                                    SyncLock objLockRemain
                                        ConcatBytes = ConcatByteArray(RemainderBytes, RecvBuffer)
                                        RemainderBytes = New Byte() {}
                                    End SyncLock
                                    Me.Invoke(Sub()
                                                  RaiseEvent Received(-1, AdaptorSocketName, CType(ConcatBytes.Clone, Byte()), CType(DataSocket.RemoteEndPoint, IPEndPoint))
                                              End Sub)
                                    NoSleep = True
                                End If
                            Else
                                LastConnected = False
                            End If
                            If (StartConnected <> LastConnected) Then
                                Me.Invoke(Sub()
                                              If (LastConnected = True) Then
                                                  RaiseEvent Connection(-1, AdaptorSocketName, LastConnected, CType(DataSocket.RemoteEndPoint, IPEndPoint))
                                              Else
                                                  RaiseEvent Connection(-1, AdaptorSocketName, LastConnected, Nothing)
                                              End If
                                          End Sub)
                            End If
                        Case Else
                            If (DataSocket IsNot Nothing) Then
                                If (DataSocket.Available > 0) Then
                                    ReDim RecvBuffer(0 To DataSocket.Available - 1)
                                    RecvCount = DataSocket.ReceiveFrom(RecvBuffer, castSenderEndPoint)
                                    ReDim Preserve RecvBuffer(0 To RecvCount - 1)
                                    SyncLock objLockRemain
                                        ConcatBytes = ConcatByteArray(RemainderBytes, RecvBuffer)
                                        RemainderBytes = New Byte() {}
                                    End SyncLock
                                    Me.Invoke(Sub()
                                                  RaiseEvent Received(-1, AdaptorSocketName, CType(ConcatBytes.Clone, Byte()), CType(castSenderEndPoint, IPEndPoint))
                                              End Sub)
                                    NoSleep = True
                                End If
                            End If
                    End Select
                    If (NoSleep = False) Then Threading.Thread.Sleep(1)
                Catch ex As Exception
                End Try
            Loop
            Try
                DataSocket.Close()
            Catch ex As Exception
            End Try
        End Sub

        Public Sub InputRemainderBytes(ByVal MainBuffer() As Byte, ByVal Remainder As Long)
            SyncLock objLockRemain
                ReDim RemainderBytes(0 To CInt(Remainder - 1))
                Array.Copy(MainBuffer, MainBuffer.Count - Remainder, RemainderBytes, 0, RemainderBytes.Count)
            End SyncLock
        End Sub

        Public Sub ClearRemainderBytes()
            SyncLock objLockRemain
                RemainderBytes = New Byte() {}
            End SyncLock
        End Sub
    End Class

    Public Enum PACKET_TYPE As Byte
        SEND = 0
        RELIABLE = 1
        ACK = 2
        ALIVE = 3
    End Enum

    Public Class RAW_DECAPPACKET
        Public IP As String = "0.0.0.0"
        Public Port As Integer = 0
        Public Buffer() As Byte = New Byte() {}
    End Class

    <Serializable()> Public Class CACHE_PACKET
        Public SenderTick As ULong = 0
        Public SenderModIndex As ULong = 0
        Public ResendCount As Byte = 0
        Public PacketType As PACKET_TYPE
        Public SocketName As String = ""
        Public SlaveIndex As Integer
        Public RemoteIP As String = "0.0.0.0"
        Public RemotePort As Integer = 0
        Public Data() As Byte = New Byte() {}
    End Class

    Private PacketCache As New Dictionary(Of String, CACHE_PACKET)
    Private sysSockets As List(Of SOCKET_ELEMENT)

    Private Function FindSocketID(ByVal SocketName As String) As Integer
        Dim Ind As Integer
        FindSocketID = -1
        For Ind = 0 To sysSockets.Count - 1
            If (sysSockets(Ind).SocketName.Trim.ToUpper = SocketName.Trim.ToUpper) Then
                FindSocketID = Ind
                Exit For
            End If
        Next Ind
    End Function

    Public ReadOnly Property Connected(ByVal SocketName As String) As Boolean
        Get
            Dim SocketID As Integer = FindSocketID(SocketName)
            If (SocketID <> -1) Then Return sysSockets(SocketID).Connected Else Return False
        End Get
    End Property

    Public ReadOnly Property IP(ByVal SocketName As String) As String
        Get
            Dim SocketID As Integer = FindSocketID(SocketName)
            If (SocketID <> -1) Then Return sysSockets(SocketID).Address Else Return "0.0.0.0"
        End Get
    End Property

    Public ReadOnly Property Port(ByVal SocketName As String) As Integer
        Get
            Dim SocketID As Integer = FindSocketID(SocketName)
            If (SocketID <> -1) Then Return sysSockets(SocketID).Port Else Return -1
        End Get
    End Property

    Public Sub AddSocket(ByVal DataSocket As SOCKET_ELEMENT)
        sysSockets.Add(DataSocket)
        AddHandler sysSockets(sysSockets.Count - 1).Connection, AddressOf sysSockets_Connection
        AddHandler sysSockets(sysSockets.Count - 1).Received, AddressOf sysSockets_Received
    End Sub

    Public Sub Connect(ByVal SocketName As String, ByVal RemoteIP As String, ByVal RemotePort As Long)
        Dim SocketID As Integer = FindSocketID(SocketName)
        If (SocketID <> -1) Then sysSockets(SocketID).Connect(RemoteIP, RemotePort)
    End Sub

    Private Sub sysSockets_Connection(ByVal SlaveIndex As Integer, ByVal SocketName As String, ByVal Connected As Boolean, ByVal RemoteEP As System.Net.IPEndPoint)
        If (Connected = True) Then RaiseEvent Connection(SlaveIndex, SocketName, RemoteEP) Else RaiseEvent Disconnection(SlaveIndex, SocketName)
    End Sub

    Private Sub sysSockets_Received(ByVal SlaveIndex As Integer, ByVal SocketName As String, ByVal ReceivedData() As Byte, ByVal RemoteEP As System.Net.IPEndPoint)
        Dim BytesRemainder As Long = 0
        Dim Packets As List(Of RAW_DECAPPACKET) = Decapsulate(ReceivedData, RemoteEP.Address.ToString.Trim, RemoteEP.Port, BytesRemainder)
        Dim RawData() As Byte = New Byte() {}, RecvData() As Byte = New Byte() {}, CachePacket As CACHE_PACKET, SocketID As Integer = FindSocketID(SocketName)
        If (BytesRemainder <> 0) And (SocketID <> -1) Then sysSockets(SocketID).InputRemainderBytes(ReceivedData, BytesRemainder)
        For Ind = 0 To Packets.Count - 1
            CachePacket = DeserializeCachePacket(Packets(Ind).Buffer)
            If (CachePacket IsNot Nothing) Then
                Select Case CachePacket.PacketType
                    Case PACKET_TYPE.ALIVE
                    Case PACKET_TYPE.SEND
                        RaiseEvent Received(SlaveIndex, SocketName, CType(CachePacket.Data.Clone, Byte()), RemoteEP)
                    Case PACKET_TYPE.RELIABLE
                        RecvData = CType(CachePacket.Data.Clone, Byte())
                        CachePacket.Data = New Byte() {}
                        CachePacket.PacketType = PACKET_TYPE.ACK
                        RawData = SerializeCachePacket(CachePacket)
                        SocketID = FindSocketID(SocketName)
                        If (SocketID <> -1) Then sysSockets(SocketID).Send(RawData, RemoteEP.Address.ToString.Trim, RemoteEP.Port, SlaveIndex)
                        RaiseEvent Received(SlaveIndex, SocketName, CType(RecvData.Clone, Byte()), RemoteEP)
                    Case PACKET_TYPE.ACK
                        Dim Key As String = CachePacket.SenderTick.ToString.Trim & "-" & CachePacket.SenderModIndex.ToString.Trim
                        If (PacketCache.ContainsKey(Key) = True) Then
                            SyncLock LockCache
                                RESEND_INTERVAL = (RESEND_INTERVAL + uSecTimer.Elapsed_mSec(CLng(CachePacket.SenderTick), TimeValue)) / 2
                                PacketCache.Remove(Key)
                            End SyncLock
                        End If
                End Select
            End If
        Next Ind
    End Sub

    Public Sub Send(ByVal SocketName As String, ByVal SendData() As Byte, Optional ByVal RemoteIP As String = "", Optional ByVal RemotePort As Long = 0, Optional ByVal SlaveIndex As Integer = -1, Optional ByVal ReliableSend As Boolean = False)
        Dim SocketID As Integer = FindSocketID(SocketName)
        If (SocketID <> -1) Then
            Dim SendPack() As Byte = New Byte() {}
            Dim CachePacket As New CACHE_PACKET
            CachePacket.Data = CType(SendData.Clone, Byte())
            CachePacket.PacketType = PACKET_TYPE.SEND
            If (ReliableSend = True) Then
                CachePacket.SenderTick = CULng(TimeValue())
                CachePacket.SenderModIndex = 0
                CachePacket.PacketType = PACKET_TYPE.RELIABLE
                CachePacket.SocketName = SocketName
                CachePacket.RemoteIP = RemoteIP
                CachePacket.RemotePort = CInt(RemotePort)
                CachePacket.SlaveIndex = SlaveIndex
                SyncLock LockCache
                    PacketCache.Add(CachePacket.SenderTick.ToString.Trim & "-" & CachePacket.SenderModIndex.ToString.Trim, CachePacket)
                End SyncLock
            End If
            SendPack = SerializeCachePacket(CachePacket)
            sysSockets(SocketID).Send(SendPack, RemoteIP, RemotePort, SlaveIndex)
        End If
    End Sub

    Private Function SerializeCachePacket(ByVal CachePacket As CACHE_PACKET) As Byte()
        Dim Data() As Byte = New Byte() {}
        Using fs As New IO.MemoryStream()
            Serializer.Serialize(fs, CachePacket)
            Data = fs.ToArray
            fs.Close()
        End Using
        Return Data
    End Function

    Private Function DeserializeCachePacket(ByVal PacketData() As Byte) As CACHE_PACKET
        Dim CachePacket As CACHE_PACKET = Nothing
        Using fs As New IO.MemoryStream(PacketData)
            Try
                CachePacket = CType(Serializer.Deserialize(fs), CACHE_PACKET)
            Catch ex As Exception
            End Try
            fs.Close()
        End Using
        Return CachePacket
    End Function

    Private Function Decapsulate(ByVal Buffer() As Byte, ByVal SrcIP As String, ByVal SrcPort As Integer, ByRef BytesRemainder As Long) As List(Of RAW_DECAPPACKET)
        Dim Size As Long, Remainder As Long = Buffer.Count, Offset As Long = 0, Packets As New List(Of RAW_DECAPPACKET), TempPacket As RAW_DECAPPACKET
        Do While (Remainder > 8)
            If ((Chr(Buffer(CInt(Offset))) & Chr(Buffer(CInt(Offset + 1))) & Chr(Buffer(CInt(Offset + 2))) & Chr(Buffer(CInt(Offset + 3)))) = "PACK") Then
                Size = (CInt(Buffer(CInt(Offset + 4))) << 24) Or (CInt(Buffer(CInt(Offset + 5))) << 16) Or (CInt(Buffer(CInt(Offset + 6))) << 8) Or CInt(Buffer(CInt(Offset + 7)))
                If (Size <= Remainder) And (Size > 8) Then
                    TempPacket = New RAW_DECAPPACKET
                    With TempPacket
                        .IP = SrcIP
                        .Port = SrcPort
                        .Buffer = New Byte(0 To CInt(Size - 9)) {}
                        Array.Copy(Buffer, Offset + 8, .Buffer, 0, Size - 8)
                    End With
                    Packets.Add(TempPacket)
                Else
                    Exit Do
                End If
                Offset = Offset + Size
                Remainder = Remainder - Size
            End If
        Loop
        BytesRemainder = Remainder
        Return Packets
    End Function
#End Region
End Class