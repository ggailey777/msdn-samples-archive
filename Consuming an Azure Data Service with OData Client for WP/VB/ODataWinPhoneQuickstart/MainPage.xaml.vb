' ----------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
' ----------------------------------------------------------
Imports System
Imports System.ComponentModel
Imports System.Data.Services.Client
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Navigation
Imports Microsoft.Phone.Controls

Partial Public Class MainPage
    Inherits PhoneApplicationPage
    ' Constructor
    Public Sub New()
        InitializeComponent()

        ' Set the view to the view model.
        Me.DataContext = App.ViewModel
    End Sub
    Protected Overrides Sub OnNavigatedTo(ByVal e As NavigationEventArgs)
        If Not App.ViewModel.IsDataLoaded Then
            App.ViewModel.LoadData()
        Else
            If Me.NavigationContext.QueryString.Count.Equals(1) Then
                ' Get the value of the requested page.
                Dim page = Integer.Parse(Me.NavigationContext.QueryString("page"))

                ' Check to see if the page is currently loaded.
                If Not page = App.ViewModel.CurrentPage Then

                    ' Load data for the specific page.
                    App.ViewModel.LoadData(page)
                End If

            Else
                ' If there is no query parameter we are at the first page.
                App.ViewModel.LoadData(0)
            End If
        End If
    End Sub
    Private Sub MoreButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        If App.ViewModel.IsDataLoaded Then
            ' Navigate to the next page of data.
            Me.NavigationService.Navigate( _
               New Uri("/MainPage.xaml?page=" & CType(App.ViewModel.CurrentPage + 1, String), UriKind.Relative))
        End If
    End Sub
    Private Sub OnSelectionChanged(sender As System.Object, e As System.Windows.Controls.SelectionChangedEventArgs)
        Dim selector = CType(sender, Selector)
        If selector.SelectedIndex = -1 Then
            Return
        End If

        Me.NavigationService.Navigate( _
            New Uri("/TitleDetailsPage.xaml?selectedIndex=" & CType(selector.SelectedIndex, String), UriKind.Relative))
        selector.SelectedIndex = -1
    End Sub
End Class
