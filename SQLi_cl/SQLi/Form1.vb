Imports System
Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Net.FtpWebRequest
Imports System.Management
Imports System.Runtime.InteropServices
Imports System.Data.SqlClient
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports excel = Microsoft.Office.Interop.Excel
Imports Microsoft.Office



Public Class Form1
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim cl = ""
        Dim UseFtp As Boolean


        cl = getConfigLine()
        If cl <> "" Then
            Dim ctb(9) As String
            ctb = Split(cl, "||")
            On Error Resume Next
            UseFtp = IIf(ctb(5) <> "UseFTP", False, True)

            Me.tSName.Text = ctb(0)
            Me.tDBname.Text = ctb(1)
            Me.tSUser.Text = ctb(2)
            Me.tSPass.Text = ctb(3)
            Me.tlPath.Text = ctb(4)
            Me.chkUseFtp.Checked = UseFtp
            Me.tFTPsrv.Text = ctb(6)
            Me.tFTPusr.Text = ctb(7)
            Me.tFTPpass.Text = ctb(8)
            Me.tFTPpath.Text = ctb(9)
            Me.RadioButton1.Checked = True
            Me.FullProcCheck.Checked = False
            On Error GoTo 0
        End If
        Me.Hide()
        Start()

    End Sub

    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
    Private Sub Start()
        Button1.Enabled = False
        Dim con As SqlConnection
        con = Nothing
        If Trim(Me.tSName.Text) <> "" And Trim(Me.tDBname.Text) <> "" And Trim(Me.tSUser.Text) <> "" Then
            saveConf(Me.tSName.Text & "||" & Me.tDBname.Text & "||" & Me.tSUser.Text & "||" & Me.tSPass.Text & "||" & Me.tlPath.Text & "||" & Me.tFTPsrv.Text & "||" & Me.tFTPusr.Text & "||" & Me.tFTPpass.Text & "||" & Me.tFTPpath.Text)
            con = connectToSQL(Me.tSName.Text, Me.tSUser.Text, Me.tSPass.Text, Me.tDBname.Text, IIf(RadioButton1.Checked = False, "SQL", "Windows"))
            Console.WriteLine(Me.tSName.Text & ", " & Me.tSUser.Text & ", " & Me.tDBname.Text)

        End If


        If Me.chkUseFtp.Checked = True Then
            If Trim(Me.tFTPsrv.Text) <> "" And Trim(Me.tFTPusr.Text) <> "" And Trim(Me.tFTPpass.Text) <> "" And Trim(Me.tFTPpath.Text) <> "" Then
                connectToFTP(Me.tFTPsrv.Text, Me.tFTPusr.Text, Me.tFTPpass.Text, Me.tFTPpath.Text, Me.tlPath.Text)
            End If
        End If
        If Not con Is Nothing Then
            ToolStripStatusLabel2.Text = "..."
            TabControl1.SelectedTab = TabPage3
            processCharges2(Me.tSName.Text, Me.tSUser.Text, Me.tSPass.Text, Me.tDBname.Text, Me.tlPath.Text, con, LogBox, ToolStripProgressBar1, FullProcCheck.Checked)
            ToolStripStatusLabel2.Text = "Processed"
            'processPayments2(Me.tSName.Text, Me.tSUser.Text, Me.tSPass.Text, Me.tDBname.Text, Me.tlPath.Text, con, LogBox, ToolStripProgressBar1)
            'ToolStripStatusLabel2.Text = "Processed"

        End If
        Button1.Enabled = True
        Me.Close()

    End Sub

    Private Sub StatusStrip2_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs)

    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click



        Dim tst As Boolean
        Dim dt As DataTable
        dt = getAPIDataTable("http://176.9.113.46:10003/api/DoctorEx/List", "List")

        Dim respondAPI As String = postAPIRequest("http://176.9.113.46:10003/api/DoctorEx/SaveList", DataTable2JSON(dt, "List"))
        'checkAPIRespond(respondAPI, "Model", "table")
        'checkAPIRespond(respondAPI, "Model", "value")
        'checkAPIRespond(respondAPI, "Model", "v")
        'checkAPIRespond(respondAPI, "Status", "v")
        'testAPI(respondAPI)

        Dim resultAPI As Object
        'GET DoctorEx/List
        resultAPI = getAPIRequest("http://176.9.113.46:10003/api/DoctorEx/List")
        dt = getAPIDataTable("http://176.9.113.46:10003/api/DoctorEx/List", "List")

        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/DoctorEx/SaveList", DataTable2JSON(dt, "List"))
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/DoctorEx/Save", GetJsonFiltered(dt, "List", "id", "1"))

        'InsuranceCompanyEx
        dt = getAPIDataTable("http://176.9.113.46:10003/api/InsuranceCompanyEx/List?clientId=" & 805, "List")
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/InsuranceCompanyEx/SaveList", DataTable2JSON(dt, "List"))
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/InsuranceCompanyEx/Save", GetJsonFiltered(dt, "List", "id", "6001"))

        'client
        resultAPI = checkAPIRespond(getAPIRequest("http://176.9.113.46:10003/api/ClientEx/Get/" & 805), "Model", "value")
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/ClientEx/Save", JsonConvert.SerializeObject(resultAPI, Formatting.Indented))
        resultAPI = checkAPIRespond(getAPIRequest("http://176.9.113.46:10003/api/ClientEx/Exist/" & 805), "Model", "v")

        'InsuranceCompanyGroup/LoadGroups1
        resultAPI = getAPIRequest("http://176.9.113.46:10003/api/InsuranceCompanyGroupEx/LoadGroups1")
        resultAPI = checkAPIRespond(resultAPI, "Message", "v")
        resultAPI = getAPIRequest("http://176.9.113.46:10003/api/InsuranceCompanyGroup/LoadGroups2Ex")

        'PracticeEx/List?clientId={clientId}
        dt = getAPIDataTable("http://176.9.113.46:10003/api/PracticeEx/List?clientId=" & 805, "List")
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/PracticeEx/SaveList", DataTable2JSON(dt, "List"))
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/PracticeEx/Save", GetJsonFiltered(dt, "List", "id", "5022"))

        'PatientEx/List?clientId={clientId} 
        dt = getAPIDataTable("http://176.9.113.46:10003/api/PatientEx/List?clientId=" & 805, "List")
        If dt.Rows.Count > 0 Then
            resultAPI = postAPIRequest("http://176.9.113.46:10003/api/PatientEx/SaveList", DataTable2JSON(dt, "List"))
            resultAPI = postAPIRequest("http://176.9.113.46:10003/api/PatientEx/Save", GetJsonFiltered(dt, "List", "id", "5022"))
        Else
            Dim pat As New Patient
            pat.Access = True
            pat.Id = -1
            pat.AccountID = "A11111"
            pat.DOB = "2016-07-19"
            pat.FirstName = "John"
            pat.MiddleName = "M."
            pat.LastName = "Doe"
            pat.Policy = "Policy0001"
            pat.PolicyNumber = "PolicyNumber0001"
            pat.SSN = "SSN 000222"
            pat.ClientId = 805
            Dim settings As New JsonSerializerSettings
            settings.NullValueHandling = NullValueHandling.Ignore
            settings.Formatting = Formatting.Indented
            Dim json As String = JsonConvert.SerializeObject(pat, settings)
            json = Replace(json, """Id"": -1", """Id"": null")

            Console.WriteLine(json)

            resultAPI = postAPIRequest("http://176.9.113.46:10003/api/PatientEx/Save", json)
        End If



        'StatusEx
        dt = getAPIDataTable("http://176.9.113.46:10003/api/StatusEx/List?clientId=" & 805, "List")
        resultAPI = postAPIRequest("http://176.9.113.46:10003/api/StatusEx/SaveList", DataTable2JSON(dt, "List"))
        'StatusEx/SaveList



    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

    End Sub

    Private Sub StatusStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles StatusStrip1.ItemClicked

    End Sub
End Class
