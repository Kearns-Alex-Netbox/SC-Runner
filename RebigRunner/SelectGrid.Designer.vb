<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SelectGrid
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SelectGrid))
		Me.B_2x3 = New System.Windows.Forms.Button()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.B_3x3 = New System.Windows.Forms.Button()
		Me.SuspendLayout
		'
		'B_2x3
		'
		Me.B_2x3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.B_2x3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.B_2x3.Location = New System.Drawing.Point(25, 41)
		Me.B_2x3.Name = "B_2x3"
		Me.B_2x3.Size = New System.Drawing.Size(63, 30)
		Me.B_2x3.TabIndex = 50
		Me.B_2x3.Text = "2 X 3"
		Me.B_2x3.UseVisualStyleBackColor = true
		'
		'Label3
		'
		Me.Label3.AutoSize = true
		Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 18!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label3.Location = New System.Drawing.Point(19, 9)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(144, 29)
		Me.Label3.TabIndex = 55
		Me.Label3.Text = "Select Grid"
		'
		'B_3x3
		'
		Me.B_3x3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.B_3x3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.B_3x3.Location = New System.Drawing.Point(94, 41)
		Me.B_3x3.Name = "B_3x3"
		Me.B_3x3.Size = New System.Drawing.Size(63, 30)
		Me.B_3x3.TabIndex = 56
		Me.B_3x3.Text = "3 X 3"
		Me.B_3x3.UseVisualStyleBackColor = true
		'
		'SelectGrid
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(183, 80)
		Me.Controls.Add(Me.B_3x3)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.B_2x3)
		Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
		Me.Name = "SelectGrid"
		Me.Text = "Select Grid"
		Me.ResumeLayout(false)
		Me.PerformLayout

End Sub

	Friend WithEvents B_2x3 As Button
	Friend WithEvents Label3 As Label
	Friend WithEvents B_3x3 As Button
End Class
