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
    /// Represents an entity on a remote Cloud Foundry instance.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// The unique identifier for the entity.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// The name of the entity.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The date and time when the entity was created.
        /// </summary>
        public DateTime CreatedDate { get; internal set; }

        /// <summary>
        /// Creates a new instance of the Entity class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <param name="name">The name of the entity.</param>
        /// <param name="createDate">The date and time when the entity was created.</param>
        internal Entity(string id, string name, DateTime createDate)
        {
            id.AssertIsNotNullOrEmpty("id", "Cannot create an entity with a null or empty id.");
            name.AssertIsNotNullOrEmpty("name", "Cannot create an entity with a null or empty name.");

            this.Id = id;
            this.Name = name;
            this.CreatedDate = createDate;
        }
    }
}
