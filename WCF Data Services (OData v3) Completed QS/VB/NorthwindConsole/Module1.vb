Imports System.Data.Services.Client
Imports NorthwindConsole.Northwind
Module Module1

    Sub Main()
        ' Define the URI of the public Northwind OData service.
        Dim northwindUri As Uri = _
            New Uri("http://services.odata.org/Northwind/Northwind.svc/", _
                UriKind.Absolute)

        ' Define a customer for filtering.
        Const customer As String = "ALFKI"

        ' Create a new instance of the typed DataServiceContext.
        Dim context As NorthwindEntities = _
            New NorthwindEntities(northwindUri)

        ' Create a LINQ query to get the orders, including line items, 
        ' for the selected customer.
        Dim query = From order In context.Orders.Expand("Order_Details") _
                    Where order.CustomerID = customer _
                    Select order
        Try
            Console.WriteLine("Writing order ID and line item information...")

            ' Enumerating returned orders sends the query request to the service.
            For Each o As Order In query

                Console.WriteLine("Order ID: {0}", o.OrderID)

                For Each item As Order_Detail In o.Order_Details

                    Console.WriteLine(vbTab & "Product ID: {0} -- Quantity: {1}", _
                        item.ProductID, item.Quantity)
                Next
            Next
        Catch ex As DataServiceQueryException
            Console.WriteLine(ex.Message)
        End Try
    End Sub

End Module
