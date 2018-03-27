' ----------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
' ----------------------------------------------------------
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Data.Services.Client
Imports System.Linq
Imports System.Windows
Imports System.Windows.Data
Imports ODataWinPhoneQuickstart.Netflix
Public Class MainViewModel
    Implements INotifyPropertyChanged

    ' Define the client paging parameters.
    Private _pageSize As Integer = 20
    Private _currentPage As Integer = 0
    Private _totalCount As Integer

    ' Define the message string.
    Private _message As String = String.Empty

    ' Defines the root URI of the data service.
    Private Shared ReadOnly _rootUri As Uri = New Uri("http://odata.netflix.com/v1/Catalog/")

    Private _isDataLoaded As Boolean

    ' Define the typed DataServiceContext.
    Private _context As NetflixCatalog

    ' Define the binding collection for Titles.
    Private _titles As DataServiceCollection(Of Title)

    ' Gets and sets the collection of Title objects from the feed.
    Public Property Titles As DataServiceCollection(Of Title)

        Get
            Return _titles
        End Get
        Set(value As DataServiceCollection(Of Title))
            ' Set the Titles collection.
            _titles = value

            'Register a handler for the LoadCompleted callback.
            AddHandler _titles.LoadCompleted, AddressOf OnTitlesLoaded

            ' Raise the PropertyChanged events.
            NotifyPropertyChanged("Titles")
        End Set
    End Property
    ' Used to determine whether the data is loaded.
    Public Property IsDataLoaded As Boolean
        Get
            Return _isDataLoaded
        End Get
        Private Set(value As Boolean)
            _isDataLoaded = value
        End Set
    End Property
    ' Loads data when the application is initialized.
    Public Sub LoadData()
        ' Instantiate the context and binding collection.
        _context = New NetflixCatalog(_rootUri)

        ' Use the public property setter to generate a notification to the binding.
        Titles = New DataServiceCollection(Of Title)(_context)

        ' Load the data.    
        _titles.LoadAsync(GetQuery())
    End Sub
    'Displays data from the stored data context and binding collection 
    Public Sub LoadData(ByVal context As NetflixCatalog, ByVal titles As DataServiceCollection(Of Title))
        _context = context
        Me.Titles = titles
        _isDataLoaded = True
    End Sub
    ' Loads data for a specific page of Titles.
    Public Sub LoadData(ByVal page As Integer)
        _currentPage = page

        ' Reset the binding collection.
        titles = New DataServiceCollection(Of Title)(_context)

        ' Load the data.
        titles.LoadAsync(GetQuery())
    End Sub
    ' Gets the pages loaded text displayed in the UI.
    Public ReadOnly Property PagesLoadedText As String
        Get
            Return String.Format("Page {0} of {1}", _currentPage + 1, _
            CType(_totalCount / _pageSize, Integer))
        End Get
    End Property
    ' Gets and sets the current page, which is needed for tombstoning.
    Public Property CurrentPage As Integer
        Get
            Return _currentPage
        End Get
        Set(value As Integer)
            ' No need to report this change since we are not binding to it.
            _currentPage = value
        End Set
    End Property
    ' Gets and sets the total item count, which is needed for tombstoning.
    Public Property TotalCount As Integer
        Get
            Return _totalCount
        End Get
        Set(value As Integer)
            ' No need to report this change since we are not binding to it.
            _totalCount = value
        End Set
    End Property
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub OnTitlesLoaded(ByVal sender As Object, ByVal e As LoadCompletedEventArgs)
        If e.Error Is Nothing Then
            ' Make sure that we load all pages of the Customers feed.
            If Not titles.Continuation Is Nothing Then
                titles.LoadNextPartialSetAsync()
            End If

            ' Set the total page count, if we requested one.
            If e.QueryOperationResponse.Query _
                .RequestUri.Query.Contains("$inlinecount=allpages") Then
                _totalCount = CType(e.QueryOperationResponse.TotalCount, Integer)
            End If

            IsDataLoaded = True

            ' Update the pages loaded text binding.
            NotifyPropertyChanged("PagesLoadedText")
        Else
            ' Display the error message in the binding.
            Me.Message = e.Error.Message
        End If
    End Sub
    ' Private method that returns the page-specific query.
    Private Function GetQuery() As DataServiceQuery(Of Title)
        ' Get a query for the Titles feed from the context.
        Dim query As DataServiceQuery(Of Title) = _context.Titles

        If _currentPage.Equals(0) Then
            ' If this is the first page, then also include a count of all titles.
            query = query.IncludeTotalCount()
        End If

        ' Add paging to the query.
        query = TryCast(query.Skip(_currentPage * _pageSize) _
                .Take(_pageSize), DataServiceQuery(Of Title))

        Return query
    End Function
    'Calls into the DataServiceContext to get the URI of the media resource.
    Public Function GetReadStreamUri(ByVal entity As Object) As Uri
        ' Return the URI for the supplied media link entry.
        Return _context.GetReadStreamUri(entity)
    End Function
' Notifies the binding about a changed property value.
    Private Sub NotifyPropertyChanged(propertyName As String)
        ' Raise the PropertyChanged event.
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    ' Return a collection of key-value pairs to store in the application state.
    Public Function SaveState() As List(Of KeyValuePair(Of String, Object))
        If App.ViewModel.IsDataLoaded Then

            Dim stateList = New List(Of KeyValuePair(Of String, Object))()

            ' Create a new dictionary to store binding collections.
            Dim collections = New Dictionary(Of String, Object)()

            'Add the current Titles binding collection.
            collections("Titles") = App.ViewModel.titles

            ' Store the current context and binding collections in the view model state.
            stateList.Add(New KeyValuePair(Of String, Object)( _
                    "DataServiceState", DataServiceState.Serialize(_context, collections)))
            stateList.Add(New KeyValuePair(Of String, Object)("CurrentPage", CurrentPage))
            stateList.Add(New KeyValuePair(Of String, Object)("TotalCount", TotalCount))

            Return stateList
        Else
            Return Nothing            
        End If
    End Function

    ' Restores the view model state from the supplied state dictionary.
    Public Sub RestoreState(ByVal dictionary As IDictionary(Of String, Object))
        ' Create a dictionary to hold any stored binding collections.
        Dim titles As Object = Nothing
        Dim stateAsString As Object = Nothing

        If dictionary.TryGetValue("DataServiceState", stateAsString) Then
            ' Rehydrate the DataServiceState object from the serialization.
            Dim state As DataServiceState = _
                DataServiceState.Deserialize(CType(stateAsString, String))

            If state.RootCollections.TryGetValue("Titles", titles) Then
                ' Initialize the application with data from the DataServiceState.
                App.ViewModel.LoadData(CType(state.Context, NetflixCatalog), _
                    CType(titles, DataServiceCollection(Of Title)))

                ' Restore other view model data.
                _currentPage = CType(dictionary("CurrentPage"), Integer)
                _totalCount = CType(dictionary("TotalCount"), Integer)
            End If
        End If
    End Sub
    Public Property Message As String
        Get
            Return _message
        End Get
        Set(value As String)
            _message = value

            ' Raise the PropertyChanged event.
            NotifyPropertyChanged("Message")
        End Set
    End Property
End Class