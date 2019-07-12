Public Class AddServers
	Dim IPlist As New List(Of String)
	Dim MaxIPs As     Integer = 0

	Public Sub New(ByVal max As Integer)

		' This call is required by the designer.
		InitializeComponent()

		' Add any initialization after the InitializeComponent() call.
		MaxIPs = max
	End Sub

	Private Sub AddMultipleServers_Load() Handles MyBase.Load
		IP_TextBox.Text = My.Settings.BaseIPString
		IP_TextBox.Focus()
		IP_TextBox.Select(IP_TextBox.Text.Length, 0)
		KeyPreview = True
	End Sub

	Private Sub Add_Button_Click() Handles Add_Button.Click
		If IP_ListBox.Items.Count = MaxIPs Then
			MsgBox("You have reached the max IP addresses that the program will handle.")
			Return
		End If

		If IP_TextBox.Text = My.Settings.BaseIPString Or IPlist.Contains(IP_TextBox.Text) = false Then
			IP_ListBox.Items.Add(IP_TextBox.Text)
			IPlist.Add(IP_TextBox.Text)
		End If
		
		IP_TextBox.Text = My.Settings.BaseIPString
		IP_TextBox.Focus()
		IP_TextBox.Select(IP_TextBox.Text.Length, 0)
	End Sub

	Private Sub Remove_Button_Click() Handles Remove_Button.Click
		If IP_ListBox.SelectedItems.Count <> 0 Then
			Dim index = IP_ListBox.SelectedIndex
			IP_ListBox.Items.RemoveAt(index)
			IPlist.RemoveAt(index)
		End If
	End Sub

	Private Sub ClearALl_Button_Click() Handles ClearALl_Button.Click
		IP_ListBox.Items.Clear()
		IPlist.Clear
	End Sub

	Private Sub Run_Button_Click() Handles Run_Button.Click
		DialogResult = DialogResult.OK
		Close()
	End Sub

	Private Sub Cancel_Button_Click() Handles Cancel_Button.Click
		DialogResult = DialogResult.Cancel
		Close()
	End Sub

	Private Sub MyBase_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
		' ----------------
		' Double Key Combo
		' ----------------
		If e.Control AndAlso e.KeyCode = Keys.Enter
			Call Run_Button_Click()

		' ----------------
		' Single Key Combo
		' ----------------
		Else If e.KeyCode = Keys.Enter Then
			Call Add_Button_Click()

		Else If e.KeyCode = Keys.Delete Then
			Call Remove_Button_Click()
		End If
	End Sub

End Class