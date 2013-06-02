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
    using Contracts.Availability;


    public interface NodeState :
        NodeAvailabilityBehavior
    {
        long CurrentTerm { get; set; }
        long LastCommitIndex { get; }
        long LastCommitTerm { get; }
        Uri VotedFor { get; set; }
        Node Node { get; }
        int QuorumSize { get; }
        NodeAvailabilityBehavior CurrentBehavior { get; }

        /// <summary>
        /// The peers of this node, which are added and removed as part of the Raft protocol to ensure
        /// that all peers have the same list of nodes
        /// </summary>
        Peers Peers { get; }



        /// <summary>
        /// Change the node behavior to the specified behavior
        /// </summary>
        /// <typeparam name="T">The behavior type to adapt</typeparam>
        void ChangeTo<T>()
            where T : NodeAvailabilityBehavior;
    }

    public static class NodeStateExtensions
    {
        public static void SuspendAllPeers(this NodeState nodeState)
        {
            if (nodeState == null)
                throw new ArgumentNullException("nodeState");

            foreach (var peer in nodeState.Peers)
            {
                peer.Suspend();
            }
        }
    }


    class ServiceNodeState : 
        NodeState
    {
        long _currentTerm;
        NodeAvailabilityBehavior _currentBehavior;

        public long CurrentTerm
        {
            get { return _currentTerm; }
            set
            {
                if (value > _currentTerm)
                {
                    _currentTerm = value;
                    VotedFor = null;

                    this.SuspendAllPeers();

                    ChangeTo<FollowerBehavior>();
                }
            }
        }

        public long LastCommitIndex { get; private set; }
        public long LastCommitTerm { get; private set; }
        public Uri VotedFor { get; set; }
        public Node Node { get; private set; }
        public int QuorumSize { get; private set; }
        public NodeAvailabilityBehavior CurrentBehavior
        {
            get { return _currentBehavior; }
        }

        public Peers Peers { get; private set; }


        public void ChangeTo<T>() where T : 
            NodeAvailabilityBehavior
        {
            throw new NotImplementedException();
        }

        public void Consume(IConsumeContext<Heartbeat> message)
        {
            _currentBehavior.Consume(message);
        }

        public void Consume(IConsumeContext<RequestVote> message)
        {
            _currentBehavior.Consume(message);
        }

        public void Consume(IConsumeContext<VoteFor> message)
        {
            _currentBehavior.Consume(message);
        }

        public void Consume(IConsumeContext<VoteAgainst> message)
        {
            _currentBehavior.Consume(message);
        }
    }
}