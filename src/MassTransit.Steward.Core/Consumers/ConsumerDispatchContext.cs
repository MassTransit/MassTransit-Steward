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
namespace MassTransit.Steward.Core.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contexts;
    using Contracts;


    public class ConsumerDispatchContext :
        DispatchContextBase,
        DispatchContext
    {
        readonly string _body;
        readonly IConsumeContext<DispatchMessage> _messageContext;

        public ConsumerDispatchContext(IConsumeContext<DispatchMessage> messageContext, string body)
        {
            _messageContext = messageContext;
            _body = body;
        }

        public Guid DispatchId
        {
            get { return _messageContext.Message.DispatchId; }
        }

        public DateTime CreateTime
        {
            get { return _messageContext.Message.CreateTime; }
        }

        public IList<Uri> Resources
        {
            get { return _messageContext.Message.Resources; }
        }

        public IList<string> DispatchTypes
        {
            get { return _messageContext.Message.PayloadTypes; }
        }

        public Uri Destination
        {
            get { return _messageContext.Message.Destination; }
        }

        public string ContentType
        {
            get { return _messageContext.ContentType; }
        }

        public string RequestId
        {
            get { return _messageContext.RequestId; }
        }

        public string ConversationId
        {
            get { return _messageContext.ConversationId; }
        }

        public string CorrelationId
        {
            get { return _messageContext.CorrelationId; }
        }

        public Uri SourceAddress
        {
            get { return _messageContext.SourceAddress; }
        }

        public Uri DestinationAddress
        {
            get { return _messageContext.DestinationAddress; }
        }

        public Uri ResponseAddress
        {
            get { return _messageContext.ResponseAddress; }
        }

        public Uri FaultAddress
        {
            get { return _messageContext.FaultAddress; }
        }

        public string Network
        {
            get { return _messageContext.Network; }
        }

        public DateTime? ExpirationTime
        {
            get { return _messageContext.ExpirationTime; }
        }

        public int RetryCount
        {
            get { return _messageContext.RetryCount; }
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _messageContext.Headers.ToDictionary(x => x.Key, x => x.Value); }
        }

        public string Body
        {
            get { return _body; }
        }

        public bool TryGetContext<T>(out MessageDispatchContext<T> context)
            where T : class
        {
            context = null;
            return false;
        }
    }
}