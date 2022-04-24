using System;
using SimpleComponents.ProducerConsumer.Event;

namespace SimpleComponents.ProducerConsumer
{
    /// <summary>
    /// Define the "Producer - Consumer pattern" interface.
    /// </summary>
    public interface IProducerConsumerWorker
    {
        /// <summary>
        /// Occurs before an item taken from the queue is executed.
        ///
        /// NOTE:
        /// - This event occurs in a worker thread, so care should be taken when registering processes that need to be performed on a special thread, such as UI operations.
        /// - Registering too heavy a process may interfere with the sequential execution of the queue.
        /// </summary>
        event EventHandler<ProducerConsumerWorkerEventArgs> PreExecute;

        /// <summary>
        /// Occurs after an item taken from the queue is executed.
        ///
        /// NOTE:
        /// - This event occurs in a worker thread, so care should be taken when registering processes that need to be performed on a special thread, such as UI operations.
        /// - Registering too heavy a process may interfere with the sequential execution of the queue.
        /// </summary>
        event EventHandler<ProducerConsumerWorkerEventArgs> PostExecute;

        /// <summary>
        /// Occurs when the queue is forcibly released from the waiting state.
        ///
        /// NOTE:
        /// - This event occurs in a worker thread, so care should be taken when registering processes that need to be performed on a special thread, such as UI operations.
        /// - Registering too heavy a process may interfere with the sequential execution of the queue.
        /// </summary>
        event EventHandler<ProducerConsumerWorkerCancelEventArgs> Canceled;

        /// <summary>
        /// Occurs when an error occurs during execution of an item retrieved from a queue.
        ///
        /// NOTE:
        /// - This event occurs in a worker thread, so care should be taken when registering processes that need to be performed on a special thread, such as UI operations.
        /// - Registering too heavy a process may interfere with the sequential execution of the queue.
        /// </summary>
        event EventHandler<ProducerConsumerWorkerErrorEventArgs> Error;

        /// <summary>
        /// Obtains the current number of elements present in the queue.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Register an item at the end of the queue.
        /// Items registered in the queue are processed in order, so they are not necessarily executed immediately.
        /// When the number of registrations in the queue reaches the capacity set in the constructor,
        /// the thread calling this method is blocked until the number of registrations in the queue is less than the capacity.
        /// </summary>
        /// <param name="item">item</param>
        void Push(ProducerConsumerWorkerItem item);
    }
}