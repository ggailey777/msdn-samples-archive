using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using System.ServiceModel;
namespace NorthwindService
{
    // Uncomment during debugging.
    //[ServiceBehavior(IncludeExceptionDetailInFaults=true)]
    public class Northwind : DataService<NorthwindEntities>
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // Grant only the rights needed to support the client application.
            config.SetEntitySetAccessRule("Orders", EntitySetRights.AllRead
                 | EntitySetRights.WriteMerge
                 | EntitySetRights.WriteReplace);
            config.SetEntitySetAccessRule("Order_Details", EntitySetRights.AllRead
                | EntitySetRights.AllWrite);
            config.SetEntitySetAccessRule("Customers", EntitySetRights.AllRead);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;

            // Set to 'true' during debugging.
            config.UseVerboseErrors = false;
        }
    }
}
