using System;

namespace SimpleComponents.ProducerConsumer.Event
{
    /// <summary>
    /// Event arguments for <see cref="IProducerConsumerWorker.Error"/>
    /// </summary>
    public class ProducerConsumerWorkerErrorEventArgs : ProducerConsumerWorkerEventArgs
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        /// <param name="item">Items taken out of the queue</param>
        public ProducerConsumerWorkerErrorEventArgs(
            Exception ex,
            ProducerConsumerWorkerItem item
        )
            : base(item)
        {
            Exception = ex;
            Handled = false;
        }

        /// <summary>
        /// Exceptions that have occurred
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// If true, the exception is considered handled and will not be thrown.
        /// </summary>
        public bool Handled { get; set; }
    }
}