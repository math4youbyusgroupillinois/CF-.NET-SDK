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
using CloudFoundry.Common;

namespace cf_net_sdk
{
    /// <summary>
    /// Represents a route on a remote Cloud Foundry instance.
    /// </summary>
    public class Route : Entity
    {
        /// <summary>
        /// The host name of the route.
        /// </summary>
        public string HostName { get { return this.Name; } }

        /// <summary>
        /// The domain name of the route.
        /// </summary>
        public string DomainName { get; internal set; }

        /// <summary>
        /// The fully qualified domain name for the mapped route.
        /// </summary>
        public string FullyQualifiedDomainName { get { return string.Join(".", this.Name, DomainName); }}

        /// <summary>
        /// Creates a new instance of the Route class.
        /// </summary>
        public Route(string id, string name, string domainName, DateTime createDate) : base(id, name, createDate)
        {
            domainName.AssertIsNotNullOrEmpty("domainName","Cannot create a route with a null or empty domain name.");

            this.DomainName = domainName;
        }
    }
}
