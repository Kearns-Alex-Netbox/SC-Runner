Imports Newtonsoft.Json
Imports System.IO
Imports System.Text
Imports System.Net

Public Class JSON_API

	Const IP_TYPE_FIXED As String = "fixed"
	Const IP_TYPE_DHCP  As String = "DHCP"
	Const IP_TYPE_AP	As String = "AP"
	Const IP_TYPE_NONE	As String = "none"

	Const MS_INSECONDS	As Integer = 1000
	Const CONN_TIMEOUT	As Integer = 1 * MS_INSECONDS
	Const REQ_TIMEOUT   As Integer = 5 * MS_INSECONDS

    ''' <summary>
	''' 
	''' </summary>
	''' <param name="IPAddress"></param>
	''' <param name="WebResponse"></param>
	''' <param name="Model"></param>
	''' <returns></returns>
    Public Function GetMachineInfo(ByRef IPAddress As String, ByRef WebResponse As String, Optional Model As String = "0") As Boolean
        Dim justURI As New Uri("http://" & IPAddress)
        Dim uri     As New Uri("http://" & IPAddress & "/process_json?name=machineinfo.json")
        Dim retval  As Boolean = False

		' check to see if we were provided a model 
		If Model = "0" Then
			' try Defender7
			retval = DoRequest(uri, justURI, "belleville", "private", WebResponse, CONN_TIMEOUT)

#If NotSupported
			' try 2300
			If retval = False Then
				retval = DoRequest(uri, justURI, "aquametrix", "private", WebResponse, CONN_TIMEOUT)
			End If
#End If
			' try ARC
			If retval = False Then
				uri = New Uri("http://" & IPAddress & "/run_json?action=systeminfo.json")

				retval = DoRequest(uri, justURI, "clearblue", "bluesky", WebResponse, CONN_TIMEOUT)
			End If
		Else
			Dim password  As String = ""
			Dim uriString As String = ""
			Dim domain	  As String = ""

			Select Case Model
				Case RuntimeEngine.MODEL_REBIG
					password  = "clearblue"
					domain    = "bluesky"
					uriString = "/run_json?action=systeminfo.json"

				Case RuntimeEngine.MODEL_D7
					password  = "belleville"
					domain    = "private"
					uriString = "/process_json?name=machineinfo.json"
#If NotSupported
				Case MODEL_2300
					password  = "aquametrix"
					domain    = "private"
					uriString = "/process_json?name=machineinfo.json"
#End If
				Case Else
					WebResponse = "Get Machine Info: " & vbNewLine & 
						           Model & " is not set up for this operation."
					Return False
			End Select

			uri    = New Uri("http://" & IPAddress & uriString)
			retval = DoRequest(uri, justURI, password, domain, WebResponse, CONN_TIMEOUT)
		End If

        Return retval
    End Function

	''' <summary>
	''' 
	''' </summary>
	''' <param name="IPAddress"></param>
	''' <param name="Model"></param>
	''' <param name="WebResponse"></param>
	''' <returns></returns>
	Public Function ConfigureFixedIP(ByRef IPAddress As String, byref Model As string, ByRef WebResponse As String) As Boolean
        Dim justURI    As New Uri("http://" & IPAddress)
		Dim uriGetting As String = ""
		Dim uriSetting As String = ""
		Dim password   As String = ""
		Dim domain     As String = ""

		Select Case Model
			Case RuntimeEngine.MODEL_REBIG
				password   = "clearblue"
				domain     = "bluesky"
				uriGetting = "http://" & IPAddress & "/run_json?action=getipinfo.json"
				uriSetting = "http://" & IPAddress & "/run_json?action=setipinfo.json&type=" & IP_TYPE_FIXED & "&ip=" & My.Settings.BaseIPString & My.Settings.FixedIP & "&gw=" & My.Settings.BaseIPString & "1&mask=255.255.255.0&dns=8.8.8.8"

