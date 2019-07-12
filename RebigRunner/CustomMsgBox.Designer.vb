<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CustomMsgBox
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
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Message_RichTextBox = New System.Windows.Forms.RichTextBox()
		Me.SuspendLayout
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left),System.Windows.Forms.AnchorStyles)
		Me.OK_Button.Location = New System.Drawing.Point(195, 153)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(75, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		Me.OK_Button.UseVisualStyleBackColor = true
		'
		'Message_RichTextBox
		'
		Me.Message_RichTextBox.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
		Me.Message_RichTextBox.BackColor = System.Drawing.SystemColors.Control
		Me.Message_RichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me.Message_RichTextBox.DetectUrls = false
		Me.Message_RichTextBox.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Message_RichTextBox.Location = New System.Drawing.Point(12, 12)
		Me.Message_RichTextBox.Name = "Message_RichTextBox"
		Me.Message_RichTextBox.ReadOnly = true
		Me.Message_RichTextBox.Size = New System.Drawing.Size(441, 135)
		Me.Message_RichTextBox.TabIndex = 1
		Me.Message_RichTextBox.Text = ""
		'
		'CustomMsgBox
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(465, 188)
		Me.Controls.Add(Me.Message_RichTextBox)
		Me.Controls.Add(Me.OK_Button)
		Me.Name = "CustomMsgBox"
		Me.ShowIcon = false
		Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Custom Msg Box"
		Me.ResumeLayout(false)

End Sub

	Friend WithEvents OK_Button As Button
	Friend WithEvents Message_RichTextBox As RichTextBox
End Class
