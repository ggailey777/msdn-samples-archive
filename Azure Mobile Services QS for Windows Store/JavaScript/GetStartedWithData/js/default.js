(function () {
    "use strict";

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;
    WinJS.strictProcessing();

    app.onactivated = function (args) {
        if (args.detail.kind === activation.ActivationKind.launch) {
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
                // TODO: This application has been newly launched. Initialize
                // your application here.
            } else {
                // TODO: This application has been reactivated from suspension.
                // Restore application state here.
            }
            args.setPromise(WinJS.UI.processAll());

            //// TODO: Configure the MobileServiceClient to communicate with your mobile service by
            //// uncommenting the following code and replacing AppUrl & AppKey with values from  
            //// your mobile service, which are obtained from the Windows Azure Management Portal.
            //// Do this after you add a reference to the Mobile Services client to your project.
            //var client = new WindowsAzure.MobileServiceClient(
            //    "AppUrl",
            //    "appKey"
            //);

            // Defined the todoItems list used for data binding.
            var todoItems = new WinJS.Binding.List();           

            //// TODO: Uncomment the following line of code that defines todoTable, 
            //// a proxy for the table in SQL Database. 
            //var todoTable = client.getTable('TodoItem');

            var insertTodoItem = function (todoItem) {

                // This code increments the ID value.
                // TODO: Delete or comment the following if-else statement; Mobile Services auto-generates the ID value.                
                if (todoItems.length > 0) {

                    var maxItemId = 0;

                    todoItems.forEach(function (value, index, array) {
                        if (value.id > maxItemId) {
                            maxItemId = value.id;
                        }
                    })
                    todoItem.id = maxItemId + 1;
                }
                else{
                    todoItem.id = 1;
                }
               
                //// TODO: Uncomment the following statement.
                //// This code inserts a new item into the database. After the operation is complete
                //// and Mobile Services has assigned an id, the item is added to the binding list
                //todoTable.insert(todoItem).done(function (item) {
                //todoItems.push(item);
                //});

                // TODO: Delete or comment out the following statement.
                todoItems.push(todoItem);
            };

            var refreshTodoItems = function () {
                //// This code refreshes the entries in the list by querying the TodoItems table;
                //// the query excludes completed items.                 
                //// TODO #1: Defines a simple query for all items. 
                //todoTable.read().done(function (results) {
                //    todoItems = new WinJS.Binding.List(results);
                //    listItems.winControl.itemDataSource = todoItems.dataSource;
                //});

                //// TODO #2: More advanced query that filters out completed items. 
                //todoTable.where({ complete: false })
                //    .read()
                //    .done(function (results) {
                //        todoItems = new WinJS.Binding.List(results);
                //        listItems.winControl.itemDataSource = todoItems.dataSource;
                //    },
                //    function (error) {

                //    });

                // TODO: Delete or comment out the following statement.
                listItems.winControl.itemDataSource = todoItems.dataSource;
            };

            var updateCheckedTodoItem = function (todoItem) {
                //// TODO: Uncomment the following statement
                //// This code takes a freshly completed TodoItem and updates the database. When the mobile service 
                //// responds, the item is removed from the list.
                //todoTable.update(todoItem);
            };

            buttonSave.addEventListener("click", function () {
                insertTodoItem({
                    text: textInput.value,
                    complete: false
                });
            });

            buttonRefresh.addEventListener("click", function () {
                refreshTodoItems();
            });

            listItems.addEventListener("change", function (eventArgs) {
                var todoItem = eventArgs.target.dataContext.backingData;
                todoItem.complete = eventArgs.target.checked;
                updateCheckedTodoItem(todoItem);
                
            });

            refreshTodoItems();

        }
    };

    app.oncheckpoint = function (args) {
        // TODO: This application is about to be suspended. Save any state
        // that needs to persist across suspensions here. You might use the
        // WinJS.Application.sessionState object, which is automatically
        // saved and restored across suspension. If you need to complete an
        // asynchronous operation before your application is suspended, call
        // args.setPromise().
    };

    app.start();
})();
