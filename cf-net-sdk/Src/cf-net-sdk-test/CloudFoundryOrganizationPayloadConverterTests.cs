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
    public class CloudFoundryOrganizationPayloadConverterTests
    {
        internal string OrganizationsFixture = @"{{
                                                        ""total_results"": {0},
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {1}
                                                        ]
                                                    }}";

        internal string OrganizationFixtrue = @"{{
                                                    ""metadata"": {{
                                                        ""guid"": ""{0}"",
                                                        ""url"": ""/v2/organizations/{0}"",
                                                        ""created_at"": ""{4}"",
                                                        ""updated_at"": null
                                                    }},
                                                    ""entity"": {{
                                                        ""name"": ""{1}"",
                                                        ""billing_enabled"": false,
                                                        ""quota_definition_guid"": ""{2}"",
                                                        ""status"": ""{3}"",
                                                        ""is_default"": false,
                                                        ""quota_definition_url"": ""/v2/quota_definitions/{2}"",
                                                        ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                                        ""domains_url"": ""/v2/organizations/{0}/domains"",
                                                        ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                                        ""users_url"": ""/v2/organizations/{0}/users"",
                                                        ""managers_url"": ""/v2/organizations/{0}/managers"",
                                                        ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                                        ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                                        ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                                    }}
                                                }}";

        public string CreateaOrganizationPayload(Organization org)
        {
            return string.Format(OrganizationFixtrue, org.Id, org.Name, Guid.NewGuid(), org.Status, org.CreatedDate);
        }

        public string CreateOrganizationsPayload(IEnumerable<Organization> orgs)
        {
            var isFirst = true;
            var orgsPayload = string.Empty;
            foreach (var org in orgs)
            {
                if (!isFirst)
                {
                    orgsPayload += ",";
                }

                orgsPayload += CreateaOrganizationPayload(org);

                isFirst = false;
            }

            return string.Format(OrganizationsFixture, orgs.Count(), orgsPayload);
        }

        #region ConvertOrganizations Tests

        [TestMethod]
        public void CanConvertOrganizationsWithValidPayload()
        {
            var org1 = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);
            var org2 = new Organization("54321", "OrgMy", "Inactive", DateTime.MaxValue);
            var payload = CreateOrganizationsPayload(new List<Organization>() {org1, org2});

            var converter = new CloudFoundryOrganizationPayloadConverter();
            var orgs = converter.ConvertOrganizations(payload).ToList();

            Assert.AreEqual(2, orgs.Count());

            Assert.AreEqual(org1.Id, orgs[0].Id);
            Assert.AreEqual(org1.Name, orgs[0].Name);
            Assert.AreEqual(org1.Status, orgs[0].Status);
            Assert.AreEqual(org1.CreatedDate, orgs[0].CreatedDate);

            Assert.AreEqual(org2.Id, orgs[1].Id);
            Assert.AreEqual(org2.Name, orgs[1].Name);
            Assert.AreEqual(org2.Status, orgs[1].Status);
            Assert.AreEqual(org2.CreatedDate.ToLongTimeString(), orgs[1].CreatedDate.ToLongTimeString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithInvalidOrganizationJson()
        {
            var orgFixtrue = @"{{
                                                    ""metadata"": {{
                                                        ""url"": ""/v2/organizations/{0}"",
                                                        ""created_at"": ""2014-07-01T19:15:01+00:00"",
                                                        ""updated_at"": null
                                                    }},
                                                    ""entity"": {{
                                                        ""name"": ""{1}"",
                                                        ""billing_enabled"": false,
                                                        ""quota_definition_guid"": ""{2}"",
                                                        ""status"": ""{3}"",
                                                        ""is_default"": false,
                                                        ""quota_definition_url"": ""/v2/quota_definitions/{2}"",
                                                        ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                                        ""domains_url"": ""/v2/organizations/{0}/domains"",
                                                        ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                                        ""users_url"": ""/v2/organizations/{0}/users"",
                                                        ""managers_url"": ""/v2/organizations/{0}/managers"",
                                                        ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                                        ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                                        ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                                    }}
                                                }}";
            var orgPayload = string.Format(orgFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "active");
            var orgsPayload = string.Format(OrganizationsFixture, "1", orgPayload);

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(orgsPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertOrganizationsWithEmptyPayload()
        {
            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertOrganizationsWithNullPayload()
        {
            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(null);
        }

        #endregion

        #region ConvertOrganization Tests

        [TestMethod]
        public void CanConvertOrganizationWithValidPayload()
        {
            var expOrg = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);
            var payload = CreateaOrganizationPayload(expOrg);

            var converter = new CloudFoundryOrganizationPayloadConverter();
            var org = converter.ConvertOrganization(payload);

            Assert.AreEqual(expOrg.Id, org.Id);
            Assert.AreEqual(expOrg.Name, org.Name);
            Assert.AreEqual(expOrg.Status, org.Status);
            Assert.AreEqual(expOrg.CreatedDate, org.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithMissingMetadataNode()
        {
            var orgFixtrue = @"{{
                                ""entity"": {{
                                    ""name"": ""{1}"",
                                    ""billing_enabled"": false,
                                    ""quota_definition_guid"": ""{2}"",
                                    ""status"": ""{3}"",
                                    ""is_default"": false,
                                    ""quota_definition_url"": ""/v2/quota_definitions/{2}"",
                                    ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                    ""domains_url"": ""/v2/organizations/{0}/domains"",
                                    ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                    ""users_url"": ""/v2/organizations/{0}/users"",
                                    ""managers_url"": ""/v2/organizations/{0}/managers"",
                                    ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                    ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                    ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                }}
                            }}";
            var orgPayload = string.Format(orgFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "active");

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(orgPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithMissingEntityNode()
        {
            var orgFixtrue = @"{{
                                                    ""metadata"": {{
                                                        ""guid"": ""{0}"",
                                                        ""url"": ""/v2/organizations/{0}"",
                                                        ""created_at"": ""2014-07-01T19:15:01+00:00"",
                                                        ""updated_at"": null
                                                    }}
                                                }}";
            var orgPayload = string.Format(orgFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "active");

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(orgPayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertOrganizationsWithMissingId()
        {
            var orgFixtrue = @"{{
                                ""metadata"": {{
                                    ""url"": ""/v2/organizations/{0}"",
                                    ""created_at"": ""{4}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""name"": ""{1}"",
                                    ""billing_enabled"": false,
                                    ""quota_definition_guid"": ""{2}"",
                                    ""status"": ""{3}"",
                                    ""is_default"": false,
                                    ""quota_definition_url"": ""/v2/quota_definitions/{2}"",
                                    ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                    ""domains_url"": ""/v2/organizations/{0}/domains"",
                                    ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                    ""users_url"": ""/v2/organizations/{0}/users"",
                                    ""managers_url"": ""/v2/organizations/{0}/managers"",
                                    ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                    ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                    ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                }}
                            }}";
            var orgPayload = string.Format(orgFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "active", DateTime.Now);

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(orgPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithMissingName()
        {
            var orgFixtrue = @"{{
                                ""metadata"": {{
                                    ""guid"": ""{0}"",
                                    ""url"": ""/v2/organizations/{0}"",
                                    ""created_at"": ""{3}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""billing_enabled"": false,
                                    ""quota_definition_guid"": ""{1}"",
                                    ""status"": ""{2}"",
                                    ""is_default"": false,
                                    ""quota_definition_url"": ""/v2/quota_definitions/{1}"",
                                    ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                    ""domains_url"": ""/v2/organizations/{0}/domains"",
                                    ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                    ""users_url"": ""/v2/organizations/{0}/users"",
                                    ""managers_url"": ""/v2/organizations/{0}/managers"",
                                    ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                    ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                    ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                }}
                            }}";
            var orgPayload = string.Format(orgFixtrue, Guid.NewGuid(), Guid.NewGuid(), "active", DateTime.Now);

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(orgPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationsWithMissingStatus()
        {
            var orgFixtrue = @"{{
                                ""metadata"": {{
                                    ""guid"": ""{0}"",
                                    ""url"": ""/v2/organizations/{0}"",
                                    ""created_at"": ""{3}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""name"": ""{1}"",
                                    ""billing_enabled"": false,
                                    ""quota_definition_guid"": ""{2}"",
                                    ""is_default"": false,
                                    ""quota_definition_url"": ""/v2/quota_definitions/{2}"",
                                    ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                    ""domains_url"": ""/v2/organizations/{0}/domains"",
                                    ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                    ""users_url"": ""/v2/organizations/{0}/users"",
                                    ""managers_url"": ""/v2/organizations/{0}/managers"",
                                    ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                    ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                    ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                }}
                            }}";
            var orgPayload = string.Format(orgFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(orgPayload);
        }

        [TestMethod]
        public void CanConvertOrganizationsWithMissingCreateDate()
        {
            var expOrg = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);

            var orgFixtrue = @"{{
                                ""metadata"": {{
                                    ""guid"": ""{0}"",
                                    ""url"": ""/v2/organizations/{0}"",
                                    ""updated_at"": null
                                }},
                                ""entity"": {{
                                    ""name"": ""{1}"",
                                    ""billing_enabled"": false,
                                    ""quota_definition_guid"": ""{2}"",
                                    ""status"": ""{3}"",
                                    ""is_default"": false,
                                    ""quota_definition_url"": ""/v2/quota_definitions/{2}"",
                                    ""spaces_url"": ""/v2/organizations/{0}/spaces"",
                                    ""domains_url"": ""/v2/organizations/{0}/domains"",
                                    ""private_domains_url"": ""/v2/organizations/{0}/private_domains"",
                                    ""users_url"": ""/v2/organizations/{0}/users"",
                                    ""managers_url"": ""/v2/organizations/{0}/managers"",
                                    ""billing_managers_url"": ""/v2/organizations/{0}/billing_managers"",
                                    ""auditors_url"": ""/v2/organizations/{0}/auditors"",
                                    ""app_events_url"": ""/v2/organizations/{0}/app_events""
                                }}
                            }}";
            var orgPayload = string.Format(orgFixtrue, expOrg.Id, expOrg.Name, Guid.NewGuid(), expOrg.Status);

            var converter = new CloudFoundryOrganizationPayloadConverter();
            var org = converter.ConvertOrganization(orgPayload);

            Assert.AreEqual(expOrg.Id, org.Id);
            Assert.AreEqual(expOrg.Name, org.Name);
            Assert.AreEqual(expOrg.Status, org.Status);
            Assert.AreEqual(expOrg.CreatedDate, DateTime.MinValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertOrganizationWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertOrganizationWithEmptyPayload()
        {
            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertOrganizationWithNullPayload()
        {
            var converter = new CloudFoundryOrganizationPayloadConverter();
            converter.ConvertOrganizations(null);
        }

        #endregion
    }
}
