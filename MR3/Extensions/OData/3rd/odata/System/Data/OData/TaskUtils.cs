//   Copyright 2011 Microsoft Corporation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#if ODATALIB_ASYNC
namespace System.Data.OData
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    #endregion Namespaces
    
    /// <summary>
    /// Class with utility methods for working with and implementing Task based APIs
    /// </summary>
    internal static class TaskUtils
    {
        /// <summary>
        /// Already completed task.
        /// </summary>
        private static Task completedTask;

        /// <summary>
        /// Returns already completed task instance.
        /// </summary>
        public static Task CompletedTask
        {
            get
            {
                // Note that in case of two threads competing here we would create two completed tasks, but only one 
                // will be stored in the static variable. In any case, they are identical for all other purposes, 
                // so it doesn't matter which one wins
                if (completedTask == null)
                {
                    // Create a TaskCompletionSource - since there's no non-generic version use a dummy one
                    // and then cast to the non-generic version.
                    TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
                    taskCompletionSource.SetResult(null);
                    completedTask = taskCompletionSource.Task;
                }

                return completedTask;
            }
        }

        #region Task.IgnoreExceptions - copied from Samples for Parallel Programming
        /// <summary>Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Need to access t.Exception to invoke the getter which will mark the Task to not throw the exception.")]
        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(
                t => { var ignored = t.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }
        #endregion Task.IgnoreExceptions - copied from Samples for Parallel Programming

        #region TaskFactory.GetTargetScheduler - copied from Samples for Parallel Programming
        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        /// <param name="factory">Factory to get the scheduler for.</param>
        /// <returns>The scheduler for the specified factory.</returns>
        public static TaskScheduler GetTargetScheduler(this TaskFactory factory)
        {
            Debug.Assert(factory != null, "factory != null");
            return factory.Scheduler ?? TaskScheduler.Current;
        }
        #endregion TaskFactory.GetTargetScheduler - copied from Samples for Parallel Programming

        #region TaskFactory.Iterate - copied from Samples for Parallel Programming
        //// Note that if we would migrate to .NET 4.5 and could get dependency on the "await" keyword, all of this is not needed
        //// and we could use the await functionality instead.

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Stores the exception so that it doesn't bring down the process but isntead rethrows on the task calling thread.")]
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<Task> source,
            object state,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            // Validate/update parameters
            Debug.Assert(factory != null, "factory != null");
            Debug.Assert(source != null, "source != null");
            Debug.Assert(scheduler != null, "scheduler != null");

            // Get an enumerator from the enumerable
            var enumerator = source.GetEnumerator();
            Debug.Assert(enumerator != null, "enumerator != null");

            // Create the task to be returned to the caller.  And ensure
            // that when everything is done, the enumerator is cleaned up.
            var trc = new TaskCompletionSource<object>(state, creationOptions);
            trc.Task.ContinueWith(_ => enumerator.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            // This will be called every time more work can be done.
            Action<Task> recursiveBody = null;
            recursiveBody = antecedent =>
            {
                try
                {
                    // If the previous task completed with any exceptions, bail 
                    if (antecedent != null && antecedent.IsFaulted)
                    {
                        trc.TrySetException(antecedent.Exception);
                    }

                    // If we should continue iterating and there's more to iterate
                    // over, create a continuation to continue processing.  We only
                    // want to continue processing once the current Task (as yielded
                    // from the enumerator) is complete.
                    if (enumerator.MoveNext())
                    {
                        var nextTask = enumerator.Current;

                        // The IgnoreException here is effective only for the recursiveBody code
                        // (not the nextTask, which is being checked by the recursiveBody above).
                        // And since the recursiveBody already catches all exceptions except for the uncatchable
                        // ones, we think it's OK to ignore all those exception in the finalizer thread.
                        nextTask.ContinueWith(recursiveBody).IgnoreExceptions();
                    }
                    else
                    {
                        // Otherwise, we're done
                        trc.TrySetResult(null);
                    }
                }
                catch (Exception exc)
                {
                    if (!ExceptionUtils.IsCatchableExceptionType(exc))
                    {
                        throw;
                    }

                    // If MoveNext throws an exception, propagate that to the user,
                    // either as cancellation or as a fault
                    var oce = exc as OperationCanceledException;
                    if (oce != null && oce.CancellationToken == cancellationToken)
                    {
                        trc.TrySetCanceled();
                    }
                    else
                    {
                        trc.TrySetException(exc);
                    }
                }
            };

            // Get things started by launching the first task
            // The IgnoreException here is effective only for the recursiveBody code
            // (not the nextTask, which is being checked by the recursiveBody above).
            // And since the recursiveBody already catches all exceptions except for the uncatchable
            // ones, we think it's OK to ignore all those exception in the finalizer thread.
            factory.StartNew(() => recursiveBody(null), CancellationToken.None, TaskCreationOptions.None, scheduler).IgnoreExceptions();

            // Return the representative task to the user
            return trc.Task;
        }
        #endregion TaskFactory.Iterate - copied from Samples for Parallel Programming
    }
}
#endif
