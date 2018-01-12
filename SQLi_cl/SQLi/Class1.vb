
Public Class fn
        Private longName As String
        Private fName As String
        Private mName As String
        Private lName As String

        ReadOnly Property FirstName() As String
            Get
                Return fName
            End Get
        End Property
    ReadOnly Property MiddleName() As String
        Get
            Return mName
        End Get
    End Property
    ReadOnly Property LastName() As String
        Get
            Return lName
        End Get
    End Property
    Public Property FullName() As String
        Get
            Return ""
        End Get
        Set(ByVal value As String)
            longName = value
            Dim scPos, sc As Integer
            sc = InStr(longName, ",") + 1
            If sc > 2 Then
                scPos = InStr(Mid(longName, sc), ",")
                lName = Trim(Left(longName, sc - 2))
            End If
            If scPos > 0 Then
                longName = Left(longName, sc + scPos - 2)

            End If

            Dim fmName As String
            fmName = Trim(Mid(longName, sc))
            fName = ""
            mName = ""
            If sc > 2 Then
                sc = InStr(fmName, " ")
                If sc > 2 Then
                    fName = Left(fmName, sc - 1)
                    mName = Mid(fmName, sc)
                Else
                    fName = fmName
                End If
            End If
        End Set
    End Property
End Class
