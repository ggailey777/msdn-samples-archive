using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

//// TODO: Add the following using statement. 
//using Microsoft.WindowsAzure.MobileServices; 
//using Newtonsoft.Json; 
 
namespace GetStartedWithData 
{ 
    public class TodoItem 
    { 
        public string Id { get; set; } 
 
        //// TODO: Add the following serialization attribute. 
        //[JsonProperty(PropertyName = "text")] 
        public string Text { get; set; } 
 
        //// TODO: Add the following serialization attribute. 
        //[JsonProperty(PropertyName = "complete")] 
        public bool Complete { get; set; } 
    } 

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // TODO: Comment out the following line that defined the in-memory collection. 
        private ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>();

        //// MobileServiceCollection implements ObservableCollection for databinding  
        //// TODO: Uncomment the following two lines of code to replace the following collection with todoTable,  
        //// a proxy for the table in SQL Database. 
        // private MobileServiceCollection<TodoItem, TodoItem> items; 
        // private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>(); 

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
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
            //// TODO #1: Mark this method as "async" and uncomment the following statment 
            //// that defines a simple query for all items.  
            //items = await todoTable.ToCollectionAsync(); 

            //// TODO #2: More advanced query that filters out completed items.  
            //items = await todoTable 
            //   .Where(todoItem => todoItem.Complete == false) 
            //   .ToCollectionAsync(); 

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
            var todoItem = new TodoItem { Text = InputText.Text };
            InsertTodoItem(todoItem);
        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            item.Complete = true;
            UpdateCheckedTodoItem(item);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshTodoItems();
        }
    }
}
