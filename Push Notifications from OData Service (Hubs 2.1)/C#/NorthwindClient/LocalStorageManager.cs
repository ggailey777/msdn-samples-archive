using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace NorthwindClient
{
class LocalStorageManager
{
    private static ApplicationDataContainer GetLocalStorageContainer()
    {
        if (!ApplicationData.Current.LocalSettings
            .Containers.ContainsKey("InstallationContainer"))
        {
            // Create the storage container if it doesn't exist.
            ApplicationData.Current.LocalSettings
                .CreateContainer("InstallationContainer",
                ApplicationDataCreateDisposition.Always);
        }
        return ApplicationData.Current.LocalSettings
            .Containers["InstallationContainer"];
    }

    public static Registration Registration
    {
        get
        {
                var container = GetLocalStorageContainer();
                if (!container.Values.ContainsKey("Registration"))
                {
                    // Return a new registration object if we 
                    // don't have a registration stored.
                    return new Registration();                     
                }
                else
                {
                    // Deserialize the stored registration into an object.
                    var registration = 
                        JsonConvert.DeserializeObject<Registration>(
                        (string)container.Values["Registration"]);

                    if (registration == null)
                    {
                        // Not sure we should ever get here.
                        registration = new Registration();
                    }

                    return registration;
                }
        }

        set
        {
            // Serialize the registration and store in the container.
            var container = GetLocalStorageContainer();
            container.Values["Registration"] = 
                JsonConvert.SerializeObject(value);
        }
    }
}
}
