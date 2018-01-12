<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.Label4 = New System.Windows.Forms.Label()
        Me.tlPath = New System.Windows.Forms.TextBox()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.FullProcCheck = New System.Windows.Forms.CheckBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.tSPass = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.tSName = New System.Windows.Forms.TextBox()
        Me.tDBname = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.tSUser = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.tFTPpass = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.tFTPsrv = New System.Windows.Forms.TextBox()
        Me.tFTPpath = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.tFTPusr = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.LogBox = New System.Windows.Forms.TextBox()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel2 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.chkUseFtp = New System.Windows.Forms.CheckBox()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.TabPage3.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(16, 214)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(82, 13)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Local Files Path"
        '
        'tlPath
        '
        Me.tlPath.Location = New System.Drawing.Point(104, 207)
        Me.tlPath.Name = "tlPath"
        Me.tlPath.Size = New System.Drawing.Size(240, 20)
        Me.tlPath.TabIndex = 6
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPage3)
        Me.TabControl1.Location = New System.Drawing.Point(12, 12)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(453, 279)
        Me.TabControl1.TabIndex = 12
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.Button2)
        Me.TabPage1.Controls.Add(Me.FullProcCheck)
        Me.TabPage1.Controls.Add(Me.GroupBox1)
        Me.TabPage1.Controls.Add(Me.tSPass)
        Me.TabPage1.Controls.Add(Me.Label4)
        Me.TabPage1.Controls.Add(Me.Label5)
        Me.TabPage1.Controls.Add(Me.tlPath)
        Me.TabPage1.Controls.Add(Me.tSName)
        Me.TabPage1.Controls.Add(Me.tDBname)
        Me.TabPage1.Controls.Add(Me.Label1)
        Me.TabPage1.Controls.Add(Me.tSUser)
        Me.TabPage1.Controls.Add(Me.Button1)
        Me.TabPage1.Controls.Add(Me.Label2)
        Me.TabPage1.Controls.Add(Me.Label3)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(445, 253)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "SQL server settings"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(347, 207)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(92, 40)
        Me.Button2.TabIndex = 16
        Me.Button2.Text = "Test API"
        Me.Button2.UseVisualStyleBackColor = True
        Me.Button2.Visible = False
        '
        'FullProcCheck
        '
        Me.FullProcCheck.AutoSize = True
        Me.FullProcCheck.Checked = True
        Me.FullProcCheck.CheckState = System.Windows.Forms.CheckState.Checked
        Me.FullProcCheck.Location = New System.Drawing.Point(104, 156)
        Me.FullProcCheck.Name = "FullProcCheck"
        Me.FullProcCheck.Size = New System.Drawing.Size(97, 17)
        Me.FullProcCheck.TabIndex = 15
        Me.FullProcCheck.Text = "Full Processing"
        Me.FullProcCheck.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.RadioButton2)
        Me.GroupBox1.Controls.Add(Me.RadioButton1)
        Me.GroupBox1.Location = New System.Drawing.Point(104, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(237, 70)
        Me.GroupBox1.TabIndex = 14
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Authentification Mode"
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Checked = True
        Me.RadioButton2.Location = New System.Drawing.Point(7, 42)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(80, 17)
        Me.RadioButton2.TabIndex = 1
        Me.RadioButton2.TabStop = True
        Me.RadioButton2.Text = "SQL Server"
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Location = New System.Drawing.Point(7, 19)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(69, 17)
        Me.RadioButton1.TabIndex = 0
        Me.RadioButton1.Text = "Windows"
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'tSPass
        '
        Me.tSPass.Location = New System.Drawing.Point(104, 129)
        Me.tSPass.Name = "tSPass"
        Me.tSPass.Size = New System.Drawing.Size(240, 20)
        Me.tSPass.TabIndex = 4
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(45, 188)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(51, 13)
        Me.Label5.TabIndex = 11
        Me.Label5.Text = "DB name"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'tSName
        '
        Me.tSName.Location = New System.Drawing.Point(104, 77)
        Me.tSName.Name = "tSName"
        Me.tSName.Size = New System.Drawing.Size(240, 20)
        Me.tSName.TabIndex = 0
        '
        'tDBname
        '
        Me.tDBname.Location = New System.Drawing.Point(104, 181)
        Me.tDBname.Name = "tDBname"
        Me.tDBname.Size = New System.Drawing.Size(240, 20)
        Me.tDBname.TabIndex = 10
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(60, 84)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Server"
        '
        'tSUser
        '
        Me.tSUser.Location = New System.Drawing.Point(104, 103)
        Me.tSUser.Name = "tSUser"
        Me.tSUser.Size = New System.Drawing.Size(240, 20)
        Me.tSUser.TabIndex = 2
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(347, 6)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(92, 67)
        Me.Button1.TabIndex = 8
        Me.Button1.Text = "FTP => SQL "
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(69, 110)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(29, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "User"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(45, 136)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Password"
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.chkUseFtp)
        Me.TabPage2.Controls.Add(Me.tFTPpass)
        Me.TabPage2.Controls.Add(Me.Label6)
        Me.TabPage2.Controls.Add(Me.tFTPsrv)
        Me.TabPage2.Controls.Add(Me.tFTPpath)
        Me.TabPage2.Controls.Add(Me.Label7)
        Me.TabPage2.Controls.Add(Me.tFTPusr)
        Me.TabPage2.Controls.Add(Me.Label8)
        Me.TabPage2.Controls.Add(Me.Label9)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(445, 253)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "FTP settings"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'tFTPpass
        '
        Me.tFTPpass.Location = New System.Drawing.Point(102, 73)
        Me.tFTPpass.Name = "tFTPpass"
        Me.tFTPpass.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.tFTPpass.Size = New System.Drawing.Size(300, 20)
        Me.tFTPpass.TabIndex = 16
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(34, 132)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(56, 13)
        Me.Label6.TabIndex = 19
        Me.Label6.Text = "FTP folder"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'tFTPsrv
        '
        Me.tFTPsrv.Location = New System.Drawing.Point(102, 21)
        Me.tFTPsrv.Name = "tFTPsrv"
        Me.tFTPsrv.Size = New System.Drawing.Size(300, 20)
        Me.tFTPsrv.TabIndex = 12
        '
        'tFTPpath
        '
        Me.tFTPpath.Location = New System.Drawing.Point(102, 125)
        Me.tFTPpath.Name = "tFTPpath"
        Me.tFTPpath.Size = New System.Drawing.Size(300, 20)
        Me.tFTPpath.TabIndex = 18
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(58, 28)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(38, 13)
        Me.Label7.TabIndex = 13
        Me.Label7.Text = "Server"
        '
        'tFTPusr
        '
        Me.tFTPusr.Location = New System.Drawing.Point(102, 47)
        Me.tFTPusr.Name = "tFTPusr"
        Me.tFTPusr.Size = New System.Drawing.Size(300, 20)
        Me.tFTPusr.TabIndex = 14
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(67, 54)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(29, 13)
        Me.Label8.TabIndex = 15
        Me.Label8.Text = "User"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(43, 80)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(53, 13)
        Me.Label9.TabIndex = 17
        Me.Label9.Text = "Password"
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.LogBox)
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage3.Size = New System.Drawing.Size(445, 253)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "Log"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'LogBox
        '
        Me.LogBox.Location = New System.Drawing.Point(6, 6)
        Me.LogBox.Multiline = True
        Me.LogBox.Name = "LogBox"
        Me.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.LogBox.Size = New System.Drawing.Size(433, 241)
        Me.LogBox.TabIndex = 0
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel2})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 293)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(471, 22)
        Me.StatusStrip1.TabIndex = 13
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripProgressBar1
        '
        Me.ToolStripProgressBar1.AutoToolTip = True
        Me.ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        Me.ToolStripProgressBar1.Size = New System.Drawing.Size(150, 16)
        '
        'ToolStripStatusLabel2
        '
        Me.ToolStripStatusLabel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        Me.ToolStripStatusLabel2.Size = New System.Drawing.Size(16, 17)
        Me.ToolStripStatusLabel2.Text = "..."
        '
        'chkUseFtp
        '
        Me.chkUseFtp.AutoSize = True
        Me.chkUseFtp.Checked = True
        Me.chkUseFtp.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseFtp.Location = New System.Drawing.Point(102, 160)
        Me.chkUseFtp.Name = "chkUseFtp"
        Me.chkUseFtp.Size = New System.Drawing.Size(60, 17)
        Me.chkUseFtp.TabIndex = 20
        Me.chkUseFtp.Text = "UseFtp"
        Me.chkUseFtp.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(471, 315)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.TabControl1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Data Insertion"
        Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage3.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label4 As Label
    Friend WithEvents tlPath As TextBox
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents tSPass As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents tSName As TextBox
    Friend WithEvents tDBname As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents tSUser As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents tFTPpass As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents tFTPsrv As TextBox
    Friend WithEvents tFTPpath As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents tFTPusr As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStripProgressBar1 As ToolStripProgressBar
    Public WithEvents ToolStripStatusLabel2 As ToolStripStatusLabel
    Friend WithEvents TabPage3 As TabPage
    Friend WithEvents LogBox As TextBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents FullProcCheck As CheckBox
    Friend WithEvents Button2 As Button
    Friend WithEvents chkUseFtp As CheckBox
End Class
