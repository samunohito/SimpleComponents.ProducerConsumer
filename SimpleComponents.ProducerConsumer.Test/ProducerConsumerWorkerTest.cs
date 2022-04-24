using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleComponents.ProducerConsumer.Test
{
    public class ProducerConsumerWorkerTest
    {
        private ProducerConsumerWorker _worker;

        [SetUp]
        public void Setup()
        {
            _worker = new ProducerConsumerWorker();
        }

        /// <summary>
        /// Execution of Consumer and checking of arguments and return value
        /// </summary>
        [Test]
        public void Test1()
        {
            var args = Guid.NewGuid().ToString();
            var results = Guid.NewGuid().ToString();

            var execCount = 0;
            var item = ProducerConsumerWorkerItem.Create(args, x =>
            {
                Assert.AreEqual(args, x.RawArgument);
                x.RawResult = results;
                execCount++;
            });

            Assert.False(item.Executed);
            Assert.AreEqual(0, execCount);

            _worker.Push(item);
            Thread.Sleep(2000);

            Assert.True(item.Executed);
            Assert.AreEqual(1, execCount);

            Assert.AreEqual(results, item.RawResult);
        }

        /// <summary>
        /// Confirmation that the execution of Consumer can be canceled
        /// </summary>
        [Test]
        public void Test2()
        {
            var args = Guid.NewGuid().ToString();
            var results = Guid.NewGuid().ToString();

            var execCount = 0;
            var item = ProducerConsumerWorkerItem.Create(args, x =>
            {
                Assert.AreEqual(args, x.RawArgument);
                x.RawResult = results;
                execCount++;
            });

            Assert.False(item.Executed);
            Assert.AreEqual(0, execCount);

            item.CancelRequest = true;

            _worker.Push(item);
            Thread.Sleep(2000);

            Assert.True(item.Executed);
            Assert.AreEqual(0, execCount);

            Assert.Null(item.RawResult);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Test3()
        {
            var item = ProducerConsumerWorkerItem.Create(x => { });

            var preExeCount = 0;
            var postExeCount = 0;

            _worker.PreExecute += (s, e) =>
            {
                Assert.AreEqual(item, e.Item);
                preExeCount++;
            };

            _worker.PostExecute += (s, e) =>
            {
                Assert.AreEqual(item, e.Item);
                postExeCount++;
            };

            Assert.AreEqual(0, preExeCount);
            Assert.AreEqual(0, postExeCount);

            _worker.Push(item);
            Thread.Sleep(2000);

            Assert.AreEqual(1, preExeCount);
            Assert.AreEqual(1, postExeCount);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Test4()
        {
            var ex = new Exception(Guid.NewGuid().ToString());
            var item = ProducerConsumerWorkerItem.Create(x => { throw ex; });


            _worker.Error += (s, e) =>
            {
                e.Handled = true;
                Assert.AreEqual(ex, e.Exception);
            };

            _worker.Push(item);
            Thread.Sleep(2000);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Test5()
        {
            var worker = new TestProducerConsumerWorker();
            var ex = new Exception(Guid.NewGuid().ToString());
            var item = ProducerConsumerWorkerItem.Create(x => { throw ex; });
            
            worker.Error += (s, e) =>
            {
                e.Handled = false;
                Assert.AreEqual(ex, e.Exception);
            };

            worker.Push(item);
            Thread.Sleep(5000);
            
            Assert.AreEqual(ex, worker.Exception);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Test6()
        {
            var errorCount = 0;
            var cancelCount = 0;

            _worker.Error += (s, e) => { errorCount++; };

            _worker.Canceled += (s, e) => { cancelCount++; };

            Assert.False(_worker.IsDisposed);
            Assert.AreEqual(0, errorCount);
            Assert.AreEqual(0, cancelCount);

            _worker.Dispose();

            Assert.True(_worker.IsDisposed);
            Assert.AreEqual(0, errorCount);
            Assert.AreEqual(1, cancelCount);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Test7()
        {
            Parallel.For(0, 4096, i => _worker.Dispose());
        }

        private class TestProducerConsumerWorker : ProducerConsumerWorker
        {
            public Exception Exception { get; private set; }
            
            protected override void OnHandleError(Exception ex, ProducerConsumerWorkerItem item)
            {
                Exception = ex;
            }
        }
    }
}