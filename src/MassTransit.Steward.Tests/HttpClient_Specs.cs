// Copyright 2007-2013 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Steward.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Events;
    using NUnit.Framework;


    [TestFixture]
    public class When_contacting_an_http_url :
        DispatchTestFixture
    {
        [Test]
        public void Should_succeed_nicely()
        {
            var succeeded = new TaskCompletionSource<HttpRequestSucceeded>(TestCancellationToken);
            LocalBus.SubscribeHandler<HttpRequestSucceeded>(x => succeeded.SetResult(x));
            var faulted = new TaskCompletionSource<HttpRequestFaulted>(TestCancellationToken);
            LocalBus.SubscribeHandler<HttpRequestFaulted>(x => faulted.SetResult(x));
            var completed = new TaskCompletionSource<ResourceUsageCompleted>(TestCancellationToken);
            LocalBus.SubscribeHandler<ResourceUsageCompleted>(x => completed.SetResult(x));
            Assert.IsTrue(WaitForSubscription<HttpRequestSucceeded>());
            Assert.IsTrue(WaitForSubscription<HttpRequestFaulted>());
            Assert.IsTrue(WaitForSubscription<ResourceUsageCompleted>());


            Uri commandUri = GetActivityContext<ExecuteHttpRequest>().ExecuteUri;
            var webUrl = new Uri("https://help.github.com/");

            var command =new ExecuteHttpRequestCommand(webUrl);


            DispatchMessageHandle<ExecuteHttpRequest> handle =
                DispatchEndpoint.DispatchMessage(command, commandUri, webUrl);

            Assert.IsTrue(_accepted.Task.Wait(TestTimeout));
            DispatchAccepted accepted = _accepted.Task.Result;
            Assert.AreEqual(handle.DispatchId, accepted.DispatchId);

            var waitAny = Task.WaitAny(new Task[] {succeeded.Task, faulted.Task}, TestTimeout);

            Assert.AreNotEqual(WaitHandle.WaitTimeout, waitAny);

            Console.WriteLine("Request Duration: {0}", completed.Task.Result.Duration);
        }

        TaskCompletionSource<DispatchAccepted> _accepted;

        [TestFixtureSetUp]
        public void Setup()
        {
            _accepted = new TaskCompletionSource<DispatchAccepted>(TestCancellationToken);

            LocalBus.SubscribeHandler<DispatchAccepted>(x => _accepted.SetResult(x));
            Assert.IsTrue(WaitForSubscription<DispatchAccepted>());
        }

        protected override void SetupCommands()
        {
            AddCommandContext<HttpProxyConsumer, ExecuteHttpRequest>(() => new HttpProxyConsumer());
        }

        class ExecuteHttpRequestCommand :
            ExecuteHttpRequest
        {
            public ExecuteHttpRequestCommand(Uri url)
            {
                Url = url;
            }

            public Uri Url { get; private set; }
        }
    }
}