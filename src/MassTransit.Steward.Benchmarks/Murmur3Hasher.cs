﻿// Copyright 2007-2013 Chris Patterson
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
    using Core.Distribution.ConsistentHashing;


    public class Murmur3Hasher :
        Hasher
    {
        readonly HashGenerator _hashGenerator;

        public Murmur3Hasher()
        {
            _hashGenerator = new Murmur3AUnsafe();
        }

        public uint Hash(string value)
        {
            return _hashGenerator.Hash(value);
        }
    }
}