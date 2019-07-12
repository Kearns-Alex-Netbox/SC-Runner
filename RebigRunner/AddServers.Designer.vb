<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AddServers
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AddServers))
		Me.ClearALl_Button = New System.Windows.Forms.Button()
		Me.Remove_Button = New System.Windows.Forms.Button()
		Me.Add_Button = New System.Windows.Forms.Button()
		Me.IP_ListBox = New System.Windows.Forms.ListBox()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.Run_Button = New System.Windows.Forms.Button()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.IP_TextBox = New System.Windows.Forms.TextBox()
		Me.FixedIP_RadioButton = New System.Windows.Forms.RadioButton()
		Me.DHCP_RadioButton = New System.Windows.Forms.RadioButton()
		Me.SuspendLayout
		'
		'ClearALl_Button
		'
		Me.ClearALl_Button.AutoSize = true
		Me.ClearALl_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.ClearALl_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.ClearALl_Button.Location = New System.Drawing.Point(331, 101)
		Me.ClearALl_Button.Name = "ClearALl_Button"
		Me.ClearALl_Button.Size = New System.Drawing.Size(77, 30)
		Me.ClearALl_Button.TabIndex = 13
		Me.ClearALl_Button.Text = "Clear All"
		'
		'Remove_Button
		'
		Me.Remove_Button.AutoSize = true
		Me.Remove_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.Remove_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Remove_Button.Location = New System.Drawing.Point(331, 65)
		Me.Remove_Button.Name = "Remove_Button"
		Me.Remove_Button.Size = New System.Drawing.Size(78, 30)
		Me.Remove_Button.TabIndex = 12
		Me.Remove_Button.Text = "Remove"
		'
		'Add_Button
		'
		Me.Add_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.Add_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Add_Button.Location = New System.Drawing.Point(331, 29)
		Me.Add_Button.Name = "Add_Button"
		Me.Add_Button.Size = New System.Drawing.Size(78, 30)
		Me.Add_Button.TabIndex = 10
		Me.Add_Button.Text = "Add"
		'
		'IP_ListBox
		'
		Me.IP_ListBox.Font = New System.Drawing.Font("Consolas", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.IP_ListBox.FormattingEnabled = true
		Me.IP_ListBox.ItemHeight = 19
		Me.IP_ListBox.Location = New System.Drawing.Point(14, 65)
		Me.IP_ListBox.Name = "IP_ListBox"
		Me.IP_ListBox.Size = New System.Drawing.Size(311, 194)
		Me.IP_ListBox.TabIndex = 11
		'
		'Cancel_Button
		'
		Me.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(331, 229)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(78, 30)
		Me.Cancel_Button.TabIndex = 15
		Me.Cancel_Button.Text = "Cancel"
		'
		'Run_Button
		'
		Me.Run_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.Run_Button.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Run_Button.Location = New System.Drawing.Point(331, 193)
		Me.Run_Button.Name = "Run_Button"
		Me.Run_Button.Size = New System.Drawing.Size(78, 30)
		Me.Run_Button.TabIndex = 14
		Me.Run_Button.Text = "Run"
		'
		'Label3
		'
		Me.Label3.AutoSize = true
		Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label3.Location = New System.Drawing.Point(10, 9)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(154, 20)
		Me.Label3.TabIndex = 8
		Me.Label3.Text = "Server IP Address"
		'
		'IP_TextBox
		'
		Me.IP_TextBox.Font = New System.Drawing.Font("Consolas", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.IP_TextBox.Location = New System.Drawing.Point(14, 32)
		Me.IP_TextBox.Name = "IP_TextBox"
		Me.IP_TextBox.Size = New System.Drawing.Size(311, 26)
		Me.IP_TextBox.TabIndex = 9
		'
		'FixedIP_RadioButton
		'
		Me.FixedIP_RadioButton.AutoSize = true
		Me.FixedIP_RadioButton.Location = New System.Drawing.Point(170, 12)
		Me.FixedIP_RadioButton.Name = "FixedIP_RadioButton"
		Me.FixedIP_RadioButton.Size = New System.Drawing.Size(50, 17)
		Me.FixedIP_RadioButton.TabIndex = 16
		Me.FixedIP_RadioButton.Text = "Fixed"
		Me.FixedIP_RadioButton.UseVisualStyleBackColor = true
		'
		'DHCP_RadioButton
		'
		Me.DHCP_RadioButton.AutoSize = true
		Me.DHCP_RadioButton.Checked = true
		Me.DHCP_RadioButton.Location = New System.Drawing.Point(226, 12)
		Me.DHCP_RadioButton.Name = "DHCP_RadioButton"
		Me.DHCP_RadioButton.Size = New System.Drawing.Size(55, 17)
		Me.DHCP_RadioButton.TabIndex = 17
		Me.DHCP_RadioButton.TabStop = true
		Me.DHCP_RadioButton.Text = "DHCP"
		Me.DHCP_RadioButton.UseVisualStyleBackColor = true
		'
		'AddServers
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(419, 269)
		Me.Controls.Add(Me.DHCP_RadioButton)
		Me.Controls.Add(Me.FixedIP_RadioButton)
		Me.Controls.Add(Me.ClearALl_Button)
		Me.Controls.Add(Me.Remove_Button)
		Me.Controls.Add(Me.Add_Button)
		Me.Controls.Add(Me.IP_ListBox)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Controls.Add(Me.Run_Button)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.IP_TextBox)
		Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
		Me.Name = "AddServers"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Add Servers"
		Me.ResumeLayout(false)
		Me.PerformLayout

End Sub

	Friend WithEvents ClearALl_Button As Button
	Friend WithEvents Remove_Button As Button
	Friend WithEvents Add_Button As Button
	Friend WithEvents IP_ListBox As ListBox
	Friend WithEvents Cancel_Button As Button
	Friend WithEvents Run_Button As Button
	Friend WithEvents Label3 As Label
	Friend WithEvents IP_TextBox As TextBox
	Friend WithEvents FixedIP_RadioButton As RadioButton
	Friend WithEvents DHCP_RadioButton As RadioButton
End Class
