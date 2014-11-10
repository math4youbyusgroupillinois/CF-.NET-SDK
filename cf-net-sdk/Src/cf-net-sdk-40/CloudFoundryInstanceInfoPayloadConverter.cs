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
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using Newtonsoft.Json.Linq;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    internal class CloudFoundryInstanceInfoPayloadConverter : IInstanceInfoPayloadConverter
    {
        /// <inheritdoc/>
        public InstanceInfo Convert(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert instance information with a null or empty payload.");

            try
            {
                var obj = JObject.Parse(payload);

                var name = (string)obj["name"];
                var build = (string)obj["build"];
                var version = (string)obj["version"];
                var authUrl = (string)obj["authorization_endpoint"];
                var tokenUrl = (string)obj["token_endpoint"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(build) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(authUrl) || string.IsNullOrEmpty(tokenUrl))
                {
                    throw new FormatException(string.Format("Instance information payload could not be parsed. A required property is missing. Payload: '{0}'", payload));
                }

                var authUri = new Uri(authUrl);
                var tokenUri = new Uri(tokenUrl);

                return new InstanceInfo(name, build, version, authUri, tokenUri);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Instance information payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }
    }
}
