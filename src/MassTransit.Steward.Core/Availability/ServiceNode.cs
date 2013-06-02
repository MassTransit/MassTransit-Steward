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
namespace MassTransit.Steward.Core.Availability
{
    using System;
    using BusConfigurators;
    using SubscriptionConfigurators;


    /// <summary>
    /// Service node availability is tracked within an entire pub/sub fabric (such as a single RabbitMQ vhost)
    /// </summary>
    public class ServiceNode
    {
        IServiceBus _serviceBus;
        IServiceBus _controlBus;

        readonly NodeState _nodeState;

        /// <summary>
        /// Creates a service node which can participate in service availability tracking
        /// </summary>
        public ServiceNode()
        {
            _nodeState = new ServiceNodeState();
        }

        /// <summary>
        /// Used to configure information about the service bus for processing
        /// </summary>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public ServiceBusConfigurator ConfigureServiceBus(ServiceBusConfigurator configurator)
        {

            return configurator;
        }

        /// <summary>
        /// Configure the control bus, giving the caller the option of adding additional subscriptions to the control bus
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="subscribeCallback"></param>
        /// <returns></returns>
        public ServiceBusConfigurator ConfigureControlBus(ServiceBusConfigurator configurator, Action<SubscriptionBusServiceConfigurator> subscribeCallback)
        {
            // we need a nice, ordered arrival of messages here
            configurator.SetConcurrentConsumerLimit(1);

            configurator.Subscribe(x =>
                {
                    // this is registered as transient because the control bus should not get messages
                    // when it isn't running (and should be a temporary queue as well)
                    x.Consumer(() => new ServiceNodeConsumer(_nodeState.CurrentBehavior))
                        .Transient();
                });

            return configurator;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceBus">The service bus on which commands are received</param>
        /// <param name="controlBus">The control bus for the synchronization messages between nodes</param>
        public void Start(IServiceBus serviceBus, IServiceBus controlBus)
        {
            _serviceBus = serviceBus;
            _controlBus = controlBus;

            _nodeState.ChangeTo<FollowerBehavior>();
        }

        public void Stop()
        {
            _serviceBus = null;
            _controlBus = null;
        }
    }
}