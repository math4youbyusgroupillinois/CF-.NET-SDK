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
    public class CloudFoundryDomainPayloadConverterTests
    {
        internal string DomainsFixture = @"{{
                                                        ""total_results"": {0},
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {1}
                                                        ]
                                                    }}";

        internal string DomainFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/shared_domains/{0}"",
                ""created_at"": ""{2}"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }},
            ""entity"": {{
                ""name"": ""{1}""
            }}
        }}";

        public string CreateaDomainPayload(Domain domain)
        {
            return string.Format(DomainFixtrue, domain.Id, domain.Name, domain.CreatedDate);
        }

        public string CreateDomainsPayload(IEnumerable<Domain> domains)
        {
            var isFirst = true;
            var domainPayload = string.Empty;
            foreach (var domain in domains)
            {
                if (!isFirst)
                {
                    domainPayload += ",";
                }

                domainPayload += CreateaDomainPayload(domain);

                isFirst = false;
            }

            return string.Format(DomainsFixture, domains.Count(), domainPayload);
        }

        #region ConvertDomains Tests

        [TestMethod]
        public void CanConvertDomainsWithValidPayload()
        {
            var dom1 = new Domain("12345", "MyDomain.com", DateTime.MinValue);
            var dom2 = new Domain("54321", "DomainMy.com", DateTime.MaxValue);
            var payload = CreateDomainsPayload(new List<Domain>() { dom1, dom2 });

            var converter = new CloudFoundryDomainPayloadConverter();
            var domains = converter.ConvertDomains(payload).ToList();

            Assert.AreEqual(2, domains.Count());

            Assert.AreEqual(dom1.Id, domains[0].Id);
            Assert.AreEqual(dom1.Name, domains[0].Name);
            Assert.AreEqual(dom1.CreatedDate.ToString(), domains[0].CreatedDate.ToString());

            Assert.AreEqual(dom2.Id, domains[1].Id);
            Assert.AreEqual(dom2.Name, domains[1].Name);
            Assert.AreEqual(dom2.CreatedDate.ToString(), domains[1].CreatedDate.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainsWithInvalidDomainJson()
        {
             string domainFixtrue = @"{{
            ""metadata"": {{
                ""url"": ""/v2/shared_domains/{0}"",
                ""created_at"": ""{2}"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }},
            ""entity"": {{
                ""name"": ""{1}""
            }}
        }}";
             var domainPayload = string.Format(domainFixtrue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);
            var domainsPayload = string.Format(DomainsFixture, "1", domainPayload);

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(domainsPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpacesWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertSpacesWithEmptyPayload()
        {
            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertSpacesWithNullPayload()
        {
            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomains(null);
        }

        #endregion

        #region ConvertDomain Tests

        [TestMethod]
        public void CanConvertDomainWithValidPayload()
        {
            var expDomain = new Domain("12345", "MyDomain.com", DateTime.MinValue);
            var payload = CreateaDomainPayload(expDomain);

            var converter = new CloudFoundryDomainPayloadConverter();
            var domain = converter.ConvertDomain(payload);

            Assert.AreEqual(expDomain.Id, domain.Id);
            Assert.AreEqual(expDomain.Name, domain.Name);
            Assert.AreEqual(expDomain.CreatedDate, domain.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithMissingMetadataNode()
        {
            var domainFixtrue = @"{{
            ""entity"": {{
                ""name"": ""{1}""
            }}
        }}";
            var domainPayload = string.Format(domainFixtrue, Guid.NewGuid());

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(domainPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithMissingEntityNode()
        {
            var domainFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/shared_domains/{0}"",
                ""created_at"": ""{1}"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }}
        }}";
            var domainPayload = string.Format(domainFixtrue, Guid.NewGuid(), DateTime.MinValue);

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(domainPayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertDomainWithMissingId()
        {
            var domainFixtrue = @"{{
            ""metadata"": {{
                ""url"": ""/v2/shared_domains/{0}"",
                ""created_at"": ""{2}"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }},
            ""entity"": {{
                ""name"": ""{1}""
            }}
        }}";
            var domainPayload = string.Format(domainFixtrue, Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue);

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(domainPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithMissingName()
        {
            var domainFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/shared_domains/{0}"",
                ""created_at"": ""{1}"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }},
            ""entity"": {{
            }}
        }}";
            var domainPayload = string.Format(domainFixtrue, Guid.NewGuid(), DateTime.MinValue);

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(domainPayload);
        }

        [TestMethod]
        public void CanConvertDomainWithMissingCreateDate()
        {
            var expDomain = new Domain("12345", "MyOrg", DateTime.MaxValue);

            var domainFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/shared_domains/{0}"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }},
            ""entity"": {{
                ""name"": ""{1}""
            }}
        }}";

            var spacePayload = string.Format(domainFixtrue, expDomain.Id, expDomain.Name);

            var converter = new CloudFoundrySpacePayloadConverter();
            var domain = converter.ConvertSpace(spacePayload);

            Assert.AreEqual(expDomain.Id, domain.Id);
            Assert.AreEqual(expDomain.Name, domain.Name);
            Assert.AreEqual(DateTime.MinValue, domain.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertDomainWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertDomainWithEmptyPayload()
        {
            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertDomainWithNullPayload()
        {
            var converter = new CloudFoundryDomainPayloadConverter();
            converter.ConvertDomain(null);
        }

        #endregion
    }
}