#If NotSupported
			Case RuntimeEngine.MODEL_D7
				password   = "belleville"
				domain     = "private"
				uriGetting = "http://" & IPAddress & "/process_json?name=getipinfo.json"
				uriSetting = 

			Case MODEL_2300
				password   = "aquametrix"
				domain     = "private"
				uriGetting = "http://" & IPAddress & "/process_json?name=getipinfo.json"
				uriSetting = "http://" & IPAddress & "/process_json?name=setipinfo.json&type=" & IP_TYPE_FIXED & "&ip=" & My.Settings.BaseIPString & My.Settings.FixedIP & "&gw=" & My.Settings.BaseIPString & "1&mask=255.255.255.0&dns=8.8.8.8"
#End If
			Case Else
				WebResponse ="Configure Fixed IP:" & vbNewLine &
							  Model & " is not set up for this operation."
				Return False
		End Select

		 Dim uri As New Uri(uriGetting)

		' get the ip information
        If DoRequest(uri, justURI, password, domain, WebResponse) = False Then
			WebResponse = "Configure Fixed IP:" & vbNewLine &
						  "Get IP Info:" & vbNewLine &  
					       WebResponse
			Return False
        End If

		' get the data from the request
		Dim obj
		Try
			obj = JsonConvert.DeserializeObject(Of JSON_InfoResult)(WebResponse)
		Catch ex As Exception
			WebResponse = "Configure Fixed IP:" & vbNewLine &
				          "Get IP Info:" & vbNewLine & 
						  "Could Not convert JSON result string: " & ex.Message & vbNewLine &
						  "Data: " & WebResponse
			Return False
		End Try

		If obj.type = IP_TYPE_FIXED Then
			' we are already set up the way we should be
			WebResponse = IPAddress
			Return True
		End If

		' we need to ping each IP to find an open one on the network
		While True
			If GetMachineInfo(My.Settings.BaseIPString & My.Settings.FixedIP, WebResponse, Model) = False Then
				Exit While
			End If

			If My.Settings.FixedIP = 254 Then
				WebResponse = "Configure Fixed IP:" & vbNewLine &
					          "There are no more open IP addresses to assign."
				Return False
			End If

			My.Settings.FixedIP += 1
			My.Settings.Save
		End While

		' this request should end with a reboot which means a false reutrn = success
		uri = New Uri(uriSetting)

		if DoRequest(uri, justURI, password, domain, WebResponse) = true Then
			WebResponse = "Configure Fixed IP:" & vbNewLine &
						  "The Controller did not reset."
			Return False
		End If

		' send back the new IP address 
		WebResponse = My.Settings.BaseIPString & My.Settings.FixedIP

		' increase the IP by 1 so we can start with the next location
		My.Settings.FixedIP += 1
		My.Settings.Save

        Return True
    End Function

	''' <summary>
	''' 
	''' </summary>
	''' <param name="URI"></param>
	''' <param name="justURI"></param>
	''' <param name="password"></param>
	''' <param name="domain"></param>
	''' <param name="WebResponse"></param>
	''' <returns></returns>
    Public Function DoRequest(ByRef URI As Uri, ByRef justURI As Uri, ByRef password As String, byref domain As String, ByRef WebResponse As String,
							  Optional Timeout As Integer = REQ_TIMEOUT) As Boolean
		Dim request As HttpWebRequest = WebRequest.Create(URI)
		request.ContentType			= "application/json"
        request.Method				= "GET"
        request.AllowAutoRedirect	= False
        request.KeepAlive			= True
        request.Timeout				= Timeout
        request.ReadWriteTimeout	= Timeout
        request.Accept				= "application/json"
        request.Proxy				= Nothing

		Dim data As String = ""

        Dim credentialCache As CredentialCache = New CredentialCache()
		Dim netCredential As NetworkCredential = New NetworkCredential("admin", password, domain)

		credentialCache.Add(justURI, "Digest", netCredential)
        request.Credentials = credentialCache

        Try
            Using response As WebResponse = request.GetResponse()

                Dim dataStream As Stream = response.GetResponseStream()
                Dim encode As Encoding = Encoding.GetEncoding("utf-8")
                Dim readStream As New StreamReader(dataStream, encode)
                Dim read(2048) As [Char]
                ' Read up to 512 charcters 
                Dim count As Integer = readStream.Read(read, 0, 2048)

                readStream.Close()
                response.Close()

                ' Convert into string from an array of Chars
                Dim str As New [String](read, 0, count)

                data = str
            End Using

        Catch e As WebException
			WebResponse = "----- Do request -----" & vbNewLine & 
						  "Request: " & URI.OriginalString & vbNewLine &
						  "Response: " & data & vbNewLine &
						  "WebException: " & e.Message & vbNewLine
            If (e.Status = WebExceptionStatus.ProtocolError) Then
				WebResponse &= vbNewLine & "Status code: " & CType(e.Response, HttpWebResponse).StatusCode & vbNewLine & "Description: " & CType(e.Response, HttpWebResponse).StatusDescription & vbNewLine
            End If
            Return False
        Catch ex As Exception
			WebResponse = "----- Do request -----" & vbNewLine & 
						  "Request: " & URI.OriginalString & vbNewLine &
						  "Response: " & data & vbNewLine &
						  "Exception: " & ex.Message & vbNewLine
            Return False
		End Try
		
		WebResponse = data

		Return True
	End Function

	''' <summary>
	''' 
	''' </summary>
	''' <param name="IPAddress"></param>
	''' <param name="Model"></param>
	''' <param name="WebResponse"></param>
	''' <returns></returns>
	Public Function DoRemoteReboot(ByRef IPAddress As String, byref Model As String, ByRef WebResponse As String) As Boolean
		Dim uriJSON   As String = ""
		Dim uriValues As String = "&restart=yes"
		Dim password  As String = ""
		Dim domain	  As String = ""

		Select Case Model
			Case RuntimeEngine.MODEL_REBIG
				password  = "clearblue"
				domain    = "bluesky"
				uriJSON   = "/run_json?action=restart.json"

			Case RuntimeEngine.MODEL_D7
				password  = "belleville"
				domain    = "private"
				uriJSON   = "process_json?name=restart.json"
