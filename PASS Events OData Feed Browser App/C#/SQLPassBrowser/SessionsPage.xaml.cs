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
using System.ComponentModel;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;

namespace SQLPassBrowser
{
    public partial class SessionsPage : PhoneApplicationPage
    {
        public SessionsPage()
        {
            InitializeComponent();

            // Handle the light these visibility.
            if ((Visibility)Resources["PhoneLightThemeVisibility"] == System.Windows.Visibility.Visible)
            {
                ImageBrush imageBrush = new ImageBrush();
                Uri uri = new Uri("Images\\Page2_light.jpg", UriKind.RelativeOrAbsolute);
                imageBrush.ImageSource = new BitmapImage(uri);
                this.LayoutRoot.Background = imageBrush;
            }

            this.DataContext = App.ViewModel;            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsSessionDataLoaded)
            {
                App.ViewModel.LoadCompleted += new EventHandler<SourcesLoadCompletedEventArgs>(ViewModel_LoadCompleted);

                this.perfProgressBar.Visibility = System.Windows.Visibility.Visible;
                this.perfProgressBar.IsIndeterminate = true;

                App.ViewModel.LoadSessions(null);
            }            
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = (Selector)sender;
            if (selector.SelectedIndex == -1)
            {
                return;
            }

            // Open the details page for the given session.
            this.NavigationService.Navigate(
                new Uri("/SessionDetailsPage.xaml?selectedIndex=" + selector.SelectedIndex, UriKind.Relative));

            selector.SelectedIndex = -1;
        }

        void ViewModel_LoadCompleted(object sender, SourcesLoadCompletedEventArgs e)
        {
            // Turn off the progress bar.
            this.perfProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            this.perfProgressBar.IsIndeterminate = false;

            App.ViewModel.LoadCompleted -= ViewModel_LoadCompleted;  
        }
    }
}