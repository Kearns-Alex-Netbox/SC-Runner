
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Module MBDeclarations
#Const DebugLoad = False

    REM This should be changed for production to point to just the name of the DLL and not a full path
    Const DLLName As String = "ModbusAPI.dll"

    rem These are the list of prototypes exposed from the ModbusAPI DLL

    <DllImport(DLLName, EntryPoint:="MB_AddLogMessage", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
    Public Function MB_AddLogMessage(ByVal mbHandle As UInteger, ByVal message as String) as UInteger
    End Function

    <DllImport(DLLName, EntryPoint:="MB_RTUClientOpen", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
    Public Function MB_RTUClientOpen(ByVal slaveid As Integer, ByVal COMPort As Integer, ByVal baudRate As Integer, ByVal dataBits As Integer, _
        ByVal parity As Byte, ByVal stopbits As Integer, ByVal swapFlags As Integer, ByVal wantLogging As Integer, ByVal logFileName As String) As UInteger
    End Function

    <DllImport(DLLName, EntryPoint:="MB_TCPClientOpen", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
    Public Function MB_TCPClientOpen(ByVal MODBUSAddress As String, ByVal MODBUSPort As Integer, ByVal swapFlags As Integer, ByVal wantLogging As Integer, _
        ByVal logFileName As String) As UInteger
    End Function

    <DllImport(DLLName, EntryPoint:="MB_ClientClose", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Sub MB_ClientClose(ByVal mbHandle As UInteger)
    End Sub

    <DllImport(DLLName, EntryPoint:="MB_RTUToggleDTR", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_RTUToggleDTR(ByVal mbHandle As UInteger, ByVal isDTRSet As Integer) As UShort
    End Function
    <DllImport(DLLName, EntryPoint:="MB_RTUToggleRTS", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_RTUToggleRTS(ByVal mbHandle As UInteger, ByVal isRTSSet As Integer) As UShort
    End Function
    <DllImport(DLLName, EntryPoint:="MB_ReadCoil", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadCoil(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UShort) As UShort
    End Function

    <DllImport(DLLName, EntryPoint:="MB_ReadRegisterFloat", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadRegisterFloat(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As Single) As UShort
    End Function
    <DllImport(DLLName, EntryPoint:="MB_ReadMultipleRegistersFloat", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadMultipleRegistersFloat(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As Single, ByVal numDataVal As Integer) As UShort
    End Function

    <DllImport(DLLName, EntryPoint:="MB_ReadRegisterWord", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadRegisterWord(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UShort) As UShort
    End Function
    <DllImport(DLLName, EntryPoint:="MB_ReadMultipleRegistersWord", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadMultipleRegistersWord(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UShort, ByVal numDataVal As Integer) As UShort
    End Function

    <DllImport(DLLName, EntryPoint:="MB_ReadRegisterDword", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadRegisterDword(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UInteger) As UShort
    End Function
    <DllImport(DLLName, EntryPoint:="MB_ReadMultipleRegistersDword", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_ReadMultipleRegistersDword(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UInteger, ByVal numDataVal As Integer) As UShort
    End Function

    <DllImport(DLLName, EntryPoint:="MB_WriteRegisterWord", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_WriteRegisterWord(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByVal DataVal As UShort) As Integer
    End Function

    <DllImport(DLLName, EntryPoint:="MB_WriteRegisterDword", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_WriteRegisterDword(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByVal DataVal As UInteger) As Integer
    End Function

    <DllImport(DLLName, EntryPoint:="MB_WriteRegisterFloat", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_WriteRegisterFloat(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByVal DataVal As Single) As Integer
    End Function
    <DllImport(DLLName, EntryPoint:="MB_WriteMultipleRegistersWord", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_WriteMultipleRegistersWord(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UShort, ByVal DataLen as Integer) As Integer
    End Function

    <DllImport(DLLName, EntryPoint:="MB_WriteMultipleRegistersDword", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_WriteMultipleRegistersDword(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As UInteger, ByVal DataLen as Integer) As Integer
    End Function

    <DllImport(DLLName, EntryPoint:="MB_WriteMultipleRegistersFloat", CharSet:=CharSet.Ansi, CallingConvention:=CallingConvention.Cdecl)> _
        Public Function MB_WriteMultipleRegistersFloat(ByVal mbHandle As UInteger, ByVal StartAddr As Integer, ByRef DataVal As Single, ByVal DataLen as Integer) As Integer
    End Function
End Module

rem This is the main class that is used by all VB consumers to talk to the Modbus/TCP Master DLL
Class ModbusXfce

    Public Const ModbusESuccess As UShort = 0
    Public Const MODBUSE_IllegalFunction As UShort = &H1
    Public Const MODBUSE_IllegalDataAddressn As UShort = &H2
    Public Const MODBUSE_IllegalDataValue As UShort = &H3
    Public Const MODBUSE_SlaveDeviceFailure As UShort = &H4
    Public Const MODBUSE_Acknowledge As UShort = &H5
    Public Const MODBUSE_SlaveDeviceBusy As UShort = &H6
    Public Const MODBUSE_MemoryParityError As UShort = &H8
    Public Const MODBUSE_GatewayPathUnavailable As UShort = &HA
    Public Const MODBUSE_GatewayDeviceNoResponse As UShort = &HB

    Public Const MODBUSE_TCPConnectionDropped As UShort = &H80
    Public Const MODBUSE_TCPConnectionNotSent As UShort = &H81

    Public Const MODBUSE_ResponseTimeout As UShort = &H103
    Public Const MODBUSE_RequestTimeout As UShort = &H104
	Public Const MODBUSE_WriteResponseNoMatch As UShort = &H105


    Public Const ParityEven As Byte = 101  ' 'e'
    Public Const ParityOdd As Byte = 111   ' 'o'
    Public Const ParityNone As Byte = 110  ' 'n'

    Public Const SwapByteFlag As Integer = 1
    Public Const SwapWordFlag As Integer = 2
    Private m_mbHandle As UInteger
    Private m_mbRTUSlaveId As Integer
    Private m_mbRTUBaud As Integer
    Private m_mbRTUDataBits As Integer
    Private m_mbRTUParity As Byte
    Private m_mbRTUStopBits As Integer
    Private m_mbRTUComPort As Integer
    Private m_mbTCPServer As String
    Private m_mbTCPPort As Integer
    Private m_mbWantLogging As Integer
    Private m_mbLogName As String
    Private m_mbSwapFlags As Integer = 0

    Public Function GetRandom(ByVal Min As Integer, ByVal Max As Integer) As Integer
        ' by making Generator static, we preserve the same instance '
        ' (i.e., do not create new instances with the same seed over and over) '
        ' between calls '
        Static Generator As System.Random = New System.Random()
        Return Generator.Next(Min, Max)
    End Function
    Public Sub EnableMessageLogging()
        m_mbWantLogging = 2
    End Sub
    Public Sub EnableLogging(ByVal LogfileName as String)
        m_mbLogName = LogfileName
        m_mbWantLogging = 1
    End Sub

    Public Sub DisableLogging()
        m_mbWantLogging = 0
    End Sub
    Public Sub SetSwapFlag(ByVal Flags As Integer)
        m_mbSwapFlags = Flags
    End Sub

    Public Function OpenTCP(ByVal ServerName As String, ByVal ServerPort As Integer) As Integer
        m_mbTCPPort = ServerPort

        m_mbHandle = MB_TCPClientOpen(ServerName, m_mbTCPPort, m_mbSwapFlags, m_mbWantLogging, m_mbLogName)
        If (m_mbHandle <> 0) Then
            OpenTCP = 1
        Else
            OpenTCP = 0
        End If
    End Function

    Public Function OpenRTU(ByVal SlaveId As Integer, ByVal ComPort As Integer, ByVal BaudRate As Integer, ByVal DataBits As Integer, ByVal Parity As Byte, ByVal StopBits As Integer) As Integer
#If DebugLoad = True Then
        OpenRTU = 1
#Else
        m_mbRTUComPort = ComPort
        m_mbRTUBaud = BaudRate
        m_mbRTUDataBits = DataBits
        m_mbRTUParity = Parity
        m_mbRTUStopBits = StopBits

        m_mbHandle = MB_RTUClientOpen(SlaveId, ComPort, BaudRate, Parity, DataBits, StopBits, m_mbSwapFlags, m_mbWantLogging, m_mbLogName)
        If (m_mbHandle <> 0) Then
            OpenRTU = 1
        Else
            OpenRTU = 0
        End If
#End If
    End Function

    Public Sub CloseHandle()
        If m_mbHandle <> 0 Then
            MB_ClientClose(m_mbHandle)
            m_mbHandle = 0
        End If
    End Sub

    Public Sub LogMessage(ByVal txt as String)
        If m_mbHandle <> 0 Then
            MB_AddLogMessage(m_mbHandle, txt)
        End If
    End Sub

    Public Function IsConnected() As Boolean
        IsConnected = (m_mbHandle <> 0)
    End Function
    Public Function ControlRTU_DTR(ByRef isDTRSet As Integer) As Integer
        If m_mbHandle <> 0 Then
            ControlRTU_DTR = MB_RTUToggleDTR(m_mbHandle, isDTRSet)
        End If

    End Function
    Public Function ControlRTU_RTS(ByRef isRTSSet As Integer) As Integer
        If m_mbHandle <> 0 Then
            ControlRTU_RTS = MB_RTUToggleRTS(m_mbHandle, isRTSSet)
        End If
    End Function

    Public Function GetCoil(ByVal Address As Integer, ByRef Value As UShort) As Integer
        If m_mbHandle <> 0 Then
            GetCoil = MB_ReadCoil(m_mbHandle, Address, Value)
        Else
            GetCoil = 0
        End If
    End Function

    Public Function GetWord(ByVal Address As Integer, ByRef Value As UShort) As Integer
#If DebugLoad = True Then
        GetWord = 0
        Value = GetRandom(10, 100)
        Thread.Sleep(20)
#Else
        If m_mbHandle <> 0 Then
            GetWord = MB_ReadRegisterWord(m_mbHandle, Address, Value)
        else
            GetWord = 0
        End If
#End If
    End Function
    Public Function GetMultipleWords(ByVal Address As Integer, ByRef Value As UShort, ByVal NumValue As Integer) As Integer
        If m_mbHandle <> 0 Then
            GetMultipleWords = MB_ReadMultipleRegistersWord(m_mbHandle, Address, Value, NumValue)
        End If
    End Function

    Public Function GetDword(ByVal Address As Integer, ByRef Value As Integer) As Integer
#If DebugLoad = True Then
        GetDword = 0
        Value = GetRandom(10, 100)
        Thread.Sleep(20)
#Else
        If m_mbHandle <> 0 Then
            GetDword = MB_ReadRegisterDword(m_mbHandle, Address, Value)
        else
            GetDword = 0
        End If
#End If
    End Function
    Public Function GetMultipleDwords(ByVal Address As Integer, ByRef Value As Integer, ByVal NumValue As Integer) As Integer
        If m_mbHandle <> 0 Then
            GetMultipleDwords = MB_ReadMultipleRegistersDword(m_mbHandle, Address, Value, NumValue)
        End If
    End Function

    Public Function GetFloat(ByVal Address As Integer, ByRef Value As Single) As Integer
#If DebugLoad = True Then
        GetFloat = 0
        Value = GetRandom(10, 100) * 1.5

        Thread.Sleep(20)
#Else
        If m_mbHandle <> 0 Then
            GetFloat = MB_ReadRegisterFloat(m_mbHandle, Address, Value)
        Else
            GetFloat = 0
        End If
#End If

    End Function
    Public Function GetMultipleFloats(ByVal Address As Integer, ByRef Value As Single, ByVal NumValue As Integer) As Integer
        If m_mbHandle <> 0 Then
            GetMultipleFloats = MB_ReadMultipleRegistersFloat(m_mbHandle, Address, Value, NumValue)
        End If
    End Function

    Public Function SetWord(ByVal Address As Integer, ByVal Value As UShort) As Integer
        If m_mbHandle <> 0 Then
            SetWord = MB_WriteRegisterWord(m_mbHandle, Address, Value)
        Else
            SetWord = False
        End If
    End Function

    Public Function SetDword(ByVal Address As Integer, ByVal Value As UInteger) As Integer
        If m_mbHandle <> 0 Then
            SetDword = MB_WriteRegisterDword(m_mbHandle, Address, Value)
        Else
            SetDword = False
        End If
    End Function


    Public Function SetSingle(ByVal Address As Integer, ByVal Value As Single) As Integer
        If m_mbHandle <> 0 Then
            SetSingle = MB_WriteRegisterFloat(m_mbHandle, Address, Value)
        Else
            SetSingle = False
        End If
    End Function

End Class
