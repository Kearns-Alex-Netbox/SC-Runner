Imports System.Data.SqlClient

Public Class SQL_API
	Private username As String = ""
	Private password As String = ""
	Public conn As SqlConnection
	Dim thisCMD As New SqlCommand("", conn)

	'Board Status'
	Const BS_BURN_IN As String = "Burn In"

	'System Status'
	Const SS_BURN_IN As String = "Burn In"

	'System Dates
	Const REGISTER_DATE  As String = "RegisterDate"
	Const PARAMETER_DATE As String = "ParameterDate"
	Const BURN_IN_DATE   As String = "BurnInDate"

	Dim myReader As SqlDataReader

	''' <summary>
	''' Either sets or returns the string value for username.
	''' </summary>
	''' <value>String value that you want to set username to.</value>
	''' <returns>Returns the string value that username is currently set to.</returns>
	''' <remarks>This is your username.</remarks>
	Public Property _Username() As String
		Get
			Return username.ToString
		End Get
		Set(ByVal value As String)
			username = value
		End Set
	End Property

	''' <summary>
	''' Either sets or returns the string value for password.
	''' </summary>
	''' <value>String value that you want to set password to.</value>
	''' <returns>Returns the string value that password is currently set to.</returns>
	''' <remarks>This is your passwrod.</remarks>
	Public Property _Password() As String
		Get
			Return password.ToString
		End Get
		Set(ByVal value As String)
			password = value
		End Set
	End Property

	Public Function SetDatabase(byref model As string, byRef results As String) As Boolean
		'If conn IsNot Nothing Then
		'	If CloseDatabase(conn, results) = False Then
		'		Return False
		'	End If
		'End If

		Select Case model
			Case RuntimeEngine.MODEL_D7
#If NotSupported
				, RuntimeEngine.MODEL_2300, RuntimeEngine.MODEL_E66, RuntimeEngine.MODEL_ETS
