Partial Public NotInheritable Class Task
    Inherits UserControl

    Public Sub New()
        Me.InitializeComponent()
        Me.DataContext = Me
    End Sub

    Public Property Number As Integer
        Get
            Return CInt(GetValue(NumberProperty))
        End Get
        Set(value As Integer)
            SetValue(NumberProperty, value)
        End Set
    End Property

    ' Using a DependencyProperty as the backing store for Number.  This enables animation, styling, binding, etc...
    Public Shared ReadOnly NumberProperty As DependencyProperty = _
        DependencyProperty.Register("Number", GetType(Integer), GetType(Task), Nothing)

    Public Property Title As String
        Get
            Return CStr(GetValue(TitleProperty))
        End Get
        Set(value As String)
            SetValue(TitleProperty, value)
        End Set
    End Property

    ' Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    Public Shared ReadOnly TitleProperty As DependencyProperty = _
        DependencyProperty.Register("Title", GetType(String), GetType(Task), Nothing)

    Public Property Description As String
        Get
            Return CType(GetValue(DescriptionProperty), String)
        End Get
        Set(value As String)
            SetValue(DescriptionProperty, value)
        End Set
    End Property

    ' Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
    Public Shared ReadOnly DescriptionProperty As DependencyProperty = _
        DependencyProperty.Register("Description", GetType(String), GetType(Task), Nothing)
End Class
