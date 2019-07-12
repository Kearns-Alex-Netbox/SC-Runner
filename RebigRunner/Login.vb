Public Module Variables
	Public Const INDEX_DEVEL As Integer = 0
	Public Const INDEX_PRODUCTION As Integer = 1

	Public Const INDEX_DATABASE As Integer = INDEX_PRODUCTION

	' databases
	Public netboxDatabase()  As String = {"Devel"       , "Production"}
	Public blueskyDatabase() As String = {"BlueSkyDevel", "BlueSkyProduction"}
	Public database          As String = "Production"

	Public Bypass As Boolean = False
	Public user As String = ""
	Public pass As String = ""

	Public Enum MeterType
		Meter1_Relay
		Meter1_PlaceHolder
		Meter2_Relay
		Meter2_Test1
		Meter2_Test2
		Meter2_Test3
		Meter3_Relay
		Meter3_Test1
		Meter3_Test2
		Meter3_Test3
		Meter4_Relay
		Meter4_Test1
		Meter4_Test2
	End Enum

#Region "Meter Setup"
	Public RequestData As String () = {
"form=analog" &				'Type of meter												METER 1 RELAY
"&deviceid=0" &				'Analog Input: Origin 0
"" &						
"&analogname=JSON+1" &		'Name
"&scaletype=linear" &		'Scale
"&enablestate=check" &		'Enabled
"" &						
"&mamplow=4" &				'Low mA value
"&vollow=0" &				'Low Scaled value
"&mamphi=20" &				'High mA value
"&volhi=100" &				'High Scaled value
"" &						
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &						
"&mydesc=" &				'Description
"" &						
"&rname1=JSON+ACT+1" &		'Name of the Action
"&relaytype1=9" &			'Action Type
"&relayno1=1" &				'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=80" &				'High Trip point
"&resettrippt1=70" &		'Reset point
"&email1=none" &			'???
"" &						
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=1",				'Meter Number that we are adding [1-8]: Origin 1
	"form=analog" &					'Type of meter												METER 1 PLACE HOLDER
	"&deviceid=0" &					'Analog Input: Origin 0
	"" &							
	"&analogname=JSON+1" &			'Name
	"&scaletype=linear" &			'Scale
	"&enablestate=check" &			'Enabled
	"" &							
	"&mamplow=4" &					'Low mA value
	"&vollow=0" &					'Low Scaled value
	"&mamphi=20" &					'High mA value
	"&volhi=100" &					'High Scaled value
	"" &							
	"&decdigits=1" &				'Decimal digits
	"&sigdigits=3" &				'Digits
	"&numerator=0" &				'Unit of measure
	"" &							
	"&mydesc=" &					'Description
	"" &							
	"&rname1=JSON+ACT+1" &			'Name of the Action
	"&relaytype1=9" &				'Action Type
	"&relayno1=98" &				'Physical relay: Origin 1
	"&alarmdelay1=0" &				'Delay Seconds to Alarm
	"&trippt1=80" &					'High Trip point
	"&resettrippt1=70" &			'Reset point
	"&email1=none" &				'???
	"" &							
	"&lcddetail=check" &			'Always set to 'check'
	"&name=tankwiz.json" &			'???
	"&fieldno=1",					'Meter Number that we are adding [1-8]: Origin 1
"form=analog" &				'Type of meter												METER 2 RELAY
"&deviceid=1" &				'Analog Input: Origin 0
"" &						
"&analogname=JSON+2" &		'Name
"&scaletype=linear" &		'Scale
"&enablestate=check" &		'Enabled
"" &						
"&mamplow=4" &				'Low mA value
"&vollow=0" &				'Low Scaled value
"&mamphi=20" &				'High mA value
"&volhi=100" &				'High Scaled value
"" &						
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &						
"&mydesc=" &				'Description
"" &						
"&rname1=JSON+ACT+2" &		'Name of the Action
"&relaytype1=8" &			'Action Type
"&relayno1=2" &				'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=30" &				'Low Trip Point
"&resettrippt1=40" &		'Reset point
"&email1=none" &			'???
"" &						
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=2",				'Meter Number that we are adding [1-8]: Origin 1
	"form=analog" &					'Type of meter												METER 2 TEST 1
	"&deviceid=1" &					'Analog Input: Origin 0
	"" &							
	"&analogname=Low+Relay" &		'Name
	"&scaletype=linear" &			'Scale
	"&enablestate=check" &			'Enabled
	"" &							
	"&mamplow=4" &					'Low mA value
	"&vollow=0" &					'Low Scaled value
	"&mamphi=20" &					'High mA value
	"&volhi=100" &					'High Scaled value
	"" &							
	"&decdigits=1" &				'Decimal digits
	"&sigdigits=3" &				'Digits
	"&numerator=0" &				'Unit of measure
	"" &							
	"&mydesc=" &					'Description
	"" &							
	"&rname1=JSON+ACT+2" &			'Name of the Action
	"&relaytype1=8" &				'Action Type
	"&relayno1=1" &					'Physical relay: Origin 1
	"&alarmdelay1=0" &				'Delay Seconds to Alarm
	"&trippt1=20" &					'Low Trip Point
	"&resettrippt1=30" &			'Reset point
	"&email1=none" &				'???
	"" &							
	"&lcddetail=check" &			'Always set to 'check'
	"&name=tankwiz.json" &			'???
	"&fieldno=2",					'Meter Number that we are adding [1-8]: Origin 1
"form=analog" &				'Type of meter												METER 2 TEST 2
"&deviceid=1" &				'Analog Input: Origin 0
"" &						
"&analogname=Empty" &		'Name
"&scaletype=kfactor" &		'Scale
"&enablestate=check" &		'Enabled
"" &						
"&kfactorval2=1" &			'K Factor
"&kunit2=gallon" &			'K Units
"&ktime2=minute" &			'K Time
"" &						
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &						
"&mydesc=" &				'Description
"" &						
"&rname1=JSON+ACT+2" &		'Name of the Action
"&relaytype1=2" &			'Action Type
"&relayno1=4" &				'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=13" &				'Low Trip Point
"&resettrippt1=17" &		'Reset point
"&email1=none" &			'???
"" &						
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=2",				'Meter Number that we are adding [1-8]: Origin 1
	"form=analog" &					'Type of meter												METER 2 TEST 3
	"&deviceid=1" &					'Analog Input: Origin 0
	"" &							
	"&analogname=In+Range" &		'Name
	"&scaletype=none" &				'Scale
	"&enablestate=check" &			'Enabled
	"" &							
	"&decdigits=1" &				'Decimal digits
	"&sigdigits=3" &				'Digits
	"&numerator=0" &				'Unit of measure
	"" &							
	"&mydesc=" &					'Description
	"" &							
	"&rname1=JSON+ACT+2" &			'Name of the Action
	"&relaytype1=5" &				'Action Type
	"&ackinput1=2" &				'Acknowldge input
	"&ack1=check" &					'Check for Acknowldge
	"&relayno1=98" &				'Physical relay: Origin 1
	"&alarmdelay1=0" &				'Delay Seconds to Alarm
	"&trippt1=10" &					'Low Range Trip Point
	"&resettrippt1=9" &				'Reset point
	"&trippthi1=15" &				'High Range Trip point
	"&resettrippthi1=16" &			'Reset point
	"&email1=none" &				'???
	"" &							
	"&lcddetail=check" &			'Always set to 'check'
	"&name=tankwiz.json" &			'???
	"&fieldno=2",					'Meter Number that we are adding [1-8]: Origin 1
"form=analog" &				'Type of meter												METER 3 RELAY
"&deviceid=2" &				'Analog Input: Origin 0
"" &						
"&analogname=JSON+3" &		'Name
"&scaletype=linear" &		'Scale
"&enablestate=check" &		'Enabled
"" &						
"&mamplow=4" &				'Low mA value
"&vollow=0" &				'Low Scaled value
"&mamphi=20" &				'High mA value
"&volhi=100" &				'High Scaled value
"" &						
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &						
"&mydesc=" &				'Description
"" &						
"&rname1=JSON+ACT+3" &		'Name of the Action
"&relaytype1=9" &			'Action Type
"&relayno1=3" &				'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=80" &				'High Trip Point
"&resettrippt1=70" &		'Reset point
"&email1=none" &			'???
"" &						
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=3",				'Meter Number that we are adding [1-8]: Origin 1
	"form=analog" &					'Type of meter												METER 3 TEST 1
	"&deviceid=2" &					'Analog Input: Origin 0 
	"" &							
	"&analogname=High+Relay" &		'Name
	"&scaletype=nonlinear" &		'Scale
	"&enablestate=check" &			'Enabled
	"" &						
	"&mytext=1,  4.00,  0.0" & vbNewLine & 
	"2,  20.0,  100.0" &			'Points for scaling [32 max]
	"" &							
	"&decdigits=1" &				'Decimal digits
	"&sigdigits=3" &				'Digits
	"&numerator=0" &				'Unit of measure
	"" &		
	"&mydesc=" &					'Description
	"" &		
	"&rname1=JSON+ACT+3" &			'Name of the Action
	"&relaytype1=9" &				'Action Type
	"&relayno1=2" &					'Physical relay: Origin 1
	"&alarmdelay1=0" &				'Delay Seconds to Alarm
	"&trippt1=80" &					'High Trip point
	"&resettrippt1=70" &			'Reset point
	"&email1=none" &				'???
	"" &		
	"&lcddetail=check" &			'Always set to 'check'
	"&name=tankwiz.json" &			'???
	"&fieldno=3",					'Meter Number that we are adding [1-8]: Origin 1
"form=analog" &				'Type of meter												METER 3 TEST 2
"&deviceid=2" &				'Analog Input: Origin 0
"" &			
"&analogname=Low+Alarm" &	'Name
"&scaletype=none" &			'Scale
"&enablestate=check" &		'Enabled
"" &
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &		
"&mydesc=" &				'Description
"" &
"&rname1=JSON+ACT+3" &		'Name of the Action
"&relaytype1=3" &			'Action Type
"&ackinput1=0" &			'ACK input 
"&relayno1=98" &			'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=6" &				'Low Trip point
"&resettrippt1=8" &			'Reset point
"&email1=none" &			'???
"" &
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=3",				'Meter Number that we are adding [1-8]: Origin 1
	"form=analog" &					'Type of meter											METER 3 TEST 3
	"&deviceid=2" &					'Analog Input: Origin 0
	"" &
	"&analogname=Out+of+Range" &	'Name
	"&scaletype=none" &				'Scale
	"&enablestate=check" &			'Enabled
	"" &
	"&mamplow=4" &					'Low mA value
	"&vollow=0" &					'Low Scaled value
	"&mamphi=20" &					'High mA value
	"&volhi=100" &					'High Scaled value
	"" &
	"&decdigits=1" &				'Decimal digits
	"&sigdigits=3" &				'Digits
	"&numerator=0" &				'Unit of measure
	"" &							
	"&mydesc=" &					'Description
	"" &							
	"&rname1=JSON+ACT+3" &			'Name of the Action
	"&relaytype1=6" &				'Action Type
	"&ackinput1=3" &
	"&ack1=check" &
	"&relayno1=98" &				'Physical relay: Origin 1
	"&alarmdelay1=0" &				'Delay Seconds to Alarm
	"&trippt1=8" &					'Low Range Trip point
	"&resettrippt1=9" &				'Reset point
	"&trippthi1=16" &				'High Range Trip point
	"&resettrippthi1=15" &			'Reset point
	"&email1=none" &				'???
	"" &
	"&lcddetail=check" &			'Always set to 'check'
	"&name=tankwiz.json" &			'???
	"&fieldno=3",					'Meter Number that we are adding [1-8]: Origin 1
"form=analog" &				'Type of meter												METER 4 RELAY
"&deviceid=3" &				'Analog Input: Origin 0
"" &						
"&analogname=JSON+4" &		'Name
"&scaletype=linear" &		'Scale
"&enablestate=check" &		'Enabled
"" &						
"&mamplow=4" &				'Low mA value
"&vollow=0" &				'Low Scaled value
"&mamphi=20" &				'High mA value
"&volhi=100" &				'High Scaled value
"" &						
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &						
"&mydesc=" &				'Description
"" &						
"&rname1=JSON+ACT+4" &		'Name of the Action
"&relaytype1=8" &			'Action Type
"&relayno1=4" &				'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=30" &				'Low Trip Point
"&resettrippt1=40" &		'Reset point
"&email1=none" &			'???
"" &						
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=4",				'Meter Number that we are adding [1-8]: Origin 1
	"form=analog" &					'Type of meter												METER 4 TEST 1
	"&deviceid=3" &					'Analog Input: Origin 0
	"" &
	"&analogname=Fill" &			'Name
	"&scaletype=sfactor" &			'Scale
	"&enablestate=check" &			'Enabled
	"" &
	"&scalefactorval2=1" &			'Factor
	"" &
	"&decdigits=1" &				'Decimal digits
	"&sigdigits=3" &				'Digits
	"&numerator=0" &				'Unit of measure
	"" &							
	"&mydesc=" &					'Description
	"" &							
	"&rname1=JSON+ACT+4" &			'Name of the Action
	"&relaytype1=1" &				'Action Type
	"&relayno1=3" &					'Physical relay: Origin 1
	"&alarmdelay1=0" &				'Delay Seconds to Alarm
	"&trippt1=15" &					'High Trip point
	"&resettrippt1=14" &			'Reset point
	"&email1=none" &				'???
	"" &							
	"&lcddetail=check" &			'Always set to 'check'
	"&name=tankwiz.json" &			'???
	"&fieldno=4",					'Meter Number that we are adding [1-8]: Origin 1
"form=analog" &				'Type of meter												METER 4 TEST 2
"&deviceid=3" &				'Analog Input: Origin 0
"" &						
"&analogname=High+Alarm" &	'Name
"&scaletype=none" &			'Scale
"&enablestate=check" &		'Enabled
"" &
"&decdigits=1" &			'Decimal digits
"&sigdigits=3" &			'Digits
"&numerator=0" &			'Unit of measure
"" &						
"&mydesc=" &				'Description
"" &						
"&rname1=JSON+ACT+4" &		'Name of the Action
"&relaytype1=4" &			'Action Type
"&ackinput1=1" &			'Acknowldge input
"&ack1=check" &				'Check for Acknowldge
"&relayno1=98" &			'Physical relay: Origin 1
"&alarmdelay1=0" &			'Delay Seconds to Alarm
"&trippt1=17" &				'High Trip point
"&resettrippt1=16" &		'Reset point
"&email1=none" &			'???
"" &						
"&lcddetail=check" &		'Always set to 'check'
"&name=tankwiz.json" &		'???
"&fieldno=4"				'Meter Number that we are adding [1-8]: Origin 1
	}
