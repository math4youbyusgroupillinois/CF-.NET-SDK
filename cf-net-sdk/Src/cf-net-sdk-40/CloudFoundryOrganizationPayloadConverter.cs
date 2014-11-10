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
using System.Linq;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using Newtonsoft.Json.Linq;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    internal class CloudFoundryOrganizationPayloadConverter : IOrganizationPayloadConverter
    {
        /// <inheritdoc/>
        public IEnumerable<Organization> ConvertOrganizations(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");

            try
            {
                var obj = JObject.Parse(payload);
                var orgTokens = obj["resources"];

                if (orgTokens == null)
                {
                    throw new FormatException(
                        string.Format(
                            "Organizations payload could not be parsed. Resources property is null. Payload: '{0}'",
                            payload));
                }

                return orgTokens.Select(this.ConvertOrganization).ToList();
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Organizations payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <inheritdoc/>
        public Organization ConvertOrganization(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");

            try
            {
                var token = JToken.Parse(payload);
                return this.ConvertOrganization(token);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Organization payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a Json token into an Organization.
        /// </summary>
        /// <param name="token">The Json token to convert.</param>
        /// <returns>The Organization object.</returns>
        internal Organization ConvertOrganization(JToken token)
        {
            token.AssertIsNotNull("token","Cannot convert an organization with a null token or payload.");

            try
            {
                var metadata = token["metadata"];
                if (metadata == null)
                {
                    throw new FormatException(string.Format("Organization payload could not be parsed. Metadata property cannot be null or empty. Payload: '{0}'", token));
                }

                var entity = token["entity"];
                if (entity == null)
                {
                    throw new FormatException(string.Format("Organization payload could not be parsed. Entity property cannot be null or empty. Payload: '{0}'", token));
                }

                var name = (string)entity["name"];
                var id = (string)metadata["guid"];
                var status = (string) entity["status"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(status))
                {
                    throw new FormatException(string.Format("Organization payload could not be parsed. A required property is missing. Payload: '{0}'", token));
                }

                var created = metadata["created_at"] == null ? DateTime.MinValue : (DateTime)metadata["created_at"];

                return new Organization(id, name, status, created);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Organization payload could not be parsed. Payload: '{0}'", token), ex);
            }
        }
    }
}
