using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using NorthwindClient.Northwind;
using System.Data.Services.Client;
using Windows.UI.Popups;
using Windows.Networking.PushNotifications;
using Newtonsoft.Json;
using Windows.UI;
using Newtonsoft.Json.Linq;

namespace NorthwindClient
{
    public sealed partial class MainPage : Page
    {
        IAsyncOperation<IUICommand> asyncMessagDialog;

        // This should be turned on unless the point is to demo auto-sync on a single machine,
        // otherwise your changes will trigger auto-sync in the same client that made the changes.
        Boolean enableLoopbackDetection = false;
        DateTime syncStartTime;
        TimeSpan  loopbackInterval = new TimeSpan(0,0,5); // 5 second interval.

        public MainPage()
        {
            InitializeComponent();

            // Set the DataContext for the page to the view model for the app instance. 
            this.DataContext = App.ViewModel;

            // Default to toast notifications.
            notificationType.SelectedIndex = 0;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Try to load the initial data set.
            if (!await App.ViewModel.LoadCustomers())
            {
                // If we fail to load, display a message dialog.
                ShowDialog(App.ViewModel.Message);
            }
            else
            {
                if (App.ViewModel.Customers.Count > 0)
                {
                    // Select the first item so that loads cascade.
                    this.customerIDComboBox.SelectedIndex = 0;
                }
                else
                {
                    // If we fail to load, display a message dialog.
                    ShowDialog(App.ViewModel.Message);
                }
            }
        }
        private async void customerIDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentCustomer = e.AddedItems[0] as Customer;
            if (currentCustomer != null && currentCustomer.Orders.Count == 0)
            {
                // Load the orders for the selected customer.
                if (!await App.ViewModel.LoadOrders(currentCustomer, false))
                {
                    // If we fail to load, display a message dialog.
                    ShowDialog(App.ViewModel.Message);
                }
                else
                {
                    // Select the first order in the list.
                    this.ordersListView.SelectedIndex = 0;
                }
            }
        }
        private async void ordersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.itemsAndButtons.Visibility = Visibility.Visible;
            var currentOrder = ((ListView)sender).SelectedItem as Order;
            App.ViewModel.CurrentOrder = currentOrder;
            if (currentOrder != null)
            {
                // Load the orders for the selected customer.
                if (!await App.ViewModel.LoadItems(currentOrder, false))
                {
                    // If we fail to load, display a message dialog.
                    ShowDialog(App.ViewModel.Message);
                }
            }
        }
        private async void saveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = true;
            string message = string.Empty;
            success = await App.ViewModel.SaveChangesAsync(this.Dispatcher);

            // Save changes to the data service.
            if (!success)
            {
                // If we fail to load, display a message dialog.
                message = App.ViewModel.Message;
                ShowDialog(message);
            }
            else
            {
                // Set the sync time for feedback detection.
                syncStartTime = DateTime.Now;
            }
        }
        private async void discardButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the stored registration info.
            var registration = LocalStorageManager.Registration;
            if (registration != null)
            {
                // Request to remove all existing registrations.
                var success = await App.ViewModel.DeleteRegistrationsAsync(registration);
                if (success)
                {
                    LocalStorageManager.Registration = null;
                }
            }
        }
        private void addItemButton_Click(object sender, RoutedEventArgs e)
        {
            this.productSelector.Visibility = Visibility.Visible;
        }
        private async void deleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Verify deletion.
            var dialog = new MessageDialog("Deleted selected item?");
            dialog.Commands.Add(new UICommand("OK"));
            dialog.Commands.Add(new UICommand("Cancel"));
            if ((await dialog.ShowAsync()).Label == "Cancel") return;

            // Removing an item from the collection propogates as a delete.  
            App.ViewModel.Items.Remove(App.ViewModel.CurrentItem);
        }
        private void itemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            var currentItem = listView.SelectedItem as Order_Detail;
            App.ViewModel.CurrentItem = currentItem;

            if (currentItem != null)
            {
                // Make sure we scroll into focus when adding new items to a long list.
                listView.ScrollIntoView(currentItem, ScrollIntoViewAlignment.Leading);
            }
        }
        private void productSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null)
            {
                var selectedProduct = combo.SelectedItem as Product;
                if (selectedProduct != null)
                {
                    // Selecting a product from the combobox triggers an addition.
                    var newItem = App.ViewModel.AddItemToOrder(selectedProduct);
                    this.productSelector.Visibility = Visibility.Collapsed;
                    this.itemsListView.SelectedItem = newItem;
                    this.productSelector.SelectedIndex = -1;
                }
            }
        }
        private void registerFeedName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            feedRegistrationTextBlock.Visibility = Visibility.Collapsed;
        }
        private async void registerButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the stored registration info. If an existing restration 
            // doesn't exist, we will get a new registration.
            var registration = LocalStorageManager.Registration;

            // Get the current channel from the services that distributes these.
            var channel = await PushNotificationChannelManager
                .CreatePushNotificationChannelForApplicationAsync();

            if (channel == null)
            {
                // If we can't get a channel, we need to warn the user and return.
                var dialog =
                    new MessageDialog(
                        "A push notification channel could not be obtained.");
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
                return;
            }

            // Set the properties of the new or current registration.
            registration.ChannelUri = channel.Uri.ToString();
            registration.Platform = "win8";

            // Get the notification type, which defaults to toast.
            // In a full-functional app, this setting should be read from the stored registration during binding.
            registration.Type = ((ComboBoxItem)notificationType.SelectedItem).Content.ToString().ToLower();

            if (registerFeedName.SelectedIndex == -1)
            {
                // User needs to select a feed--
                // another option is to disable the button before selection. 
                feedRegistrationTextBlock.Visibility = Visibility.Visible;
                feedRegistrationTextBlock.Text = "You must select a feed for registration:";
                feedRegistrationTextBlock.Foreground =
                    new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                // Add the selected feed to the tags array.
                string selectedFeed =
                    ((ComboBoxItem)registerFeedName.SelectedItem).Content.ToString();
                if (registration.Tags == null)
                {
                    registration.Tags = new HashSet<string> { selectedFeed };
                }
                else
                {
                    // Add the feed to the tag collection, if it's not already there.
                    if (!registration.Tags.Contains(selectedFeed))
                    {
                        registration.Tags.Add(selectedFeed);
                    }
                }
            }
            try
            {
                // Send the async request for registration to the data service.
                registration =
                    await App.ViewModel.RegisterForPushNotificationsAsync(registration);

                // Store the returned registration. 
                LocalStorageManager.Registration = registration;
                feedRegistrationTextBlock.Visibility = Visibility.Visible;
                feedRegistrationTextBlock.Text = App.ViewModel.Message;

                // Register for notification received event so that we can give the user
                // the option to update the feed when the app is running.
                channel.PushNotificationReceived += channel_PushNotificationReceived;

            }
            catch (DataServiceQueryException ex)
            {
                ShowDialog(ex.InnerException.Message);
            }
        }
        private async void channel_PushNotificationReceived(PushNotificationChannel sender,
     PushNotificationReceivedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {

                if (args.NotificationType == PushNotificationType.Raw)
                {
                    // Check if feedback detection is enabled.
                    if (enableLoopbackDetection)
                    {
                        // Return if we are still within the feedback interval.
                        if (DateTime.Now.Subtract(syncStartTime) < loopbackInterval )
                        {
                            return;
                        }
                    }

                    // We can't handle more than one message box at a time.
                    // This is a weakness in the current design.
                    if (asyncMessagDialog != null)
                    {
                        return;
                        //await asyncMessagDialog;
                    }
                    Notification update =
                        JsonConvert.DeserializeObject<Notification>(args.RawNotification.Content);

                    var dialog = new MessageDialog(string.Format("The entity'{0}' was {1}.\n"
                        + "Do you want to reload the feed from the Northwind service?",
                        update.Entity, update.Operation, update.Feed),
                        string.Format("The {0} feed has changed...", update.Feed));
                    dialog.Commands.Add(new UICommand("Reload"));
                    dialog.Commands.Add(new UICommand("Ignore"));
                    asyncMessagDialog = dialog.ShowAsync();

                    var result = await asyncMessagDialog;

                    if (result.Label == "Reload")
                    {
                        // Attempt to reload the current collection of the updated entity type.
                        switch (update.Feed)
                        {
                            case "Orders":
                                await App.ViewModel.LoadOrders();
                                break;
                            case "Order_Details":
                                await App.ViewModel.LoadItems();
                                break;
                            case "Products":
                                await App.ViewModel.LoadProducts();
                                break;
                            case "Customers":
                                await App.ViewModel.LoadCustomers();
                                break;
                            default:
                                ShowDialog(string.Format("The feed {0} could not be synchronized.", update.Feed));
                                break;
                        }
                    }
                    asyncMessagDialog = null;
                }

            });
        }
        private async void ShowDialog(string message)
        {
            // Common dialog used for errors.
            if (!string.IsNullOrEmpty(message))
            {
                if (asyncMessagDialog != null)
                {
                    await asyncMessagDialog;
                }
                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                asyncMessagDialog = dialog.ShowAsync();
            }
        }
    }
}
