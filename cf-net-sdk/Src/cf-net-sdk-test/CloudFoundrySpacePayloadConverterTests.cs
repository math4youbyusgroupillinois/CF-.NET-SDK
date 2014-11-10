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
    public class CloudFoundrySpacePayloadConverterTests
    {
        internal string SpacesFixture = @"{{
                                                        ""total_results"": {0},
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {1}
                                                        ]
                                                    }}";

        internal string SpaceFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/spaces/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""name"": ""{1}"",
                ""organization_guid"": ""{2}"",
                ""is_default"": false,
                ""organization_url"": ""/v2/organizations/{2}"",
                ""developers_url"": ""/v2/spaces/{0}/developers"",
                ""managers_url"": ""/v2/spaces/{0}/managers"",
                ""auditors_url"": ""/v2/spaces/{0}/auditors"",
                ""apps_url"": ""/v2/spaces/{0}/apps"",
                ""domains_url"": ""/v2/spaces/{0}/domains"",
                ""service_instances_url"": ""/v2/spaces/{0}/service_instances"",
                ""app_events_url"": ""/v2/spaces/{0}/app_events"",
                ""events_url"": ""/v2/spaces/{0}/events""
            }}
        }}";

        public string CreateaSpacePayload(Space space)
        {
            return string.Format(SpaceFixtrue, space.Id, space.Name, Guid.NewGuid(), space.CreatedDate);
        }

        public string CreateSpacesPayload(IEnumerable<Space> spaces)
        {
            var isFirst = true;
            var spacesPayload = string.Empty;
            foreach (var space in spaces)
            {
                if (!isFirst)
                {
                    spacesPayload += ",";
                }

                spacesPayload += CreateaSpacePayload(space);

                isFirst = false;
            }

            return string.Format(SpacesFixture, spaces.Count(), spacesPayload);
        }

        #region ConvertSpaces Tests

        [TestMethod]
        public void CanConvertSpacesWithValidPayload()
        {
            var spc1 = new Space("12345", "MySpace", DateTime.MinValue);
            var spc2 = new Space("54321", "SpaceMy", DateTime.MaxValue);
            var payload = CreateSpacesPayload(new List<Space>() { spc1, spc2 });

            var converter = new CloudFoundrySpacePayloadConverter();
            var spaces = converter.ConvertSpaces(payload).ToList();

            Assert.AreEqual(2, spaces.Count());

            Assert.AreEqual(spc1.Id, spaces[0].Id);
            Assert.AreEqual(spc1.Name, spaces[0].Name);
            Assert.AreEqual(spc1.CreatedDate, spaces[0].CreatedDate);

            Assert.AreEqual(spc2.Id, spaces[1].Id);
            Assert.AreEqual(spc2.Name, spaces[1].Name);
            Assert.AreEqual(spc2.CreatedDate.ToLongTimeString(), spaces[1].CreatedDate.ToLongTimeString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithInvalidOrganizationJson()
        {
            var spaceFixtrue = @"{{
                                    ""metadata"": {{
                                        ""url"": ""/v2/spaces/{0}"",
                                        ""created_at"": ""{3}"",
                                        ""updated_at"": null
                                    }},
                                    ""entity"": {{
                                        ""name"": ""{1}"",
                                        ""organization_guid"": ""{2}"",
                                        ""is_default"": false,
                                        ""organization_url"": ""/v2/organizations/{2}"",
                                        ""developers_url"": ""/v2/spaces/{0}/developers"",
                                        ""managers_url"": ""/v2/spaces/{0}/managers"",
                                        ""auditors_url"": ""/v2/spaces/{0}/auditors"",
                                        ""apps_url"": ""/v2/spaces/{0}/apps"",
                                        ""domains_url"": ""/v2/spaces/{0}/domains"",
                                        ""service_instances_url"": ""/v2/spaces/{0}/service_instances"",
                                        ""app_events_url"": ""/v2/spaces/{0}/app_events"",
                                        ""events_url"": ""/v2/spaces/{0}/events""
                                    }}
                                }}";
            var spacePayload = string.Format(spaceFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);
            var spacesPayload = string.Format(SpacesFixture, "1", spacePayload);

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(spacesPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertSpacesWithEmptyPayload()
        {
            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertSpacesWithNullPayload()
        {
            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpaces(null);
        }

        #endregion

        #region ConvertSpace Tests

        [TestMethod]
        public void CanConvertSpaceWithValidPayload()
        {
            var expSpace = new Space("12345", "MySpace", DateTime.MinValue);
            var payload = CreateaSpacePayload(expSpace);

            var converter = new CloudFoundrySpacePayloadConverter();
            var space = converter.ConvertSpace(payload);

            Assert.AreEqual(expSpace.Id, space.Id);
            Assert.AreEqual(expSpace.Name, space.Name);
            Assert.AreEqual(expSpace.CreatedDate, space.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithMissingMetadataNode()
        {
            var spaceFixtrue = @"
            ""entity"": {{
                ""name"": ""{1}"",
                ""organization_guid"": ""{2}"",
                ""is_default"": false,
                ""organization_url"": ""/v2/organizations/{2}"",
                ""developers_url"": ""/v2/spaces/{0}/developers"",
                ""managers_url"": ""/v2/spaces/{0}/managers"",
                ""auditors_url"": ""/v2/spaces/{0}/auditors"",
                ""apps_url"": ""/v2/spaces/{0}/apps"",
                ""domains_url"": ""/v2/spaces/{0}/domains"",
                ""service_instances_url"": ""/v2/spaces/{0}/service_instances"",
                ""app_events_url"": ""/v2/spaces/{0}/app_events"",
                ""events_url"": ""/v2/spaces/{0}/events""
            }}
        }}";
            var spacePayload = string.Format(spaceFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(spacePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithMissingEntityNode()
        {
            var spaceFixtrue = @"{{
                                ""metadata"": {{
                                    ""guid"": ""{0}"",
                                    ""url"": ""/v2/spaces/{0}"",
                                    ""created_at"": ""{1}"",
                                    ""updated_at"": null
                                }}
                            }}";
            var spacePayload = string.Format(spaceFixtrue, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(spacePayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertSpaceWithMissingId()
        {
            var spaceFixtrue = @"{{
                                ""metadata"": {{
                                    ""url"": ""/v2/spaces/{0}"",
                                    ""created_at"": ""{3}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""name"": ""{1}"",
                                    ""organization_guid"": ""{2}"",
                                    ""is_default"": false,
                                    ""organization_url"": ""/v2/organizations/{2}"",
                                    ""developers_url"": ""/v2/spaces/{0}/developers"",
                                    ""managers_url"": ""/v2/spaces/{0}/managers"",
                                    ""auditors_url"": ""/v2/spaces/{0}/auditors"",
                                    ""apps_url"": ""/v2/spaces/{0}/apps"",
                                    ""domains_url"": ""/v2/spaces/{0}/domains"",
                                    ""service_instances_url"": ""/v2/spaces/{0}/service_instances"",
                                    ""app_events_url"": ""/v2/spaces/{0}/app_events"",
                                    ""events_url"": ""/v2/spaces/{0}/events""
                                }}
                            }}";
            var spacePayload = string.Format(spaceFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(spacePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithMissingName()
        {
            var spaceFixture = @"{{
                                ""metadata"": {{
                                    ""guid"": ""{0}"",
                                    ""url"": ""/v2/spaces/{0}"",
                                    ""created_at"": ""{2}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""organization_guid"": ""{1}"",
                                    ""is_default"": false,
                                    ""organization_url"": ""/v2/organizations/{1}"",
                                    ""developers_url"": ""/v2/spaces/{0}/developers"",
                                    ""managers_url"": ""/v2/spaces/{0}/managers"",
                                    ""auditors_url"": ""/v2/spaces/{0}/auditors"",
                                    ""apps_url"": ""/v2/spaces/{0}/apps"",
                                    ""domains_url"": ""/v2/spaces/{0}/domains"",
                                    ""service_instances_url"": ""/v2/spaces/{0}/service_instances"",
                                    ""app_events_url"": ""/v2/spaces/{0}/app_events"",
                                    ""events_url"": ""/v2/spaces/{0}/events""
                                }}
                            }}";
            var spacePayload = string.Format(spaceFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(spacePayload);
        }

        [TestMethod]
        public void CanConvertSpaceWithMissingCreateDate()
        {
            var expSpace = new Space("12345", "MyOrg", DateTime.MinValue);

            var spaceFixtrue = @"{{
                                ""metadata"": {{
                                    ""guid"": ""{0}"",
                                    ""url"": ""/v2/spaces/{0}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""name"": ""{1}"",
                                    ""organization_guid"": ""{2}"",
                                    ""is_default"": false,
                                    ""organization_url"": ""/v2/organizations/{2}"",
                                    ""developers_url"": ""/v2/spaces/{0}/developers"",
                                    ""managers_url"": ""/v2/spaces/{0}/managers"",
                                    ""auditors_url"": ""/v2/spaces/{0}/auditors"",
                                    ""apps_url"": ""/v2/spaces/{0}/apps"",
                                    ""domains_url"": ""/v2/spaces/{0}/domains"",
                                    ""service_instances_url"": ""/v2/spaces/{0}/service_instances"",
                                    ""app_events_url"": ""/v2/spaces/{0}/app_events"",
                                    ""events_url"": ""/v2/spaces/{0}/events""
                                }}
                            }}";
            var spacePayload = string.Format(spaceFixtrue, expSpace.Id, expSpace.Name, Guid.NewGuid());

            var converter = new CloudFoundrySpacePayloadConverter();
            var space = converter.ConvertSpace(spacePayload);

            Assert.AreEqual(expSpace.Id, space.Id);
            Assert.AreEqual(expSpace.Name, space.Name);
            Assert.AreEqual(expSpace.CreatedDate, DateTime.MinValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertSpaceWithEmptyPayload()
        {
            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertSpaceWithNullPayload()
        {
            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(null);
        }

        #endregion
    }
}
