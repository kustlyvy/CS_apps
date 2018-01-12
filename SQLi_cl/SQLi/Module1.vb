Imports System
Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Net.FtpWebRequest
Imports System.Management
Imports System.Runtime.InteropServices
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports excel = Microsoft.Office.Interop.Excel
Imports Microsoft.Office
Imports System.Data
Imports System.Configuration

Module Module1
    Public Sub saveConf(ByVal conf As String)
        Dim ConfigFileName As String = "SQLi.cfg"
        Dim cPath As String = ""
        cPath = Application.StartupPath & "\" & ConfigFileName
        Using outfile As New StreamWriter(cPath)
            outfile.Write(conf)
        End Using

    End Sub
    Public Function getConfigLine() As String
        Dim ConfigFileName As String = "SQLi.cfg"
        getConfigLine = ""
        If Dir(Application.StartupPath & "\" & ConfigFileName) = "" Then Exit Function
        Using sr As New StreamReader(Application.StartupPath & "\" & ConfigFileName)
            getConfigLine = Trim(sr.ReadToEnd.ToString)
        End Using
    End Function
    Public Function connectToSQL(ByVal serverName As String, ByVal username As String, ByVal pass As String, ByVal dbName As String, ByVal authMode As String) As SqlConnection
        Dim con As SqlConnection
        Dim cmd As SqlCommand
        Dim dread As SqlDataReader
        Dim found As Boolean
        connectToSQL = Nothing
        If authMode = "SQL" Then
            If Left(serverName, 9) = "localhost" Then
                con = New SqlConnection("Data Source=" & serverName & ";Initial Catalog=" & dbName & ";Integrated Security=True; User ID=" & username & "; password=" & pass & "; Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
            Else
                con = New SqlConnection("Data Source=" & serverName & ";Initial Catalog=" & dbName & ";Integrated Security=False; User ID=" & username & "; password=" & pass & "; Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
            End If
        Else
            con = New SqlConnection("Data Source=" & serverName & ";Initial Catalog=" & dbName & ";Integrated Security=True; User ID=" & username & "; password=" & pass & "; Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
        End If
        'Data Source=176.9.113.46;Initial Catalog=CollectionSystem;Integrated Security=False;User ID=cs_user;Password=********;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False
        Try
            con.Open()
            cmd = New SqlCommand("select * from sysdatabases", con)
            dread = cmd.ExecuteReader
            While dread.Read
                If dread(0) = dbName Then found = True
            End While
            If found Then connectToSQL = con
            dread.Close()
        Catch
            MsgBox("Check your settings!")
        End Try
    End Function
    Public Function connectToFTP(ByVal srvName As String, ByVal usrName As String, ByVal usrPass As String, ByVal ftpPath As String, ByVal localPath As String) As Boolean
        connectToFTP = False
        processFTP(srvName, usrName, usrPass, ftpPath, localPath)
    End Function
    Public Sub processFTP(ByVal srvName As String, ByVal usrName As String, ByVal usrPass As String, ByVal ftpPath As String, ByVal localPath As String)
        Dim ftpFolder As String = "/tmp"
        Dim request As FtpWebRequest = DirectCast(WebRequest.Create("ftp://" & srvName & ftpFolder), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.ListDirectory 'ListDirectoryDetails
        ' This example assumes the FTP site uses anonymous logon.
        request.Credentials = New NetworkCredential(usrName, usrPass, "")
        request.UseBinary = False
        request.UsePassive = True

        Dim response As FtpWebResponse
        response = DirectCast(request.GetResponse(), FtpWebResponse)

        Dim responseStream As Stream = response.GetResponseStream()
        Dim reader As New StreamReader(responseStream)
        Dim ftpFile() As String
        ftpFile = Split(reader.ReadToEnd(), vbCrLf)

        'Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription)

        For i = LBound(ftpFile) To UBound(ftpFile)
            If ftpFile(i).IndexOf("..") = -1 And Trim(ftpFile(i)) <> "" Then
                ftpDownloadFile(srvName, usrName, usrPass, ftpPath, ftpFolder, ftpFile(i), localPath)
                ftpDeleteFile(srvName, usrName, usrPass, ftpPath, ftpFolder, ftpFile(i))
            End If
        Next


        reader.Close()
        response.Close()
    End Sub
    Public Sub ftpDownloadFile(ByVal srvName As String, ByVal usrName As String, ByVal usrPass As String, ByVal ftpPath As String, ByVal ftpFolder As String, ByVal ftpFile As String, ByVal localPath As String)
        Dim ff As String
        ff = "ftp://" & srvName & "/" & ftpFile
        Dim request As FtpWebRequest = DirectCast(WebRequest.Create(ff), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.DownloadFile  'ListDirectoryDetails
        ' This example assumes the FTP site uses anonymous logon.
        request.Credentials = New NetworkCredential(usrName, usrPass, "")
        request.UsePassive = True
        request.KeepAlive = False
        request.UseBinary = True

        Dim LFN As String = localPath & "/" & Mid(ftpFile, ftpFile.IndexOf("/") + 2)


        Using FtpResponse As FtpWebResponse = CType(request.GetResponse, FtpWebResponse)
            Using ResponseStream As IO.Stream = FtpResponse.GetResponseStream
                Using fs As New IO.FileStream(LFN, FileMode.Create)
                    Dim buffer(2047) As Byte
                    Dim read As Integer = 0
                    Do
                        read = ResponseStream.Read(buffer, 0, buffer.Length)
                        fs.Write(buffer, 0, read)
                        'Console.Write(".")
                    Loop Until read = 0
                    ResponseStream.Close()
                    fs.Flush()
                    fs.Close()
                End Using
                ResponseStream.Close()
            End Using
        End Using
    End Sub
    Public Sub ftpDeleteFile(ByVal srvName As String, ByVal usrName As String, ByVal usrPass As String, ByVal ftpPath As String, ByVal ftpFolder As String, ByVal ftpFile As String)
        Dim FTPDelReq As FtpWebRequest = WebRequest.Create("ftp://" & srvName & "/" & ftpFile)
        FTPDelReq.Credentials = New Net.NetworkCredential(usrName, usrPass)
        FTPDelReq.Method = WebRequestMethods.Ftp.DeleteFile
        Dim FTPDelResp As FtpWebResponse = FTPDelReq.GetResponse

    End Sub
    Public Sub processCharges(ByVal srvName As String, ByVal userName As String, ByVal Pass As String, ByVal DBname As String, ByVal localPath As String, ByVal con As SqlConnection, ByRef stLabel As ToolStripStatusLabel, ByRef stBar As ToolStripProgressBar)
        Dim fname As String

        Dim xlsapp As Object
        Dim sqlSS As String
        Dim data_rng As Microsoft.Office.Interop.Excel.Range
        Dim cmd As SqlCommand
        Dim passed As Boolean = True
        Dim recordCount As Long
        Dim docName As New fn
        Dim patName As New fn
        fname = Dir(localPath & "\chargeSQL*.*")
        While fname <> ""

            cmd = New SqlCommand("select count(*) from ImportFiles where ImportFileName='" & fname & "'", con)
            If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then


                'On Error Resume Next
                If passed Then
                    xlsapp = CreateObject("Excel.Application")

                    xlsapp.workbooks.Open(localPath & "\" & fname)

                End If
                data_rng = xlsapp.activeworkbook.activesheet.usedrange

                stLabel.Text = fname
                stBar.Maximum = data_rng.Rows.Count
                For r = 2 To data_rng.Rows.Count
                    sqlSS = "--"
                    If (data_rng.Cells(r, 1).value Is Nothing) Then sqlSS = ""
                    If sqlSS <> "" Then
                        If data_rng.Cells(r, 1).value.ToString() = "" Then
                            sqlSS = ""
                        End If
                    End If

                    If sqlSS <> "" Then
                        docName.FullName = Replace(data_rng.Cells(r, 14).value, "'", "''")
                        patName.FullName = Replace(data_rng.Cells(r, 15).value, "'", "''")
                        sqlSS = "'" & data_rng.Cells(r, 13).value & "', '" &
                                    Format(data_rng.Cells(r, 2).value, "yyyy-MM-dd") & "', '" &
                                    Format(data_rng.Cells(r, 3).value, "yyyy-MM-dd") & "', '" &
                                    data_rng.Cells(r, 1).value & "', '" &
                                    docName.FirstName & "', '" &
                                    docName.MiddleName & "', '" &
                                    docName.LastName & "', " &
                                    data_rng.Cells(r, 4).value & ", '" &
                                    data_rng.Cells(r, 5).value & "', '" &
                                    patName.FirstName & "', '" &
                                    patName.MiddleName & "', '" &
                                    patName.LastName & "', '" &
                                    Format(data_rng.Cells(r, 16).value, "yyyy-MM-dd") & "', '" &
                                    data_rng.Cells(r, 17).value & "', '" &
                                    data_rng.Cells(r, 18).value & "', '" &
                                    data_rng.Cells(r, 6).value & "', '" &
                                    data_rng.Cells(r, 12).value & "', '" &
                                    data_rng.Cells(r, 7).value & "', " &
                                    data_rng.Cells(r, 8).value & ", " &
                                    data_rng.Cells(r, 9).value & ", " &
                                    data_rng.Cells(r, 10).value & ", " &
                                    data_rng.Cells(r, 11).value & ", '" &
                                    fname & "')"

                        cmd = New SqlCommand("insert into ImportCharges (  [ClientName]
                                                                      ,[DateOfService]
                                                                      ,[DateOfPosting]
                                                                      ,[Practice]
                                                                      ,[DocFname]
                                                                      ,[DocMname]
                                                                      ,[DocLname]
                                                                      ,[DocProviderID]
                                                                      ,[Patient]
                                                                      ,[PatientFName]
                                                                      ,[PatientMName]
                                                                      ,[PatientLName]
                                                                      ,[PatientDOB]
                                                                      ,[PolicyNumber]
                                                                      ,[Policy]
                                                                      ,[CPT]
                                                                      ,[InsuranceCompany]
                                                                      ,[InsuranceCompanyID]
                                                                      ,[Billed]
                                                                      ,[Payment]
                                                                      ,[Adjustment]
                                                                      ,[Balance]
                                                                      ,[ImportFileName] ) values ( " & sqlSS, con)





                        Try
                            cmd.ExecuteNonQuery()
                        Catch
                            con.Close()
                            con.Open()
                            cmd = New SqlCommand("insert into ImportCharges (  [ClientName]
                                                                      ,[DateOfService]
                                                                      ,[DateOfPosting]
                                                                      ,[Practice]
                                                                      ,[DocFname]
                                                                      ,[DocMname]
                                                                      ,[DocLname]
                                                                      ,[DocProviderID]
                                                                      ,[Patient]
                                                                      ,[PatientFName]
                                                                      ,[PatientMName]
                                                                      ,[PatientLName]
                                                                      ,[PatientDOB]
                                                                      ,[PolicyNumber]
                                                                      ,[Policy]
                                                                      ,[CPT]
                                                                      ,[InsuranceCompany]
                                                                      ,[InsuranceCompanyID]
                                                                      ,[Billed]
                                                                      ,[Payment]
                                                                      ,[Adjustment]
                                                                      ,[Balance]
                                                                      ,[ImportFileName] ) values ( " & sqlSS, con)
                            cmd.ExecuteNonQuery()
                        End Try
                    End If
                    stBar.Value = r
                    'stBar.Increment(1)
                Next

                cmd = New SqlCommand("select count(*) from ImportCharges where ImportFileName='" & fname & "'", con)
                recordCount = Convert.ToInt32(cmd.ExecuteScalar())
                If recordCount = data_rng.Rows.Count - 1 Then
                    'data_rng = Nothing
                    xlsapp.activeworkbook.close(True)
                    xlsapp.Quit()
                    Marshal.ReleaseComObject(data_rng)

                    Try
                        Dim intRel As Integer = 0
                        Do
                            intRel = Marshal.ReleaseComObject(xlsapp)
                        Loop While intRel > 0
                    Catch ex As Exception
                        MsgBox("Error releasing object" & ex.ToString)
                        xlsapp = Nothing
                    Finally
                        GC.Collect()
                    End Try

                    xlsapp = Nothing
                    xlsapp = Nothing

                    Kill(localPath & "\" & fname)

                    cmd = New SqlCommand("insert into ImportFiles (importFileName) select '" & fname & "'", con)
                    cmd.ExecuteNonQuery()
                    fname = Dir()
                Else
                    cmd = New SqlCommand("delete from ImportCharges where ImportFileName='" & fname & "'", con)
                    cmd.ExecuteNonQuery()
                    passed = False
                End If


            Else
                Kill(localPath & "\" & fname)
                fname = Dir()
            End If

        End While

        con.Close()
    End Sub



    Public Sub processPayments(ByVal srvName As String, ByVal userName As String, ByVal Pass As String, ByVal DBname As String, ByVal localPath As String, ByVal con As SqlConnection, ByRef stLabel As ToolStripStatusLabel, ByRef stBar As ToolStripProgressBar)
        Dim fname As String

        Dim xlsapp As Object
        Dim sqlSS As String
        Dim data_rng As Microsoft.Office.Interop.Excel.Range
        Dim cmd As SqlCommand
        Dim recordCount As Long

        fname = Dir(localPath & "\paymentSQL!*.*")
        While fname <> ""
            'On Error Resume Next
            xlsapp = CreateObject("Excel.Application")
            xlsapp.workbooks.Open(localPath & "\" & fname)
            data_rng = xlsapp.activeworkbook.activesheet.usedrange

            stLabel.Text = fname
            stBar.Maximum = data_rng.Rows.Count

            For r = 2 To data_rng.Rows.Count
                If data_rng.Cells(r, 1).value.ToString() <> "" Then
                    sqlSS = "'" & Format(data_rng.Cells(r, 2).value, "yyyy-MM-dd") & "', '" &
                                    Format(data_rng.Cells(r, 3).value, "yyyy-MM-dd") & "', " &
                                    data_rng.Cells(r, 4).value & ", " &
                                    data_rng.Cells(r, 5).value & ", " &
                                    data_rng.Cells(r, 6).value & ", " &
                                    data_rng.Cells(r, 7).value & ", '" &
                                    data_rng.Cells(r, 9).value & "', '" &
                                    data_rng.Cells(r, 10).value & "')"

                    cmd = New SqlCommand("insert into ImportTransactions (DateOfService, 
                                                                          DateOfPosting, 
                                                                          Billed, 
                                                                          Payment, 
                                                                          Adjustment, 
                                                                          Balance, 
                                                                          CPT, 
                                                                          InsuranceCompany) values ( " & sqlSS, con)
                    Try
                        cmd.ExecuteNonQuery()
                    Catch
                        con.Close()
                        con.Open()
                        cmd = New SqlCommand("insert into ImportTransactions (DateOfService, 
                                                                          DateOfPosting, 
                                                                          Billed, 
                                                                          Payment, 
                                                                          Adjustment, 
                                                                          Balance, 
                                                                          CPT, 
                                                                          InsuranceCompany) values ( " & sqlSS, con)
                        cmd.ExecuteNonQuery()
                    End Try
                End If
                stBar.Value = r
            Next
            'data_rng = Nothing
            xlsapp.activeworkbook.close()
            xlsapp.Quit()
            Marshal.ReleaseComObject(data_rng)
            Marshal.ReleaseComObject(xlsapp)
            '

            Try
                Dim intRel As Integer = 0
                Do
                    intRel = Marshal.ReleaseComObject(xlsapp)
                Loop While intRel > 0
            Catch ex As Exception
                MsgBox("Error releasing object" & ex.ToString)
                xlsapp = Nothing
            Finally
                GC.Collect()
            End Try
            xlsapp = Nothing



            cmd = New SqlCommand("select count(*) from ImportCharges where ImportFileName='" & fname & "'", con)
            recordCount = Convert.ToInt32(cmd.ExecuteScalar())
            If recordCount = data_rng.Rows.Count - 1 Then
                Kill(localPath & "\" & fname)
                cmd = New SqlCommand("insert into ImportFiles (ImportFileName) select '" & fname & "'", con)
                cmd.ExecuteNonQuery()
                fname = Dir()
            Else
                cmd = New SqlCommand("delete from ImportCharges where ImportFileName='" & fname & "'", con)
                cmd.ExecuteNonQuery()
            End If
        End While
        con.Close()
    End Sub

    Public Sub processCharges2(ByVal srvName As String, ByVal userName As String, ByVal Pass As String, ByVal DBname As String, ByVal localPath As String, ByVal con As SqlConnection, ByRef logbox As TextBox, ByRef stBar As ToolStripProgressBar, ByVal ProcessInternalProcedures As Boolean)
        Dim fname As String
        Dim cmd As SqlCommand
        Dim passed As Boolean = True
        Dim recordCount As Long
        Dim docName As New fn
        Dim patName As New fn
        fname = Dir(localPath & "\chargeSQL*.*")
        recordCount = 0

        While fname <> ""
            fname = Dir()
            recordCount = recordCount + 1
        End While

        fname = Dir(localPath & "\chargeSQL*.*")
        stBar.Value = 0
        stBar.Maximum = recordCount
        While fname <> ""
            stBar.Value = stBar.Value + 1
            'logbox.AppendText(Now().ToString() & ":: Started :: " & fname & vbCrLf)
            Console.WriteLine(Now().ToString() & ":: Started :: " & fname & vbCrLf)


            cmd = New SqlCommand("select count(*) from ImportFiles where ImportFileName='" & fname & "'", con)
            If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then

                Dim connString As String = String.Empty
                connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & localPath & "\\" & fname & ";Extended Properties=""Excel 12.0 Xml;"""
                Using excel_con As New OleDbConnection(connString)
                    excel_con.Open()
                    Dim sheet1 As String = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, Nothing).Rows(0)("TABLE_NAME").ToString()
                    Dim dtExcelData As New DataTable()

                    '[OPTIONAL]: It is recommended as otherwise the data will be considered as String by default.
                    dtExcelData.Columns.AddRange(New DataColumn(22) {
                                                                    New DataColumn("ClientName", GetType(String)),
                                                                    New DataColumn("Practice", GetType(String)),
                                                                    New DataColumn("IdCode", GetType(String)),
                                                                    New DataColumn("Patient", GetType(String)),
                                                                    New DataColumn("CPT", GetType(String)),
                                                                    New DataColumn("docFirstName", GetType(String)),
                                                                    New DataColumn("docMiddleName", GetType(String)),
                                                                    New DataColumn("docLastName", GetType(String)),
                                                                    New DataColumn("patientFirstName", GetType(String)),
                                                                    New DataColumn("patientMiddleName", GetType(String)),
                                                                    New DataColumn("patientLastName", GetType(String)),
                                                                    New DataColumn("DOB", GetType(Date)),
                                                                    New DataColumn("PolicyNumber", GetType(String)),
                                                                    New DataColumn("Policy", GetType(String)),
                                                                    New DataColumn("Billed", GetType(Decimal)),
                                                                    New DataColumn("Payment", GetType(Decimal)),
                                                                    New DataColumn("Adjustment", GetType(Decimal)),
                                                                    New DataColumn("Balance", GetType(Decimal)),
                                                                    New DataColumn("InsuranceCompanyID", GetType(String)),
                                                                    New DataColumn("DateOfService", GetType(Date)),
                                                                    New DataColumn("DateOfPosting", GetType(Date)),
                                                                    New DataColumn("InsuranceCompany", GetType(String)),
                                                                    New DataColumn("ImportFileName", GetType(String))})

                    '


                    Using oda As New OleDbDataAdapter((Convert.ToString("Select * FROM [") & sheet1) + "] where len([Practice])>0 and len([InsuranceCompany])>0", excel_con)
                        oda.Fill(dtExcelData)
                    End Using
                    excel_con.Close()
                    Dim r As DataRow
                    For Each r In dtExcelData.Rows
                        r("ImportFileName") = fname
                        If IsDBNull(r("docFirstName")) Then
                            r("docFirstName") = ""
                        Else
                            r("docFirstName") = Trim(r("docFirstName"))
                        End If
                        If IsDBNull(r("docMiddleName")) Then
                            r("docMiddleName") = ""
                        Else
                            r("docMiddleName") = Trim(r("docMiddleName"))
                        End If
                        If IsDBNull(r("docLastName")) Then
                            r("docLastName") = ""
                        Else
                            r("docLastName") = Trim(r("docLastName"))
                        End If
                        If IsDBNull(r("patientFirstName")) Then
                            r("patientFirstName") = ""
                        Else
                            r("patientFirstName") = Trim(r("patientFirstName"))
                        End If
                        If IsDBNull(r("patientMiddleName")) Then
                            r("patientMiddleName") = ""
                        Else
                            r("patientMiddleName") = Trim(r("patientMiddleName"))
                        End If
                        If IsDBNull(r("patientLastName")) Then
                            r("patientLastName") = ""
                        Else
                            r("patientLastName") = Trim(r("patientLastName"))
                        End If
                    Next
                    Try
                        If dtExcelData.Rows.Count > 0 Then

                            Using sqlBulkCopy As New SqlBulkCopy(con)
                                sqlBulkCopy.DestinationTableName = "ImportCharges"
                                sqlBulkCopy.ColumnMappings.Add("Practice", "Practice")
                                sqlBulkCopy.ColumnMappings.Add("DateOfService", "DateOfService")
                                sqlBulkCopy.ColumnMappings.Add("DateOfPosting", "DateOfPosting")
                                sqlBulkCopy.ColumnMappings.Add("IdCode", "DocProviderID")
                                sqlBulkCopy.ColumnMappings.Add("Patient", "Patient")
                                sqlBulkCopy.ColumnMappings.Add("CPT", "CPT")
                                sqlBulkCopy.ColumnMappings.Add("InsuranceCompany", "InsuranceCompany")
                                sqlBulkCopy.ColumnMappings.Add("Billed", "Billed")
                                sqlBulkCopy.ColumnMappings.Add("Payment", "Payment")
                                sqlBulkCopy.ColumnMappings.Add("Adjustment", "Adjustment")
                                sqlBulkCopy.ColumnMappings.Add("Balance", "Balance")
                                sqlBulkCopy.ColumnMappings.Add("InsuranceCompanyID", "InsuranceCompanyID")
                                sqlBulkCopy.ColumnMappings.Add("ClientName", "ClientName")
                                sqlBulkCopy.ColumnMappings.Add("docFirstName", "DocFname")
                                sqlBulkCopy.ColumnMappings.Add("docMiddleName", "DocMname")
                                sqlBulkCopy.ColumnMappings.Add("docLastName", "DocLname")
                                sqlBulkCopy.ColumnMappings.Add("patientFirstName", "PatientFName")
                                sqlBulkCopy.ColumnMappings.Add("patientMiddleName", "PatientMName")
                                sqlBulkCopy.ColumnMappings.Add("patientLastName", "PatientLName")
                                sqlBulkCopy.ColumnMappings.Add("DOB", "PatientDOB")
                                sqlBulkCopy.ColumnMappings.Add("PolicyNumber", "PolicyNumber")
                                sqlBulkCopy.ColumnMappings.Add("Policy", "Policy")
                                sqlBulkCopy.ColumnMappings.Add("ImportFileName", "ImportFileName")
                                sqlBulkCopy.BatchSize = 50
                                sqlBulkCopy.WriteToServer(dtExcelData)
                            End Using
                        End If
                    Catch
                        con.Close()
                        con.Open()
                    End Try


                    cmd = New SqlCommand("select count(*) from ImportCharges where ImportFileName='" & fname & "'", con)
                    recordCount = Convert.ToInt32(cmd.ExecuteScalar())
                    If recordCount = dtExcelData.Rows.Count Then
                        'logbox.AppendText(Now().ToString() & ":: Passed :: " & recordCount & " rows :: " & fname & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Passed :: " & recordCount & " rows :: " & fname & vbCrLf)
                        Kill(localPath & "\" & fname)

                        cmd = New SqlCommand("insert into ImportFiles (importFileName) select '" & fname & "'", con)
                        cmd.ExecuteNonQuery()
                        fname = Dir()
                        passed = True

                    Else
                        'logbox.AppendText(Now().ToString() & ":: Error, Retry :: " & fname & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Error, Retry :: " & fname & vbCrLf)

                        cmd = New SqlCommand("delete from ImportCharges where ImportFileName='" & fname & "'", con)
                        cmd.ExecuteNonQuery()
                        passed = False
                        stBar.Value = stBar.Value - 1
                    End If

                End Using
            Else
                'stBar.Value = stBar.Value + 1
                'logbox.AppendText(Now().ToString() & ":: Already processed :: " & fname & vbCrLf)
                Console.WriteLine(Now().ToString() & ":: Already processed :: " & fname & vbCrLf)
                Kill(localPath & "\" & fname)
                fname = Dir()
            End If
            GC.Collect()
            If recordCount > 0 Then
                Try
                    cmd = New SqlCommand("exec InsertRefs", con)
                    'logbox.AppendText(Now().ToString() & ":: Processing References..." & vbCrLf)
                    Console.WriteLine(Now().ToString() & ":: Processing References..." & vbCrLf)
                    cmd.CommandTimeout = 60000
                    cmd.ExecuteNonQuery()
                    'logbox.AppendText(Now().ToString() & ":: Processed" & vbCrLf)
                    Console.WriteLine(Now().ToString() & ":: Processed" & vbCrLf)

                    If ProcessInternalProcedures = True Then
                        cmd = New SqlCommand("exec InsertCharges", con)
                        'logbox.AppendText(Now().ToString() & ":: Adding Charges..." & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Adding Charges..." & vbCrLf)

                        cmd.CommandTimeout = 12000
                        cmd.ExecuteNonQuery()
                        'logbox.AppendText(Now().ToString() & ":: Processed" & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Processed" & vbCrLf)

                        cmd = New SqlCommand("exec UpdateCharges", con)
                        'logbox.AppendText(Now().ToString() & ":: Updating Charges..." & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Updating Charges..." & vbCrLf)

                        cmd.CommandTimeout = 36000
                        'cmd.BeginExecuteNonQuery()
                        cmd.ExecuteNonQuery()
                        'logbox.AppendText(Now().ToString() & ":: Processed" & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Processed" & vbCrLf)

                    End If
                Catch e As Exception
                End Try



            End If
        End While
        'stBar.Value += 1
        'con.Close()
    End Sub

    Public Sub processPayments2(ByVal srvName As String, ByVal userName As String, ByVal Pass As String, ByVal DBname As String, ByVal localPath As String, ByVal con As SqlConnection, ByRef logbox As TextBox, ByRef stBar As ToolStripProgressBar)
        Dim fname As String
        Dim cmd As SqlCommand
        Dim passed As Boolean = True
        Dim recordCount As Long
        Dim docName As New fn
        Dim patName As New fn
        fname = Dir(localPath & " \paymentSQL*.*")
        recordCount = 0

        While fname <> ""
            fname = Dir()
            recordCount = recordCount + 1
        End While

        fname = Dir(localPath & " \paymentSQL*.*")
        stBar.Value = 0
        stBar.Maximum = recordCount
        While fname <> ""
            stBar.Value = stBar.Value + 1
            'logbox.AppendText(Now().ToString() & ": Started :: " & fname & vbCrLf)
            Console.WriteLine(Now().ToString() & ": Started :: " & fname & vbCrLf)


            cmd = New SqlCommand("select count(*) from ImportFiles where ImportFileName='" & fname & "'", con)
            If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then

                Dim connString As String = String.Empty
                connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & localPath & "\\" & fname & ";Extended Properties=""Excel 12.0 Xml;"""
                Using excel_con As New OleDbConnection(connString)
                    excel_con.Open()
                    Dim sheet1 As String = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, Nothing).Rows(0)("TABLE_NAME").ToString()
                    Dim dtExcelData As New DataTable()

                    '[OPTIONAL]: It is recommended as otherwise the data will be considered as String by default.
                    dtExcelData.Columns.AddRange(New DataColumn(10) {
                                                                    New DataColumn("DateOfService", GetType(Date)),
                                                                    New DataColumn("DateOfPosting", GetType(Date)),
                                                                    New DataColumn("Practice", GetType(String)),
                                                                    New DataColumn("Patient", GetType(String)),
                                                                    New DataColumn("CPT", GetType(String)),
                                                                    New DataColumn("Billed", GetType(Decimal)),
                                                                    New DataColumn("Payment", GetType(Decimal)),
                                                                    New DataColumn("Adjustment", GetType(Decimal)),
                                                                    New DataColumn("Balance", GetType(Decimal)),
                                                                    New DataColumn("InsuranceCompany", GetType(String)),
                                                                    New DataColumn("ImportFileName", GetType(String))})

                    dtExcelData.Columns("InsuranceCompany").AllowDBNull = True
                    Using oda As New OleDbDataAdapter((Convert.ToString("Select * FROM [") & sheet1) + "] where len([Practice])>0", excel_con)
                        oda.Fill(dtExcelData)
                    End Using
                    excel_con.Close()
                    Dim r As DataRow
                    For Each r In dtExcelData.Rows
                        r("ImportFileName") = fname
                    Next
                    Try
                        If dtExcelData.Rows.Count > 0 Then
                            Using sqlBulkCopy As New SqlBulkCopy(con)
                                sqlBulkCopy.DestinationTableName = "ImportTransactions"
                                sqlBulkCopy.ColumnMappings.Add("Practice", "Practice")
                                sqlBulkCopy.ColumnMappings.Add("DateOfService", "DateOfService")
                                sqlBulkCopy.ColumnMappings.Add("DateOfPosting", "DateOfPosting")
                                sqlBulkCopy.ColumnMappings.Add("Patient", "Patient")
                                sqlBulkCopy.ColumnMappings.Add("CPT", "CPT")
                                sqlBulkCopy.ColumnMappings.Add("InsuranceCompany", "InsuranceCompany")
                                sqlBulkCopy.ColumnMappings.Add("Billed", "Billed")
                                sqlBulkCopy.ColumnMappings.Add("Payment", "Payment")
                                sqlBulkCopy.ColumnMappings.Add("Adjustment", "Adjustment")
                                sqlBulkCopy.ColumnMappings.Add("Balance", "Balance")
                                sqlBulkCopy.ColumnMappings.Add("ImportFileName", "ImportFileName")
                                'sqlBulkCopy.BatchSize = 500
                                sqlBulkCopy.WriteToServer(dtExcelData)
                            End Using
                        End If
                    Catch
                        con.Close()
                        con.Open()
                    End Try


                    cmd = New SqlCommand("select count(*) from ImportTransactions where ImportFileName='" & fname & "'", con)
                    recordCount = Convert.ToInt32(cmd.ExecuteScalar())
                    If recordCount = dtExcelData.Rows.Count Then
                        'logbox.AppendText(Now().ToString() & ":: Passed :: " & recordCount & " rows :: " & fname & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Passed :: " & recordCount & " rows :: " & fname & vbCrLf)

                        Kill(localPath & "\" & fname)

                        cmd = New SqlCommand("insert into ImportFiles (importFileName) select '" & fname & "'", con)
                        cmd.ExecuteNonQuery()
                        fname = Dir()
                        passed = True

                    Else
                        'logbox.AppendText(Now().ToString() & ":: Error, Retry :: " & fname & vbCrLf)
                        Console.WriteLine(Now().ToString() & ":: Error, Retry :: " & fname & vbCrLf)
                        cmd = New SqlCommand("delete from ImportTransactions where ImportFileName='" & fname & "'", con)
                        cmd.ExecuteNonQuery()
                        passed = False
                        stBar.Value = stBar.Value - 1
                    End If

                End Using
            Else
                'stBar.Value = stBar.Value + 1
                'logbox.AppendText(Now().ToString() & ":: Already processed :: " & fname & vbCrLf)
                Console.WriteLine(Now().ToString() & ":: Already processed :: " & fname & vbCrLf)

                Kill(localPath & "\" & fname)
                fname = Dir()
            End If
            GC.Collect()

            If recordCount > 0 Then
                cmd = New SqlCommand("exec InsertPayments", con)
                'logbox.AppendText(Now().ToString() & ":: Processing References..." & vbCrLf)
                Console.Write(Now().ToString() & ":: Processing References..." & vbCrLf)
                cmd.CommandTimeout = 2400
                cmd.ExecuteNonQuery()
                'logbox.AppendText(Now().ToString() & ":: Processed" & vbCrLf)
                Console.WriteLine(Now().ToString() & ":: Processed" & vbCrLf)

            End If
        End While
        'stBar.Value += 1
        'con.Close()
    End Sub
End Module
