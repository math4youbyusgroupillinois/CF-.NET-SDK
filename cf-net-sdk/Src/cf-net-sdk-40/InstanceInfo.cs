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
    /// Includes information regarding a remote Cloud Foundry instance.
    /// </summary>
    public class InstanceInfo
    {
        public string Name { get; internal set; }

        public string Build { get; internal set; }

        public string Version { get; internal set; }

        public Uri AuthorizationEndpoint { get; internal set; }

        public Uri TokenEndpoint { get; internal set; }

        internal InstanceInfo(string name, string build, string version, Uri authEndpoint, Uri tokenEndpoint)
        {
            name.AssertIsNotNullOrEmpty("name","Cannot create InstanceInfo with a null or empty name.");
            build.AssertIsNotNullOrEmpty("build", "Cannot create InstanceInfo with a null or empty build.");
            version.AssertIsNotNullOrEmpty("version", "Cannot create InstanceInfo with a null or empty version.");

            this.Name = name;
            this.Build = build;
            this.Version = version;
            this.AuthorizationEndpoint = authEndpoint;
            this.TokenEndpoint = tokenEndpoint;
        }
    }
}
