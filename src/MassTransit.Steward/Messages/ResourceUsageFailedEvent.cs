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
namespace MassTransit.Steward.Messages
{
    using System;
    using Contracts;


    class ResourceUsageFailedEvent :
        ResourceUsageFailed
    {
        public ResourceUsageFailedEvent(Guid dispatchId, Uri resource, string reason)
        {
            Resource = resource;
            Reason = reason;
            DispatchId = dispatchId;

            EventId = NewId.NextGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Guid EventId { get; private set; }
        public Guid DispatchId { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Uri Resource { get; private set; }
        public string Reason { get; private set; }
    }
}