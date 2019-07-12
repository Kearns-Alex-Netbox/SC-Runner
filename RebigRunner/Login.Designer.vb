<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Login
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Login))
		Me.Bypass_CheckBox = New System.Windows.Forms.CheckBox()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.B_Exit = New System.Windows.Forms.Button()
		Me.TB_Password = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.TB_User = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.B_Login = New System.Windows.Forms.Button()
		Me.L_Version = New System.Windows.Forms.Label()
		Me.L_Database = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		CType(Me.PictureBox1,System.ComponentModel.ISupportInitialize).BeginInit
		Me.SuspendLayout
		'
		'Bypass_CheckBox
		'
		Me.Bypass_CheckBox.AutoSize = true
		Me.Bypass_CheckBox.Location = New System.Drawing.Point(79, 193)
		Me.Bypass_CheckBox.Name = "Bypass_CheckBox"
		Me.Bypass_CheckBox.Size = New System.Drawing.Size(60, 17)
		Me.Bypass_CheckBox.TabIndex = 3
		Me.Bypass_CheckBox.Text = "Bypass"
		Me.Bypass_CheckBox.UseVisualStyleBackColor = true
		'
		'Label3
		'
		Me.Label3.AutoSize = true
		Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 18!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label3.Location = New System.Drawing.Point(41, 59)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(204, 29)
		Me.Label3.TabIndex = 9
		Me.Label3.Text = "Runner Program"
		'
		'B_Exit
		'
		Me.B_Exit.AutoSize = true
		Me.B_Exit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.B_Exit.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.B_Exit.Location = New System.Drawing.Point(221, 184)
		Me.B_Exit.Name = "B_Exit"
		Me.B_Exit.Size = New System.Drawing.Size(49, 30)
		Me.B_Exit.TabIndex = 4
		Me.B_Exit.Text = "Exit"
		Me.B_Exit.UseVisualStyleBackColor = true
		'
		'TB_Password
		'
		Me.TB_Password.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.TB_Password.Location = New System.Drawing.Point(110, 152)
		Me.TB_Password.Name = "TB_Password"
		Me.TB_Password.Size = New System.Drawing.Size(160, 26)
		Me.TB_Password.TabIndex = 1
		Me.TB_Password.UseSystemPasswordChar = true
		'
		'Label2
		'
		Me.Label2.AutoSize = true
		Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label2.Location = New System.Drawing.Point(22, 155)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(82, 20)
		Me.Label2.TabIndex = 6
		Me.Label2.Text = "Password:"
		'
		'TB_User
		'
		Me.TB_User.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.TB_User.Location = New System.Drawing.Point(110, 120)
		Me.TB_User.Name = "TB_User"
		Me.TB_User.Size = New System.Drawing.Size(160, 26)
		Me.TB_User.TabIndex = 0
		'
		'Label1
		'
		Me.Label1.AutoSize = true
		Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Label1.Location = New System.Drawing.Point(11, 123)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(93, 20)
		Me.Label1.TabIndex = 7
		Me.Label1.Text = "User Name:"
		'
		'B_Login
		'
		Me.B_Login.AutoSize = true
		Me.B_Login.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.B_Login.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.B_Login.Location = New System.Drawing.Point(152, 184)
		Me.B_Login.Name = "B_Login"
		Me.B_Login.Size = New System.Drawing.Size(63, 30)
		Me.B_Login.TabIndex = 2
		Me.B_Login.Text = "Login"
		Me.B_Login.UseVisualStyleBackColor = true
		'
		'L_Version
		'
		Me.L_Version.AutoSize = true
		Me.L_Version.Location = New System.Drawing.Point(12, 194)
		Me.L_Version.Name = "L_Version"
		Me.L_Version.Size = New System.Drawing.Size(53, 13)
		Me.L_Version.TabIndex = 5
		Me.L_Version.Text = "V: 0.0.0.0"
		'
		'L_Database
		'
		Me.L_Database.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
		Me.L_Database.Font = New System.Drawing.Font("Microsoft Sans Serif", 18!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.L_Database.Location = New System.Drawing.Point(12, 88)
		Me.L_Database.Name = "L_Database"
		Me.L_Database.Size = New System.Drawing.Size(263, 29)
		Me.L_Database.TabIndex = 8
		Me.L_Database.Text = "DATABASE"
		Me.L_Database.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'PictureBox1
		'
		Me.PictureBox1.Image = Global.Runner.My.Resources.Resources.netboxsc
		Me.PictureBox1.InitialImage = Nothing
		Me.PictureBox1.Location = New System.Drawing.Point(44, 12)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(199, 44)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
		Me.PictureBox1.TabIndex = 58
		Me.PictureBox1.TabStop = false
		'
		'Login
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(287, 219)
		Me.Controls.Add(Me.L_Database)
		Me.Controls.Add(Me.PictureBox1)
		Me.Controls.Add(Me.L_Version)
		Me.Controls.Add(Me.Bypass_CheckBox)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.B_Exit)
		Me.Controls.Add(Me.TB_Password)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.TB_User)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.B_Login)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
		Me.MaximizeBox = false
		Me.MinimizeBox = false
		Me.Name = "Login"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "Login"
		CType(Me.PictureBox1,System.ComponentModel.ISupportInitialize).EndInit
		Me.ResumeLayout(false)
		Me.PerformLayout

End Sub

	Friend WithEvents Bypass_CheckBox As CheckBox
	Friend WithEvents Label3 As Label
	Friend WithEvents B_Exit As Button
	Friend WithEvents TB_Password As TextBox
	Friend WithEvents Label2 As Label
	Friend WithEvents TB_User As TextBox
	Friend WithEvents Label1 As Label
	Friend WithEvents B_Login As Button
	Friend WithEvents L_Version As Label
	Friend WithEvents PictureBox1 As PictureBox
	Friend WithEvents L_Database As Label
End Class
