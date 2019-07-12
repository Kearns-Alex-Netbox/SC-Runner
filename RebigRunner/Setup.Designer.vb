<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Setup
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Setup))
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.BaseIPString = New System.Windows.Forms.TextBox()
		Me.BrowseCommandFolder_B = New System.Windows.Forms.Button()
		Me.CommandFolder_TB = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.BrowseSaveResults_B = New System.Windows.Forms.Button()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ResultFolder_TB = New System.Windows.Forms.TextBox()
		Me.SuspendLayout
		'
		'Cancel_Button
		'
		Me.Cancel_Button.AutoSize = true
		Me.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(211, 241)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(68, 30)
		Me.Cancel_Button.TabIndex = 39
		Me.Cancel_Button.Text = "Cancel"
		'
		'OK_Button
		'
		Me.OK_Button.AutoSize = true
		Me.OK_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.OK_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.OK_Button.Location = New System.Drawing.Point(284, 241)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(41, 30)
		Me.OK_Button.TabIndex = 38
		Me.OK_Button.Text = "OK"
		'
		'Label2
		'
		Me.Label2.AutoSize = true
		Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label2.Location = New System.Drawing.Point(10, 186)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(194, 20)
		Me.Label2.TabIndex = 37
		Me.Label2.Text = "Base IP (###.###.###.)"
		'
		'BaseIPString
		'
		Me.BaseIPString.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.BaseIPString.Location = New System.Drawing.Point(14, 209)
		Me.BaseIPString.Name = "BaseIPString"
		Me.BaseIPString.Size = New System.Drawing.Size(311, 26)
		Me.BaseIPString.TabIndex = 36
		'
		'BrowseCommandFolder_B
		'
		Me.BrowseCommandFolder_B.AutoSize = true
		Me.BrowseCommandFolder_B.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.BrowseCommandFolder_B.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.BrowseCommandFolder_B.Location = New System.Drawing.Point(14, 152)
		Me.BrowseCommandFolder_B.Name = "BrowseCommandFolder_B"
		Me.BrowseCommandFolder_B.Size = New System.Drawing.Size(72, 30)
		Me.BrowseCommandFolder_B.TabIndex = 35
		Me.BrowseCommandFolder_B.Text = "Browse"
		Me.BrowseCommandFolder_B.UseVisualStyleBackColor = true
		'
		'CommandFolder_TB
		'
		Me.CommandFolder_TB.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.CommandFolder_TB.Location = New System.Drawing.Point(14, 120)
		Me.CommandFolder_TB.Name = "CommandFolder_TB"
		Me.CommandFolder_TB.Size = New System.Drawing.Size(311, 26)
		Me.CommandFolder_TB.TabIndex = 34
		'
		'Label1
		'
		Me.Label1.AutoSize = true
		Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label1.Location = New System.Drawing.Point(10, 97)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(145, 20)
		Me.Label1.TabIndex = 33
		Me.Label1.Text = "Command Folder"
		'
		'BrowseSaveResults_B
		'
		Me.BrowseSaveResults_B.AutoSize = true
		Me.BrowseSaveResults_B.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.BrowseSaveResults_B.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.BrowseSaveResults_B.Location = New System.Drawing.Point(14, 63)
		Me.BrowseSaveResults_B.Name = "BrowseSaveResults_B"
		Me.BrowseSaveResults_B.Size = New System.Drawing.Size(72, 30)
		Me.BrowseSaveResults_B.TabIndex = 32
		Me.BrowseSaveResults_B.Text = "Browse"
		Me.BrowseSaveResults_B.UseVisualStyleBackColor = true
		'
		'Label3
		'
		Me.Label3.AutoSize = true
		Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label3.Location = New System.Drawing.Point(10, 8)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(141, 20)
		Me.Label3.TabIndex = 31
		Me.Label3.Text = "Save Results to:"
		'
		'ResultFolder_TB
		'
		Me.ResultFolder_TB.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.ResultFolder_TB.Location = New System.Drawing.Point(14, 31)
		Me.ResultFolder_TB.Name = "ResultFolder_TB"
		Me.ResultFolder_TB.Size = New System.Drawing.Size(311, 26)
		Me.ResultFolder_TB.TabIndex = 30
		'
		'Setup
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(334, 278)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.BaseIPString)
		Me.Controls.Add(Me.BrowseCommandFolder_B)
		Me.Controls.Add(Me.CommandFolder_TB)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.BrowseSaveResults_B)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.ResultFolder_TB)
		Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
		Me.Name = "Setup"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Setup"
		Me.ResumeLayout(false)
		Me.PerformLayout

End Sub

	Friend WithEvents Cancel_Button As Button
	Friend WithEvents OK_Button As Button
	Friend WithEvents Label2 As Label
	Friend WithEvents BaseIPString As TextBox
	Friend WithEvents BrowseCommandFolder_B As Button
	Friend WithEvents CommandFolder_TB As TextBox
	Friend WithEvents Label1 As Label
	Friend WithEvents BrowseSaveResults_B As Button
	Friend WithEvents Label3 As Label
	Friend WithEvents ResultFolder_TB As TextBox
End Class