#End If
				database = netboxDatabase(INDEX_DATABASE)

			Case RuntimeEngine.MODEL_REBIG
				database = blueskyDatabase(INDEX_DATABASE)

			Case Else
				Results = "We are not set up for " & model
				Return false

		End Select

		if OpenDatabase(results) = False Then
			results = "Error opening Database: " & results
			Return False
		End If

		Return True
	End Function

	''' <summary>
	''' Opens the connection to the database and saves the user who is logged in.
	''' </summary>
	''' <param name="result">OUTPUT: Error string if somethign does not go as planned.</param>
	''' <returns>True: successful open and return username. False: unsuccessful, see result message for details.</returns>
	''' <remarks>This needs to be called before you make anyother commands.</remarks>
	Public Function OpenDatabase(ByRef result As String) As Boolean

		conn = New SqlConnection("server=tcp:nas1,1622;Database=" & database & ";User ID=" & username & ";password= " & password & ";")
		Try
			conn.Open()
			thisCMD = New SqlCommand("", conn)
			thisCMD = conn.CreateCommand

			'Get the logged in user name from Windows to the database.
			thisCMD.CommandText = "SELECT ORIGINAL_LOGIN()"
			myReader = thisCMD.ExecuteReader()
			If myReader.Read() Then
				'Check to see if we are returned a NULL value.
				If myReader.IsDBNull(0) Then
					result = "Login name returned NULL."
					Return False
				End If
			Else
				'If nothing is returned then it does not exist.
				result = "Login name does not exist."
				Return False
			End If

			myReader.Close()
			Return True
		Catch ex As Exception
			result = ex.Message
			Return False
		End Try
	End Function

	''' <summary>
	''' Closes the database connection that gets passed through.
	''' </summary>
	''' <param name="myConn">The connection that you want to close.</param>
	''' <param name="result">OUTPUT: Error result if somthing does not go right.</param>
	''' <returns>True: Successful close. False: unsuccessful close, see result message for information.</returns>
	''' <remarks>Make sure the connection is already open first before trying to close it.</remarks>
	Public Function CloseDatabase(ByRef myConn As SqlConnection, ByRef result As String) As Boolean
		Try
			If myConn.State <> ConnectionState.Closed Then
				myConn.Close()
			End If
			Return True
		Catch ex As Exception
			result = ex.Message
			Return False
		End Try
	End Function

	''' <summary>
	''' Adds a comment to the system serial number that is passed through.
	''' </summary>
	''' <param name="myCmd">The SQL Command that you will be using to make this action.</param>
	''' <param name="serialNumber">The serial number of the System that you want to add the comment to.</param>
	''' <param name="comment">The comment string that you want to add.</param>
	''' <param name="record">OUTPUT: The ID associated with the System.</param>
	''' <param name="result">OUTPUT: Error result when things do not work.</param>
	''' <returns>True: Comment was added successfully. False: There was a problem, see result message.</returns>
	''' <remarks></remarks>
	Public Function AddSystemComment(ByRef myCmd As SqlCommand, ByRef serialNumber As String, ByRef comment As String, ByRef record As Guid, ByRef result As String) As Boolean
		Try
			'Get the GUID from the passed in serial number.
			myCmd.CommandText = "SELECT systemid FROM dbo.System WHERE SerialNumber = '" & serialNumber & "' and 
[dbo.SystemStatus.id] != (Select id from SystemStatus where name = 'Scrapped')"
			myReader = myCmd.ExecuteReader()
			If myReader.Read() Then
				'Check to see if we are returned a NULL value.
				If myReader.IsDBNull(0) Then
					result = "[AddSystemComment1] System serial number '" & serialNumber & "' is NULL"
					myReader.Close()
					Return False
				Else
					record = myReader.GetGuid(0)
				End If
			Else
				'If nothing is returned then it does not exist.
				result = "[AddSystemComment2] System serial number '" & serialNumber & "' does not exist."
				myReader.Close()
				Return False
			End If
			myReader.Close()

			'Check to see if the comment has been put in already
			Dim today As String = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")
			myCmd.CommandText = "SELECT * FROM SystemAudit WHERE [dbo.System.systemid] = '" & record.ToString() & "' AND CONVERT(VARCHAR(200),Comment) = '" & comment & "' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
			Dim dt_results As New DataTable
			dt_results.Load(myCmd.ExecuteReader)

			If dt_results.Rows.Count = 0 Then
				'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
				myCmd.CommandText = "INSERT INTO dbo.SystemAudit(id,[dbo.System.systemid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record.ToString() & "','" & comment & "',GETDATE(),'" & username & "')"
				myCmd.ExecuteNonQuery()
			End If

			Return True
		Catch ex As Exception
			result = "[AddSystemComment exception] " & ex.Message
			myReader.Close()
			Return False
		End Try
	End Function

	Public Function AddSystemBurnComment(ByRef myCmd As SqlCommand, ByRef serialNumber As String,
									 ByRef result As String, ByRef loops As String, ByRef errors As String, ByRef today As String) As Boolean
		Try
			Dim record As String = ""
			'Get the GUID from the passed in serial number.
			myCmd.CommandText = "SELECT systemid FROM dbo.System WHERE SerialNumber = '" & serialNumber & "' and 
[dbo.SystemStatus.id] != (Select id from SystemStatus where name = 'Scrapped')"
			Dim dt_results As New DataTable
			dt_results.Load(myCmd.ExecuteReader)

			If dt_results.Rows.Count = 0 Then
				'If nothing is returned then it does not exist.
				result = "[AddSystemComment2] System serial number '" & serialNumber & "' does not exist."
				Return False
			Else
				record = dt_results(0)("systemid").ToString()
			End If

			'Check to see if the comment has been put in already
			myCmd.CommandText = "SELECT * FROM SystemAudit WHERE [dbo.System.systemid] = '" & record & "' AND CONVERT(VARCHAR(200),Comment) LIKE '%Burn In Loops: %' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
			dt_results = New DataTable
			dt_results.Load(myCmd.ExecuteReader)

			If dt_results.Rows.Count = 0 Then
				'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
				myCmd.CommandText = "INSERT INTO dbo.SystemAudit(id,[dbo.System.systemid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record & "','Burn In Loops: " & loops & " Errors: " & errors & "',GETDATE(),'" & username & "')"
				myCmd.ExecuteNonQuery()
			Else
				myCmd.CommandText = "UPDATE SystemAudit SET Comment = 'Burn In Loops: " & loops & " Errors: " & errors & "' WHERE id = '" & dt_results(0)("id").ToString() & "'"
				myCmd.ExecuteNonQuery()
			End If

			Return True
		Catch ex As Exception
			result = "[AddSystemComment exception] " & ex.Message
			myReader.Close()
			Return False
		End Try
	End Function

	''' <summary>
	''' Updates the status of the boards assocciated with the system serial number that gets passed through.
	''' </summary>
	''' <param name="myCmd">The sql Command that will be used.</param>
	''' <param name="status">The board status that we want to change to.</param>
	''' <param name="systemSerialNumber">The system serial number that we are working with.</param>
	''' <param name="comment">The comment that we want to attach to the boards.</param>
	''' <param name="result">OUTPUT: Message regarding if anything does not work.</param>
	''' <returns>True: Everything worked out. False: Something did not work, see result message for details.</returns>
	''' <remarks></remarks>
	Public Function UpdateSystemBoards(byRef myCmd As SqlCommand, ByRef status As String, ByRef systemSerialNumber As String, ByRef comment As String, ByRef result As String) As Boolean
		Dim record As Guid = Nothing
		Dim boardStatus As String = ""
		Try
			'Get board status that is passed through.
			If GetBoardStatus(status, boardStatus, result) = False Then
				result = "[UpdateSystemBoards1] " & result
				Return False
			End If

			Dim maxboards As Integer = 0

			If database = netboxDatabase(INDEX_DEVEL) Or database = netboxDatabase(INDEX_PRODUCTION) Then
				maxboards = 10
			ElseIf database = blueskyDatabase(INDEX_DEVEL) Or database = blueskyDatabase(INDEX_PRODUCTION) Then
				maxboards = 5
			End If

			'---------------------------'
			'   M O T H E R B O A R D   '
			'---------------------------'

			'Grab the motherboard id associated with the passed in serial number.
			If GetBoardGUIDBySystemSerialNumber(myCmd, systemSerialNumber, "Motherboard", record, result) = False Then
				result = "[UpdateSystemBoards2] " & result
				Return False
			End If

			If record <> Guid.Empty Then
				'Update the status.
				myCmd.CommandText = "UPDATE dbo.Board SET LastUpdate=GETDATE(), [dbo.BoardStatus.id]='" & boardStatus & "' WHERE boardid = '" & record.ToString & "'"
				myCmd.ExecuteNonQuery()

				'Insert the comment.
				'Check to see if the comment has been put in already
				Dim today As String = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")
				myCmd.CommandText = "SELECT * FROM BoardAudit WHERE [dbo.Board.boardid] = '" & record.ToString() & "' AND CONVERT(VARCHAR(200),Comment) = '" & comment & "' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
				Dim dt_results As New DataTable
				dt_results.Load(myCmd.ExecuteReader)

				If dt_results.Rows.Count = 0 Then
					'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
					myCmd.CommandText = "INSERT INTO dbo.BoardAudit(id,[dbo.Board.boardid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record.ToString() & "','" & comment & "',GETDATE(),'" & username & "')"
					myCmd.ExecuteNonQuery()
				End If

			End If

			'-------------------------'
			'   M A S T E R   C P U   '
			'-------------------------'

			'Grab the Main CPU id associated with the passed in serial number.
			If GetBoardGUIDBySystemSerialNumber(myCmd, systemSerialNumber, "MainCPU", record, result) = False Then
				result = "[UpdateSystemBoards3] " & result
				Return False
			End If

			If record <> Guid.Empty Then
				'Update the status.
				myCmd.CommandText = "UPDATE dbo.Board SET LastUpdate=GETDATE(), [dbo.BoardStatus.id]='" & boardStatus & "' WHERE boardid = '" & record.ToString & "'"
				myCmd.ExecuteNonQuery()

				'Insert the comment.
				'Check to see if the comment has been put in already
				Dim today As String = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")
				myCmd.CommandText = "SELECT * FROM BoardAudit WHERE [dbo.Board.boardid] = '" & record.ToString() & "' AND CONVERT(VARCHAR(200),Comment) = '" & comment & "' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
				Dim dt_results As New DataTable
				dt_results.Load(myCmd.ExecuteReader)

				If dt_results.Rows.Count = 0 Then
					'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
					myCmd.CommandText = "INSERT INTO dbo.BoardAudit(id,[dbo.Board.boardid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record.ToString() & "','" & comment & "',GETDATE(),'" & username & "')"
					myCmd.ExecuteNonQuery()
				End If
			End If

			'---------------------'
			'   S L O T   2 - 10   '
			'---------------------'

			For i = 2 To maxboards
				'Grab the board id for the slot we are dealing with using 'i' to cycle through each slot number.
				If GetBoardGUIDBySystemSerialNumber(myCmd, systemSerialNumber, "Slot" & i, record, result) = False Then
					result = "[UpdateSystemBoards4] " & result
					Return False
				End If

				'Check to see if our record got an id back or if it is empty.
				If record <> Guid.Empty Then
					'Update our board status.
					myCmd.CommandText = "UPDATE dbo.Board SET LastUpdate=GETDATE(), [dbo.BoardStatus.id]='" & boardStatus & "' WHERE boardid = '" & record.ToString & "'"
					myCmd.ExecuteNonQuery()

					'Insert the comment corresponding to the board serial number into the BoardAudit table form this user.
					'Check to see if the comment has been put in already
					Dim today As String = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")
					myCmd.CommandText = "SELECT * FROM BoardAudit WHERE [dbo.Board.boardid] = '" & record.ToString() & "' AND CONVERT(VARCHAR(200),Comment) = '" & comment & "' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
					Dim dt_results As New DataTable
					dt_results.Load(myCmd.ExecuteReader)

					If dt_results.Rows.Count = 0 Then
						'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
						myCmd.CommandText = "INSERT INTO dbo.BoardAudit(id,[dbo.Board.boardid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record.ToString() & "','" & comment & "',GETDATE(),'" & username & "')"
						myCmd.ExecuteNonQuery()
					End If
				Else
					Exit For
				End If
			Next i
			Return True
		Catch ex As Exception
			result = "[UpdateSystemBoards exception] " & ex.Message
			Return False
		End Try
	End Function

	Public Function UpdateSystemBurnBoards(ByRef myCmd As SqlCommand, ByRef systemSerialNumber As String, ByRef result As String, ByRef loops As String, ByRef errors As String, ByRef today As String) As Boolean
		Dim record As Guid = Nothing
		Try

			Dim maxboards As Integer = 0

			If database = netboxDatabase(INDEX_DEVEL) Or database = netboxDatabase(INDEX_PRODUCTION) Then
				maxboards = 10
			ElseIf database = blueskyDatabase(INDEX_DEVEL) Or database = blueskyDatabase(INDEX_PRODUCTION) Then
				maxboards = 5
			End If

			'---------------------------'
			'   M O T H E R B O A R D   '
			'---------------------------'

			'Grab the motherboard id associated with the passed in serial number.
			If GetBoardGUIDBySystemSerialNumber(myCmd, systemSerialNumber, "Motherboard", record, result) = False Then
				result = "[UpdateSystemBoards2] " & result
				Return False
			End If

			If record <> Guid.Empty Then
				'Insert the comment.
				'Check to see if the comment has been put in already
				myCmd.CommandText = "SELECT * FROM BoardAudit WHERE [dbo.Board.boardid] = '" & record.ToString & "' AND CONVERT(VARCHAR(200),Comment) LIKE '%Burn In Loops: %' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
				Dim dt_results = New DataTable
				dt_results.Load(myCmd.ExecuteReader)

				If dt_results.Rows.Count = 0 Then
					'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
					myCmd.CommandText = "INSERT INTO BoardAudit(id,[dbo.Board.boardid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record.ToString & "','Burn In Loops: " & loops & " Errors: " & errors & "',GETDATE(),'" & username & "')"
					myCmd.ExecuteNonQuery()
				Else
					myCmd.CommandText = "UPDATE BoardAudit SET Comment = 'Burn In Loops: " & loops & " Errors: " & errors & "' WHERE id = '" & dt_results(0)("id").ToString() & "'"
					myCmd.ExecuteNonQuery()
				End If
			End If

			'-------------------------'
			'   M A S T E R   C P U   '
			'-------------------------'

			'Grab the Main CPU id associated with the passed in serial number.
			If GetBoardGUIDBySystemSerialNumber(myCmd, systemSerialNumber, "MainCPU", record, result) = False Then
				result = "[UpdateSystemBoards3] " & result
				Return False
			End If

			If record <> Guid.Empty Then
				'Insert the comment.
				'Check to see if the comment has been put in already
				myCmd.CommandText = "SELECT * FROM BoardAudit WHERE [dbo.Board.boardid] = '" & record.ToString & "' AND CONVERT(VARCHAR(200),Comment) LIKE '%Burn In Loops: %' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
				Dim dt_results = New DataTable
				dt_results.Load(myCmd.ExecuteReader)

				If dt_results.Rows.Count = 0 Then
					'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
					myCmd.CommandText = "INSERT INTO BoardAudit(id,[dbo.Board.boardid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
						& record.ToString & "','Burn In Loops: " & loops & " Errors: " & errors & "',GETDATE(),'" & username & "')"
					myCmd.ExecuteNonQuery()
				Else
					myCmd.CommandText = "UPDATE BoardAudit SET Comment = 'Burn In Loops: " & loops & " Errors: " & errors & "' WHERE id = '" & dt_results(0)("id").ToString() & "'"
					myCmd.ExecuteNonQuery()
				End If
			End If

			'---------------------'
			'   S L O T   2 - 10   '
			'---------------------'

			For i = 2 To maxboards
				'Grab the board id for the slot we are dealing with using 'i' to cycle through each slot number.
				If GetBoardGUIDBySystemSerialNumber(myCmd, systemSerialNumber, "Slot" & i, record, result) = False Then
					result = "[UpdateSystemBoards4] " & result
					Return False
				End If

				'Check to see if our record got an id back or if it is empty.
				If record <> Guid.Empty Then
					'Insert the comment.
					'Check to see if the comment has been put in already
					myCmd.CommandText = "SELECT * FROM BoardAudit WHERE [dbo.Board.boardid] = '" & record.ToString & "' AND CONVERT(VARCHAR(200),Comment) LIKE '%Burn In Loops: %' AND CONVERT(VARCHAR(10),LastUpdate,120) LIKE '%" & today & "%'"
					Dim dt_results = New DataTable
					dt_results.Load(myCmd.ExecuteReader)

					If dt_results.Rows.Count = 0 Then
						'Insert the comment corresponding to the serial number into the SystemAudit table form this user.
						myCmd.CommandText = "INSERT INTO BoardAudit(id,[dbo.Board.boardid],Comment,LastUpdate, [User]) VALUES(NEWID(), '" _
							& record.ToString & "','Burn In Loops: " & loops & " Errors: " & errors & "',GETDATE(),'" & username & "')"
						myCmd.ExecuteNonQuery()
					Else
						myCmd.CommandText = "UPDATE BoardAudit SET Comment = 'Burn In Loops: " & loops & " Errors: " & errors & "' WHERE id = '" & dt_results(0)("id").ToString() & "'"
						myCmd.ExecuteNonQuery()
					End If
				Else
					Exit For
				End If
			Next i
			Return True
		Catch ex As Exception
			result = "[UpdateSystemBoards exception] " & ex.Message
			Return False
		End Try
	End Function

	''' <summary>
	''' Checks to see if the passed system serial number exists in the database. Closes the passed reader before exit.
	''' </summary>
	''' <param name="systemSerialNumber">The system serial number that we are checking for.</param>
	''' <param name="result">OUTPUT: Error message that gives us a hint on what went wrong.</param>
	''' <returns>True: The record exists. False: The record does not exists, see result for details.</returns>
	''' <remarks>Make sure the SQL reader that is being passed through is already set to the SQL command reader before calling this function.</remarks>
	Public Function FindSystemSerialNumber(ByRef systemSerialNumber As String, ByRef result As String) As Boolean
		'Check to see if the record with the passed serial number exists or not.
		thisCMD.CommandText = "IF EXISTS(SELECT systemid FROM dbo.System WHERE SerialNumber = '" & systemSerialNumber & "' and 
