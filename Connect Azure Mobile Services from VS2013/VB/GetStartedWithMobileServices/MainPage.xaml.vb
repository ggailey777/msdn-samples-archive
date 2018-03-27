Imports System.Runtime.InteropServices.WindowsRuntime
'Imports Microsoft.WindowsAzure.MobileServices
'Imports Newtonsoft.Json

'''<summary>
'''Represents a todo item.
'''</summary>
Public Class TodoItem
    Public Property Id As String

    '' TODO: Add the following serialization attribute. 
    '<JsonProperty(PropertyName:= "text")> 
    Public Property Text As String

    '' TODO: Add the following serialization attribute. 
    '<JsonProperty(PropertyName:= "complete")> 
    Public Property Complete As Boolean

    '' TODO: Uncomment the following property after you add  
    '' the createdAt timestamp column in the table.         
    '<JsonProperty(PropertyName:= "createdAt")>
    'Public Property CreatedAt As Nullable(Of DateTime)
End Class
Public NotInheritable Class MainPage
    Inherits Page

    ' TODO: Comment out the following line that defined the in-memory collection. 
    Private items As ObservableCollection(Of TodoItem) = _
        New ObservableCollection(Of TodoItem)()

    '' TODO: Uncomment the following lines that defines the Mobile Servies table.
    '' TODO: Replace yourClient with the MobileServiceClient instance added to 
    '' the App.xaml.cs file when connecting to your service.
    'Private items As MobileServiceCollection(Of TodoItem, TodoItem)
    'Private todoTable As IMobileServiceTable(Of TodoItem) = _
    '    App.yourClient.GetTable(Of TodoItem)()

    Private Sub InsertTodoItem(ByVal todoItem As TodoItem)

        ' TODO: Delete or comment the following line of code; Mobile Services auto-generates the ID.        
        todoItem.Id = Guid.NewGuid().ToString()

        '' This code inserts a new TodoItem into the database. When the operation completes 
        '' and Mobile Services has assigned an Id, the item is added to the CollectionView 
        '' TODO: Mark this method as "async" and uncomment the following statement. 
        'Await todoTable.InsertAsync(todoItem)

        items.Add(todoItem)
    End Sub
    Private Sub RefreshTodoItems()
        '' TODO #1: Mark this method as "async" and uncomment the following statment 
        '' that defines a simple query for all items.  
        'items = await todoTable.ToCollectionAsync() 

        '' TODO #2: More advanced query that filters out completed items.  
        'items = await todoTable _
        '   .Where(todoItem => todoItem.Complete == false) _
        '   .ToCollectionAsync()

        ListItems.ItemsSource = items
    End Sub
    Private Sub UpdateCheckedTodoItem(ByVal item As TodoItem)

        '' This code takes a freshly completed TodoItem and updates the database. When the MobileService  
        '' responds, the item is removed from the list. 
        '' TODO: Mark this method as "async" and uncomment the following statement 
        'Await todoTable.UpdateAsync(item)
    End Sub
    Private Sub ButtonRefresh_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        RefreshTodoItems()
    End Sub
    Private Sub ButtonSave_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim todoItem = New TodoItem
        todoItem.Text = TextInput.Text
        InsertTodoItem(todoItem)
    End Sub
    Private Sub CheckBoxComplete_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim cb As CheckBox = CType(sender, CheckBox)
        Dim item As TodoItem = CType(cb.DataContext, TodoItem)
        UpdateCheckedTodoItem(item)
    End Sub
    Protected Overrides Sub OnNavigatedTo(e As Navigation.NavigationEventArgs)
        RefreshTodoItems()
    End Sub
End Class
