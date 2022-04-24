namespace SimpleComponents.ProducerConsumer
{
    /// <summary>
    /// Defines the body of processing to be executed from <see cref="IProducerConsumerWorker"/>.
    /// </summary>
    public interface IProducerConsumerWorkerConsumer
    {
        /// <summary>
        /// Executed from <see cref="IProducerConsumerWorker"/>.
        ///
        /// NOTE:
        /// - This method is called in a worker thread, so care should be taken when performing UI operations or other processes that need to be executed in a special thread.
        /// </summary>
        /// <param name="args">Item object that encompasses this interface</param>
        void Execute(ProducerConsumerWorkerItem args);
    }
}