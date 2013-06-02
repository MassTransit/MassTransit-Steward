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
    using Messages;


    public static class ResourceUsageExtensions
    {
        public static void NotifyResourceUsageFailed(this IConsumeContext context, Uri resource, string reason)
        {
            var dispatchId = context.GetDispatchId();

            var @event = new ResourceUsageFailedEvent(dispatchId, resource, reason);

            context.Bus.Publish(@event);
        }

        public static void NotifyResourceUsageCompleted(this IConsumeContext context, Uri resource, DateTime timestamp,
            TimeSpan duration)
        {
            var dispatchId = context.GetDispatchId();

            if (timestamp.Kind == DateTimeKind.Local)
                timestamp = timestamp.ToUniversalTime();

            var @event = new ResourceUsageCompletedEvent(resource, dispatchId, timestamp, duration);

            context.Bus.Publish(@event);
        }

        public static void NotifyResourceUsageCompleted(this IConsumeContext context, Guid dispatchId, Uri resource,
            DateTime timestamp,
            TimeSpan duration)
        {
            if (timestamp.Kind == DateTimeKind.Local)
                timestamp = timestamp.ToUniversalTime();

            var @event = new ResourceUsageCompletedEvent(resource, dispatchId, timestamp, duration);

            context.Bus.Publish(@event);
        }

        public static void NotifyResourceUsageCompleted(this IServiceBus bus, Uri resource)
        {
            var @event = new ResourceUsageCompletedEvent(resource, DateTime.UtcNow, TimeSpan.Zero);

            bus.Publish(@event);
        }

        public static Guid GetDispatchId(this IConsumeContext context)
        {
            Guid dispatchId = Guid.Empty;

            var value = context.Headers["X-MT-DispatchId"];
            if (value != null)
            {
                Guid.TryParse(value, out dispatchId);
            }

            return dispatchId;
        }
    }
}