Public Class CustomMsgBox
	'Dim Message As String = ""

	Public Sub New(byref messageString As String) 

	    ' This call is required by the designer.
	    InitializeComponent()

	    ' Add any initialization after the InitializeComponent() call.
		Message_RichTextBox.text = messageString
	End Sub

	Private Sub CustomMsgBox_Load() Handles MyBase.Load

	End Sub

	Private Sub OK_Button_Click() Handles OK_Button.Click
		Close
	End Sub

End Class