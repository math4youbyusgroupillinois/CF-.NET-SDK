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
    internal class CloudFoundryInstancePayloadConverter : IInstancePayloadConverter
    {
        /// <inheritdoc/>
        public IEnumerable<Instance> ConvertInstances(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");

            try
            {
                var instances = new List<Instance>();
                var obj = JObject.Parse(payload);

                foreach (var prop in obj.Properties())
                {
                    var id = prop.Path;
                    var token = prop.Value;
                    instances.Add(ConvertInstance(id, token)); 
                }
                return instances;
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Instances payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <inheritdoc/>
        public Instance ConvertInstance(string id, string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "Cannot convert a null or empty payload.");
            id.AssertIsNotNullOrEmpty("id","Cannot convert an instance with a null or empty id.");

            try
            {
                var token = JToken.Parse(payload);
                return this.ConvertInstance(id, token);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Instance payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a Json token into an Instance object.
        /// </summary>
        /// <param name="token">The Json token to convert.</param>
        /// <param name="id">The id of the instance.</param>
        /// <returns>The Instance object.</returns>
        internal Instance ConvertInstance(string id, JToken token)
        {
            token.AssertIsNotNull("token", "Cannot convert an instance with a null token or payload.");

            try
            {
                var stats = token["stats"];
                if (stats == null)
                {
                    throw new FormatException(string.Format("Instance payload could not be parsed. Stats property cannot be null or empty. Payload: '{0}'", token));
                }

                var usage = stats["usage"];
                if (usage == null)
                {
                    throw new FormatException(string.Format("Instance payload could not be parsed. Usage property cannot be null or empty. Payload: '{0}'", token));
                }

                var state = ((string)token["state"]).GetInstanceState();
                var host = (string)stats["host"];
                var port = (int)stats["port"];
                var uptime = (int) stats["uptime"];
                var memUsage = (long)usage["mem"];
                var cpuUsage = (long)usage["cpu"];
                var diskUsage = (long)usage["disk"];
                var memQuota = (long)stats["mem_quota"];
                var diskQuota = (long)stats["disk_quota"];

                if (string.IsNullOrEmpty(host))
                {
                    throw new FormatException(string.Format("Instance payload could not be parsed. A required property is missing. Payload: '{0}'", token));
                }

                var created = usage["time"] == null ? DateTime.MinValue : (DateTime)usage["time"];

                return new Instance(id, host, state, port, memUsage, cpuUsage, diskUsage, diskQuota, memQuota, uptime, created);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Instance payload could not be parsed. Payload: '{0}'", token), ex);
            }
        }
    }
}
