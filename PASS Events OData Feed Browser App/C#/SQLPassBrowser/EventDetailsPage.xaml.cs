//   Copyright 2011 Glenn Gailey

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using SQLPassBrowser.OratorModelNS;

namespace SQLPassBrowser
{
    public partial class EventDetailsPage : PhoneApplicationPage
    {
        Event currentEvent;
 
        // Constructor
        public EventDetailsPage()
        {
            InitializeComponent();

            // Handle the visibility for the light theme.
            if ((Visibility)Resources["PhoneLightThemeVisibility"] == System.Windows.Visibility.Visible)
            {
                ImageBrush imageBrush = new ImageBrush();
                Uri uri = new Uri("Images\\SQLPASS_Banner_light.jpg", UriKind.RelativeOrAbsolute);
                imageBrush.ImageSource = new BitmapImage(uri);
                this.Panorama.Background = imageBrush;

                ImageBrush goImageBrush = new ImageBrush();
                Uri goUri = new Uri("Images\\goButton_light.png", UriKind.RelativeOrAbsolute);
                goImageBrush.ImageSource = new BitmapImage(goUri);
                this.BrowseToSessionsButton.Background = goImageBrush;
                this.SearchSessionsButton.Background = goImageBrush;
            }

            this.DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string indexAsString = this.NavigationContext.QueryString["selectedIndex"];
            int index = int.Parse(indexAsString);
            this.currentEvent = (Event)App.ViewModel.Events[index];

            if ((!App.ViewModel.IsSessionDataLoaded || this.currentEvent != App.ViewModel.CurrentEvent) && !App.ViewModel.IsDataLoading)
            {
                App.ViewModel.CurrentEvent = this.currentEvent;

                // Check the network status.
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    App.ViewModel.LoadCompleted += new EventHandler<SourcesLoadCompletedEventArgs>(Sessions_LoadCompleted);

                    this.perfProgressBar.Visibility = System.Windows.Visibility.Visible;
                    this.perfProgressBar.IsIndeterminate = true;

                    App.ViewModel.LoadSessions(this.currentEvent);

                    this.SearchText.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show("The PASS data service could not be reached. Retry the operation when network connectivity is available.");

                    NavigationService.GoBack();
                }
            }       
        }

        void Sessions_LoadCompleted(object sender, SourcesLoadCompletedEventArgs e)
        {
            // Turn off the progress bar.
            this.perfProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            this.perfProgressBar.IsIndeterminate = false;

            App.ViewModel.LoadCompleted -= Sessions_LoadCompleted;
        }
      
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton link = sender as HyperlinkButton;

            if (link != null)
            {
                WebBrowserTask browser = new WebBrowserTask();
                browser.Uri = new Uri(currentEvent.EventURL);
                browser.Show();
            }
        }
       
        private void BrowseToSessionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Start the query with set filters.
            BuildAndExecuteQuery();
        }

        private void SearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Start search from the keytap.
            if (e.Key == Key.Enter)
            {
                BuildAndExecuteQuery();               
            }
        }

        private void BuildAndExecuteQuery()
        {
            // Set any query filters and string.
            QueryHelper query = new QueryHelper();

            if (!this.sessionCategoryListPicker.SelectedIndex.Equals(0))
            {
                query.Category = this.sessionCategoryListPicker.SelectedItem as string;
            }

            if (!this.sessionLevelsListPicker.SelectedIndex.Equals(0))
            {
                query.Level = this.sessionLevelsListPicker.SelectedItem as string;
            }

            if (!this.sessionTracksListPicker.SelectedIndex.Equals(0))
            {
                query.Track = this.sessionTracksListPicker.SelectedItem as string;
            }

            if (!this.sessionDateListPicker.SelectedIndex.Equals(0))
            {
                query.Date = this.sessionDateListPicker.SelectedItem as string;
            }

            if (!this.SearchText.Equals(string.Empty))
            {
                query.QueryString = this.SearchText.Text;
            }

            App.ViewModel.QueryInfo = query;
            NavigationService.Navigate(new Uri("/SessionsPage.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (App.ViewModel.IsDataLoading)
            {
                // Try to cancel a paged load.
                App.ViewModel.TryCancelLoadAsync();
            }
        }

        // We need this to fix a bug in the Windows Phone SDK 7.1 version of the toolkit.
        // Remove this once the bug gets fixed.
        private void ListPicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListPicker lp = (ListPicker)sender;

            lp.Open();
        }        
    }
}