#If NotSupported
			Case MODEL_2300
				password  = "aquametrix"
				domain    = "private"
				uriJSON   = "/process_json?name=machineinfo.json"
#End If
			Case Else
				WebResponse = "Do Remote Reboot:" & vbNewLine & 
					           Model & " is not set up for this operation."
				Return False
		End Select

		Dim justURI   As New Uri("http://" & IPAddress)
        Dim uri       As New Uri("http://" & IPAddress & "/" & uriJSON & uriValues)

        if DoRequest(uri, justURI, password, domain, WebResponse) = False Then
			WebResponse = "Do Remote Reboot:" & vbNewLine & 
					      "Could not Reboot " & IPAddress & vbNewLine & 
						   WebResponse
			Return False	
        End If

		Return True
    End Function

	Public Function SetParameters(ByRef IPAddress As String, ByRef model As String, ByRef cpuid As String, ByRef WebResponse As String) As Boolean
		Dim password   As String = ""
		Dim geturiJSON As String = ""
		Dim seturiJSON As String = ""
		Dim domain	   As String = ""
		Dim uriValues  As String = ""

		Select Case Model
			Case RuntimeEngine.MODEL_REBIG
				password   = "clearblue"
				domain	   = "bluesky"
				seturiJSON = "/run_json?action=setparameters.json"
				geturiJSON = "/run_json?action=getparameters.json"
				' once the temp fix is no longer needed we can do this here
				uriValues  = "&deltap=0&anpressureenable=0&p42=0&p46=0"
				'uriValues  = "&cpuid=" & cpuid & "&deltap=0&anpressureenable=0&p42=0&p46=0"

			Case RuntimeEngine.MODEL_D7
				password   = "belleville"
				domain	   = "private"
				seturiJSON = "/process_json?name=setparameters.json"
				geturiJSON = "/process_json?name=getparameters.json"
				uriValues  = "&deltap=0&anpressureenable=0&p48=0"
				'uriValues  = "&cpuid=" & cpuid & "&deltap=0&anpressureenable=0&p48=0"
