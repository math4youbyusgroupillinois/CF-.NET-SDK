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

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryJobPayloadConverterTests
    {
        internal string JobFixtrue = @"{{
            ""metadata"": {{
            ""guid"": ""{0}"",
            ""created_at"": ""{2}"",
            ""url"": ""/v2/jobs/{0}""
            }},
            ""entity"": {{
            ""guid"": ""{0}"",
            ""status"": ""{1}""
            }}
        }}";

        public string CreateaJobPayload(Job job)
        {
            return string.Format(JobFixtrue, job.Id, job.State, job.CreatedDate);
        }

        #region Convert Tests

        [TestMethod]
        public void CanConvertJobWithValidPayload()
        {
            var expJob = new Job("12345", "12345", JobState.Queued, DateTime.MinValue);
            var payload = CreateaJobPayload(expJob);

            var converter = new CloudFoundryJobPayloadConverter();
            var job = converter.Convert(payload);

            Assert.AreEqual(expJob.Id, job.Id);
            Assert.AreEqual(expJob.Name, job.Name);
            Assert.AreEqual(expJob.State, job.State);
            Assert.AreEqual(expJob.CreatedDate, job.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJobWithMissingMetadataNode()
        {
             var jobFixtrue = @"{{
                ""entity"": {{
                ""guid"": ""{0}"",
                ""status"": ""{1}""
                }}
            }}";

            var jobPayload = string.Format(jobFixtrue, Guid.NewGuid(), JobState.Failed);

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(jobPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithMissingEntityNode()
        {
            var jobFixtrue = @"{{
                ""metadata"": {{
                ""guid"": ""{0}"",
                ""created_at"": ""{1}"",
                ""url"": ""/v2/jobs/{0}""
                }}
            }}";

            var jobPayload = string.Format(jobFixtrue, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(jobPayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertSpaceWithMissingId()
        {
            var jobFixtrue = @"{{
                ""metadata"": {{
                ""created_at"": ""{2}"",
                ""url"": ""/v2/jobs/{0}""
                }},
                ""entity"": {{
                ""guid"": ""{0}"",
                ""status"": ""{1}""
                }}
            }}";

            var jobPayload = string.Format(jobFixtrue, Guid.NewGuid(), JobState.Failed, DateTime.Now);

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(jobPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithMissingState()
        {
            var jobFixtrue = @"{{
                ""metadata"": {{
                ""guid"": ""{0}"",
                ""created_at"": ""{1}"",
                ""url"": ""/v2/jobs/{0}""
                }},
                ""entity"": {{
                ""guid"": ""{0}""
                }}
            }}";

            var jobPayload = string.Format(jobFixtrue, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(jobPayload);
        }

        [TestMethod]
        public void CanConvertSpaceWithMissingCreateDate()
        {
            var expJob = new Job("12345", "12345", JobState.Queued, DateTime.MaxValue);
            var jobFixtrue = @"{{
                ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/jobs/{0}""
                }},
                ""entity"": {{
                ""guid"": ""{0}"",
                ""status"": ""{1}""
                }}
            }}";

            var jobPayload = string.Format(jobFixtrue, expJob.Id, expJob.State);

            var converter = new CloudFoundryJobPayloadConverter();
            var job = converter.Convert(jobPayload);

            Assert.AreEqual(expJob.Id, job.Id);
            Assert.AreEqual(expJob.Name, job.Name);
            Assert.AreEqual(expJob.State, job.State);
            Assert.AreEqual(DateTime.MinValue, job.CreatedDate );
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJobWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJobWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJobWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJobWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertJobWithEmptyPayload()
        {
            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertJobWithNullPayload()
        {
            var converter = new CloudFoundryJobPayloadConverter();
            converter.Convert(null);
        }

        #endregion
    }
}
