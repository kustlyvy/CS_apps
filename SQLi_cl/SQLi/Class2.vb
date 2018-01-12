Public Class respondAPIListItem
    Property ID As Long
End Class
Public Class respondAPIList
    Inherits List(Of respondAPIListItem)
    Overloads Sub add(ByVal ID As Long)
        Dim l As New respondAPIListItem
        l.ID = ID
    End Sub
    Protected Function GetItemName(ByVal item As respondAPIListItem) As String
        Return item.ID
    End Function
End Class
Public Class Patient
    Private pnum As String
    Private accID As String
    Private fName As String
    Private mName As String
    Private lName As String
    Private DB As Date
    Private sN As String
    Private pN As String
    Private act As Boolean
    Private pID As VariantType = vbNull
    Private clID As Long
    Public Property PolicyNumber As String
        Set(value As String)
            pnum = value
        End Set
        Get
            Return pnum
        End Get
    End Property
    Public Property AccountID As String
        Set(value As String)
            accID = value
        End Set
        Get
            Return accID
        End Get
    End Property
    Public Property FirstName As String
        Set(value As String)
            fName = value
        End Set
        Get
            Return fName
        End Get
    End Property
    Public Property MiddleName As String
        Set(value As String)
            mName = value
        End Set
        Get
            Return mName
        End Get
    End Property
    Public Property LastName As String
        Set(value As String)
            lName = value
        End Set
        Get
            Return lName
        End Get
    End Property
    Public Property DOB As Date
        Set(value As Date)
            DB = value
        End Set
        Get
            Return DB
        End Get
    End Property
    Public Property SSN As String
        Set(value As String)
            sN = value
        End Set
        Get
            Return sN
        End Get
    End Property
    Public Property Policy As String
        Set(value As String)
            pN = value
        End Set
        Get
            Return pN
        End Get
    End Property
    Public Property Access As Boolean
        Set(value As Boolean)
            act = value
        End Set
        Get
            Return act
        End Get
    End Property
    Public Property Id As VariantType
        Set(value As VariantType)
            pID = value
        End Set
        Get
            Return pID
        End Get
    End Property
    Public Property ClientId As Long
        Set(value As Long)
            clID = value
        End Set
        Get
            Return clID
        End Get
    End Property
End Class