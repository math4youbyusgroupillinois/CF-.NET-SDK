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
using cf_net_sdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryInstancePayloadConverterTests
    {
        internal string StatsFixture = @"{{ {0} }}";

        internal string StatFixture = @"
            {{
                ""state"": ""{1}"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""{0}"",
                    ""port"": {2},
                    ""uptime"": {6},
                    ""mem_quota"": {8},
                    ""disk_quota"": {9},
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""{7}"",
                        ""cpu"": {3},
                        ""mem"": {4},
                        ""disk"": {5}
                    }}
                }}
            }}";

        public string CreateStatPayload(Instance instance)
        {
            return string.Format(StatFixture, instance.Name, instance.State, instance.Port, instance.CpuUsage, instance.MemoryUsage, instance.DiskUsage, instance.UpTime.TotalSeconds, instance.CreatedDate, instance.MemoryQuota, instance.DiskQuota);
        }

        public string CreateStatsPayload(IEnumerable<Instance> instances)
        {
            var isFirst = true;
            var instancesPayload = string.Empty;
            foreach (var instance in instances)
            {
                if (!isFirst)
                {
                    instancesPayload += ",";
                }

                instancesPayload += "\""+instance.Id +"\":" +CreateStatPayload(instance);

                isFirst = false;
            }

            return string.Format(StatsFixture, instancesPayload);
        }

        #region ConvertInstances Tests

        [TestMethod]
        public void CanConvertInstancesWithValidPayload()
        {
            var int1 = new Instance("12345", "Instance1", InstanceState.Running, 555, 512, 45, 100, 128, 256, 60, DateTime.MinValue);
            var int2 = new Instance("54321", "Instance2", InstanceState.Unknown, 666, 1024, 10, 200, 256, 512, 120, DateTime.MaxValue);
            var payload = CreateStatsPayload(new List<Instance>() { int1, int2 });

            var converter = new CloudFoundryInstancePayloadConverter();
            var instances = converter.ConvertInstances(payload).ToList();

            Assert.AreEqual(2, instances.Count());

            Assert.AreEqual(int1.Id, instances[0].Id);
            Assert.AreEqual(int1.Name, instances[0].Name);
            Assert.AreEqual(int1.State, instances[0].State);
            Assert.AreEqual(int1.Port, instances[0].Port);
            Assert.AreEqual(int1.MemoryUsage, instances[0].MemoryUsage);
            Assert.AreEqual(int1.CpuUsage, instances[0].CpuUsage);
            Assert.AreEqual(int1.DiskUsage, instances[0].DiskUsage);
            Assert.AreEqual(int1.MemoryQuota, instances[0].MemoryQuota);
            Assert.AreEqual(int1.DiskQuota, instances[0].DiskQuota);
            Assert.AreEqual(int1.UpTime, instances[0].UpTime);
            Assert.AreEqual(int1.CreatedDate.ToString(), instances[0].CreatedDate.ToString());

            Assert.AreEqual(int2.Id, instances[1].Id);
            Assert.AreEqual(int2.Name, instances[1].Name);
            Assert.AreEqual(int2.State, instances[1].State);
            Assert.AreEqual(int2.Port, instances[1].Port);
            Assert.AreEqual(int2.MemoryUsage, instances[1].MemoryUsage);
            Assert.AreEqual(int2.CpuUsage, instances[1].CpuUsage);
            Assert.AreEqual(int2.DiskUsage, instances[1].DiskUsage);
            Assert.AreEqual(int2.MemoryQuota, instances[1].MemoryQuota);
            Assert.AreEqual(int2.DiskQuota, instances[1].DiskQuota);
            Assert.AreEqual(int2.UpTime, instances[1].UpTime);
            Assert.AreEqual(int2.CreatedDate.ToString(), instances[1].CreatedDate.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstancesWithInvalidInstanceJson()
        {
            var statFixture = @"
            ""0"": {{
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""port"": 543,
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""mem"": 0,
                        ""disk"": 0
                    }}
                }}
            }}
        ";
            var statsPayload = string.Format(StatFixture, statFixture);

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstances(statsPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstancesWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstances(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstancesWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstances(payloadFixture);
        }

        [TestMethod]
        public void CanConvertInstancesWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryInstancePayloadConverter();
            var instances = converter.ConvertInstances(payloadFixture);

            Assert.AreEqual(0,instances.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstancesWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstances(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertInstancesWithEmptyPayload()
        {
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstances(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertInstancesWithNullPayload()
        {
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstances(null);
        }

        #endregion

        #region ConvertInstance Tests

        [TestMethod]
        public void CanConvertInstanceWithValidPayload()
        {
            var expInst = new Instance("12345", "Instance1", InstanceState.Running, 555, 512, 45, 128, 256, 512, 60, DateTime.MinValue);
            var payload = CreateStatPayload(expInst);

            var converter = new CloudFoundryInstancePayloadConverter();
            var instance = converter.ConvertInstance(expInst.Id, payload);

            Assert.AreEqual(expInst.Id, instance.Id);
            Assert.AreEqual(expInst.Name, instance.Name);
            Assert.AreEqual(expInst.State, instance.State);
            Assert.AreEqual(expInst.Port, instance.Port);
            Assert.AreEqual(expInst.MemoryUsage, instance.MemoryUsage);
            Assert.AreEqual(expInst.CpuUsage, instance.CpuUsage);
            Assert.AreEqual(expInst.DiskUsage, instance.DiskUsage);
            Assert.AreEqual(expInst.MemoryQuota, instance.MemoryQuota);
            Assert.AreEqual(expInst.DiskQuota, instance.DiskQuota);
            Assert.AreEqual(expInst.UpTime, instance.UpTime);
            Assert.AreEqual(expInst.CreatedDate, instance.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingStatsNode()
        {
            var statFixture = @"""0"": {{ ""state"": ""RUNNING"" }} ";
           
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingUsageNode()
        {
            var statFixture = @"
           {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""port"": 543,
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384
                }}
            }}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingHostName()
        {
            var statFixture = @"
            {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""port"": 543,
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""mem"": 0,
                        ""disk"": 0
                    }}
                }}
            }}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingPortElement()
        {
            var statFixture = @"
            {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""mem"": 0,
                        ""disk"": 0
                    }}
                }}
            }}";
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingUptimeElement()
        {
            var statFixture = @"
            {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""port"": 543,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""mem"": 0,
                        ""disk"": 0
                    }}
                }}
            }}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingCpuElement()
        {
            var statFixture = @"
            {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""port"": 543,
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""mem"": 0,
                        ""disk"": 0
                    }}
                }}
            }}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingMemElement()
        {
            var statFixture = @"
            {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""port"": 543,
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""disk"": 0
                    }}
                }}
            }}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingDiskElement()
        {
            var statFixture = @"
            {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""123.123.123.123"",
                    ""port"": 543,
                    ""uptime"": 111,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""mem"": 0,
                    }}
                }}
            }}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", statFixture);
        }
        
        [TestMethod]
        public void CanConvertInstanceWithMissingCreateDate()
        {
            var expInst = new Instance("12345", "Instance1", InstanceState.Running, 555, 512, 45, 128, 256, 512, 60, DateTime.MinValue);

            var statFixture = @"
            {{
                ""state"": ""{1}"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""{0}"",
                    ""port"": {2},
                    ""uptime"": {6},
                    ""mem_quota"": {8},
                    ""disk_quota"": {9},
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""{7}"",
                        ""cpu"": {3},
                        ""mem"": {4},
                        ""disk"": {5} 
                    }}
                }}
            }}";
            var instPayload = string.Format(statFixture, expInst.Name, expInst.State, expInst.Port, expInst.CpuUsage, expInst.MemoryUsage, expInst.DiskUsage, expInst.UpTime.TotalSeconds, expInst.CreatedDate, expInst.MemoryQuota, expInst.DiskQuota);

            var converter = new CloudFoundryInstancePayloadConverter();
            var instance = converter.ConvertInstance(expInst.Id, instPayload);

            Assert.AreEqual(expInst.Id, instance.Id);
            Assert.AreEqual(expInst.Name, instance.Name);
            Assert.AreEqual(expInst.State, instance.State);
            Assert.AreEqual(expInst.Port, instance.Port);
            Assert.AreEqual(expInst.MemoryUsage, instance.MemoryUsage);
            Assert.AreEqual(expInst.CpuUsage, instance.CpuUsage);
            Assert.AreEqual(expInst.DiskUsage, instance.DiskUsage);
            Assert.AreEqual(expInst.MemoryQuota, instance.MemoryQuota);
            Assert.AreEqual(expInst.DiskQuota, instance.DiskQuota);
            Assert.AreEqual(expInst.UpTime, instance.UpTime);
            Assert.AreEqual(expInst.CreatedDate, instance.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertInstanceWithEmptyPayload()
        {
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertInstanceWithNullPayload()
        {
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance("0", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertInstanceWithEmptyId()
        {
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance(string.Empty, "{}");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertInstanceWithNullId()
        {
            var converter = new CloudFoundryInstancePayloadConverter();
            converter.ConvertInstance(null, "{}");
        }

        #endregion
    }
}
