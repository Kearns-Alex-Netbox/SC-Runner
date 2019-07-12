Imports System.IO
Imports System.Threading

Public Class Grid2x3
	Public Const MAX_THREADS As Integer = 6

	Public ServerList(MAX_THREADS - 1) As String
	Public ServerExitCommand()		   As Integer = {0, 0, 0, 0, 0, 0}

	Public IPArray()         As TextBox
	Public LineArray()       As TextBox
	Public ErrorsArray()     As TextBox
	Public LoopsArray()      As TextBox
	Public LastTimeArray()   As TextBox
	Public StateArray()      As TextBox
	Public TestArray()       As TextBox
	Public CloseArray()      As  Button
	Public DocumentArray()   As  Button
	Public IndicatorArray()  As TextBox
	Public IndicatorTArray() As TextBox
	Public ModelArray()		 As   Label
	
	Public ServerThreads(MAX_THREADS - 1) As ThreadComm

	Dim ThreadList As New List(Of Thread)
	Dim LastThreadNumber As Integer = 0
	Dim today As String  = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")

	Private Sub Control_Load() Handles MyBase.Load
		Timer1_Tick()

		IPArray         = New TextBox() {IP1        , IP2        , IP3        , IP4        , IP5        , IP6}
		LineArray       = New TextBox() {Line1      , Line2      , Line3      , Line4      , Line5      , Line6}
		ErrorsArray     = New TextBox() {Errors1    , Errors2    , Errors3    , Errors4    , Errors5    , Errors6}
		LoopsArray      = New TextBox() {Loops1     , Loops2     , Loops3     , Loops4     , Loops5     , Loops6}
		LastTimeArray   = New TextBox() {LastTime1  , LastTime2  , LastTime3  , LastTime4  , LastTime5  , LastTime6}
		StateArray      = New TextBox() {State1     , State2     , State3     , State4     , State5     , State6}
		CloseArray      = New  Button() {Close1     , Close2     , Close3     , Close4     , Close5     , Close6}
		TestArray       = New TextBox() {Test1      , Test2      , Test3      , Test4      , Test5      , Test6}
		DocumentArray   = New  Button() {Document1  , Document2  , Document3  , Document4  , Document5  , Document6}
		IndicatorArray  = New TextBox() {Indicator1 , Indicator2 , Indicator3 , Indicator4 , Indicator5 , Indicator6}
		IndicatorTArray = New TextBox() {Indicator1t, Indicator2t, Indicator3t, Indicator4t, Indicator5t, Indicator6t}
		ModelArray		= New   Label() {Model1		, Model2	 , Model3	  , Model4	   , Model5		, Model6}

		'Initialize the server array
		For i = 0 To MAX_THREADS - 1
			ServerThreads(i) = New ThreadComm
			ServerThreads(i).ExitCommand = 0
			ServerThreads(i).ServerName = ""
			ServerThreads(i).ThreadNumber = -1
			ServerThreads(i).TimeStamp = ""
			CloseArray(i).Enabled = False
			AddHandler CloseArray(i).Click, AddressOf Close_Click
			DocumentArray(i).Enabled = False
			AddHandler DocumentArray(i).Click, AddressOf Document_Click
			IndicatorArray(i).BackColor = Color.Gray
			IndicatorTArray(i).BackColor = Color.Gray
			ModelArray(i).Text = ""
		Next

		VersionNo.Text = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion

		KeyPreview = True
	End Sub

	Private Sub Control_Closing() Handles MyBase.FormClosing
		' close all running threads
		For i = 0 To MAX_THREADS - 1
			If ServerThreads(i).ThreadNumber <> -1 And
				ServerThreads(i).ServerName.ToString.Contains("closed") = False Then
				CloseServer(i)
			Else
				' if the server is already closed then set exit to 0 to make sure for next check
				ServerThreads(i).ExitCommand = 0
			End If
		Next

		' wait until all of the threads verify they have closed their files
		Do 
			Dim allDone = true

			For i = 0 To MAX_THREADS - 1
				If ServerThreads(i).ExitCommand <> 0 Then
					allDone = False
				End If
			Next

			If allDone = True Then
				Exit Do
			End If
		Loop
		
	End Sub

	Private Sub Close_Click(ByVal sender As Object, ByVal e As EventArgs)
		Dim number As Integer = (sender.Name.Substring("close".Length)) - 1 ' subtract 1 because this is a 0 base array

		CloseServer(number)
	End Sub

	Private Sub SetupButton_Click() Handles SetupButton.Click
		Dim DoSetupForm As New Setup
		DoSetupForm.ShowDialog()
	End Sub

	Private Sub ExitButton_Click() Handles ExitButton.Click	
		Close()
	End Sub

	Private Sub Timer1_Tick() Handles Timer1.Tick
		Dim theDate As DateTime = Date.Now
		CurrentTime.Text = theDate.ToString("HH:mm:ss")
	End Sub

	Private Sub CloseServer(ByVal ServerIndex As Integer)
		ServerThreads(ServerIndex).ExitCommand = 1
		CloseArray(ServerIndex).Enabled = False
		DocumentArray(ServerIndex).Enabled = False

		ServerThreads(ServerIndex).ServerName = "closed"
		ServerThreads(ServerIndex).ThreadNumber = -1
		IPArray(ServerIndex).Clear()
		ServerList(ServerIndex) = ""
		LineArray(ServerIndex).Clear()
		ErrorsArray(ServerIndex).Clear()
		LoopsArray(ServerIndex).Clear()
		LastTimeArray(ServerIndex).Clear()
		StateArray(ServerIndex).Clear()
		TestArray(ServerIndex).Clear()
		IndicatorArray(ServerIndex).BackColor = Color.Gray
		IndicatorTArray(ServerIndex).BackColor = Color.Gray
		ModelArray(ServerIndex).Text = ""
	End Sub

	Private Sub CloseAllServers()
		For index As Integer = 0 To MAX_THREADS - 1
			CloseServer(index)
		Next
	End Sub

	Public Sub CreateThread(ByRef ServerName As String, byRef isFixedIP As Boolean)
		Dim ThreadNumber As Integer = LastThreadNumber

		If LastThreadNumber >= MAX_THREADS Then
			' start from the begining and find any open slots
			For i As Integer = 0 To MAX_THREADS - 1
				If ServerThreads(i).ThreadNumber = -1 Then
					ThreadNumber = i
					Exit For
				End If
			Next
		End If

		If ThreadNumber >= MAX_THREADS Then
			MsgBox("Could not find empty slot for new Server")
			Return
		End If

		If ServerName = My.Settings.BaseIPString Then
			' we use a dumby function. We will not be starting these threads
			If ThreadNumber < LastThreadNumber Then
				ThreadList(ThreadNumber) = New Thread(New ThreadStart(AddressOf Timer1_Tick))
			Else
				ThreadList.Add(New Thread(New ThreadStart(AddressOf Timer1_Tick)))
			End If

			If LastThreadNumber >= MAX_THREADS Then
				LastThreadNumber = MAX_THREADS
			Else
				LastThreadNumber += 1
			End If

			Return
		End If

		ServerThreads(ThreadNumber).TimeStamp = today
		ServerThreads(ThreadNumber).ThreadNumber = ThreadNumber
		ServerThreads(ThreadNumber).ExitCommand = 0
		ServerThreads(ThreadNumber).ServerName = ServerName
		ServerThreads(ThreadNumber).IPStable = 0
		Dim tws As New RuntimeEngine(ServerThreads(ThreadNumber), LineArray(ThreadNumber), ErrorsArray(ThreadNumber), LoopsArray(ThreadNumber), 
									 LastTimeArray(ThreadNumber), StateArray(ThreadNumber),ServerExitCommand(ThreadNumber), TestArray(ThreadNumber), 
									 IndicatorArray(ThreadNumber), IndicatorTArray(ThreadNumber), ModelArray(ThreadNumber), IPArray(ThreadNumber),
									 isFixedIP)

		'If we are re-using an existing thread slot, don't add it again
		If ThreadNumber < LastThreadNumber Then
			ThreadList(ThreadNumber) = New Thread(New ThreadStart(AddressOf tws.ProcessThread))
		Else
			'New thread slot, so add it
			ThreadList.Add(New Thread(New ThreadStart(AddressOf tws.ProcessThread)))
		End If

		ThreadList(ThreadNumber).Start()
		ThreadList(ThreadNumber).Name = ServerName
		ThreadList(ThreadNumber).IsBackground = True
		LineArray(ThreadNumber).Text = "Starting"
		CloseArray(ThreadNumber).Enabled = True
		ServerList(ThreadNumber) = ServerName
		DocumentArray(ThreadNumber).Enabled = True
		IndicatorArray(ThreadNumber).BackColor = Color.LightGreen
		IndicatorTArray(ThreadNumber).BackColor = Color.LightGreen

		'While ServerThreads(ThreadNumber).IPStable = 0
		'	'	Thread.Sleep(1000)
		'End While

		If LastThreadNumber >= MAX_THREADS Then
			LastThreadNumber = MAX_THREADS
		Else
			LastThreadNumber += 1
		End If
		
	End Sub

	Private Sub B_NewWindow_Click() Handles B_NewWindow.Click
		Dim DoSelectGrid As New SelectGrid
		DoSelectGrid.Show()
	End Sub

	Private Sub Document_Click(sender As Object, e As EventArgs)
		Dim number As Integer = (sender.Name.Substring("document".Length)) - 1 ' subtract 1 because this is a 0 base array

		OpenLog(number)
	End Sub

	Private Sub OpenLog(ByRef Number As Integer)
		' check to see if this is an active thread
		If ServerThreads(Number).ServerName.Length = 0 Or ServerThreads(Number).ServerName.Contains("closed") = True Then
			Return
		End If

		Dim ip As String = ServerThreads(Number).ServerName
		Dim timestamp As String = ServerThreads(Number).TimeStamp

		Process.Start(My.Settings.SaveResultsLocation & "\" & timestamp & "_Runner\" & ip & ".log")
	End Sub

	Private Sub OpenClientLog(ByRef Number As Integer)
		' check to see if this is an active thread
		If ServerThreads(Number).ServerName.Length = 0 Or ServerThreads(Number).ServerName.Contains("closed") = True Then
			Return
		End If

		Dim ip As String = ServerThreads(Number).ServerName
		Dim timestamp As String = ServerThreads(Number).TimeStamp

		Process.Start(My.Settings.SaveResultsLocation & "\" & timestamp & "_Runner\" & ip & "-Client.txt")
	End Sub

	Private Sub DeleteFiles()
		Dim timestamp As String = ServerThreads(0).TimeStamp
		Dim path	  As String = My.Settings.SaveResultsLocation & "\" & timestamp & "_Runner"

		If Directory.Exists(path) = false Then
			Return
		End If

		For Each deleteFile In Directory.GetFiles(path,"*.*", SearchOption.TopDirectoryOnly)
			File.Delete(deleteFile)
		Next
	End Sub

	Private Sub AddMulti_Button_Click() Handles AddMulti_Button.Click
		If My.Settings.CommandDirectory.Length = 0 Then
			MsgBox("Please put in the command file path in the setup menu.")
			Return
		End If

		If Directory.Exists(My.Settings.CommandDirectory) = False Then
			MsgBox("File path " & My.Settings.CommandDirectory & " does not exist.")
			Return
		End If

		Dim DoAddServerForm As New AddServers(MAX_THREADS)

		If DoAddServerForm.ShowDialog() <> DialogResult.OK Then
			Return
		End If

		' get the date
		today = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")

		' loop thorugh each IP we return
		For Each listItem As String In DoAddServerForm.IP_ListBox.Items

			Dim isAlreadyAdded As Boolean = False

			For i As Integer = 0 To MAX_THREADS - 1
				If ServerList(i) = listItem Then
					MsgBox("Server: " + listItem + " already added")
					isAlreadyAdded = True
				End If
			Next

			If isAlreadyAdded = True Then
				Continue For
			End If
			
			CreateThread(listItem, DoAddServerForm.FixedIP_RadioButton.Checked)
		Next

	End Sub

	Private Sub MyBase_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
		' ----------------
		' Double Key Combo
		' ----------------
		If (e.Control AndAlso e.KeyCode = Keys.N)
			Call B_NewWindow_Click()

		ElseIf (e.Control AndAlso e.KeyCode = Keys.X) Then
			Call CloseAllServers()

		ElseIf (e.Alt AndAlso e.KeyCode = Keys.D1) Or (e.Alt AndAlso e.KeyCode = Keys.NumPad1) Then
			Call OpenClientLog(0)

		ElseIf (e.Alt AndAlso e.KeyCode = Keys.D2) Or (e.Alt AndAlso e.KeyCode = Keys.NumPad2) Then
			Call OpenClientLog(1)

		ElseIf (e.Alt AndAlso e.KeyCode = Keys.D3) Or (e.Alt AndAlso e.KeyCode = Keys.NumPad3) Then
			Call OpenClientLog(2)

		ElseIf (e.Alt AndAlso e.KeyCode = Keys.D4) Or (e.Alt AndAlso e.KeyCode = Keys.NumPad4) Then
			Call OpenClientLog(3)

		ElseIf (e.Alt AndAlso e.KeyCode = Keys.D5) Or (e.Alt AndAlso e.KeyCode = Keys.NumPad5) Then
			Call OpenClientLog(4)

		ElseIf (e.Alt AndAlso e.KeyCode = Keys.D6) Or (e.Alt AndAlso e.KeyCode = Keys.NumPad6) Then
			Call OpenClientLog(5)

		' ----------------
		' Single Key Combo
		' ----------------
		ElseIf (e.KeyCode = Keys.D1) Or (e.KeyCode = Keys.NumPad1) Then
			Call OpenLog(0)

		ElseIf (e.KeyCode = Keys.D2) Or (e.KeyCode = Keys.NumPad2) Then
			Call OpenLog(1)

		ElseIf (e.KeyCode = Keys.D3) Or (e.KeyCode = Keys.NumPad3) Then
			Call OpenLog(2)

		ElseIf (e.KeyCode = Keys.D4) Or (e.KeyCode = Keys.NumPad4) Then
			Call OpenLog(3)

		ElseIf (e.KeyCode = Keys.D5) Or (e.KeyCode = Keys.NumPad5)Then
			Call OpenLog(4)

		ElseIf (e.KeyCode = Keys.D6) Or (e.KeyCode = Keys.NumPad6) Then
			Call OpenLog(5)

		ElseIf (e.KeyCode = Keys.F12) then
			Call DeleteFiles()

		End If
	End Sub

End Class