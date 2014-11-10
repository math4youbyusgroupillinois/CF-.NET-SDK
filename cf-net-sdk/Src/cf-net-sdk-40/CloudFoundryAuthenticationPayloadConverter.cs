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
    internal class CloudFoundryAuthenticationPayloadConverter : IAuthenticationPayloadConverter
    {
        /// <inheritdoc/>
        public string ConvertAccessToken(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload","Cannot convert a null or empty payload.");

            try
            {
                var obj = JObject.Parse(payload);
                var token = (string) obj["access_token"];

                if (token == null)
                {
                    throw new FormatException(string.Format("Authentication payload could not be parsed. Access token is null. Payload: '{0}'", payload));
                }

                return token;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Authentication payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }
    }
}
