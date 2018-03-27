//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Services.Client;

namespace NorthwindClient
{
    static class ODataAsyncExtensions
    {
        // Extension methods coded by Glenn Gailey.
        /// <summary>
        /// Asynchronously loads items into the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/>.
        /// </summary>
        /// <typeparam name="T">Entity type of the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance.</typeparam>
        /// <param name="bindingCollection">The <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance on which this extension method is enabled.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns a <see cref="T:System.Data.Services.Client.LoadCompletedEventArgs"/>.</returns>
        public static async Task<LoadCompletedEventArgs> LoadAsync<T>(this DataServiceCollection<T> bindingCollection)
        {
            var tcs = new TaskCompletionSource<LoadCompletedEventArgs>();

            EventHandler<LoadCompletedEventArgs> handler = delegate(object o, LoadCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(e);
                }
            };

            bindingCollection.LoadCompleted += handler;
            bindingCollection.LoadAsync();

            LoadCompletedEventArgs eventArgs = await tcs.Task;
            bindingCollection.LoadCompleted -= handler;

            return eventArgs;
        }
        /// <summary>
        /// Asynchronously executes an <see cref="T:System.Linq.IQueryable`1"/> and loads results into the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/>.
        /// </summary>
        /// <typeparam name="T">Entity type of the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance.</typeparam>
        /// <param name="bindingCollection">The <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance on which this extension method is enabled.</param>
        /// <param name="query">Query that when executed loads entities into the collection.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns a <see cref="T:System.Data.Services.Client.LoadCompletedEventArgs"/>.</returns>
        public static async Task<LoadCompletedEventArgs> LoadAsync<T>(this DataServiceCollection<T> bindingCollection, IQueryable<T> query)
        {
            var tcs = new TaskCompletionSource<LoadCompletedEventArgs>();

            EventHandler<LoadCompletedEventArgs> handler =
                delegate(object o, LoadCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        tcs.TrySetException(e.Error);
                    }
                    else if (e.Cancelled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(e);
                    }
                };

            bindingCollection.LoadCompleted += handler;
            bindingCollection.LoadAsync(query);

            LoadCompletedEventArgs eventArgs = await tcs.Task;
            bindingCollection.LoadCompleted -= handler;

