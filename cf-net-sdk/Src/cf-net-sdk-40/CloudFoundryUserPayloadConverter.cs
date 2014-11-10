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
    internal class CloudFoundryUserPayloadConverter : IUserPayloadConverter
    {
        /// <inheritdoc/>
        public IEnumerable<User> ConvertUsers(string payload)
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
                            "Users payload could not be parsed. Resources property is null. Payload: '{0}'",
                            payload));
                }

                return usrTokens.Select(this.ConvertUser).ToList();
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Users payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <inheritdoc/>
        public User ConvertUser(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");

            try
            {
                var token = JToken.Parse(payload);
                return this.ConvertUser(token);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("User payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a Json token into a User object.
        /// </summary>
        /// <param name="token">The Json token to convert.</param>
        /// <returns>The User object.</returns>
        internal User ConvertUser(JToken token)
        {
            token.AssertIsNotNull("token", "Cannot convert a user with a null token or payload.");

            try
            {
                var metadata = token["metadata"];
                if (metadata == null)
                {
                    throw new FormatException(string.Format("User payload could not be parsed. Metadata property cannot be null or empty. Payload: '{0}'", token));
                }

                var entity = token["entity"];
                if (entity == null)
                {
                    throw new FormatException(string.Format("User payload could not be parsed. Entity property cannot be null or empty. Payload: '{0}'", token));
                }

                var name = (string)entity["username"];
                var id = (string)metadata["guid"];

                //This has been added because this is a special user that does not have a user name. 
                //This can and should be removed as soon as the CF api no longer includes this user/workaround for legacy applications.
                if (id == "legacy-api")
                {
                    name = "legacy-api";
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
                {
                    throw new FormatException(string.Format("User payload could not be parsed. A required property is missing. Payload: '{0}'", token));
                }

                var created = metadata["created_at"] == null ? DateTime.MinValue : (DateTime)metadata["created_at"];

                return new User(id, name, created);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("User payload could not be parsed. Payload: '{0}'", token), ex);
            }
        }
    }
}
