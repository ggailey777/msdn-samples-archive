using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Client;
using NorthwindConsole.Northwind;

namespace NorthwindConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the URI of the public Northwind OData service.
            Uri northwindUri =
                new Uri("http://services.odata.org/Northwind/Northwind.svc/",
                    UriKind.Absolute);

            // Define a customer for filtering.
            const string customer = "ALFKI";

            // Create a new instance of the typed DataServiceContext.
            NorthwindEntities context = new NorthwindEntities(northwindUri);

            // Create a LINQ query to get the orders, including line items, 
            // for the selected customer.
            var query = from order in context.Orders.Expand("Order_Details")
                        where order.CustomerID == customer
                        select order;
            try
            {
                Console.WriteLine("Writing order ID and line item information...");

                // Enumerating returned orders sends the query request to the service.
                foreach (Order o in query)
                {
                    Console.WriteLine("Order ID: {0}", o.OrderID);

                    foreach (Order_Detail item in o.Order_Details)
                    {
                        Console.WriteLine("\tProduct ID: {0} -- Quantity: {1}",
                            item.ProductID, item.Quantity);
                    }
                }
            }
            catch (DataServiceQueryException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
