using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Services.Client;
using Newtonsoft.Json;
using NorthwindClient.Northwind;
using System.Net;
using Microsoft.Data.OData;

namespace NorthwindClient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // This provides a base filter to reduce data transfers.
        private const string customerCountry = "USA";

        // If you change the service URI, then update this constant.
        private const string svcUri = "http://localhost:51440/Northwind.svc/";

        public MainViewModel()
        {
            // Instantiate the client context.
            _context = new NorthwindEntities(new Uri(svcUri));

            // Instantiate the root binding collections.
            Customers = new DataServiceCollection<Customer>(_context);
            Products = new DataServiceCollection<Product>(_context);
        }

        #region Data access methods:
        public async Task<bool> LoadCustomers()
        {
            bool success = false;

            // Create a LINQ query that returns customers with related orders.
            var customerQuery = from cust in _context.Customers
                                where cust.Country == customerCountry
                                select cust;
            try
            {
                // Load products.
                await LoadProducts();

                // Create a new collection for binding based on the LINQ query.
                await this.Customers.LoadAsync<Customer>(customerQuery);

                while (_customers.Continuation != null)
                {
                    // Load all remaining pages of the response.
                    await this.Customers.LoadAsync<Customer>(_customers.Continuation.NextLinkUri);
                }

                if (Customers.Count == 0)
                {
                    Message = string.Format("Customers could not be retrieved for country '{0}'.", customerCountry);
                    success = false;
                }
                else
                {
                    success = true;
                }
            }
            catch (DataServiceQueryException ex)
            {
                this.Message = "The query could not be completed:\n" + ex.ToString();
            }
            catch (InvalidOperationException ex)
            {
                this.Message = "The following error occurred:\n" + ex.ToString();
            }

            return success;
        }

        public async Task<bool> LoadOrders(Customer currentCustomer, bool reload)
        {
            bool success = false;

            this._currentCustomer = currentCustomer;

            if (reload || currentCustomer.Orders.Count == 0)
            {
                try
                {
                    // Load the first page of related orders for the selected customer.
                    await currentCustomer.Orders.LoadAsync<Order>();

                    while (currentCustomer.Orders.Continuation != null)
                    {
                        // Load all remaining pages.
                        await currentCustomer.Orders.LoadAsync<Order>(currentCustomer.Orders.Continuation.NextLinkUri);
                    }
                    success = true;
                    this.Orders = currentCustomer.Orders;
                }
                catch (Exception ex)
                {
                    this.Message = ex.ToString();
                }
            } 
            return success;
        }

        public async Task<bool> LoadOrders()
        {
            return await this.LoadOrders(this._currentCustomer, true);
        }

        public async Task<bool> LoadItems(Order currentOrder, bool reload)
        {
            bool success = true;

            if (reload || currentOrder.Order_Details.Count == 0)
            {
                try
                {
                    // Load the first page of related orders for the selected customer.
                    await currentOrder.Order_Details.LoadAsync<Order_Detail>();

                    while (currentOrder.Order_Details.Continuation != null)
                    {
                        // Load all remaining pages.
                        await currentOrder.Order_Details.LoadAsync<Order_Detail>(currentOrder.Order_Details.Continuation.NextLinkUri);
                    }                    
                }
                catch (Exception ex)
                {
                    this.Message = ex.ToString();
                    success = false;
                }
            }
            this.Items = currentOrder.Order_Details;
            return success;
        }

        public async Task<bool> LoadItems()
        {
            return await LoadItems(_currentOrder, true);
        }

        public async Task<bool> LoadProducts()
        {
            return await LoadProducts(false);
        }
        public async Task<bool> LoadProducts(bool reload)
        {
            bool success = true;
            try
            {
                // First try and load Products.
                if (reload || Products.Count == 0)
                {
                    // Load the first page of products.                
                    await Products.LoadAsync<Product>(_context.Products);

                    while (Products.Continuation != null)
                    {
                        // Load all remaining pages.
                        await Products.LoadAsync<Product>(Products.Continuation.NextLinkUri);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Message = ex.ToString();
                success = false;
            }
            return success;
        }

        public Order_Detail AddItemToOrder(Product product)
        {
            // Create a new item with relationships to the 
            // current order and supplied product.
            Order_Detail item = new Order_Detail();
            item.Product = product;
            item.ProductID = product.ProductID;
            item.OrderID = CurrentOrder.OrderID;
            item.Quantity = 1; // Set a default quantity to avoid server errors.
            this.CurrentOrder.Order_Details.Add(item);

            // Return the new item so that we can select it in the UI.
            return item;
        }
        public async Task<bool> SaveChangesAsync(Windows.UI.Core.CoreDispatcher dispatcher)
        {
            bool success = true;
            try
            {
                var response = await _context.SaveChangesAsync(SaveChangesOptions.ReplaceOnUpdate, dispatcher);
                foreach (var or in response)
                {
                    if (or.Error != null)
                    {
                        this.Message = "Changes could not be saved.";
                        success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Message = ex.InnerException.Message;
                success = false;
            }            
            return success;
        }
        #endregion
        #region Notification registration methods:
        public async Task<Registration>
            RegisterForPushNotificationsAsync(Registration registration)
        {
            // Serialize the Registration as JSON.
            var registrationJson = JsonConvert.SerializeObject(registration);
            string message = string.Empty;
            try
            {              
                // Calling FirstOrDefault executes the request 
                // to return a singleton registration ID.
                var result = (await
                    _context.ExecuteAsync<string>(new Uri(new Uri(svcUri),
                    "RegisterForPushNotifications"), "GET", true,
                    new UriOperationParameter("registration",
                        registrationJson))).FirstOrDefault();

                if (!string.IsNullOrEmpty(result))
                {
                    // Store the current state of the registration. 
                    registration.RegistrationId = result;


                    this.Message = string.Format("Registration ID: {0}", result);
                }
            }
            catch (Exception ex)
            {
                this.Message = ex.Message;
            }
            return registration;
        }

        public async Task<bool> DeleteRegistrationsAsync(Registration registration)
        {
            bool success = true;
            if (registration.RegistrationId != null)
            {
                try
                {
                    // Calling FirstOrDefault executes the request to return a singleton Boolean value.
                    success = (await _context.ExecuteAsync<bool>(new Uri(new Uri(svcUri),
                        "DeleteRegistrations"), "POST", true,
                        new UriOperationParameter("registrationId", registration.RegistrationId))).FirstOrDefault();
                }
                catch (DataServiceRequestException ex)
                {
                    this.Message = ex.Message;
                    success = false;
                }
            }
            return success;
        }
        #endregion
        #region Properties for binding:
        private NorthwindEntities _context;
        public DataServiceContext Context
        {
            get { return _context; }
        }
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyPropertyChanged("Message");
            }
        }

        private DataServiceCollection<Customer> _customers;
        public DataServiceCollection<Customer> Customers
        {
            get { return _customers; }
            set
            {
                _customers = value;
                NotifyPropertyChanged("Customers");
            }
        }

        private DataServiceCollection<Order> _orders;
        public DataServiceCollection<Order> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                NotifyPropertyChanged("Orders");
            }
        }
        private DataServiceCollection<Order_Detail> _items;
        public DataServiceCollection<Order_Detail> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyPropertyChanged("Items");
            }
        }
        private DataServiceCollection<Product> _products;
        public DataServiceCollection<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                NotifyPropertyChanged("Products");
            }
        }

        private Customer _currentCustomer;

        private Order _currentOrder;
        public Order CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                _currentOrder = value;
                NotifyPropertyChanged("CurrentOrder");
            }
        }
        
        private Order_Detail _currentItem;
        public Order_Detail CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                NotifyPropertyChanged("CurrentItem");
            }
        }
        #endregion
        #region INotifyPropertyChanged implementation:
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

}
