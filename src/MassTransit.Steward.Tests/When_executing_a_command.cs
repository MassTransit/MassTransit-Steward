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
    using Magnum.Extensions;
    using NUnit.Framework;


    [TestFixture]
    public class When_executing_a_command :
        CommandTestFixture
    {
        [Test]
        public void Should_execute()
        {
            Uri commandUri = GetActivityContext<MakeMagicHappen>().ExecuteUri;
            var command = new MakeMagicHappenCommand("Hello, World.");


            DispatchCommandHandle<MakeMagicHappenCommand> handle = DispatchEndpoint.DispatchCommand(command, commandUri);

            DispatchAccepted accepted = _accepted.Task.Result;

            Assert.AreEqual(handle.DispatchId, accepted.DispatchId);

            Assert.IsTrue(MagicMakingConsumer.Happened.WaitOne(8.Seconds()));
        }

        TaskCompletionSource<DispatchAccepted> _accepted;

        [TestFixtureSetUp]
        public void Setup()
        {
            _accepted = new TaskCompletionSource<DispatchAccepted>(TestCancellationToken);

            LocalBus.SubscribeHandler<DispatchAccepted>(x => _accepted.SetResult(x));
            Assert.IsTrue(WaitForSubscription<DispatchAccepted>());
        }


        public class MakeMagicHappenCommand :
            MakeMagicHappen
        {
            public MakeMagicHappenCommand(string text)
            {
                Text = text;
            }

            public string Text { get; private set; }
        }


        protected override void SetupCommands()
        {
            AddCommandContext<MagicMakingConsumer, MakeMagicHappen>(() => new MagicMakingConsumer());
        }
    }


    public interface MakeMagicHappen
    {
        string Text { get; }
    }


    class MagicMakingConsumer :
        Consumes<MakeMagicHappen>.Context
    {
        static MagicMakingConsumer()
        {
            Happened = new ManualResetEvent(false);
        }

        public MagicMakingConsumer()
        {
            Happened.Reset();
        }

        public static ManualResetEvent Happened { get; private set; }

        public void Consume(IConsumeContext<MakeMagicHappen> context)
        {
            Happened.Set();
        }
    }
}