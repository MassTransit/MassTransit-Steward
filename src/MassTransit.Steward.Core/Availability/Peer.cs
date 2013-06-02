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
    public interface Peer
    {
        /// <summary>
        /// The control endpoint of the peer for direct communication
        /// </summary>
        IEndpoint ControlEndpoint { get; }

        /// <summary>
        /// The service endpoint of the peer for monitoring and resource availability tracking
        /// </summary>
        IEndpoint ServiceEndpoint { get; }

        /// <summary>
        /// Suspend any communication with the peer (no more commands, pings)
        /// </summary>
        void Suspend();
    }
}