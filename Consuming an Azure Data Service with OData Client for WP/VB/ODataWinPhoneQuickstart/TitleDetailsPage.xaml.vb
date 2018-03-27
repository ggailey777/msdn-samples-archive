' ----------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
' ----------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Navigation
Imports Microsoft.Phone.Controls
Imports ODataWinPhoneQuickstart.Netflix

Partial Public Class TitleDetailsPage
    Inherits PhoneApplicationPage
    Dim currentTitle As Title

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        Dim indexAsString = Me.NavigationContext.QueryString("selectedIndex")
        Dim index = Integer.Parse(indexAsString)
        Me.currentTitle _
                = CType(App.ViewModel.titles(index), Title)
        Me.DataContext = currentTitle
    End Sub
End Class
