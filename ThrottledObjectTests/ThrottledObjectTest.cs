using System;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;
using ThrottedObject;

namespace ThrottledObjectTests
{
    [TestFixture]
    public class ThrottledObjectTest
    {
        [Test]
        public void TestThrottle()
        {
            var throttle = new ThrottledObject<TestItem>(TimeSpan.FromMilliseconds(2000), Generator);

            Observable.Interval(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ =>
                               Console.WriteLine("[now: {0}] - Last time generated: {1}",
                                                         DateTime.Now,
                                                         throttle.LazyGetItem.TimeGenerated));

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        public TestItem Generator()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new TestItem
                       {
                           TimeGenerated = DateTime.Now
                       };
        }
    }

    public class TestItem
    {
        public DateTime TimeGenerated { get; set; }
    }
}
