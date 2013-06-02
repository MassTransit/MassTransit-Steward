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
    using Messages;


    public class StoppedBehavior :
        NodeAvailabilityBehavior
    {
        readonly NodeState _state;

        public StoppedBehavior(NodeState state)
        {
            _state = state;
        }

        public void Consume(IConsumeContext<Heartbeat> context)
        {
            // do nothing, we're stopped, so bugger off
            
        }

        public void Start()
        {
            _state.CurrentTerm = 0; // start at zero
            
            // get the last term for this node

            _state.ChangeTo<FollowerBehavior>();

        }

        public void Consume(IConsumeContext<RequestVote> context)
        {
            context.Respond<VoteAgainst>(new VoteAgainstResponse(_state.CurrentTerm));
        }

        public void Consume(IConsumeContext<VoteFor> message)
        {
        }

        public void Consume(IConsumeContext<VoteAgainst> message)
        {
        }
    }
}