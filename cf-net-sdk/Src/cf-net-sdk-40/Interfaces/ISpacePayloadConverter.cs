﻿// /* ============================================================================
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

using System.Collections.Generic;

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Converts Json payloads in to Space objects.
    /// </summary>
    public interface ISpacePayloadConverter
    {
        /// <summary>
        /// Converts a Json payload to a list of Space objects.
        /// </summary>
        /// <param name="payload">A Json payload.</param>
        /// <returns>An enumerable list of Space objects.</returns>
        IEnumerable<Space> ConvertSpaces(string payload);

        /// <summary>
        /// Converts a Json payload to a Space object.
        /// </summary>
        /// <param name="payload">The Json payload to convert</param>
        /// <returns>The Space object.</returns>
        Space ConvertSpace(string payload);
    }
}
