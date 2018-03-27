using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel; 
using GetStartedWithData.DataModel;

//// TODO: Add the following using statement.  
//using Microsoft.WindowsAzure.MobileServices; 

namespace GetStartedWithData
{
    sealed partial class MainPage: Page
    {
        // TODO: Comment out the following line that defined the in-memory collection.  
        private ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>(); 

        //// TODO: Uncomment the following lines of code that defines todoTable,   
        //// a proxy for the table in the SQL Database, and the binding collection.  
        //private IMobileServiceTable<TodoItem> todoTable = MobileService.GetTable<TodoItem>(); 
        //private MobileServiceCollection<TodoItem, TodoItem> items;
        
        public MainPage()
        {
            this.InitializeComponent();
        }
        private void InsertTodoItem(TodoItem todoItem)
        {
            // TODO: Delete or comment the following statement; Mobile Services auto-generates the ID.  
            todoItem.Id = Guid.NewGuid().ToString();

            //// This code inserts a new TodoItem into the database. When the operation completes  
            //// and Mobile Services has assigned an Id, the item is added to the CollectionView  
            //// TODO: Mark this method as "async" and uncomment the following statement.  
            // await todoTable.InsertAsync(todoItem);  

            items.Add(todoItem);
        }
        private void RefreshTodoItems()
        {
            // TODO: Mark this method as "async" and uncomment the following block of code. 
            //MobileServiceInvalidOperationException exception = null;
            //try
            //{
            //    // TODO #1: uncomment the following statment  
            //    // that defines a simple query for all items.   
            //    items = await todoTable.ToCollectionAsync();  

            ////    // TODO #2: More advanced query that filters out completed items.   
            ////    items = await todoTable  
            ////       .Where(todoItem => todoItem.Complete == false)  
            ////       .ToCollectionAsync();  
            //}
            //catch (MobileServiceInvalidOperationException e)
            //{
            //    exception = e;
            //}

            //if (exception != null)
            //{
            //    await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            //}
            //else
            //{
            //    ListItems.ItemsSource = items;
            //    this.ButtonSave.IsEnabled = true;
            //}    

            // Comment-out or delete the following lines of code.
            this.ButtonSave.IsEnabled = true;
            ListItems.ItemsSource = items;
        }
        private void UpdateCheckedTodoItem(TodoItem item)
        {
            //// This code takes a freshly completed TodoItem and updates the database. When the MobileService   
            //// responds, the item is removed from the list.  
            //// TODO: Mark this method as "async" and uncomment the following statement  
            // await todoTable.UpdateAsync(item);        
            items.Remove(item);
        }
        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTodoItems();
        } 
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var todoItem = new TodoItem { Text = TextInput.Text };
            InsertTodoItem(todoItem);
        }
        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            item.Complete = true;
            UpdateCheckedTodoItem(item);
        } 

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshTodoItems();
        }
    }
}
