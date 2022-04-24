using System;

namespace SimpleComponents.ProducerConsumer.Event
{
    /// <summary>
    /// Event arguments for <see cref="IProducerConsumerWorker.Canceled"/>
    /// </summary>
    public class ProducerConsumerWorkerCancelEventArgs
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ex">Exceptions that have occurred</param>
        public ProducerConsumerWorkerCancelEventArgs(OperationCanceledException ex)
        {
            Exception = ex;
        }

        /// <summary>
        /// Exceptions that have occurred
        /// </summary>
        public OperationCanceledException Exception { get; }
    }
}