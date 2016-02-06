<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
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
        Me.txtIncoming = New System.Windows.Forms.TextBox()
        Me.txtOutgoing = New System.Windows.Forms.TextBox()
        Me.txtLayout = New System.Windows.Forms.TextBox()
        Me.txtFlood = New System.Windows.Forms.TextBox()
        Me.lblIncoming = New System.Windows.Forms.Label()
        Me.lblOutgoing = New System.Windows.Forms.Label()
        Me.lblLayoutMap = New System.Windows.Forms.Label()
        Me.lblFloodMap = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.picMaze = New System.Windows.Forms.PictureBox()
        Me.lblMaze = New System.Windows.Forms.Label()
        Me.cmdManualStart = New System.Windows.Forms.Button()
        Me.cmdFastrun = New System.Windows.Forms.Button()
        Me.cboDisplay = New System.Windows.Forms.ComboBox()
        CType(Me.picMaze, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtIncoming
        '
        Me.txtIncoming.BackColor = System.Drawing.Color.Black
        Me.txtIncoming.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtIncoming.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtIncoming.ForeColor = System.Drawing.Color.White
        Me.txtIncoming.Location = New System.Drawing.Point(7, 64)
        Me.txtIncoming.Margin = New System.Windows.Forms.Padding(2)
        Me.txtIncoming.Multiline = True
        Me.txtIncoming.Name = "txtIncoming"
        Me.txtIncoming.ReadOnly = True
        Me.txtIncoming.Size = New System.Drawing.Size(150, 265)
        Me.txtIncoming.TabIndex = 0
        '
        'txtOutgoing
        '
        Me.txtOutgoing.BackColor = System.Drawing.Color.Black
        Me.txtOutgoing.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtOutgoing.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtOutgoing.ForeColor = System.Drawing.Color.White
        Me.txtOutgoing.Location = New System.Drawing.Point(7, 353)
        Me.txtOutgoing.Margin = New System.Windows.Forms.Padding(2)
        Me.txtOutgoing.Multiline = True
        Me.txtOutgoing.Name = "txtOutgoing"
        Me.txtOutgoing.ReadOnly = True
        Me.txtOutgoing.Size = New System.Drawing.Size(150, 265)
        Me.txtOutgoing.TabIndex = 1
        '
        'txtLayout
        '
        Me.txtLayout.BackColor = System.Drawing.Color.Black
        Me.txtLayout.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtLayout.Font = New System.Drawing.Font("Courier New", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLayout.ForeColor = System.Drawing.Color.White
        Me.txtLayout.Location = New System.Drawing.Point(159, 64)
        Me.txtLayout.Margin = New System.Windows.Forms.Padding(2)
        Me.txtLayout.Multiline = True
        Me.txtLayout.Name = "txtLayout"
        Me.txtLayout.ReadOnly = True
        Me.txtLayout.Size = New System.Drawing.Size(470, 265)
        Me.txtLayout.TabIndex = 2
        '
        'txtFlood
        '
        Me.txtFlood.BackColor = System.Drawing.Color.Black
        Me.txtFlood.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtFlood.Font = New System.Drawing.Font("Courier New", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtFlood.ForeColor = System.Drawing.Color.White
        Me.txtFlood.Location = New System.Drawing.Point(159, 353)
        Me.txtFlood.Margin = New System.Windows.Forms.Padding(2)
        Me.txtFlood.Multiline = True
        Me.txtFlood.Name = "txtFlood"
        Me.txtFlood.ReadOnly = True
        Me.txtFlood.Size = New System.Drawing.Size(470, 265)
        Me.txtFlood.TabIndex = 3
        '
        'lblIncoming
        '
        Me.lblIncoming.AutoSize = True
        Me.lblIncoming.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblIncoming.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblIncoming.ForeColor = System.Drawing.Color.White
        Me.lblIncoming.Location = New System.Drawing.Point(7, 42)
        Me.lblIncoming.MinimumSize = New System.Drawing.Size(150, 20)
        Me.lblIncoming.Name = "lblIncoming"
        Me.lblIncoming.Size = New System.Drawing.Size(150, 20)
        Me.lblIncoming.TabIndex = 4
        Me.lblIncoming.Text = "Incoming Request"
        Me.lblIncoming.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblOutgoing
        '
        Me.lblOutgoing.AutoSize = True
        Me.lblOutgoing.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblOutgoing.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOutgoing.ForeColor = System.Drawing.Color.White
        Me.lblOutgoing.Location = New System.Drawing.Point(7, 331)
        Me.lblOutgoing.MinimumSize = New System.Drawing.Size(150, 20)
        Me.lblOutgoing.Name = "lblOutgoing"
        Me.lblOutgoing.Size = New System.Drawing.Size(150, 20)
        Me.lblOutgoing.TabIndex = 5
        Me.lblOutgoing.Text = "Outgoing Request"
        Me.lblOutgoing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblLayoutMap
        '
        Me.lblLayoutMap.AutoSize = True
        Me.lblLayoutMap.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblLayoutMap.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLayoutMap.ForeColor = System.Drawing.Color.White
        Me.lblLayoutMap.Location = New System.Drawing.Point(159, 42)
        Me.lblLayoutMap.MinimumSize = New System.Drawing.Size(470, 20)
        Me.lblLayoutMap.Name = "lblLayoutMap"
        Me.lblLayoutMap.Size = New System.Drawing.Size(470, 20)
        Me.lblLayoutMap.TabIndex = 6
        Me.lblLayoutMap.Text = "SolverPath Layout Map"
        Me.lblLayoutMap.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblFloodMap
        '
        Me.lblFloodMap.AutoSize = True
        Me.lblFloodMap.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblFloodMap.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFloodMap.ForeColor = System.Drawing.Color.White
        Me.lblFloodMap.Location = New System.Drawing.Point(159, 331)
        Me.lblFloodMap.MinimumSize = New System.Drawing.Size(470, 20)
        Me.lblFloodMap.Name = "lblFloodMap"
        Me.lblFloodMap.Size = New System.Drawing.Size(470, 20)
        Me.lblFloodMap.TabIndex = 7
        Me.lblFloodMap.Text = "SolverPath Flood Map"
        Me.lblFloodMap.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.BackColor = System.Drawing.Color.Transparent
        Me.lblTitle.Font = New System.Drawing.Font("Arial", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(3, 9)
        Me.lblTitle.MinimumSize = New System.Drawing.Size(1000, 0)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(1000, 22)
        Me.lblTitle.TabIndex = 8
        Me.lblTitle.Text = "MDP Obstacle Avoidance && Path Finder Module"
        '
        'picMaze
        '
        Me.picMaze.BackColor = System.Drawing.Color.Black
        Me.picMaze.Location = New System.Drawing.Point(631, 64)
        Me.picMaze.Name = "picMaze"
        Me.picMaze.Size = New System.Drawing.Size(370, 554)
        Me.picMaze.TabIndex = 9
        Me.picMaze.TabStop = False
        '
        'lblMaze
        '
        Me.lblMaze.AutoSize = True
        Me.lblMaze.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lblMaze.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaze.ForeColor = System.Drawing.Color.White
        Me.lblMaze.Location = New System.Drawing.Point(631, 42)
        Me.lblMaze.MinimumSize = New System.Drawing.Size(370, 20)
        Me.lblMaze.Name = "lblMaze"
        Me.lblMaze.Size = New System.Drawing.Size(370, 20)
        Me.lblMaze.TabIndex = 10
        Me.lblMaze.Text = "Layout Diagram"
        Me.lblMaze.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cmdManualStart
        '
        Me.cmdManualStart.Location = New System.Drawing.Point(861, 9)
        Me.cmdManualStart.Name = "cmdManualStart"
        Me.cmdManualStart.Size = New System.Drawing.Size(140, 30)
        Me.cmdManualStart.TabIndex = 11
        Me.cmdManualStart.Text = "Manual Start"
        Me.cmdManualStart.UseVisualStyleBackColor = True
        '
        'cmdFastrun
        '
        Me.cmdFastrun.Location = New System.Drawing.Point(715, 9)
        Me.cmdFastrun.Name = "cmdFastrun"
        Me.cmdFastrun.Size = New System.Drawing.Size(140, 30)
        Me.cmdFastrun.TabIndex = 12
        Me.cmdFastrun.Text = "Fastrun Start"
        Me.cmdFastrun.UseVisualStyleBackColor = True
        '
        'cboDisplay
        '
        Me.cboDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDisplay.FormattingEnabled = True
        Me.cboDisplay.Items.AddRange(New Object() {"Combined Layout Map", "Explored String", "Explored Hex", "Blocked String", "Blocked Hex"})
        Me.cboDisplay.Location = New System.Drawing.Point(486, 41)
        Me.cboDisplay.Name = "cboDisplay"
        Me.cboDisplay.Size = New System.Drawing.Size(143, 21)
        Me.cboDisplay.TabIndex = 13
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.DimGray
        Me.ClientSize = New System.Drawing.Size(1193, 626)
        Me.Controls.Add(Me.cboDisplay)
        Me.Controls.Add(Me.cmdFastrun)
        Me.Controls.Add(Me.cmdManualStart)
        Me.Controls.Add(Me.lblMaze)
        Me.Controls.Add(Me.picMaze)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.lblFloodMap)
        Me.Controls.Add(Me.lblLayoutMap)
        Me.Controls.Add(Me.lblOutgoing)
        Me.Controls.Add(Me.lblIncoming)
        Me.Controls.Add(Me.txtFlood)
        Me.Controls.Add(Me.txtLayout)
        Me.Controls.Add(Me.txtOutgoing)
        Me.Controls.Add(Me.txtIncoming)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.MaximizeBox = False
        Me.Name = "frmMain"
        Me.Text = "MDP Optimized Algo"
        CType(Me.picMaze, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtIncoming As System.Windows.Forms.TextBox
    Friend WithEvents txtOutgoing As System.Windows.Forms.TextBox
    Friend WithEvents txtLayout As System.Windows.Forms.TextBox
    Friend WithEvents txtFlood As System.Windows.Forms.TextBox
    Friend WithEvents lblIncoming As System.Windows.Forms.Label
    Friend WithEvents lblOutgoing As System.Windows.Forms.Label
    Friend WithEvents lblLayoutMap As System.Windows.Forms.Label
    Friend WithEvents lblFloodMap As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents picMaze As System.Windows.Forms.PictureBox
    Friend WithEvents lblMaze As System.Windows.Forms.Label
    Friend WithEvents cmdManualStart As System.Windows.Forms.Button
    Friend WithEvents cmdFastrun As System.Windows.Forms.Button
    Friend WithEvents cboDisplay As System.Windows.Forms.ComboBox

End Class
