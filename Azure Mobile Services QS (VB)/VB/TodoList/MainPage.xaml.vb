Imports System.Runtime.Serialization
Imports Windows.UI.Xaml.Navigation
Imports Microsoft.WindowsAzure.MobileServices
Imports Newtonsoft.Json

Public Class TodoItem
    Private _Id As Integer
    Public Property Id As Integer
        Get
            Return _Id
        End Get
        Set(value As Integer)
            _Id = value
        End Set
    End Property
    Private _Text As String
    <JsonProperty(PropertyName:="text")>
    Public Property Text As String
        Get
            Return _Text
        End Get
        Set(value As String)
            _Text = value
        End Set
    End Property
    Private _Complete As Boolean
    <JsonProperty(PropertyName:="complete")>
    Public Property Complete As Boolean
        Get
            Return _Complete
        End Get
        Set(value As Boolean)
            _Complete = value
        End Set
    End Property
    '' TODO: Uncomment the following property after you add 
    '' the createdAt timestamp column in the table in the data tutorial. 
    'Private _CreatedAt As Nullable(Of DateTime)
    '<JsonProperty(PropertyName:="createdAt")>
    'Public Property CreatedAt As Nullable(Of DateTime)
    '    Get
    '        Return _CreatedAt
    '    End Get
    '    Set(value As Nullable(Of DateTime))
    '        _CreatedAt = value
    '    End Set
    'End Property
End Class

Public NotInheritable Class MainPage
    Inherits Page

    ' MobileServiceCollectionView implements ICollectionView (useful for databinding to lists) and 
    ' is integrated with your Mobile Service to make it easy to bind your data to the ListView.    
    Private items As MobileServiceCollection(Of TodoItem, TodoItem)
    Private todoTable As IMobileServiceTable(Of TodoItem) = App.MobileService.GetTable(Of TodoItem)()

    Private Async Sub InsertTodoItem(ByVal todoItem As TodoItem)

        ' This code inserts a new TodoItem into the database. When the operation completes
        ' and Mobile Services has assigned an Id, the item is added to the CollectionView.
        Await todoTable.InsertAsync(todoItem)

        items.Add(todoItem)
    End Sub

    Private Async Sub RefreshTodoItems()
        ' Defines a simple query for all items. 
        items = Await todoTable _
                .Where(Function(todoItem) todoItem.Complete = False) _
        .ToCollectionAsync()

        ListItems.ItemsSource = items
    End Sub

    Private Async Sub UpdateCheckedTodoItem(ByVal item As TodoItem)
        ' This code takes a freshly completed TodoItem and updates the database. When the MobileService 
        ' responds, the item is removed from the list.
        Await todoTable.UpdateAsync(item)
    End Sub

    Private Sub ButtonRefresh_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        RefreshTodoItems()
    End Sub

    Private Sub ButtonSave_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim todoItem = New TodoItem()
        todoItem.Text = TextInput.Text
        InsertTodoItem(todoItem)
    End Sub

    Private Sub CheckBoxComplete_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim cb As CheckBox = CType(sender, CheckBox)
            Dim item = CType(cb.DataContext, TodoItem)
            UpdateCheckedTodoItem(item)
    End Sub
    Protected Overrides Sub OnNavigatedTo(ByVal e As NavigationEventArgs)
        RefreshTodoItems()
    End Sub
End Class
