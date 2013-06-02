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
    using System.Threading;
    using Taskell;


    class ServiceNodePeer :
        Peer
    {
        CancellationTokenSource _cancel;
        TaskComposer<Peer> _composer;
        int _pingTimeout;

        public ServiceNodePeer(IEndpoint controlEndpoint, IEndpoint serviceEndpoint, int pingTimeout)
        {
            ControlEndpoint = controlEndpoint;
            ServiceEndpoint = serviceEndpoint;
            _pingTimeout = pingTimeout;

            _cancel = new CancellationTokenSource();
            _composer = new TaskComposer<Peer>(_cancel.Token);
        }

        public IEndpoint ControlEndpoint { get; private set; }
        public IEndpoint ServiceEndpoint { get; private set; }

        public void Suspend()
        {
            _cancel.Cancel();
        }


        public void Ping(IServiceBus nodeControlBus)
        {
            // send ping to peer

            _cancel = new CancellationTokenSource();
            _composer = new TaskComposer<Peer>(_cancel.Token);

            _composer.Delay(_pingTimeout);
            _composer.Execute(() =>
                {
//                    nodeControlBus.Endpoint.Send(new PeerPingTimeout(ControlEndpoint));
                });

        }
    }
}