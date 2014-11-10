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
using System.Collections.Generic;

namespace cf_net_sdk
{
    /// <summary>
    /// Represents an application on a remote Cloud Foundry instance.
    /// </summary>
    public class Application : Entity
    {
        /// <summary>
        /// The current state of the application.
        /// </summary>
        public ApplicationStatus Status { get; internal set; }

        /// <summary>
        /// The number of instances currently running for this application.
        /// </summary>
        public int Instances { get; internal set; }

        /// <summary>
        /// The maximum limit of memory for this application.
        /// </summary>
        public int MemoryLimit { get; internal set; }

        /// <summary>
        /// The list of routes mapped to this application.
        /// </summary>
        public IEnumerable<Route> Routes { get; internal set; }

        /// <summary>
        /// Creates and instance of the Application class.
        /// </summary>
        /// <param name="id">The id of the application.</param>
        /// <param name="name">The name of the application.</param>
        /// <param name="status">The current state of the application.</param>
        /// <param name="memoryLimit">The maximum limit of memory for this application.</param>
        /// <param name="createdDate">The date that the application was created.</param>
        /// <param name="instances">The number of instances currently running for this application.</param>
        internal Application(string id, string name, ApplicationStatus status, int instances, int memoryLimit, DateTime createdDate)
            : base(id, name, createdDate)
        {
            this.Routes = new List<Route>();
            this.Instances = instances;
            this.MemoryLimit = memoryLimit;
            this.Status = status;
        }
    }
}
