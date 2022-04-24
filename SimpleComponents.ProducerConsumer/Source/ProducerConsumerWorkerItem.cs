using System;

namespace SimpleComponents.ProducerConsumer
{
    public class ProducerConsumerWorkerItem
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="args">optional-parameter object</param>
        /// <param name="consumer">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        internal ProducerConsumerWorkerItem(
            object args,
            IProducerConsumerWorkerConsumer consumer
        )
        {
            RawArgument = args;
            RawResult = default;
            Consumer = consumer;

            CancelRequest = false;
            Executed = false;
        }

        /// <summary>
        /// optional-parameter object
        /// </summary>
        public object RawArgument { get; }

        /// <summary>
        /// optional-result object
        /// </summary>
        public object RawResult { get; set; }

        /// <summary>
        /// Implementation of processes executed from <see cref="IProducerConsumerWorker"/>
        /// </summary>
        protected IProducerConsumerWorkerConsumer Consumer { get; }

        /// <summary>
        /// Flag whether or not to cancel this item.
        /// Changing it to true before the execution of <see cref="Consumer"/> will suppress the execution of <see cref="Consumer"/>.
        /// </summary>
        public bool CancelRequest { get; set; }

        /// <summary>
        /// This is a flag indicating whether or not the <see cref="Consumer"/> execution is complete.
        /// </summary>
        public bool Executed { get; private set; }

        /// <summary>
        /// Returns a cast of <see cref="RawArgument"/> to type <see cref="T"/>.
        /// It is assumed that the original type of <see cref="RawArgument"/> and the type of <see cref="T"/> are compatible.
        /// </summary>
        /// <typeparam name="T">Cast destination type</typeparam>
        /// <returns><see cref="RawArgument"/> cast to type <see cref="T"/></returns>
        public T GetTypedArgument<T>()
        {
            return (T)RawArgument;
        }

        /// <summary>
        /// Returns a cast of <see cref="RawResult"/> to type <see cref="T"/>.
        /// It is assumed that the original type of <see cref="RawResult"/> and the type of <see cref="T"/> are compatible.
        /// </summary>
        /// <typeparam name="T">Cast destination type</typeparam>
        /// <returns><see cref="RawResult"/> cast to type <see cref="T"/></returns>
        public T GetTypedResult<T>()
        {
            return (T)RawResult;
        }

        /// <summary>
        /// Execute <see cref="Consumer"/>.
        /// </summary>
        internal void Execute()
        {
            if (!CancelRequest)
            {
                Consumer.Execute(this);
            }

            Executed = true;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="act">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        /// <returns>instance</returns>
        public static ProducerConsumerWorkerItem Create(Action act)
        {
            return Create(null, act);
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="args">optional-parameter object</param>
        /// <param name="act">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        /// <returns>instance</returns>
        public static ProducerConsumerWorkerItem Create(object args, Action act)
        {
            return Create(args, new ConsumerDelegateNonArgs(act));
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="act">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        /// <returns>instance</returns>
        public static ProducerConsumerWorkerItem Create(Action<ProducerConsumerWorkerItem> act)
        {
            return Create(null, act);
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="args">optional-parameter object</param>
        /// <param name="act">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        /// <returns>instance</returns>
        public static ProducerConsumerWorkerItem Create(object args, Action<ProducerConsumerWorkerItem> act)
        {
            return Create(args, new ConsumerDelegate(act));
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="consumer">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        /// <returns>instance</returns>
        public static ProducerConsumerWorkerItem Create(IProducerConsumerWorkerConsumer consumer)
        {
            return Create(null, consumer);
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="args">optional-parameter object</param>
        /// <param name="consumer">Implementation of processes executed from <see cref="IProducerConsumerWorker"/></param>
        /// <returns>instance</returns>
        public static ProducerConsumerWorkerItem Create(object args, IProducerConsumerWorkerConsumer consumer)
        {
            return new ProducerConsumerWorkerItem(args, consumer);
        }

        private class ConsumerDelegateNonArgs : IProducerConsumerWorkerConsumer
        {
            private readonly Action _action;

            public ConsumerDelegateNonArgs(Action action)
            {
                _action = action;
            }

            public void Execute(ProducerConsumerWorkerItem args)
            {
                _action();
            }
        }

        private class ConsumerDelegate : IProducerConsumerWorkerConsumer
        {
            private readonly Action<ProducerConsumerWorkerItem> _action;

            public ConsumerDelegate(Action<ProducerConsumerWorkerItem> action)
            {
                _action = action;
            }

            public void Execute(ProducerConsumerWorkerItem args)
            {
                _action(args);
            }
        }
    }
}