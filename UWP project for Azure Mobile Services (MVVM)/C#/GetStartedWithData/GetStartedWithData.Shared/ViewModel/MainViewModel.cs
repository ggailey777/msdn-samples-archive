using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using GetStartedWithData.DataModel;

using Microsoft.WindowsAzure.MobileServices; 

namespace GetStartedWithData.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        // TODO: Configure the MobileServiceClient to communicate with your mobile service by 
        // replacing AppUrl & AppKey in the following code with values from   
        // your mobile service, which are obtained from the Azure Management Portal. 
        private static MobileServiceClient MobileService = new MobileServiceClient(
            "AppUrl",
            "AppKey");

        // The following line of code defines todoTable, a proxy for the table in the SQL Database. 
        private IMobileServiceTable<TodoItem> todoTable = MobileService.GetTable<TodoItem>();

        // The Items property returns a MobileServiceCollection for data binding. 
        // MobileServiceCollection implements ObservableCollection for binding.  
        private MobileServiceCollection<TodoItem, TodoItem> _items;
        public MobileServiceCollection<TodoItem, TodoItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                NotifyPropertyChanged("Items");
            }
        }

        public async void RefreshTodoItems()
        {
            //// The following statment defines and executes a simple query for all items.  
            //Items = await todoTable.ToCollectionAsync(); 

            // The following query returns items, with completed ones filtered out.  
            Items = await todoTable
               .Where(todoItem => todoItem.Complete == false)
               .ToCollectionAsync();            
        }
        public async void InsertTodoItem(TodoItem item)
        {
            // This code inserts a new TodoItem into the database. When the operation completes 
            // and Mobile Services has assigned an Id, the item is added to the CollectionView 
            await todoTable.InsertAsync(item); 

            Items.Add(item);
        }

        public async void UpdateCheckedTodoItem(TodoItem item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService  
            // responds, the item is removed from the list. 
            await todoTable.UpdateAsync(item);       
            Items.Remove(item);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
