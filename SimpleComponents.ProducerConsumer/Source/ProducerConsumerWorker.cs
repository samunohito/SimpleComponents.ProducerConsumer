using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SimpleComponents.ProducerConsumer.Event;

namespace SimpleComponents.ProducerConsumer
{
    /// <summary>
    /// Provides an implementation of the "Producer - Consumer pattern".
    /// </summary>
    public class ProducerConsumerWorker : IDisposable, IProducerConsumerWorker
    {
        /// <summary>
        /// If the maximum number of queues is not set in the constructor, this number is used.
        /// </summary>
        public const int QueueDefaultCapacity = 4096;

        /// <inheritdoc cref="IProducerConsumerWorker.PreExecute" />
        public event EventHandler<ProducerConsumerWorkerEventArgs> PreExecute;

        /// <inheritdoc cref="IProducerConsumerWorker.PostExecute" />
        public event EventHandler<ProducerConsumerWorkerEventArgs> PostExecute;

        /// <inheritdoc cref="IProducerConsumerWorker.Canceled" />
        public event EventHandler<ProducerConsumerWorkerCancelEventArgs> Canceled;

        /// <inheritdoc cref="IProducerConsumerWorker.Error" />
        public event EventHandler<ProducerConsumerWorkerErrorEventArgs> Error;

        private readonly object _gate;
        private readonly BlockingCollection<ProducerConsumerWorkerItem> _items;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _loopTask;

        /// <summary>
        /// ctor
        /// </summary>
        public ProducerConsumerWorker() : this(QueueDefaultCapacity)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="capacity">Maximum number of queues registered</param>
        public ProducerConsumerWorker(int capacity)
        {
            _gate = new object();
            _items = new BlockingCollection<ProducerConsumerWorkerItem>(capacity);
            _cancellation = new CancellationTokenSource();
            _loopTask = new Task(DoProcess, _cancellation.Token);
            _loopTask.Start();
        }

        /// <summary>
        /// destructor
        /// </summary>
        ~ProducerConsumerWorker()
        {
            Dispose(false);
        }

        /// <inheritdoc cref="IProducerConsumerWorker.Count" />
        public int Count => _items.Count;

        /// <inheritdoc cref="IProducerConsumerWorker.Push" />
        public void Push(ProducerConsumerWorkerItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            lock (_gate)
            {
                if (IsDisposed)
                {
                    return;
                }
            }

            _items.Add(item);
        }

        /// <summary>
        /// Gets whether this object has been destroyed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Infinite loop process called by worker thread.
        /// </summary>
        private void DoProcess()
        {
            while (true)
            {
                ProducerConsumerWorkerItem item = null;

                try
                {
                    // If the queue is empty, block this thread until an item is added.
                    // If the argument token is cancelled, an OperationCanceledException is raised.
                    item = _items.Take(_cancellation.Token);
                    if (item == null)
                    {
                        continue;
                    }

                    OnPreExecuteInternal(item);
                    DoExecute(item);
                    OnPostExecuteInternal(item);
                }
                catch (OperationCanceledException ex)
                {
                    // When the CancellationTokenSource is in a cancellation state, it will come here.
                    OnCanceledInternal(ex);
                    break;
                }
                catch (Exception ex)
                {
                    OnHandleErrorInternal(ex, item);
                }
            }
        }

        /// <summary>
        /// This method is executed before the item taken from the queue is executed.
        /// </summary>
        /// <param name="item">Items taken out of the queue</param>
        private void OnPreExecuteInternal(ProducerConsumerWorkerItem item)
        {
            PreExecute?.Invoke(this, new ProducerConsumerWorkerEventArgs(item));
            OnPreExecute(item);
        }

        /// <summary>
        /// This method is executed after the item retrieved from the queue is executed.
        /// </summary>
        /// <param name="item">Items taken out of the queue</param>
        private void OnPostExecuteInternal(ProducerConsumerWorkerItem item)
        {
            PostExecute?.Invoke(this, new ProducerConsumerWorkerEventArgs(item));
            OnPostExecute(item);
        }

