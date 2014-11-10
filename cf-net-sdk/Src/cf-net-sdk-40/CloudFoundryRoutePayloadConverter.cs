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
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    internal class CloudFoundryRoutePayloadConverter : IRoutePayloadConverter
    {
        /// <inheritdoc/>
        public IEnumerable<Route> ConvertRoutes(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");

            try
            {
                var obj = JObject.Parse(payload);
                var usrTokens = obj["resources"];

                if (usrTokens == null)
                {
                    throw new FormatException(
                        string.Format(
                            "Routes payload could not be parsed. Resources property is null. Payload: '{0}'",
                            payload));
                }

                return usrTokens.Select(this.ConvertRoute).ToList();
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Routes payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <inheritdoc/>
        public Route ConvertRoute(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");

            try
            {
                var token = JToken.Parse(payload);
                return this.ConvertRoute(token);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Route payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a Json token into a Route object.
        /// </summary>
        /// <param name="token">The Json token to convert.</param>
        /// <returns>The Route object.</returns>
        internal Route ConvertRoute(JToken token)
        {
            token.AssertIsNotNull("token", "Cannot convert a route with a null token or payload.");

            try
            {
                var metadata = token["metadata"];
                if (metadata == null)
                {
                    throw new FormatException(string.Format("Route payload could not be parsed. Metadata property cannot be null or empty. Payload: '{0}'", token));
                }

                var entity = token["entity"];
                if (entity == null)
                {
                    throw new FormatException(string.Format("Route payload could not be parsed. Entity property cannot be null or empty. Payload: '{0}'", token));
                }

                var domainEntity = entity["domain"];
                if (domainEntity == null)
                {
                    throw new FormatException(string.Format("Route payload could not be parsed. Domain property cannot be null or empty. Payload: '{0}'", token));
                }

                var hostName = (string)entity["host"];
                var id = (string)metadata["guid"];
                var domainName = GetDomainName(domainEntity);

                if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(domainName))
                {
                    throw new FormatException(string.Format("Route payload could not be parsed. A required property is missing. Payload: '{0}'", token));
                }

                var created = metadata["created_at"] == null ? DateTime.MinValue : (DateTime)metadata["created_at"];

                return new Route(id, hostName, domainName, created);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Route payload could not be parsed. Payload: '{0}'", token), ex);
            }
        }

        /// <summary>
        /// Gets the domain name for a route using it's domain Json token.
        /// </summary>
        /// <param name="domainToken">The domain Json Token.</param>
        /// <returns>The domain name.</returns>
        internal string GetDomainName(JToken domainToken)
        {
            domainToken.AssertIsNotNull("token", "Cannot convert a route with a null or empty domain token.");

            try
            {
                var entity = domainToken["entity"];
                if (entity == null)
                {
                    throw new FormatException(string.Format("Domain entity could not be parsed. Entity property cannot be null or empty. Payload: '{0}'", domainToken));
                }

                var domainName = (string)entity["name"];

                if (string.IsNullOrEmpty(domainName))
                {
                    throw new FormatException(string.Format("Domain entity could not be parsed. A required property is missing. Payload: '{0}'", domainToken));
                }

               return domainName;
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Domain entity could not be parsed. Payload: '{0}'", domainToken), ex);
            }
        }
    }
}