            return eventArgs;
        }
        /// <summary>
        /// Asynchronously executes a URI-based query and loads results into the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/>.
        /// </summary>
        /// <typeparam name="T">Entity type of the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance.</typeparam>
        /// <param name="bindingCollection">The <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance on which this extension method is enabled.</param>
        /// <param name="requestUri">Represents a query that returns a collection of type <typeparam name="T"/>.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns a <see cref="T:System.Data.Services.Client.LoadCompletedEventArgs"/>.</returns>
        public static async Task<LoadCompletedEventArgs> LoadAsync<T>(this DataServiceCollection<T> bindingCollection, Uri requestUri)
        {
            var tcs = new TaskCompletionSource<LoadCompletedEventArgs>();

            EventHandler<LoadCompletedEventArgs> handler = delegate(object o, LoadCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(e);
                }
            };

            bindingCollection.LoadCompleted += handler;
            bindingCollection.LoadAsync(requestUri);

            LoadCompletedEventArgs eventArgs = await tcs.Task;
            bindingCollection.LoadCompleted -= handler;

            return eventArgs;
        }
        /// <summary>
        /// Asynchronously loads the next page of data results into the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/>.
        /// </summary>
        /// <typeparam name="T">Entity type of the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance.</typeparam>
        /// <param name="bindingCollection">The <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance on which this extension method is enabled.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns a <see cref="T:System.Data.Services.Client.LoadCompletedEventArgs"/>.</returns>
        public static async Task<LoadCompletedEventArgs> LoadNextPartialSetAsync<T>(this DataServiceCollection<T> bindingCollection)
        {
            var tcs = new TaskCompletionSource<LoadCompletedEventArgs>();

            EventHandler<LoadCompletedEventArgs> handler = delegate(object o, LoadCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(e);
                }
            };

            bindingCollection.LoadCompleted += handler;
            bindingCollection.LoadNextPartialSetAsync();

            LoadCompletedEventArgs eventArgs = await tcs.Task;
            bindingCollection.LoadCompleted -= handler;

            return eventArgs;
        }

        // New WCF Data Services v5 extension methods added by Glenn Gailey.
        /// <summary>
        /// Asychronously executes the specified URI-based query or operation. 
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="requestUri">The URI of the query or operation that returns type <typeparam name="TResult"/>.</param>
        /// <param name="httpMethod">The HTTP method of the request, either GET or POST.</param>
        /// <param name="singleResult">A boolean value that is true if a single result is expected, otherwise false.</param>
        /// <param name="operationParameters">An array of <see cref="T:System.Data.Services.Client.OperationParameter"/> objects that are parameters in the request URI.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(this DataServiceContext context, Uri requestUri, string httpMethod, bool singleResult,
            params System.Data.Services.Client.OperationParameter[] operationParameters)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(context.BeginExecute<TResult>(requestUri, null, null, httpMethod, singleResult, operationParameters),
                   (queryAsyncResult) =>
                   {
                       var results = context.EndExecute<TResult>(queryAsyncResult);
                       return results;
                   });

            return await queryTask;
        }
        /// <summary>
        /// Asychronously saves changes in the data service by using the provided <see cref="T:Windows.UI.Core.CoreDispatcher"/>.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="options">The save changes options emuneration, which supports flags.</param>
        /// <param name="dispatcher">The <see cref="T:Windows.UI.Core.CoreDispatcher"/> used to marshal the response back to the UI thread.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the result as a <see cref="T:System.Data.Services.Client.DataServiceResponse"/>.</returns>
        public static async Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, SaveChangesOptions options, Windows.UI.Core.CoreDispatcher dispatcher)
        {
            var tcs = new TaskCompletionSource<DataServiceResponse>();
            context.BeginSaveChanges(async iar =>
            {
                await dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            tcs.SetResult(context.EndSaveChanges(iar));
                        }
                        catch (DataServiceRequestException ex)
                        {
                            throw ex;
                        }
                    });
            }, null);
            return await tcs.Task;
        }
        // Extension methods coded by Phani Raju.
        /// <summary>
        /// Asychronously executes a specific <see cref="T:System.Data.Services.Client.DataServiceQuery`1"/>. 
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="query">The <see cref="T:System.Data.Services.Client.DataServiceQuery`1"/> instance on which this extension method is enabled.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(this DataServiceQuery<TResult> query)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(query.BeginExecute(null, null),
                (queryAsyncResult) =>
                {
                    var results = query.EndExecute(queryAsyncResult);
                    return results;
                });

            return await queryTask;
        }
        /// <summary>
        /// Asychronously executes a specific request URI.
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="requestUri">The URI of the query or operation that returns type <typeparam name="TResult"/>.</param>
        /// <param name="operationParameters">An array of <see cref="T:System.Data.Services.Client.OperationParameter"/> objects that are parameters in the request URI.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(this DataServiceContext context, Uri requestUri,
            params System.Data.Services.Client.OperationParameter[] operationParameters)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(context.BeginExecute<TResult>(requestUri, null, null),
                   (queryAsyncResult) =>
                   {
                       var results = context.EndExecute<TResult>(queryAsyncResult);
                       return results;
                   });

            return await queryTask;
        }
        /// <summary>
        /// Asynchronously requests the next page of data in the results.
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="requestUri">The URI of the query or operation that returns type <typeparam name="TResult"/>.</param>
        /// <param name="queryContinuationToken">A <see cref="T:System.Data.Services.Client.DataServiceQueryContinuation`1"/> token that is used to request the next page of data.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(this DataServiceContext context, DataServiceQueryContinuation<TResult> queryContinuationToken)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(context.BeginExecute<TResult>(queryContinuationToken, null, null),
                   (queryAsyncResult) =>
                   {
                       var results = context.EndExecute<TResult>(queryAsyncResult);
                       return results;
                   });

            return await queryTask;
        }
        /// <summary>
        /// Asynchronously loads a collection of related entities from a navigation property.
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="entity">The parent entity.</param>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> LoadPropertyAsync<TResult>(this DataServiceContext context, object entity, string propertyName)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(context.BeginLoadProperty(entity, propertyName, null, null),
                   (loadPropertyAsyncResult) =>
                   {
                       var results = context.EndLoadProperty(loadPropertyAsyncResult);
                       return (IEnumerable<TResult>)results;
                   });

            return await queryTask;
        }
        /// <summary>
        /// Asynchronously loads the next page of related entities from a navigation property.
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="entity">The parent entity.</param>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <param name="queryContinuationToken">A <see cref="T:System.Data.Services.Client.DataServiceQueryContinuation`1"/> token that is used to request the next page of data.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> LoadPropertyAsync<TResult>(this DataServiceContext context, object entity, string propertyName, DataServiceQueryContinuation continuation)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(context.BeginLoadProperty(entity, propertyName, continuation, null, null),
                  (loadPropertyAsyncResult) =>
                  {
                      var results = context.EndLoadProperty(loadPropertyAsyncResult);
                      return (IEnumerable<TResult>)results;
                  });

            return await queryTask;
        }
        /// <summary>
        /// Asynchronously loads the next page of related entities from a navigation property.
        /// </summary>
        /// <typeparam name="TResult">Entity type of the result of the execution.</typeparam>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="entity">The parent entity.</param>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <param name="nextLinkUri">The URI that is used to request the next page of data.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the results of the execution.</returns>
        public static async Task<IEnumerable<TResult>> LoadPropertyAsync<TResult>(this DataServiceContext context, object entity, string propertyName, Uri nextLinkUri)
        {
            var queryTask = Task.Factory.FromAsync<IEnumerable<TResult>>(context.BeginLoadProperty(entity, propertyName, nextLinkUri, null, null),
                    (loadPropertyAsyncResult) =>
                    {
                        var results = context.EndLoadProperty(loadPropertyAsyncResult);
                        return (IEnumerable<TResult>)results;
                    });

            return await queryTask;
        }
        /// <summary>
        /// Asynchronously executes one or more operations in a single batch request.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="requests">An array of <see cref="T:System.Data.Services.Client.DataServiceRequest"/> objects that define operations to execute in the batch.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the <see cref="T:System.Data.Services.Client.DataServiceResponse"/> for the batch operation.</returns>
        public static async Task<DataServiceResponse> ExecuteBatchAsync(this DataServiceContext context, params DataServiceRequest[] requests)
        {
            var queryTask = Task.Factory.FromAsync<DataServiceResponse>(context.BeginExecuteBatch(null, null, requests),
                   (queryAsyncResult) =>
                   {
                       var results = context.EndExecuteBatch(queryAsyncResult);
                       return results;
                   });

            return await queryTask;
        }        
        /// <summary>
        /// Asychronously saves changes in the data service.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the result as a <see cref="T:System.Data.Services.Client.DataServiceResponse"/>.</returns>
        public static async Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context)
        {
            return await SaveChangesAsync(context, SaveChangesOptions.None);
        }
        /// <summary>
        /// Asychronously saves changes in the data service by using the provided <see cref="T:Windows.UI.Core.CoreDispatcher"/>.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Data.Services.Client.DataServiceContext"/> instance on which this extension method is enabled. </param>
        /// <param name="options">The save changes options emuneration, which supports flags.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns the result as a <see cref="T:System.Data.Services.Client.DataServiceResponse"/>.</returns>
        public static async Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, SaveChangesOptions options)
        {
            var queryTask = Task.Factory.FromAsync<DataServiceResponse>(
                context.BeginSaveChanges(options, null, null),
                    (queryAsyncResult) =>
                    {
                        var results = context.EndSaveChanges(queryAsyncResult);
                        return results;
                    });

            return await queryTask;
        }
        /// <summary>
        /// Asynchronously executes an <see cref="T:System.Data.Services.Client.DataServiceQuery`1"/> and loads results into the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/>.
        /// </summary>
        /// <typeparam name="T">Entity type of the <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance.</typeparam>
        /// <param name="bindingCollection">The <see cref="T:System.Data.Services.Client.DataServiceCollection`1"/> instance on which this extension method is enabled.</param>
        /// <param name="query">Query that when executed loads entities into the collection.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1"/> that, when completed, returns a <see cref="T:System.Data.Services.Client.LoadCompletedEventArgs"/>.</returns>
        public static async Task<LoadCompletedEventArgs> LoadAsync<T>(DataServiceCollection<T> bindingCollection, DataServiceQuery<T> query)
        {
            var tcs = new TaskCompletionSource<LoadCompletedEventArgs>();

            EventHandler<LoadCompletedEventArgs> handler =
                delegate(object o, LoadCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        tcs.TrySetException(e.Error);
                    }
                    else if (e.Cancelled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(e);
                    }
                };

            bindingCollection.LoadCompleted += handler;
            bindingCollection.LoadAsync(query);

            LoadCompletedEventArgs eventArgs = await tcs.Task;
            bindingCollection.LoadCompleted -= handler;

            return eventArgs;
        }
    }
}
