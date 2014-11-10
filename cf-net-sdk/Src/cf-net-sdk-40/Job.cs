// /* ============================================================================
// Copyright 2014 Hewlett Packard
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System;

namespace cf_net_sdk
{
    /// <summary>
    /// Represents a space on a remote Cloud Foundry instance.
    /// </summary>
    public class Job : Entity
    {
        /// <summary>
        /// The current state of the instance.
        /// </summary>
        public JobState State { get; internal set; }

        /// <summary>
        /// Creates a new instance of the Space class.
        /// </summary>
        /// <param name="id">The id of the Job.</param>
        /// <param name="name">The name of the Job.</param>
        /// <param name="state">The state of the job.</param>
        /// <param name="createdDate">The time when the Job was created.</param>
        internal Job(string id, string name, JobState state, DateTime createdDate)
            : base(id, name, createdDate)
        {
            this.State = state;
        }
    }
}
