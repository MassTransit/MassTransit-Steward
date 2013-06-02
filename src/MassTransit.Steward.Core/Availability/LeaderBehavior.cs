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
    using Contracts.Availability;

    /// <summary>
    /// The leader is responsible for sending heartbeats to every other node to keep
    /// the outerlying systems in line.
    /// </summary>
    public class LeaderBehavior :
        NodeAvailabilityBehavior
    {
        public void Consume(IConsumeContext<Heartbeat> context)
        {
        }

        public void Consume(IConsumeContext<RequestVote> message)
        {
            throw new System.NotImplementedException();
        }

        public void Consume(IConsumeContext<VoteFor> message)
        {
            throw new System.NotImplementedException();
        }

        public void Consume(IConsumeContext<VoteAgainst> message)
        {
            throw new System.NotImplementedException();
        }
    }
}