#If NotSupported
			Case MODEL_2300
				password   = "aquametrix"
				domain	   = "private"
				seturiJSON = "/process_json?name=setparameters.json"
				geturiJSON = "/process_json?name=getparameters.json"
#End If
			Case Else
				WebResponse = "Set Parameters:" & vbNewLine & 
					           Model & " is not set up for this operation."
				Return False
		End Select

		Dim justURI As New Uri("http://" & IPAddress)
		Dim uri     As     Uri
#If 1 
		' get the current parameters. this will give us the correct cpuid to use to set
		'Temp fix until others have been updated.
		If (model = RuntimeEngine.MODEL_D7) Then
			uri = New Uri("http://" & IPAddress & geturiJSON)

			If DoRequest(uri, justURI, password, domain, WebResponse) = False Then
				WebResponse = "Set Parameters:" & vbNewLine &
				              "Get Parameters:" & vbNewLine & 
							  "Could not get CPUID: " & IPAddress & vbNewLine & 
							   WebResponse
				Return False
			End If

			Dim obj
			Try
				obj = JsonConvert.DeserializeObject(Of JSON_InfoResult)(WebResponse)
			Catch ex As Exception
				WebResponse = "Set Parameters:" & vbNewLine &
				              "Get Parameters:" & vbNewLine & 
							  "Could Not convert JSON result string: " & ex.Message & vbNewLine &
							  "Data: " & WebResponse
				Return False
			End Try

			cpuid = obj.cpuid
		End If

		uriValues  = "&cpuid=" & cpuid & uriValues
#End If
       
		' set the parameters
		uri = New Uri("http://" & IPAddress & seturiJSON & uriValues)

		If DoRequest(uri, justURI, password, domain, WebResponse) = False Then
			WebResponse = "Set Parameters:" & vbNewLine &
				          "Could Not Set Parameters: " & IPAddress & vbNewLine & 
						   WebResponse
            Return False
		End If
		
        Return True
    End Function

	Public Function MeterSetup(ByRef IP As String, ByRef Model As String, ByRef Meter As MeterType, ByRef WebResponse As String) As Boolean
		Dim uriJSON   As String = ""
		Dim password  As String = ""
		Dim domain	  As String = ""

		Select Case Model
			Case RuntimeEngine.MODEL_2300
				password  = "aquametrix"
				domain    = "private"
				uriJSON   = "/process_json?name=machineinfo.json"
			Case Else
				WebResponse = "Meter Setup:" & vbNewLine & 
					           Model & " is not set up for this operation."
				Return False
		End Select

		Dim uri As New Uri("http://" & IP & uriJSON & "?" & RequestData(Meter))
		Dim justURI As New Uri("http://" & IP)

		If DoRequest(uri, justURI, password, domain, WebResponse) = False Then
			'The error data should be in the results already so now we need to exit.
			WebResponse = "Meter Setup:" & vbNewLine & 
					      "Could not Setup Meter for " & IP & vbNewLine & 
						   WebResponse
			Return False
		End If

		Dim json As Linq.JObject = Linq.JObject.Parse(WebResponse)

		'Check to see if we have errors or not.
		If json.HasValues Then
			'If our success is 0 then we have an error.
			Try
				If json.SelectToken("success").ToString = 0 Then
					WebResponse = "Returned Data:" & vbNewLine & json.SelectToken("error").ToString
					Return False
				End If
			Catch ex As Exception
				WebResponse = "Current version does not support ."
				Return False
			End Try
		End If

		Return True
	End Function

End Class