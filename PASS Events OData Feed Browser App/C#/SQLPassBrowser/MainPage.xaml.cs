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
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SQLPassBrowser.OratorModelNS;

namespace SQLPassBrowser
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Handle the light theme visibility.
            if ((Visibility)Resources["PhoneLightThemeVisibility"] == System.Windows.Visibility.Visible)
            {
                ImageBrush imageBrush = new ImageBrush();
                Uri uri = new Uri("Images\\SQLPASS_Banner_light.jpg", UriKind.RelativeOrAbsolute);
                imageBrush.ImageSource = new BitmapImage(uri);
                this.Panorama.Background = imageBrush;
            }

            this.DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsEventDataLoaded  && !App.ViewModel.IsDataLoading)
            {
                // Check for network availability.
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    App.ViewModel.LoadCompleted += new EventHandler<SourcesLoadCompletedEventArgs>(ViewModel_LoadCompleted);

                    this.perfProgressBar.Visibility = System.Windows.Visibility.Visible;
                    this.perfProgressBar.IsIndeterminate = true;

                    App.ViewModel.LoadData(true);
                }
                else
                {
                    MessageBox.Show("The PASS data service could not be reached. Restart the application when network connectivity is available.");
                    this.MessageTextBlock.Text = "Network connection unavailable.";
               }
            }
        }
 
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = (Selector)sender;
            if (selector.SelectedIndex == -1)
            {
                return;
            }

            // Navigate to the selected event details.
            this.NavigationService.Navigate(
                new Uri("/EventDetailsPage.xaml?selectedIndex=" + selector.SelectedIndex, UriKind.Relative));

            selector.SelectedIndex = -1;
        }

        void ViewModel_LoadCompleted(object sender, SourcesLoadCompletedEventArgs e)
        {
            // Turn off the progress bar.
            this.perfProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            this.perfProgressBar.IsIndeterminate = false;

            App.ViewModel.LoadCompleted -= ViewModel_LoadCompleted;
        }

        private void PassSiteLink_Click(object sender, RoutedEventArgs e)
        {
            // Open the PASS web site.
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://www.sqlpass.org/");
            browser.Show();
        }

        private void ODataSiteLink_Click(object sender, RoutedEventArgs e)
        {
            // Open the OData web site.
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://www.odata.org/");
            browser.Show();
        }

        private void ODataOnWpPage_Click(object sender, RoutedEventArgs e)
        {
            // Open the OData and WP library topic.
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://msdn.microsoft.com/en-us/library/gg521146(v=VS.92).aspx");
            browser.Show();       
        }

        private void AuthorLink_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://blogs.msdn.com/b/writingdata_services/");
            browser.Show(); 
        }

        private void ReviewLink_Click(object sender, RoutedEventArgs e)
        {
            // Open the review task.
            MarketplaceReviewTask mpTask = new MarketplaceReviewTask();
            mpTask.Show();            
        }

        private void ReportBugLink_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailTask = new EmailComposeTask();
            emailTask.To = "ggailey777@hotmail.com";
            emailTask.Subject = "I found a bug in the PASS Event Browser";
            emailTask.Body = "Please report the following information: page where the error occurred, what were you trying to do, and if you received an error message, what was it?";
            emailTask.Show();
        }

        private void ContactLink_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailTask = new EmailComposeTask();
            emailTask.To = "ggailey777@hotmail.com";
            emailTask.Subject = "About your PASS Event Browser";
            emailTask.Show();
        }
    }
}