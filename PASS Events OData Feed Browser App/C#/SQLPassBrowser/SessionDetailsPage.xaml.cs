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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using SQLPassBrowser.OratorModelNS;
using Microsoft.Phone.Tasks;

namespace SQLPassBrowser
{
    public partial class SessionDetailsPage : PhoneApplicationPage
    {
        private Session currentSession;

        public SessionDetailsPage()
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
            string indexAsString = this.NavigationContext.QueryString["selectedIndex"];
            int index = int.Parse(indexAsString);
            this.currentSession = App.ViewModel.CurrentSession
                = (Session)App.ViewModel.Sessions[index];

            // Check network availability.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                App.ViewModel.LoadCompleted += new EventHandler<SourcesLoadCompletedEventArgs>(Speakers_LoadCompleted);

                this.perfProgressBar.Visibility = System.Windows.Visibility.Visible;
                this.perfProgressBar.IsIndeterminate = true;

                App.ViewModel.LoadSpeakers(this.currentSession);
            }
            else
            {
                MessageBox.Show("The PASS data service could not be reached. Retry the operation when network connectivity is available.");

                NavigationService.GoBack();
            }
        } 

        private void SessionInfoOnlineButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSession.SessionURL != null)
            {
                // Opend the session URL in the browser.
                WebBrowserTask browser = new WebBrowserTask();
                browser.Uri = new Uri(currentSession.SessionURL);
                browser.Show();
            }
        }

        private void RateSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSession.SessionEvalURL != null)
            {
                // Opend the session Eval URL in the browser.
                WebBrowserTask browser = new WebBrowserTask();
                browser.Uri = new Uri(currentSession.SessionEvalURL);
                browser.Show();
            }
        }  

        void Speakers_LoadCompleted(object sender, SourcesLoadCompletedEventArgs e)
        {
            // Turn off the progress bar.
            this.perfProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            this.perfProgressBar.IsIndeterminate = false;

            if (App.ViewModel.SingleSpeaker == null)
            {
                // Switch to the ListBox instead of the scrollviewer for multiple speakers.
                ((PanoramaItem)(this.Panorama.Items[3])).Visibility = Visibility.Visible;
                ((PanoramaItem)(this.Panorama.Items[2])).Visibility = Visibility.Collapsed;
            }
            else
            {
                // Switch to the item for a single speaker.
                ((PanoramaItem)(this.Panorama.Items[2])).Visibility = Visibility.Visible;
                ((PanoramaItem)(this.Panorama.Items[3])).Visibility = Visibility.Collapsed;
            }

            App.ViewModel.LoadCompleted -= Speakers_LoadCompleted;

            App.ViewModel.LoadCompleted += new EventHandler<SourcesLoadCompletedEventArgs>(SessionFiles_LoadCompleted);
            
            // Turn back on the progress bar.
            this.perfProgressBar.Visibility = System.Windows.Visibility.Visible;
            this.perfProgressBar.IsIndeterminate = true;

            // Finally, load the session files.
            App.ViewModel.LoadSessionFiles(this.currentSession);
        }

        void SessionFiles_LoadCompleted(object sender, SourcesLoadCompletedEventArgs e)
        {
            // Turn off the progress bar.
            this.perfProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            this.perfProgressBar.IsIndeterminate = false;

            App.ViewModel.LoadCompleted -= SessionFiles_LoadCompleted;

            if (App.ViewModel.SessionFiles.Count > 0)
            {
                // Turn on the list header text if there are session files.
                this.SessionFilesText.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Reset the seletected session and speakers
            App.ViewModel.CurrentSession = null;
            App.ViewModel.Speakers.Clear();
            App.ViewModel.SingleSpeaker = null;
            App.ViewModel.SessionFiles.Clear();
        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = (Selector)sender;
            if (selector.SelectedIndex == -1)
            {
                return;
            }

            if (selector.SelectedItem != null)
            {
                SessionFile selectedFile = selector.SelectedItem as SessionFile;

                if (selectedFile.FileURL != null)
                {
                    // Try to get the selected file.
                    WebBrowserTask browser = new WebBrowserTask();
                    browser.Uri = new Uri(selectedFile.FileURL);
                    browser.Show();
                }
            }
        }
    }
}