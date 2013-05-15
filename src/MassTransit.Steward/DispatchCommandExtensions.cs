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
    using System.Linq;
    using Contracts;


    public static class DispatchCommandExtensions
    {
        /// <summary>
        /// Dispatches a command to a service
        /// </summary>
        /// <typeparam name="T">The command type</typeparam>
        /// <param name="endpoint">The dispatcher endpoint</param>
        /// <param name="command">The command</param>
        /// <param name="destination">The destination address of the service endpoint</param>
        /// <param name="resources">The resources required by the command</param>
        /// <returns>A handle to the dispatch</returns>
        public static DispatchCommandHandle<T> DispatchCommand<T>(this IEndpoint endpoint, T command, Uri destination,
            params Uri[] resources)
            where T : class
        {
            var message = new DispatchCommandMessage<T>(destination, command, resources);

            endpoint.Send<DispatchCommand<T>>(message);

            return new DispatchedCommandHandle<T>(message.DispatchId, message.CreateTime, message.Destination, command);
        }


        class DispatchCommandMessage<T> :
            DispatchCommand<T>
            where T : class
        {
            public DispatchCommandMessage(Uri destination, T command, IEnumerable<Uri> resources)
            {
                if (destination == null)
                    throw new ArgumentNullException("destination");
                if (command == null)
                    throw new ArgumentNullException("command");
                if (resources == null)
                    throw new ArgumentNullException("resources");

                Destination = destination;
                Command = command;

                DispatchId = NewId.NextGuid();
                CreateTime = DateTime.UtcNow;

                CommandTypes = typeof(T).GetMessageTypes()
                                        .Select(x => new MessageUrn(x).ToString())
                                        .ToList();

                Resources = resources.ToList();
            }

            public Guid DispatchId { get; private set; }
            public DateTime CreateTime { get; private set; }
            public IList<Uri> Resources { get; private set; }
            public IList<string> CommandTypes { get; private set; }
            public Uri Destination { get; private set; }
            public T Command { get; private set; }
        }


        class DispatchedCommandHandle<T> :
            DispatchCommandHandle<T>
            where T : class
        {
            public DispatchedCommandHandle(Guid commandId, DateTime createTime, Uri destination, T command)
            {
                DispatchId = commandId;
                CreateTime = createTime;
                Destination = destination;
                Command = command;
            }

            public Guid DispatchId { get; private set; }
            public DateTime CreateTime { get; private set; }
            public Uri Destination { get; private set; }
            public T Command { get; private set; }
        }
    }
}