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
namespace MassTransit.Steward.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Contracts;
    using Logging;


    public class BusCommandExecutionAgent :
        CommandExecutionAgent
    {
        readonly IServiceBus _bus;
        readonly ILog _log = Logger.Get<BusCommandExecutionAgent>();

        public BusCommandExecutionAgent(IServiceBus bus)
        {
            _bus = bus;
        }

        public void Execute(CommandExecutionContext context)
        {
            try
            {
                Uri sourceAddress = _bus.Endpoint.Address.Uri;

                IEndpoint endpoint = _bus.GetEndpoint(context.Destination);

                ISendContext messageContext = CreateMessageContext(context, sourceAddress);

                endpoint.OutboundTransport.Send(messageContext);

                PublishCommandForwardedEvent(context);
            }
            catch (Exception ex)
            {
                string message = string.Format(CultureInfo.InvariantCulture,
                    "An exception occurred sending message {0} to {1}", context.MessageType, context.Destination);
                _log.Error(message, ex);

                throw new DispatchException(message, ex);
            }
        }

        void PublishCommandForwardedEvent(CommandExecutionContext context)
        {
            var @event = new DispatchAcceptedEvent(context);

            _bus.Publish(@event);
        }


        ISendContext CreateMessageContext(CommandExecutionContext executionContext, Uri sourceAddress)
        {
            var context = new CommandMessageContext(executionContext.Body);

            context.SetSourceAddress(sourceAddress);

            context.SetDestinationAddress(executionContext.DestinationAddress);
            context.SetResponseAddress(executionContext.ResponseAddress);
            context.SetFaultAddress(executionContext.FaultAddress);

            context.SetMessageId(executionContext.MessageId);
            context.SetRequestId(executionContext.RequestId);
            context.SetConversationId(executionContext.ConversationId);
            context.SetCorrelationId(executionContext.CorrelationId);

            if (executionContext.ExpirationTime.HasValue)
                context.SetExpirationTime(executionContext.ExpirationTime.Value);

            context.SetNetwork(executionContext.Network);
            context.SetRetryCount(executionContext.RetryCount);
            context.SetContentType(executionContext.ContentType);

            foreach (var header in executionContext.Headers)
                context.SetHeader(header.Key, header.Value);

            return context;
        }


        class DispatchAcceptedEvent :
            DispatchAccepted
        {
            readonly CommandExecutionContext _context;

            public DispatchAcceptedEvent(CommandExecutionContext context)
            {
                EventId = NewId.NextGuid();
                Timestamp = DateTime.UtcNow;

                _context = context;
            }

            public Guid DispatchId
            {
                get { return _context.CommandId; }
            }

            public DateTime CreateTime
            {
                get { return _context.CreateTime; }
            }

            public IList<Uri> Resources
            {
                get { return _context.Resources; }
            }

            public IList<string> CommandType
            {
                get { return _context.CommandType; }
            }

            public Uri Destination
            {
                get { return _context.Destination; }
            }

            public Guid EventId { get; private set; }
            public DateTime Timestamp { get; private set; }
        }
    }
}