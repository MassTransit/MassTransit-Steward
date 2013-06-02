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
namespace MassTransit.Steward
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Pipeline;


    public class DispatchConsumerFactory<TConsumer> :
        IConsumerFactory<TConsumer>
        where TConsumer : class
    {
        readonly IConsumerFactory<TConsumer> _consumerFactory;

        public DispatchConsumerFactory(IConsumerFactory<TConsumer> consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        public IEnumerable<Action<IConsumeContext<TMessage>>> GetConsumer<TMessage>(IConsumeContext<TMessage> context,
            InstanceHandlerSelector<TConsumer, TMessage> selector)
            where TMessage : class
        {
            foreach (var action in _consumerFactory.GetConsumer(context, selector))
            {
                Action<IConsumeContext<TMessage>> consumer = action;
                yield return x =>
                    {
                        DateTime startTime = DateTime.UtcNow;
                        Stopwatch timer = Stopwatch.StartNew();
                        try
                        {
                            consumer(x);

                            timer.Stop();

                            x.NotifyResourceUsageCompleted(x.Bus.Endpoint.Address.Uri, startTime, timer.Elapsed);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    };
            }
        }
    }
}