using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using SQLPassBrowser.OratorModelNS;
using System.Data.Services.Client;


namespace SQLPassBrowser
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string defaultCategory = "<All Categories>";
        private const string defaultLevel = "<All Levels>";
        private const string defaultTrack = "<All Tracks>";
        private const string defaultDate = "<All Dates>";
        
        private int pageSize = 25;
        private int pageCount;
        private QueryHelper _queryInfo;
 

        // Defines the root URI of the data service.
        private static readonly Uri _rootUri = new Uri("http://feeds.sqlpass.org/public/events/OratorDataService.svc/");

        // Define the typed DataServiceContext.
        private OratorEntities _context;

        // Define the binding collection for Events.
        private DataServiceCollection<Event> _events;

        // Define a base paged LINQ query for sessions.
        //DataServiceQuery<Session> sessionsQuery; 
        public MainViewModel()
        {
            // Instantiate the context and binding collection.
            _context = new OratorEntities(_rootUri);           
        }

        public string ServiceUriString
        {
            get
            {
                return _rootUri.ToString();
            }
        }

        public void LoadData(bool upcomingOnly)
        {           
            this.Message = string.Empty;

            _events = new DataServiceCollection<Event>(_context);

            // Register a handler for the LoadCompleted callback.
            _events.LoadCompleted += OnEventsLoaded;

            // Define a query for all events.
            var query = _context.Events.OrderBy(e=>e.EventStartDate);       

            // Load the data.
            _events.LoadAsync(query);

            // We have started loading data asynchronously.
            IsDataLoading = true;
        }

        private void OnEventsLoaded(object sender, LoadCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    // Make sure that we load all pages of the Content feed.
                    if (Events.Continuation != null)
                    {
                        Events.LoadNextPartialSetAsync();
                    }

                    // We have loaded all the events.
                    IsEventDataLoaded = true;
                    IsDataLoading = false;

                    if (this.Events.Count == 0)
                    {
                        this.Message = "No events found.";
                    }
                }
                else
                {
                    IsEventDataLoaded = false;
                    IsDataLoading = false;

                    // Set the message
                    this.Message = e.Error.Message;
                }

                // Deregister a handler for the LoadCompleted callback.
                _events.LoadCompleted -= OnEventsLoaded;

                if (LoadCompleted != null)
                {
                    // Notify the UI that we are done.
                    LoadCompleted(this, new SourcesLoadCompletedEventArgs(e.Error));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void LoadSessions(Event selectedEvent)
        {
            this.Message = string.Empty;

            // Create a new sessions collection.
            Sessions = new DataServiceCollection<Session>(_context);

             // Register a handler for the LoadCompleted callback.
             Sessions.LoadCompleted += OnSessionsLoaded;

            if (selectedEvent != null)
            {
                this.pageCount = 0;                        
                      
                // Define a query to load the first page of Sessions, with total count.
                var query = this.BuildSessionsQuery().IncludeTotalCount();

                // Execute the query asynchronously.
                this.Sessions.LoadAsync(query);

                IsDataLoading = true;
            }
            else
            {
                // Get a query for all sessions
                var query = _context.Sessions;
               
                // Add query filters.
                if (this.QueryInfo.Category != null && !this.QueryInfo.Category.Equals(string.Empty))
                {
                    query = query.Where(s => s.SessionCategory.Equals(this.QueryInfo.Category)) as DataServiceQuery<Session>;                  
                }

                if (this.QueryInfo.Track != null && !this.QueryInfo.Track.Equals(string.Empty))
                {
                    query = query.Where(s => s.SessionTrack.Equals(this.QueryInfo.Track)) as DataServiceQuery<Session>;                     
                }

                if (this.QueryInfo.Level != null && !this.QueryInfo.Level.Equals(string.Empty))
                {
                    query = query.Where(s => s.SessionLevel.Equals(this.QueryInfo.Level)) as DataServiceQuery<Session>; 
                }

                if (this.QueryInfo.Date != null && !this.QueryInfo.Date.Equals(string.Empty))
                {
                    DateTime date = DateTime.Parse(this.QueryInfo.Date);

                    query = query.Where(s => s.SessionDateTimeStart < date.AddDays(1) && s.SessionDateTimeStart >= date) as DataServiceQuery<Session>;                    
                }

                if (this.QueryInfo.QueryString != null && !this.QueryInfo.QueryString.Equals(string.Empty))
                {                   
                    // Search description and title.
                    query = query.Where(s => s.SessionDescription.Contains(this.QueryInfo.QueryString)
                        | s.SessionName.Contains(this.QueryInfo.QueryString)) as DataServiceQuery<Session>;                    
                }

                query = query.Where(s => s.EventID.Equals(CurrentEvent.EventID))
                    .OrderBy(s => s.SessionName) as DataServiceQuery<Session>;                              

                // Exceute the filtering query asynchronously.
                this.Sessions.LoadAsync(query);

                // Loading filtered sessions.
                IsDataLoading = true;
            }
        }

        private void OnSessionsLoaded(object sender, LoadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // Stop loading if user requested.
                if (!cancelLoadAsync)
                {


                    // Make sure that we load all pages of the Content feed.
                    if (Sessions.Continuation != null)
                    {
                        Sessions.LoadNextPartialSetAsync();
                    }

                    if (e.QueryOperationResponse.Query.RequestUri.Query.Contains("$inlinecount"))
                    {
                        if (e.QueryOperationResponse.Query.RequestUri.Query.Contains("$inlinecount=allpages"))
                        {
                            // Increase the page count by one.
                            pageCount += 1;

                            // This is the intial query for all sessions.
                            TotalSessionCount = (int)e.QueryOperationResponse.TotalCount;

                            CountMessage = string.Format("{0} total sessions.", TotalSessionCount);
                            NotifyPropertyChanged("CountMessage");
                        }
                        if (Sessions.Count < TotalSessionCount)
                        {
                            try
                            {
                                // We need to distinguish a query for all sessions, so we use $inlinecount
                                // even when we don't need it returned.
                                var query = this.BuildSessionsQuery().AddQueryOption("$inlinecount", "none");

                                // Load the next set of pages.
                                this.Sessions.LoadAsync(query);
                            }
                            catch (Exception ex)
                            {
                                this.Message = ex.Message.ToString();
                            }

                            // Increase the page count by one.
                            pageCount += 1;

                            BuildSessionFilters();
                        }
                        else
                        {
                            // We are done loading pages.
                            this.Sessions.LoadCompleted -= OnSessionsLoaded;
                            IsSessionDataLoaded = true;
                            IsDataLoading = false;

                            if (LoadCompleted != null)
                            {
                                LoadCompleted(this, new SourcesLoadCompletedEventArgs(e.Error));
                            }
                        }
                    }
                    else
                    {
                        // This is a filtered query.
                        CountMessage = string.Format("Returned {0} of {1} total sessions.", this.Sessions.Count, TotalSessionCount);
                        NotifyPropertyChanged("CountMessage");

                        IsDataLoading = false;
                        IsSessionDataLoaded = true;

                        this.Sessions.LoadCompleted -= OnSessionsLoaded;

                        if (LoadCompleted != null)
                        {
                            LoadCompleted(this, new SourcesLoadCompletedEventArgs(e.Error));
                        }
                    }

                    //IsSessionDataLoaded = true;

                    if (this.Sessions.Count == 0)
                    {
                        this.CountMessage = "No topics found.";
                    }
                }
                else
                {
                    // Set the message
                    this.Message = e.Error.Message;
                }
            }
            else
            {
                IsDataLoading = false;

                this.Sessions.LoadCompleted -= OnSessionsLoaded;

                // Raise the load completed event.
                if (LoadCompleted != null)
                {
                    LoadCompleted(this, new SourcesLoadCompletedEventArgs(e.Error));
                }
            }           
        }    

        public void LoadSpeakers(Session selectedSession)
        {
            if (selectedSession != null)
            {
                selectedSession.PASS_OData_Orator_SessionSpeaker.LoadCompleted 
                    += new EventHandler<LoadCompletedEventArgs>(OnSpeakersLoaded);

                this.Message = string.Empty;

                this.Speakers = new DataServiceCollection<SessionSpeaker>(_context);

                if (selectedSession.PASS_OData_Orator_SessionSpeaker.Count == 0)
                {
                    // Load the speakers from the parent DataServiceCollection<Session>.
                    selectedSession.PASS_OData_Orator_SessionSpeaker.LoadAsync();

                    //// Loading Speakers from a query works on reactivation (under FAS).
                    //// Define a filtered LINQ query for session speakers.
                    //var query = _context.SessionSpeakers.Where(ss=>ss.SessionID.Equals(selectedSession.SessionID));

                    //// Load the speakers asynchronously.               
                    //this.Speakers.LoadAsync(query);

                    // Data is loading.
                    this.IsDataLoading = true;
                }
                else
                {
                    // Report loading completed.
                    if (LoadCompleted != null)
                    {
                        LoadCompleted(this, new SourcesLoadCompletedEventArgs(null));
                    }
                }
            }
        }

        private void OnSpeakersLoaded(object sender, LoadCompletedEventArgs e)
        {
            var speakers = sender as DataServiceCollection<SessionSpeaker>;
            try
            {
                if (e.Error == null)
                {
                    // We handle single speakers differently in the UI.
                    if (speakers.Count == 1)
                    {
                        this.SingleSpeaker = speakers[0];
                    }
                    else
                    {
                        this.Speakers = speakers;
                    }
                }

                else
                {
                    // Set the message
                    this.Message = e.Error.Message;
                }

                speakers.LoadCompleted 
                    -= new EventHandler<LoadCompletedEventArgs>(OnSpeakersLoaded);

                if (LoadCompleted != null)
                {
                    LoadCompleted(this, new SourcesLoadCompletedEventArgs(e.Error));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void LoadSessionFiles(Session selectedSession)
        {
            if (selectedSession != null)
            {
                selectedSession.PASS_OData_Orator_SessionFile.LoadCompleted
                    += new EventHandler<LoadCompletedEventArgs>(OnSessionFilesLoaded);

                this.Message = string.Empty;

                this.SessionFiles = new DataServiceCollection<SessionFile>(_context);

                if (selectedSession.PASS_OData_Orator_SessionFile.Count == 0)
                {
                    // Load the speakers from the parent DataServiceCollection<Session>.
                    selectedSession.PASS_OData_Orator_SessionFile.LoadAsync();

                    //// Loading Speakers from a query works on reactivation (under FAS).
                    //// Define a filtered LINQ query for session speakers.
                    //var query = _context.SessionSpeakers.Where(ss=>ss.SessionID.Equals(selectedSession.SessionID));

                    //// Load the speakers asynchronously.               
                    //this.Speakers.LoadAsync(query);

                    // Data is loading.
                    this.IsDataLoading = true;
                }
                else
                {
                    // Report loading completed.
                    if (LoadCompleted != null)
                    {
                        LoadCompleted(this, new SourcesLoadCompletedEventArgs(null));
                    }
                }
            }
        }

        private void OnSessionFilesLoaded(object sender, LoadCompletedEventArgs e)
        {
            var files = sender as DataServiceCollection<SessionFile>;
            try
            {
                if (e.Error == null)
                {
                    this.SessionFiles = files;
                }

                files.LoadCompleted
                    -= new EventHandler<LoadCompletedEventArgs>(OnSpeakersLoaded);

                if (LoadCompleted != null)
                {
                    LoadCompleted(this, new SourcesLoadCompletedEventArgs(e.Error));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool cancelLoadAsync;

        public void TryCancelLoadAsync()
        {
            cancelLoadAsync = true;
        }

        public string CountMessage { get; private set; }
        public int TotalSessionCount = 0;

        
        private SessionSpeaker _singleSpeaker;

        public SessionSpeaker SingleSpeaker
        {
            get
            {
                return _singleSpeaker;
            }
            set
            {
                _singleSpeaker = value;

                NotifyPropertyChanged("SingleSpeaker");
            }
        }

        public DataServiceCollection<Event> Events
        {
            get
            {
                return _events;
            }
            private set
            {
                // Set the Events collection.
                _events = value;              

                NotifyPropertyChanged("Events");               
            }
        }
        private DataServiceCollection<Session> _sessions;
        public DataServiceCollection<Session> Sessions
        {

            get
            {
                return _sessions;
            }
            private set
            {
                // Set the Sessions collection.
                _sessions = value;

                NotifyPropertyChanged("Sessions");

                // Register a handler for the LoadCompleted callback.
               // _sessions.LoadCompleted += OnSessionsLoaded;
            }
        }

        private DataServiceCollection<SessionSpeaker> _speakers;
        public DataServiceCollection<SessionSpeaker> Speakers
        {

            get
            {
                return _speakers;
            }
            private set
            {
                // Set the Speakers collection.
                _speakers = value;

                NotifyPropertyChanged("Speakers");

                // Register a handler for the LoadCompleted callback.
               // _speakers.LoadCompleted += OnSpeakersLoaded;
            }
        }

        private DataServiceCollection<SessionFile> _sessionFiles;
        public DataServiceCollection<SessionFile> SessionFiles
        {

            get
            {
                return _sessionFiles;
            }
            private set
            {
                // Set the Speakers collection.
                _sessionFiles = value;

                NotifyPropertyChanged("SessionFiles");
            }
        }
 
        private List<string> _sessionTracks;

        public List<string> SessionTracks
        {
            get
            {
                return _sessionTracks;
            }
            set
            {
                _sessionTracks = value;

                NotifyPropertyChanged("SessionTracks");
            }
        }

        private List<string> _sessionCategories;

        public QueryHelper QueryInfo { 
            get
            { return _queryInfo; }
            set
            {
                _queryInfo = value;

                this.IsSessionDataLoaded = false;
            }
        }

        public List<string> SessionCategories
        {
            get
            {
                return _sessionCategories;
            }
            set
            {
                _sessionCategories = value;

                NotifyPropertyChanged("SessionCategories");
            }
        }

        private List<string> _sessionLevels;

        public List<string> SessionLevels
        {
            get
            {
                return _sessionLevels;
            }
            set
            {
                _sessionLevels = value;

                NotifyPropertyChanged("SessionLevels");
            }
        }

        private List<string> _sessionDates;

        public List<string> SessionDates
        {
            get
            {
                return _sessionDates;
            }
            set
            {
                _sessionDates = value;

                NotifyPropertyChanged("SessionDates");
            }
        }

        private DataServiceQuery<Session> BuildSessionsQuery()
        {
            return _context.Sessions
                  .Where(s => s.EventID.Equals(_currentEvent.EventID))
                  .Skip(pageCount * pageSize).Take(pageSize)
                  as DataServiceQuery<Session>;
        }

        private void BuildSessionFilters()
        {
            _sessionTracks = new List<string>();
            _sessionCategories = new List<string>();
            _sessionLevels = new List<string>();
            _sessionDates = new List<string>();

            foreach (Session s in this.Sessions)
            {
                if (s.SessionTrack!=null && !_sessionTracks.Contains(s.SessionTrack))
                {
                    _sessionTracks.Add(s.SessionTrack);
                }

                if (s.SessionCategory != null && !_sessionCategories.Contains(s.SessionCategory))
                {
                    _sessionCategories.Add(s.SessionCategory);
                }

                if (s.SessionLevel != null && !_sessionLevels.Contains(s.SessionLevel))
                {
                    _sessionLevels.Add(s.SessionLevel);
                }

                string dateString = ((DateTime)(s.SessionDateTimeStart)).ToShortDateString();

                if (s.SessionDateTimeStart != null && !_sessionDates.Contains(dateString))
                {
                    _sessionDates.Add(dateString);
                }
            }

            _sessionCategories.Sort();
            _sessionCategories.Insert(0, defaultCategory);
            _sessionLevels.Sort();
            _sessionLevels.Insert(0, defaultLevel);
            _sessionTracks.Sort();
            _sessionTracks.Insert(0, defaultTrack);
            _sessionTracks.Sort();
            _sessionDates.Insert(0, defaultDate);
            _sessionDates.Sort();

            NotifyPropertyChanged("SessionTracks");
            NotifyPropertyChanged("SessionCategories");
            NotifyPropertyChanged("SessionLevels");
            NotifyPropertyChanged("SessionDates");
        }

        private Event _currentEvent;

        public Event CurrentEvent
        {
            get
            {
                return _currentEvent;
            }
            set
            {
                _currentEvent = value;

                NotifyPropertyChanged("CurrentEvent");
            }
        }

        private Session _currentSession;

        public Session CurrentSession
        {
            get
            {
                return _currentSession;
            }
            set
            {
                _currentSession = value;

                NotifyPropertyChanged("CurrentSession");
            }
        }

        private string _message = string.Empty;

        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }

        public bool IsEventDataLoaded { get; private set; }
        public bool IsSessionDataLoaded { get; private set; }
        public bool IsDataLoading { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    

        public event EventHandler<SourcesLoadCompletedEventArgs> LoadCompleted;

        public List<KeyValuePair<string, object>> SaveState()
        {
            Uri storageUri;

            List<KeyValuePair<string, object>> stateList = new List<KeyValuePair<string, object>>();

            Dictionary<string, object> collections = new Dictionary<string, object>();        

            if (this.Events != null)
            {
                collections.Add("Events", this.Events);

                if (this.Sessions != null)
                {
                    collections.Add("Sessions", this.Sessions);
                 
                    if (this.Speakers != null)
                    {
                        collections.Add("Speakers", this.Speakers);
                    }

                    if (this.SessionFiles != null)
                    {
                        collections.Add("SessionFiles", this.SessionFiles);
                    }
                }
            }

            // Add the DataServiceState to the collection.
            stateList.Add(new KeyValuePair<string, object>("DataServiceState", DataServiceState.Serialize(_context, collections)));

            // We can't store entities directly, so store the URI instead.
            if (CurrentSession != null && _context.TryGetUri(CurrentSession, out storageUri))
            {
                // Store the URI of the CurrentSession.
                stateList.Add(new KeyValuePair<string, object>("CurrentSession", storageUri));
            }

            if (CurrentEvent != null && _context.TryGetUri(CurrentEvent, out storageUri))
            {
                // Store the URI of the CurrentEvent.
                stateList.Add(new KeyValuePair<string, object>("CurrentEvent", storageUri));
            }

            if (SingleSpeaker != null && _context.TryGetUri(SingleSpeaker, out storageUri))
            {
                // Store the URI of the Speaker.
                stateList.Add(new KeyValuePair<string, object>("SingleSpeaker", storageUri));
            }
   
            // Add other non-entity binding properties to the collection.          
            stateList.Add(new KeyValuePair<string, object>("TotalSessionCount", TotalSessionCount));
            stateList.Add(new KeyValuePair<string, object>("Message", Message));
            stateList.Add(new KeyValuePair<string, object>("CountMessage", CountMessage));
            stateList.Add(new KeyValuePair<string, object>("QueryInfo", QueryInfo));
            stateList.Add(new KeyValuePair<string, object>("SessionCategories", SessionCategories));
            stateList.Add(new KeyValuePair<string, object>("SessionDates", SessionDates));
            stateList.Add(new KeyValuePair<string, object>("SessionLevels", SessionLevels));
            stateList.Add(new KeyValuePair<string, object>("SessionTracks", SessionTracks));        

            return stateList;
        }

        public void RestoreState(IDictionary<string, object> storedState)
        {
            object stateAsString;

            Dictionary<string, object> collections;

            if (storedState.TryGetValue("DataServiceState", out stateAsString))
            {                
                var dsState = DataServiceState.Deserialize((string)stateAsString);

                this._context = dsState.Context as OratorEntities;
                collections = dsState.RootCollections;

                if (collections != null)
                {
                    
                    if (collections.ContainsKey("Events"))
                    {
                        this.Events = collections["Events"] as DataServiceCollection<Event>;
                        
                        if (collections.ContainsKey("Sessions"))
                        {
                            this.Sessions = collections["Sessions"] as DataServiceCollection<Session>;
                            
                            if (collections.ContainsKey("Speakers"))
                            {
                                this.Speakers = collections["Speakers"] as DataServiceCollection<SessionSpeaker>;
                            }

                            if (collections.ContainsKey("SessionFiles"))
                            {
                                this.SessionFiles = collections["SessionFiles"] as DataServiceCollection<SessionFile>;
                            }
                        }
                    }                    
                }

                // Restore entity properties from the stored URI.                  
                _context.TryGetEntity<Session>(storedState["CurrentSession"] as Uri, out _currentSession);
                _context.TryGetEntity<Event>(storedState["CurrentEvent"] as Uri, out _currentEvent);
                _context.TryGetEntity<SessionSpeaker>(storedState["SingleSpeaker"] as Uri, out _singleSpeaker);
             
                // Restore other non-entity properties.
                this.TotalSessionCount = (int)storedState["TotalSessionCount"];
                this.Message = (string)storedState["Message"];
                this.CountMessage = (string)storedState["CountMessage"];
                this.QueryInfo = (QueryHelper)storedState["QueryInfo"];
                this.SessionCategories = (List<string>)storedState["SessionCategories"];
                this.SessionDates = (List<string>)storedState["SessionDates"];
                this.SessionTracks = (List<string>)storedState["SessionTracks"];                
            }
        }
    }

    // Event args used to notify the UI that the load is completed.
    public class SourcesLoadCompletedEventArgs : EventArgs
    {
        public SourcesLoadCompletedEventArgs(Exception error)
        {
            this.Error = error;
        }

        private Exception Error { get; set; }
    }
}