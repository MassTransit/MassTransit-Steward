namespace MassTransit.Steward.Core.Availability
{
    using System;
    using System.Linq;
    using Contracts.Availability;
    using Magnum.Caching;
    using Messages;


    public class CandidateBehavior :
        NodeAvailabilityBehavior
    {
        NodeState _state;
        bool _elected;
        Cache<Uri, bool> _votes;
        readonly long _candidateTerm;

        public CandidateBehavior()
        {
            PromoteToCandidate();
            _candidateTerm = _state.CurrentTerm;

            var voteRequest = new RequestVoteRequest(_state.Node, _candidateTerm, _state.LastCommitIndex, _state.LastCommitTerm);

            foreach (var peer in _state.Peers)
            {
                peer.ControlEndpoint.Send(voteRequest);
            }

//            ScheduleElectionExpiration(_electionTimeout * 2);
        }

        public void Consume(IConsumeContext<VoteFor> context)
        {
            if (context.Message.Term > _candidateTerm)
            {
                _state.CurrentTerm = context.Message.Term;
                _state.ChangeTo<FollowerBehavior>();
                return;
            }

            _votes[context.Message.Supporter.ControlUri] = true;

            var votesGranted = _votes.Count(x => x);

            if (votesGranted >= _state.QuorumSize)
            {
                _elected = true;
            }

            if (_elected)
            {
//                if (_state.PromoteToLeader(_candidateTerm, lastLogIndex, lastLogTerm))
//                    _state.ChangeTo<LeaderBehavior>();
            }
        }

        public void Consume(IConsumeContext<VoteAgainst> context)
        {
            if (context.Message.Term > _candidateTerm)
            {
                _state.CurrentTerm = context.Message.Term;
                _state.ChangeTo<FollowerBehavior>();
                return;
            }

//            _votes[context.Message.Supporter.ControlUri] = false;
        }


        void PromoteToCandidate()
        {
            _state.CurrentTerm++;
            _state.VotedFor = _state.Node.ControlUri;

            // no election timer
        }


        public void Consume(IConsumeContext<Heartbeat> message)
        {
            throw new NotImplementedException();
        }

        public void Consume(IConsumeContext<RequestVote> message)
        {
            throw new NotImplementedException();
        }
    }
}