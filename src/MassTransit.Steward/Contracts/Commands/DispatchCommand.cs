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
namespace MassTransit.Steward.Contracts
{
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// Execute a command using a resource
    /// </summary>
    public interface DispatchCommand
    {
        /// <summary>
        /// Uniquely identifies the command to execute
        /// </summary>
        Guid DispatchId { get; }

        /// <summary>
        /// The timestamp at which the command execution was requested
        /// </summary>
        DateTime CreateTime { get; }

        /// <summary>
        /// A unique URI identifying the resource(s) being accessed by the command
        /// </summary>
        IList<Uri> Resources { get; }

        /// <summary>
        /// The message types implemented by the command message
        /// </summary>
        IList<string> CommandTypes { get; }

        /// <summary>
        /// The destination where the command message should be sent for execution
        /// </summary>
        Uri Destination { get; }
    }


    /// <summary>
    /// A strongly typed version of the execute command
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface DispatchCommand<out T> :
        DispatchCommand
    {
        /// <summary>
        /// The command to be published
        /// </summary>
        T Command { get; }
    }
}