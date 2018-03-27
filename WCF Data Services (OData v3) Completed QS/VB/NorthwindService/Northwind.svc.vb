Imports System.Data.Services
Imports System.Data.Services.Common
Imports System.Linq
Imports System.ServiceModel.Web
Imports System.ServiceModel

' Uncomment during debugging.
'<ServiceBehavior(IncludeExceptionDetailInFaults:=True)>
Public Class Northwind
    Inherits DataService(Of NorthwindEntities)

    ' This method is called only once to initialize service-wide policies.
    Public Shared Sub InitializeService(ByVal config As DataServiceConfiguration)
        ' Grant only the rights needed to support the client application.
        config.SetEntitySetAccessRule("Orders", EntitySetRights.AllRead _
             Or EntitySetRights.WriteMerge _
             Or EntitySetRights.WriteReplace)
        config.SetEntitySetAccessRule("Order_Details", EntitySetRights.AllRead _
            Or EntitySetRights.AllWrite)
        config.SetEntitySetAccessRule("Customers", EntitySetRights.AllRead)
        config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3

        ' Set to 'true' during debugging.
        config.UseVerboseErrors = False
    End Sub
End Class
