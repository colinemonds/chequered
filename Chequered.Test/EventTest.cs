using System.Collections.Generic;
using System.Linq;
using Chequered.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chequered.Test
{
    [TestClass]
    public class EventTest
    {
        [TestMethod]
        public void Send_WithOneListener_CallsListener()
        {
            var eb = new EventBus();
            var subscriberCalled = false;
            eb.GetHandle<float>().Subscribe(f => subscriberCalled = true);

            eb.GetHandle<float>().Send(0.5f);

            Assert.IsTrue(subscriberCalled);
        }

        [TestMethod]
        public void Send_WithTwoListeners_CallsBothListeners()
        {
            var eb = new EventBus();
            var subscriberCalled = 0;
            eb.GetHandle<float>().Subscribe(f => subscriberCalled++);
            eb.GetHandle<float>().Subscribe(f => subscriberCalled++);

            eb.GetHandle<float>().Send(0.5f);

            Assert.IsTrue(subscriberCalled == 2);
        }

        [TestMethod]
        public void MultipleSend_WithSubscribeOnceListener_FiresOnce()
        {
            var eb = new EventBus();
            var subscriberCalled = 0;
            eb.GetHandle<float>().SubscribeOnce(f => subscriberCalled++);

            eb.GetHandle<float>().Send(0.5f);
            eb.GetHandle<float>().Send(0.4f);
            eb.GetHandle<float>().Send(222.22f);

            Assert.IsTrue(subscriberCalled == 1);
        }

        [TestMethod]
        public void BreadthFirstEventBusSend_WhenListenerRetriggers_ProcessedInBreadthFirstOrder()
        {
            var eb = new EventBus();
            var list = new List<int>();
            //floats should be processed completely
            eb.GetHandle<float>().SubscribeOnce(
                f =>
                {
                    list.Add(1);
                    eb.GetHandle<int>().Send(0);
                });
            eb.GetHandle<float>().SubscribeOnce(f => list.Add(2));
            //before this int listener is called
            eb.GetHandle<int>().SubscribeOnce(i => list.Add(3));

            eb.GetHandle<float>().Send(3.0f);

            Assert.IsTrue(list.SequenceEqual(new[] {1, 2, 3}));
        }

        [TestMethod]
        public void DepthFirstEventBusSend_WhenListenerRetriggers_ProcessedInDepthFirstOrder()
        {
            var eb = new EventBus(EventProcessingMode.DepthFirst);
            var list = new List<int>();
            //this float listener will trigger the int listener before the remaining float listener is called
            eb.GetHandle<float>().SubscribeOnce(
                f =>
                {
                    list.Add(1);
                    eb.GetHandle<int>().Send(0);
                });
            eb.GetHandle<float>().SubscribeOnce(f => list.Add(3));
            eb.GetHandle<int>().SubscribeOnce(i => list.Add(2));

            eb.GetHandle<float>().Send(3.0f);

            Assert.IsTrue(list.SequenceEqual(new[] { 1, 2, 3 }));
        }
    }
}
