using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Services.Client;
using NorthwindClient.Northwind;

namespace NorthwindClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private NorthwindEntities context;
        private string customerId = "ALFKI";

        // Replace the host server and port number with the values 
        // for the test server hosting your Northwind data service instance.
        private Uri svcUri = new Uri("http://localhost:12345/Northwind.svc");

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Instantiate the DataServiceContext.
                context = new NorthwindEntities(svcUri);

                // Define a LINQ query that returns Orders and 
                // Order_Details for a specific customer.
                var ordersQuery = from o in context.Orders.Expand("Order_Details")
                                  where o.Customer.CustomerID == customerId
                                  select o;

                // Create an DataServiceCollection<T> based on 
                // execution of the LINQ query for Orders.
                DataServiceCollection<Order> customerOrders = new
                    DataServiceCollection<Order>(ordersQuery);

                // Make the DataServiceCollection<T> the binding source for the Grid.
                this.orderItemsGrid.DataContext = customerOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void buttonSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save changes made to objects tracked by the context.
                context.SaveChanges();
            }
            catch (DataServiceRequestException ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
