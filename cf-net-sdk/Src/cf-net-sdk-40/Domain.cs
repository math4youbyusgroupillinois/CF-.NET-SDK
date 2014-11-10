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
    public class Domain : Entity
    {
        /// <summary>
        /// Creates a new instance of the Domain class.
        /// </summary>
        /// <param name="id">The id of the domain.</param>
        /// <param name="name">The name of the domain.</param>
        /// <param name="createdDate">The time when the domain was created.</param>
        internal Domain(string id, string name, DateTime createdDate)
            : base(id, name, createdDate)
        {

        }
    }
}