#End Region

End Module

Public Class Login
	Dim sqlapi As New SQL_API()

	Private Sub Login_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		L_Version.Text = "V:" & Application.ProductVersion

		If INDEX_DATABASE = INDEX_DEVEL Then
			L_Database.Text = "DEVEL"
		Else If INDEX_DATABASE = INDEX_PRODUCTION Then
			L_Database.Text = "PRODUCTION"
		End If

		KeyPreview = true

		' we need to reset the fixed IP each time we load the program
		My.Settings.FixedIP = 2
	End Sub

	Private Sub MyBase_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
		If e.KeyCode.Equals(Keys.Enter) Then
			Call B_Login_Click()
		End If
	End Sub

	Private Sub B_Login_Click() Handles B_Login.Click
		user = TB_User.Text
		pass = TB_Password.Text

		sqlapi._Username = user
		sqlapi._Password = pass
		Dim result As String = ""

		If Bypass_CheckBox.Checked = False Then
			If sqlapi.OpenDatabase(result) = False Then
				MsgBox(result)
				Return
			End If
		Else
			Bypass = True
		End If

		Dim DoSelectGrid As New SelectGrid
		DoSelectGrid.Show()
		Close()
	End Sub

	Private Sub B_Exit_Click() Handles B_Exit.Click
		Application.Exit()
	End Sub

End Class

Public Class ThreadComm
	Public ExitCommand As Integer
	Public ServerName As String
	Public ThreadNumber As Integer
	Public TimeStamp As string
	Public IPStable As Integer
End Class

Public Class JSON_InfoResult
	Public cpuid		As String
	Public model		As String
	Public version		As String
	Public serial		As String
	Public cpuserial	As String
	Public ioversion	As String
	Public blversion	As String
	Public macaddress	As String
	Public type			As String
End Class