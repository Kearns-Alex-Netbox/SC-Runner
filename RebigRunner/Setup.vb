Public Class Setup

	Private Sub Setup_Load() Handles MyBase.Load
		ResultFolder_TB.Text = My.Settings.SaveResultsLocation
		CommandFolder_TB.Text = My.Settings.CommandDirectory
		BaseIPString.Text = My.Settings.BaseIPString
	End Sub

	Private Sub BrowseDirectoryButton_Click() Handles BrowseSaveResults_B.Click
		OpenFolderLocation(ResultFolder_TB.Text, "Select folder - Save Results", ResultFolder_TB.Text)
	End Sub

	Private Sub BrowseFile_Click() Handles BrowseCommandFolder_B.Click
		OpenFolderLocation(CommandFolder_TB.Text, "Select folder - Command Scripts", CommandFolder_TB.Text)
	End Sub

	Private Sub OK_Button_Click() Handles OK_Button.Click
		My.Settings.SaveResultsLocation = ResultFolder_TB.Text
		My.Settings.CommandDirectory = CommandFolder_TB.Text
		My.Settings.BaseIPString = BaseIPString.Text
		My.Settings.Save()
		Close()
	End Sub

	Private Sub Cancel_Button_Click() Handles Cancel_Button.Click
		Close()
	End Sub

	Private Sub OpenFolderLocation(ByVal root As String, ByVal windowTitle As String, ByRef finalLocation As String)
		' method: https://stackoverflow.com/questions/32370524/setting-root-folder-for-folderbrowser
		Using obj As New OpenFileDialog
			obj.Filter = "foldersOnly|*.none"
			obj.CheckFileExists = False
			obj.CheckPathExists = False
			obj.InitialDirectory = root
			obj.CustomPlaces.Add("\\Server1\EngineeringReleased\Utilities")
			obj.CustomPlaces.Add("C:")
			obj.Title = windowTitle
			obj.FileName = "OpenFldrPath"

			' make sure that the path exists
			If IO.Directory.Exists(root) = False Then
				obj.InitialDirectory = "C:"
			End If

			If obj.ShowDialog = Windows.Forms.DialogResult.OK Then
				finalLocation = IO.Directory.GetParent(obj.FileName).FullName
			End If
		End Using
	End Sub

End Class