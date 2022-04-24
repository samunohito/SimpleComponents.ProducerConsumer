using System;

namespace SimpleComponents.ProducerConsumer.Event
{
    /// <summary>
    /// Common arguments for events fired by <see cref="IProducerConsumerWorker"/>
    /// </summary>
    public class ProducerConsumerWorkerEventArgs : EventArgs
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="item">Items taken out of the queue</param>
        public ProducerConsumerWorkerEventArgs(ProducerConsumerWorkerItem item)
        {
            Item = item;
        }

        /// <summary>
        /// Items taken out of the queue
        /// </summary>
        public ProducerConsumerWorkerItem Item { get; }
    }
}