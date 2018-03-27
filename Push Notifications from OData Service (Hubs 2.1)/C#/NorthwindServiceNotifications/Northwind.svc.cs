//------------------------------------------------------------------------------
// <copyright file="WebDataService.svc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;

//Add new using statements.
using System.ServiceModel;
using Newtonsoft.Json;
using Microsoft.ServiceBus.Notifications;
using System.Threading.Tasks;
using System.Configuration;

namespace NorthwindServiceNotifications
{
    //// Use for debugging.
    //[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Northwind : DataService<NorthwindEntities>
    {
        // Define the Notification Hubs client.
        private NotificationHubClient hubClient;

        // Create the client in the constructor.
        public Northwind()
        {
            ////Create a new Notification Hub client using the stored info.
            // hubClient = NotificationHubClient
                // .CreateClientFromConnectionString(
                // ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"],
                // ConfigurationManager.AppSettings["Microsoft.ServiceBus.NotificationHubName"]
                // );
        }

        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // Enable access to entity sets and service operations.
            config.SetEntitySetAccessRule("Orders", EntitySetRights.AllRead | EntitySetRights.AllWrite);
            config.SetEntitySetAccessRule("Order_Details", EntitySetRights.AllRead | EntitySetRights.AllWrite);
            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            config.SetServiceOperationAccessRule("RegisterForPushNotifications", ServiceOperationRights.ReadSingle);
            config.SetServiceOperationAccessRule("DeleteRegistrations", ServiceOperationRights.ReadSingle);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;

            //// Use for debugging.
            //config.UseVerboseErrors = true;

        }
        // Public-facing service operation method for registration.
        [WebGet]
        public string RegisterForPushNotifications(string registration)
        {
            // Get the registration info that we need from the request. 
            var registrationObject = JsonConvert.DeserializeObject<Registration>(registration);

            // Call the async registration method.
            var task = RegisterForPushNotificationsAsync(registrationObject);

            try
            {
                // Wait for the async task to complete.
                task.Wait();

                if (task.IsCompleted)
                {
                    return task.Result;
                }
                else
                {
                    throw new DataServiceException(500, "Registration timeout.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // Private async method that performs the actual registration, 
        // since we can't return Task<T> from a service operation.
        private async Task<string> RegisterForPushNotificationsAsync(
            Registration clientRegistration)
        {
            // If the client gave us a registration ID, use it to try and get the registration.
            RegistrationDescription currentRegistration = null;
            if (!string.IsNullOrEmpty(clientRegistration.RegistrationId))
            {
                // Try to get the current registration by ID.
                currentRegistration =
                    await hubClient.GetRegistrationAsync<RegistrationDescription>(
                    clientRegistration.RegistrationId);
            }

            // Update a current registration.
            if (currentRegistration != null)
            {
                // Update the current set of tags.
                foreach (string tag in clientRegistration.Tags)
                {
                    if (!currentRegistration.Tags.Contains(tag))
                    {
                        currentRegistration.Tags.Add(tag);
                    }
                }

                // We need to update each platform separately.
                switch (clientRegistration.Platform)
                {
                    case "windows":
                    case "win8": 
                        // In the Windows Store case we need to delete and recreate the 
                        // registration because there's no way to change from a toast template 
                        // to a native raw notification.
                        await DeleteRegistrationsAsync(currentRegistration.RegistrationId);
                        clientRegistration.RegistrationId = 
                            await createWindowsRegistration(clientRegistration, 
                            currentRegistration.Tags);
                        break;

                    case "wp":
                    case "windowsphone":
                        await DeleteRegistrationsAsync(currentRegistration.RegistrationId);
                        clientRegistration.RegistrationId = 
                            await createWindowsRegistration(clientRegistration, 
                            currentRegistration.Tags);
                        break;

                    case "ios":
                        var iosReg = currentRegistration as AppleRegistrationDescription;
                        // Update tags and device token.
                        iosReg.DeviceToken = clientRegistration.DeviceToken;
                        clientRegistration.RegistrationId =
                            (await hubClient.UpdateRegistrationAsync(iosReg)).RegistrationId;
                        break;
                }
            }
            // Create a new registration.
            else
            {      
                // Create an ISet<T> of the supplied tags.
                HashSet<string> tags = new HashSet<string>(clientRegistration.Tags);

                // We need to create each platform separately.
                switch (clientRegistration.Platform)
                {
                    case "windows":
                    case "win8":
                        // Call the method that creates Windows Store registrations.
                        clientRegistration.RegistrationId = await createWindowsRegistration(clientRegistration, tags);           
                        break;
                    case "ios":
                        var template = @"{""aps"":{""alert"":""$(message)""}, ""inAppMessage"":""$(message)""}";
                        clientRegistration.RegistrationId =
                            (await hubClient.CreateAppleTemplateRegistrationAsync(
                            clientRegistration.DeviceToken, template, tags)).RegistrationId;
                        break;
                    default:
                        throw new DataServiceException("Unsupported client platform.");       
                }
            }

            return clientRegistration.RegistrationId;
        }

        private async Task<string> createWindowsRegistration(Registration clientRegistration, ISet<string> tags)
        {
            string newId = string.Empty;

            // Register either a standard toast notification using templates or a raw noative notification
            // depending on what is requested buy the client.
            switch (clientRegistration.Type)
            {
                case "toast":
                    var template =
                                    @"<toast>
                                            <visual>
                                                <binding template=""ToastText04"">
                                                    <text id=""1"">{'Northwind data was ' + $(operation) + '...'}</text>
                                                    <text id=""2"">{'Feed: ' + $(feed)}</text>
                                                    <text id=""3"">{'Entity: ' + $(entity)}</text>
                                                </binding>  
                                            </visual>
                                        </toast>";

                    // Register for the templated toast notification, and return the ID.
                    newId= clientRegistration.RegistrationId =
                        (await hubClient.CreateWindowsTemplateRegistrationAsync(
                        clientRegistration.ChannelUri, template, tags)).RegistrationId;
                    break;
                case "auto-update":
                    // Register for the raw notification used by auto-updates, and return the ID.
                    newId = clientRegistration.RegistrationId = (await hubClient.CreateWindowsNativeRegistrationAsync(
                        clientRegistration.ChannelUri, tags)).RegistrationId;
                    break;
                default:
                    throw new DataServiceException("Unexpected notification type.");                    
            }
            return newId;
        }

        private async Task<string> createWindowsPhoneRegistration(Registration clientRegistration, ISet<string> tags)
        {
            string newId = string.Empty;

            // Register either a standard toast notification using templates or a raw noative notification
            // depending on what is requested buy the client.
            switch (clientRegistration.Type)
            {
                case "toast":
                    var template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                    "<wp:Notification xmlns:wp=\"WPNotification\">" +
                                        "<wp:Toast>" +
                                            "<wp:Text1>{'Northwind ' + $(feed) + ' data was ' + $(operation) + '...'}</wp:Text1>" +
                                            "<wp:Text2>{'Entity: ' + $(entity)}</wp:Text2>" +                                            
                                        "</wp:Toast>" +
                                    "</wp:Notification>";

                    // Register for the templated toast notification, and return the ID.
                    newId = clientRegistration.RegistrationId =
                        (await hubClient.CreateMpnsTemplateRegistrationAsync(
                        clientRegistration.ChannelUri, template, tags)).RegistrationId;
                    break;
                case "auto-update":
                    // Register for the raw notification used by auto-updates, and return the ID.
                    newId = clientRegistration.RegistrationId = (await hubClient.CreateMpnsNativeRegistrationAsync(
                        clientRegistration.ChannelUri, tags)).RegistrationId;
                    break;
                default:
                    throw new DataServiceException("Unexpected notification type.");
            }
            return newId;
        }

        [WebInvoke(Method = "POST")]
        public bool DeleteRegistrations(string registrationId)
        {
            // Call the async registration method.
            var task = DeleteRegistrationsAsync(registrationId);

            // Wait for the async task to complete.
            task.Wait();

            if (task.IsCompleted)
            {
                return task.Result;
            }
            else
            {
                throw new DataServiceException(500, "Registration timeout.");
            }
        }
        private async Task<bool> DeleteRegistrationsAsync(string registrationId)
        {
            bool success = true;
            try
            {
                // Try to delete the registration by ID.
                await hubClient.DeleteRegistrationAsync(registrationId);
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }        

        // Define a change interceptor for the Orders entity set.
        [ChangeInterceptor("Orders")]
        public void OnChangeOrders(Order order, UpdateOperations operations)
        {
            SendFeedNotification<Order>("Orders", order,
                new int[] { order.OrderID }, operations);
        }

        // Define a change interceptor for the Order_Details entity set.
        [ChangeInterceptor("Order_Details")]
        public void OnChangeOrder_Details(Order_Detail item, UpdateOperations operations)
        {
            var keys = new int[] { item.OrderID, item.ProductID };
            SendFeedNotification<Order_Detail>("Order_Details", item, keys, operations);
        }

        private async void SendFeedNotification<TEntity>(
            string feed, TEntity entity, int[] entityKeys, UpdateOperations operations)
        {
            var entityType = entity.GetType();
            string baseTypeName = entityType.BaseType.Name;
            string operationString = string.Empty;
            switch (operations)
            {
                case UpdateOperations.Change:
                    operationString = "updated";
                    break;
                case UpdateOperations.Add:
                    operationString = "added";
                    break;
                case UpdateOperations.Delete:
                    operationString = "deleted";
                    break;
            }
            string keysAsString = string.Empty;
            foreach (int key in entityKeys)
            {
                keysAsString += string.Format("{0},", key);
            }
            keysAsString = keysAsString.TrimEnd(',');
            var entityAsString =  string.Format("{0}({1})", baseTypeName, keysAsString);
            var message = string.Format("The entity '{0}' was {2} in the '{1}' feed.",
                entityAsString, feed, operationString);
            
            // Define the parameters for the notification templates.
            var parameters =
                new Dictionary<string, string>();
            parameters.Add("feed", feed);
            parameters.Add("operation", operationString);
            parameters.Add("entity", entityAsString);
            parameters.Add("message", message); // only used for iOS clients.
            
            // Send a cross-plaform notification by using templates, 
            // including toasts to Windows Store apps.       
            await hubClient.SendTemplateNotificationAsync(parameters, feed);   

            // Create and send a raw notification to auto-update 
            // any Windows Store or Windows Phone apps.
            var payload =  await JsonConvert.SerializeObjectAsync(parameters);

            try
            {
                // Create the WNS with the header for a raw notification.           
                WindowsNotification wns = new WindowsNotification(payload,
                    new Dictionary<string, string>() { { "X-WNS-Type", "wns/raw" } }, feed);
                wns.ContentType = "application/octet-stream";
                var result = await hubClient.SendNotificationAsync(wns);
                
                // Create the MPNS with the header for a raw notification. 
                MpnsNotification mpns = new MpnsNotification(payload,
                  new Dictionary<string, string>() { { "X-NotificationClass", "3" } }, feed);
                mpns.ContentType = "text/xml";
                await hubClient.SendNotificationAsync(mpns);
            }
            catch (ArgumentException ex)
            {
                // Send methods return an error response when the notification hub hasn't yet 
                // been configured for a particular client platform.
            }
        }
    }

    public class Registration
    {
        public string ChannelUri { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
        public string RegistrationId { get; set; }
        public ISet<string> Tags { get; set; }
        public string Type { get; set; }
    }
}
