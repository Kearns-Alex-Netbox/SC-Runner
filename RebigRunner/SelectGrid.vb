Public Class SelectGrid
	Private Sub SelectGrid_Load() Handles MyBase.Load
		CenterToParent()
	End Sub

	Private Sub B_2x3_Click() Handles B_2x3.Click
		Dim DoGrid2x3 As New Grid2x3
		DoGrid2x3.Show()
		Close()
	End Sub

	Private Sub B_3x3_Click() Handles B_3x3.Click
		Dim DoGrid3x3 As New Grid3x3
		DoGrid3x3.Show()
		Close()
	End Sub

End Class