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
    using BusConfigurators;
    using Subscriptions.Coordinator;


    public interface CommandTestContext :
        IDisposable
    {
        Uri ExecuteUri { get; }

        IServiceBus ExecuteBus { get; }

        void ConfigureServiceBus(ServiceBusConfigurator configurator);
    }


    public class CommandTestContext<TConsumer, T> :
        CommandTestContext
        where T : class
        where TConsumer : class, Consumes<T>.Context
    {
        readonly Func<TConsumer> _consumerFactory;
        SubscriptionLoopback _executeLoopback;

        public CommandTestContext(Uri baseUri, Func<TConsumer> consumerFactory)
        {
            _consumerFactory = consumerFactory;

            Name = GetServiceName();

            ExecuteUri = BuildQueueUri(baseUri, "execute");

            ExecuteBus = ServiceBusFactory.New(ConfigureExecuteBus);
        }

        public string Name { get; private set; }
        public IServiceBus ExecuteBus { get; protected set; }
        public Uri ExecuteUri { get; private set; }

        public void ConfigureServiceBus(ServiceBusConfigurator configurator)
        {
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    var loopback = new SubscriptionLoopback(bus, coordinator);
                    loopback.SetTargetCoordinator(_executeLoopback.Router);
                    return loopback;
                });
        }

        public void Dispose()
        {
            ExecuteBus.Dispose();
        }

        static string GetServiceName()
        {
            return typeof(T).Name + "Service";
        }

        Uri BuildQueueUri(Uri baseUri, string prefix)
        {
            return new Uri(baseUri, string.Format("{0}_{1}", prefix, typeof(T).Name.ToLowerInvariant()));
        }

        protected virtual void ConfigureExecuteBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(ExecuteUri);
            configurator.AddSubscriptionObserver((bus, coordinator) =>
                {
                    _executeLoopback = new SubscriptionLoopback(bus, coordinator);
                    return _executeLoopback;
                });

            configurator.Subscribe(s => s.Consumer(_consumerFactory));
        }
    }
}