        /// <summary>
        /// Called before an item taken from the queue has been executed.
        /// This method can be extended if necessary.
        ///
        /// NOTE:
        /// - If too heavy processing is written, it may interfere with the sequential execution of the queue.
        /// </summary>
        /// <param name="item">Items taken out of the queue</param>
        protected virtual void OnPreExecute(ProducerConsumerWorkerItem item)
        {
        }

        /// <summary>
        /// Called after an item taken from the queue has been executed.
        /// This method can be extended if necessary.
        ///
        /// NOTE:
        /// - If too heavy processing is written, it may interfere with the sequential execution of the queue.
        /// </summary>
        /// <param name="item">Items taken out of the queue</param>
        protected virtual void OnPostExecute(ProducerConsumerWorkerItem item)
        {
        }

        /// <summary>
        /// Execute items taken from the queue.
        /// This method can be extended if necessary.
        /// </summary>
        /// <param name="item">Items taken out of the queue</param>
        protected virtual void DoExecute(ProducerConsumerWorkerItem item)
        {
            item.Execute();
        }

        /// <summary>
        /// Called when the queue is forcibly released from the waiting state.
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        private void OnCanceledInternal(OperationCanceledException ex)
        {
            Canceled?.Invoke(this, new ProducerConsumerWorkerCancelEventArgs(ex));
            OnCanceled(ex);
        }

        /// <summary>
        /// Called when the queue is forcibly released from the waiting state.
        /// This method can be extended if necessary.
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        protected virtual void OnCanceled(OperationCanceledException ex)
        {
        }

        /// <summary>
        /// Called when an error occurs during execution of an item retrieved from a queue.
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        /// <param name="item">Items taken out of the queue</param>
        private void OnHandleErrorInternal(Exception ex, ProducerConsumerWorkerItem item)
        {
            var eventArgs = new ProducerConsumerWorkerErrorEventArgs(ex, item);
            Error?.Invoke(this, eventArgs);

            if (!eventArgs.Handled)
            {
                OnHandleError(ex, item);
            }
        }

        /// <summary>
        /// Called when an error occurs during execution of an item retrieved from a queue.
        /// This method can be extended if necessary.
        /// 
        /// NOTE:
        /// - If too heavy processing is written, it may interfere with the sequential execution of the queue.
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        /// <param name="item">Items taken out of the queue</param>
        /// <exception cref="Exception"></exception>
        protected virtual void OnHandleError(Exception ex, ProducerConsumerWorkerItem item)
        {
            throw ex;
        }

        /// <summary>
        /// Destroy any resources this object has.
        /// </summary>
        /// <seealso cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Destroy any resources this object has.
        /// </summary>
        /// <seealso cref="IDisposable.Dispose"/>
        void IDisposable.Dispose()
        {
            Dispose();
        }

        /// <summary>
        /// Main part of the destruction process.
        /// Since we do not want it to be executed many times,
        /// we will also implement locking using the <see cref="IsDisposed"/> flag.
        /// </summary>
        /// <param name="calledFromDispose">Whether it was called from the Dispose method or not</param>
        private void Dispose(bool calledFromDispose)
        {
            lock (_gate)
            {
                if (IsDisposed)
                {
                    return;
                }

                IsDisposed = true;
            }

            if (calledFromDispose)
            {
                // Tells GC that destructor processing is not necessary if explicitly Dispose() is called
                GC.SuppressFinalize(this);
            }

            _cancellation.Cancel(false);

            switch (_loopTask.Status)
            {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                case TaskStatus.RanToCompletion:
                    break;
                default:
                    _loopTask.Wait();
                    break;
            }

            _loopTask.Dispose();
            _cancellation.Dispose();
            _items.Dispose();
        }
    }
}