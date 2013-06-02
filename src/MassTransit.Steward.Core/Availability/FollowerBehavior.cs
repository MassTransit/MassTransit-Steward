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


    public class FollowerBehavior :
        NodeAvailabilityBehavior
    {
        NodeState _state;
        int _timeoutHandle;

        public FollowerBehavior()
        {
            _state.SuspendAllPeers();

//            ScheduleElectionTimeout();
        }

        public void Consume(IConsumeContext<Heartbeat> context)
        {
//            if (context.Message.Term < _state.CurrentTerm)
//                context.Respond(new YouAreNotMyMommy(_state.CurrentTerm));
//
//            _state.CurrentTerm = context.Message.Term;
//
//            _state.SuspendPeers();
//            ScheduleElectionTimeout();

            // if this heartbeat's previous entry id is not equal to the last one received, that's bad

            // update our to the latest version

            // update commit index

//            context.Respond(new ByYourCommand(_state.CurrentTerm, _commitIndex));
        }


        public void Consume(IConsumeContext<RequestVote> context)
        {
            // reject outdated requests
            if (context.Message.Term >= _state.CurrentTerm)
            {
                _state.CurrentTerm = context.Message.Term;

                if (_state.VotedFor == null || context.Message.Candidate.ControlUri == _state.VotedFor)
                {
                    // check last RPC commit index if it's not recently up to date, don't vote
                    if (_state.LastCommitIndex <= context.Message.LastLogIndex
                        && _state.LastCommitTerm <= context.Message.LastLogTerm)
                    {
                        _state.VotedFor = context.Message.Candidate.ControlUri;

  //                      ScheduleElectionTimeout();

                        context.Respond<VoteFor>(new VoteForResponse(_state.Node, _state.CurrentTerm));
                        return;
                    }
                }
            }

            context.Respond<VoteAgainst>(new VoteAgainstResponse(_state.CurrentTerm));
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