[dbo.SystemStatus.id] != (Select id from SystemStatus where name = 'Scrapped')) SELECT 1 ELSE SELECT 0"
		myReader = thisCMD.ExecuteReader()
		If myReader.Read() Then
			If myReader.GetInt32(0) = 0 Then
				result = "[FindSystemSerialNumber1]  System serial '" & systemSerialNumber & "' number does not exist inside the database."
				myReader.Close()
				Return False
			End If
		End If
		myReader.Close()
		Return True
	End Function

	''' <summary>
	''' Gets the system status from the SQL reader that is passed throguh. Closes the passed reader before exit.
	''' </summary>
	''' <param name="status">Status that we are looking for.</param>
	''' <param name="systemStatus">OUTPUT: String that will hold the GUID of the status.</param>
	''' <param name="result">OUTPUT: Error message that will give us some insight as to what went wrong.</param>
	''' <returns>True: Everything worked out, returns our systemStatus. False: Somethign went wrong, see result for insight.</returns>
	''' <remarks>Make sure the SQL reader that is being passed through is already set to the SQL command reader before calling this function.</remarks>
	Public Function GetSystemStatus(ByRef status As String, ByRef systemStatus As String,
									ByRef result As String) As Boolean
		thisCMD.CommandText = "SELECT id FROM dbo.SystemStatus WHERE name = '" & status & "'"
		myReader = thisCMD.ExecuteReader()

		If myReader.Read() Then
			'Check to see if we are returned a NULL value.
			If myReader.IsDBNull(0) Then
				result = "[GetSystemStatus1] System status name '" & status & "' is NULL."
				myReader.Close()
				Return False
			Else
				systemStatus = myReader.GetGuid(0).ToString
			End If
		Else
			'If nothing is returned then it does not exist.
			result = "[GetSystemStatus2] System status name '" & status & "' does not exist."
			myReader.Close()
			Return False
		End If
		myReader.Close()
		Return True
	End Function

	''' <summary>
	''' Gets the board status from the SQL reader that is passed through. Closes the passed reader before exit.
	''' </summary>
	''' <param name="status">Status that we are looking for.</param>
	''' <param name="boardStatus">OUTPUT: String that will hold the GUID of the status.</param>
	''' <param name="result">OUTPUT: Error message that will give us some insight as to what went wrong.</param>
	''' <returns>True: Everything worked out, returns our boardStatus. False: Somethign went wrong, see result for insight.</returns>
	''' <remarks>Make sure the SQL reader that is being passed through is already set to the SQL command reader before calling this function.</remarks>
	Public Function GetBoardStatus(ByRef status As String, ByRef boardStatus As String, ByRef result As String) As Boolean
		thisCMD.CommandText = "SELECT id FROM dbo.boardStatus WHERE name = '" & status & "'"
		myReader = thisCMD.ExecuteReader()

		If myReader.Read() Then
			'Check to see if we are returned a NULL value.
			If myReader.IsDBNull(0) Then
				result = "[GetBoardStatus1] Board status name '" & status & "' is NULL."
				myReader.Close()
				Return False
			Else
				boardStatus = myReader.GetGuid(0).ToString
			End If
		Else
			'If nothing is returned then it does not exist.
			result = "[GetBoardStatus2] Board status name '" & status & "' does not exist."
			myReader.Close()
			Return False
		End If
		myReader.Close()
		Return True
	End Function

	''' <summary>
	''' Gets the Board Serial Number using the passed in Board ID and System serial number.
	''' </summary>
	''' <param name="systemSerialNumber">The system serial number that we are working with.</param>
	''' <param name="board">Board ID that we want the serial number from.</param>
	''' <param name="record">OUTPUT: The ID associated with the board.</param>
	''' <param name="result">OUTPUT: Error result when things do not work.</param>
	''' <returns>True: The record exists, returns the Board Serial Number. False: The record does not exists, see result for details.</returns>
	''' <remarks>Make sure the SQL reader that is being passed through is already set to the SQL command reader before calling this function.</remarks>
	Public Function GetBoardGUIDBySystemSerialNumber(ByRef myCmd As SqlCommand, ByRef systemSerialNumber As String, ByRef board As String, ByRef record As Guid, ByRef result As String) As Boolean
		myCmd.CommandText = "SELECT [" & board & ".boardid] FROM dbo.System WHERE SerialNumber = '" & systemSerialNumber & "' and 
[dbo.SystemStatus.id] != (Select id from SystemStatus where name = 'Scrapped')"
		myReader = myCmd.ExecuteReader()
		If myReader.Read() Then
			'Check to see if the Reader is empty/NULL or not.
			If myReader.IsDBNull(0) Then
				record = Guid.Empty
			Else
				'If not, set our record GUID to whatever was passed back to us.
				record = myReader.GetGuid(0)
			End If
		Else
			'If nothing is returned then it does not exist.
			result = "[GetBoardGUIDBySystemSerialNumber1] Board '" & board & "'/SerialNumber '" & systemSerialNumber & "' does not exist."
			myReader.Close()
			Return False
		End If
		myReader.Close()
		Return True
	End Function

	''' <summary>
	''' Roll back the transaction so we do not commit anything into the database.
	''' </summary>
	''' <param name="transaction">The transaction that we want to roll back.</param>
	''' <param name="result">OUTPUT: If there is an issue trying to roll back the transaction.</param>
	''' <remarks></remarks>
	Private Sub RollBack(ByRef transaction As SqlTransaction, ByRef result As String)
		Try
			'Attempt to roll back the transaction. 
			transaction.Rollback()
		Catch ex As Exception
			'Handles any errors that may have occurred on the server that would cause the rollback to fail, such as a closed connection.
			result = result & " :: " & ex.Message
		End Try
	End Sub

	''' <summary>
	''' Updates the status of the system to 'Burn In'.
	''' </summary>
	''' <param name="systemSerialNumber">The serial number of the system that we are working with.</param>
	''' <param name="result">OUTPUT: Error message to describe what went wrong.</param>
	''' <returns>True: Each board was updated in the database. False: One of the boards was not able to be updated, errors occured.</returns>
	''' <remarks>This is a all or nothing function. If one does not work then they all do not get updated to the database.</remarks>
	Public Function BurnInAudit(ByRef systemSerialNumber As String, ByRef result As String, ByRef loops As String, ByRef errors As String, ByRef today As String) As Boolean
		Dim transaction As SqlTransaction = Nothing
		Dim myCmd As New SqlCommand("", conn)
		Try
			'------------------------------'
			'   U P D A T E   S Y S T E M  '
			'------------------------------'

			'Start our transaction. Must assign both transaction object and connection to the command object for a pending local transaction.
			'transaction = conn.BeginTransaction("Burn In Transaction")
			'myCmd.Connection = conn
			'myCmd.Transaction = transaction

			'Create a generic comment for the new system update from the user.
			If AddSystemBurnComment(myCmd, systemSerialNumber, result, loops, errors, today) = False Then
				RollBack(transaction, result)
				result = "Something went wrong while trying to add a comment to system " & systemSerialNumber & ": " & result
				Return False
			End If

			'Update each board that is associated with the passed in system serial number.
			If UpdateSystemBurnBoards(myCmd, systemSerialNumber, result, loops, errors, today) = False Then
				RollBack(transaction, result)
				result = "Something went wrong while trying to update boards: " & result
				Return False
			End If

			'transaction.Commit()
			Return True
		Catch ex As Exception
			result = ex.Message
			If Not transaction Is Nothing Then
				RollBack(transaction, result)
			End If
			Return False
		End Try
	End Function

	Public Function BurnInDate(ByRef systemSerialNumber As String, ByRef result As String) As Boolean

		Dim systemStatus As String = ""
		Dim transaction As SqlTransaction = Nothing
		Dim record As Guid = Nothing

		Try
			'------------------------------'
			'   U P D A T E   S Y S T E M  '
			'------------------------------'

			'Get system status 'Burn In'.
			If GetSystemStatus(SS_BURN_IN, systemStatus, result) = False Then
				Return False
			End If

			'Update the corresponding record in the System table.
			thisCMD.CommandText = "UPDATE dbo.System SET BurnInDate=GETDATE(),[dbo.SystemStatus.id]='" & systemStatus & "', LastUpdate=GETDATE()" &
								"WHERE SerialNumber = '" & systemSerialNumber & "' and [dbo.SystemStatus.id] != (Select id from SystemStatus where name = 'Scrapped')"

			'Start our transaction. Must assign both transaction object and connection to the command object for a pending local transaction.
			'transaction = conn.BeginTransaction("Burn In Transaction")
			'thisCMD.Connection = conn
			'thisCMD.Transaction = transaction

			thisCMD.ExecuteNonQuery()

			'Create a generic comment for the new system update from the user.
			If AddSystemComment(thisCMD, systemSerialNumber, "System in Burn.", record, result) = False Then
				RollBack(transaction, result)
				result = "Something went wrong while trying to add a comment to system " & systemSerialNumber & ": " & result
				Return False
			End If
			'Update each board that is associated with the passed in system serial number.
			If UpdateSystemBoards(thisCMD, BS_BURN_IN, systemSerialNumber, "Board in Burn.", result) = False Then
				RollBack(transaction, result)
				result = "Something went wrong while trying to update boards: " & result
				Return False
			End If
			'End If

			'transaction.Commit()
			Return True
		Catch ex As Exception
			result = ex.Message
			If Not transaction Is Nothing Then
				RollBack(transaction, result)
			End If
			Return False
		End Try
	End Function

End Class