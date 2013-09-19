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
namespace MassTransit.Steward.Benchmarks
{
    using System.Collections.Generic;
    using Benchmarque;


    public class ThroughputHasherBenchmark :
        Benchmark<Hasher>
    {
        string[] _targets;

        public void WarmUp(Hasher instance)
        {
            const string firstValue = "abcdefghijkmnopqrstuvwxyzabcdefghijkmnopqrstuvwxyzabcdefghijkmnopqrstuvwxyz";
            const string secondValue = "ABCDEFGHIJKMNOPQRSTUVWXYZABCDEFGHIJKMNOPQRSTUVWXYZABCDEFGHIJKMNOPQRSTUVWXYZ";

            _targets = new[] {firstValue, secondValue};
            instance.Hash("Let's get started");
        }

        public void Shutdown(Hasher instance)
        {
        }

        public void Run(Hasher instance, int iterationCount)
        {
            for (int i = 0; i < iterationCount; i++)
                instance.Hash(_targets[i % 2]);
        }

        public IEnumerable<int> Iterations
        {
            get
            {
                yield return 100000;
                yield return 1000000;
            }
        }
    }
}