Imports System
Imports System.IO
Imports System.Net
Imports System.Text

Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Converters
Imports Newtonsoft.Json.Schema

Module Module2

    Public Function getAPIRequest(ByVal URI As String) As String
        Dim ie As New MSXML2.XMLHTTP60
        ie = New MSXML2.XMLHTTP60
        ie.open("GET", URI)
        ie.send()
        Do While ie.readyState <> 4
            System.Threading.Thread.Sleep(100)
        Loop
        getAPIRequest = ie.responseText
        GC.Collect()
    End Function



    Public Function getAPIDataTable(ByVal URL As String, ByVal tName As String) As DataTable
        getAPIDataTable = Nothing

        Dim ieResponse As String = getAPIRequest(URL)
        Dim ieXML As Object = Nothing

        Dim dData As New DataTable()
        Dim dSet As New DataSet
        Try
            ieXML = JsonConvert.DeserializeObject(ieResponse)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        Try
            dData = JsonConvert.DeserializeAnonymousType(Of DataTable)(ieXML.SelectToken(tName).ToString, dData)
            getAPIDataTable = dData
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function
    Public Function postAPIRequest(ByVal URI As String, ByVal json As String) As String
        If json = "" Then Return ""
        Dim request As WebRequest = WebRequest.Create(URI)
        request.Method = "POST"
        request.ContentType = "application/json"
        Dim postData As String = json
        Dim byteArray As Byte() = Encoding.ASCII.GetBytes(postData)
        Dim responseFromServer As String = ""
        request.ContentLength = byteArray.Length
        Dim dataStream As Stream = request.GetRequestStream()
        dataStream.Write(byteArray, 0, byteArray.Length)
        dataStream.Close()
        Try
            Dim response As WebResponse = request.GetResponse()
            dataStream = response.GetResponseStream()

            Dim reader As New StreamReader(dataStream)
            responseFromServer = reader.ReadToEnd()
        Catch ex As Exception
            Console.WriteLine(json)
            MessageBox.Show(ex.Message)
        End Try
        postAPIRequest = responseFromServer
        GC.Collect()
    End Function




    Public Function checkAPIRespond(ByVal json As String, ByVal tokenName As String, ByVal tokenType As String) As Object
        checkAPIRespond = Nothing
        If json = "" Then Return Nothing
        Dim ieXML As Object = Nothing
        Dim ieXMLtest As Object = Nothing
        Dim dTable As New DataTable()
        Dim dSet As New DataSet
        Try
            ieXML = JsonConvert.DeserializeObject(json)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try

        Try
            Dim DataSet As New DataSet
            DataSet.Namespace = "NetFrameWork"

            Select Case tokenType
                Case "table"
                    dTable = JsonConvert.DeserializeAnonymousType(Of DataTable)(ieXML.SelectToken(tokenName).ToString, dTable)
                    DataSet.Tables.Add(dTable)
                    DataSet.Tables(0).TableName = tokenName
                    checkAPIRespond = DataSet
                Case "value"
                    'respondAPIList
                    ieXMLtest = JsonConvert.DeserializeAnonymousType(Of Object)(ieXML.SelectToken(tokenName).ToString, ieXMLtest)
                    checkAPIRespond = ieXMLtest
                Case "v"
                    Dim token As JToken = ieXML.SelectToken(tokenName)
                    checkAPIRespond = token
            End Select
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Function
    Public Function testAPI(ByVal json As String)
        Dim reader = New JsonTextReader(New StringReader(json))
        While (reader.Read())

            If VarType(reader.Value) <> vbNull Then

                Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value)

            Else

                Console.WriteLine("Token: {0}", reader.TokenType)
            End If

        End While
        Dim jSchema As JObject = JObject.Parse(json)

        For Each prop In jSchema.Properties
            Console.WriteLine(prop.Name & " - " & prop.Value.ToString & " - " & prop.Value.Type)
        Next

    End Function



    Public Function DataTable2JSON(ByVal dTable As DataTable, ByVal tableName As String) As String
        Dim DataSet As New DataSet
        DataSet.Namespace = "NetFrameWork"
        DataSet.Tables.Add(dTable.Copy)
        DataSet.Tables(0).TableName = tableName

        Try
            DataTable2JSON = JsonConvert.SerializeObject(DataSet, Formatting.Indented)
            'DataTable2JSON = JsonConvert.SerializeObject(dTable)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function

    Public Function GetJsonFiltered(ByVal dTable As DataTable, Optional ByVal tblName As String = "", Optional ByVal fltFieldName As String = "", Optional fltFieldValue As String = "0") As String
        GetJsonFiltered = ""
        Dim s As String
        Dim dat As DataRow()
        Dim DataSet As New DataSet
        Dim dT As DataTable = dTable.Copy
        DataSet.Namespace = "NetFrameWork"
        If fltFieldName <> "" Then
            dat = dT.Select("" & fltFieldName & "<>" & fltFieldValue.ToString)
            For i = 0 To dat.Length - 1
                dT.Rows.Remove(dat(i))
            Next
        End If
        DataSet.Tables.Add(dT)
        DataSet.Tables(0).TableName = tblName
        s = JsonConvert.SerializeObject(DataSet, Formatting.Indented)

        Dim ieXML As Object = JsonConvert.DeserializeObject(s)
        Dim token As JToken = ieXML.SelectToken(tblName)
        If Not IsNothing(token.First) Then
            GetJsonFiltered = token.First.ToString
        End If
    End Function


End Module
