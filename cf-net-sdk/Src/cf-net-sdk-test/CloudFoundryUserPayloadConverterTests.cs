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
    public class CloudFoundryUserPayloadConverterTests
    {
        internal string UsersFixture = @"{{
                                                        ""total_results"": {0},
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {1}
                                                        ]
                                                    }}";

        internal string UserFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/users/{0}"",
                ""created_at"": ""{2}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""admin"": true,
                ""active"": true,
                ""default_space_guid"": null,
                ""guid"": ""{0}"",
                ""username"": ""{1}"",
                ""spaces_url"": ""/v2/users/{0}/spaces"",
                ""organizations_url"": ""/v2/users/{0}/organizations"",
                ""managed_organizations_url"": ""/v2/users/{0}/managed_organizations"",
                ""billing_managed_organizations_url"": ""/v2/users/{0}/billing_managed_organizations"",
                ""audited_organizations_url"": ""/v2/users/{0}/audited_organizations"",
                ""managed_spaces_url"": ""/v2/users/{0}/managed_spaces"",
                ""audited_spaces_url"": ""/v2/users/{0}/audited_spaces""
            }}
        }}";

        public string CreateaUserPayload(User user)
        {
            return string.Format(UserFixtrue, user.Id, user.Name, user.CreatedDate);
        }

        public string CreateUsersPayload(IEnumerable<User> users)
        {
            var isFirst = true;
            var usersPayload = string.Empty;
            foreach (var user in users)
            {
                if (!isFirst)
                {
                    usersPayload += ",";
                }

                usersPayload += CreateaUserPayload(user);

                isFirst = false;
            }

            return string.Format(UsersFixture, users.Count(), usersPayload);
        }

        #region ConvertUsers Tests

        [TestMethod]
        public void CanConvertUsersWithValidPayload()
        {
            var usr1 = new User("12345", "User1", DateTime.MinValue);
            var usr2 = new User("54321", "USer2", DateTime.MaxValue);
            var payload = CreateUsersPayload(new List<User>() { usr1, usr2 });

            var converter = new CloudFoundryUserPayloadConverter();
            var users = converter.ConvertUsers(payload).ToList();

            Assert.AreEqual(2, users.Count());

            Assert.AreEqual(usr1.Id, users[0].Id);
            Assert.AreEqual(usr1.Name, users[0].Name);
            Assert.AreEqual(usr1.CreatedDate, users[0].CreatedDate);

            Assert.AreEqual(usr2.Id, users[1].Id);
            Assert.AreEqual(usr2.Name, users[1].Name);
            Assert.AreEqual(usr2.CreatedDate.ToLongTimeString(), users[1].CreatedDate.ToLongTimeString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUsersWithInvalidOrganizationJson()
        {
            var userFixture = @"{{
            ""metadata"": {{
                ""url"": ""/v2/users/{0}"",
                ""created_at"": ""{2}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""admin"": true,
                ""active"": true,
                ""default_space_guid"": null,
                ""guid"": ""{0}"",
                ""username"": ""{1}"",
                ""spaces_url"": ""/v2/users/{0}/spaces"",
                ""organizations_url"": ""/v2/users/{0}/organizations"",
                ""managed_organizations_url"": ""/v2/users/{0}/managed_organizations"",
                ""billing_managed_organizations_url"": ""/v2/users/{0}/billing_managed_organizations"",
                ""audited_organizations_url"": ""/v2/users/{0}/audited_organizations"",
                ""managed_spaces_url"": ""/v2/users/{0}/managed_spaces"",
                ""audited_spaces_url"": ""/v2/users/{0}/audited_spaces""
            }}
        }}";
            var userPayload = string.Format(userFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);
            var usersPayload = string.Format(UsersFixture, "1", userPayload);

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(usersPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUsersWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUsersWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUsersWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUsersWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertUsersWithEmptyPayload()
        {
            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertUsersWithNullPayload()
        {
            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUsers(null);
        }

        #endregion

        #region ConvertUser Tests

        [TestMethod]
        public void CanConvertUserWithValidPayload()
        {
            var expUser = new User("12345", "USer1", DateTime.MinValue);
            var payload = CreateaUserPayload(expUser);

            var converter = new CloudFoundryUserPayloadConverter();
            var user = converter.ConvertUser(payload);

            Assert.AreEqual(expUser.Id, user.Id);
            Assert.AreEqual(expUser.Name, user.Name);
            Assert.AreEqual(expUser.CreatedDate, user.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithMissingMetadataNode()
        {
            var userFixture = @"{{
            ""entity"": {{
                ""admin"": true,
                ""active"": true,
                ""default_space_guid"": null,
                ""guid"": ""{0}"",
                ""username"": ""{1}"",
                ""spaces_url"": ""/v2/users/{0}/spaces"",
                ""organizations_url"": ""/v2/users/{0}/organizations"",
                ""managed_organizations_url"": ""/v2/users/{0}/managed_organizations"",
                ""billing_managed_organizations_url"": ""/v2/users/{0}/billing_managed_organizations"",
                ""audited_organizations_url"": ""/v2/users/{0}/audited_organizations"",
                ""managed_spaces_url"": ""/v2/users/{0}/managed_spaces"",
                ""audited_spaces_url"": ""/v2/users/{0}/audited_spaces""
            }}
        }}";
            var userPayload = string.Format(userFixture, Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(userPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithMissingEntityNode()
        {
            var userFixture = @"{{
                ""metadata"": {{
                    ""guid"": ""{0}"",
                    ""url"": ""/v2/users/{0}"",
                    ""created_at"": ""{1}"",
                    ""updated_at"": null
                }}
            }}";
            var spacePayload = string.Format(userFixture, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundrySpacePayloadConverter();
            converter.ConvertSpace(spacePayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertUserWithMissingId()
        {
            var userFixture = @"{{
            ""metadata"": {{
                ""url"": ""/v2/users/{0}"",
                ""created_at"": ""{2}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""admin"": true,
                ""active"": true,
                ""default_space_guid"": null,
                ""username"": ""{1}"",
                ""spaces_url"": ""/v2/users/{0}/spaces"",
                ""organizations_url"": ""/v2/users/{0}/organizations"",
                ""managed_organizations_url"": ""/v2/users/{0}/managed_organizations"",
                ""billing_managed_organizations_url"": ""/v2/users/{0}/billing_managed_organizations"",
                ""audited_organizations_url"": ""/v2/users/{0}/audited_organizations"",
                ""managed_spaces_url"": ""/v2/users/{0}/managed_spaces"",
                ""audited_spaces_url"": ""/v2/users/{0}/audited_spaces""
            }}
        }}";
            var userPayload = string.Format(userFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(userPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithMissingName()
        {
            var userFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/users/{0}"",
                ""created_at"": ""{1}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""admin"": true,
                ""active"": true,
                ""default_space_guid"": null,
                ""guid"": ""{0}"",
                ""spaces_url"": ""/v2/users/{0}/spaces"",
                ""organizations_url"": ""/v2/users/{0}/organizations"",
                ""managed_organizations_url"": ""/v2/users/{0}/managed_organizations"",
                ""billing_managed_organizations_url"": ""/v2/users/{0}/billing_managed_organizations"",
                ""audited_organizations_url"": ""/v2/users/{0}/audited_organizations"",
                ""managed_spaces_url"": ""/v2/users/{0}/managed_spaces"",
                ""audited_spaces_url"": ""/v2/users/{0}/audited_spaces""
            }}
        }}";
            var userPayload = string.Format(userFixture, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(userPayload);
        }

        [TestMethod]
        public void CanConvertUserWithMissingCreateDate()
        {
            var expUser = new User("12345", "User1", DateTime.MinValue);

            var userFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/users/{0}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""admin"": true,
                ""active"": true,
                ""default_space_guid"": null,
                ""guid"": ""{0}"",
                ""username"": ""{1}"",
                ""spaces_url"": ""/v2/users/{0}/spaces"",
                ""organizations_url"": ""/v2/users/{0}/organizations"",
                ""managed_organizations_url"": ""/v2/users/{0}/managed_organizations"",
                ""billing_managed_organizations_url"": ""/v2/users/{0}/billing_managed_organizations"",
                ""audited_organizations_url"": ""/v2/users/{0}/audited_organizations"",
                ""managed_spaces_url"": ""/v2/users/{0}/managed_spaces"",
                ""audited_spaces_url"": ""/v2/users/{0}/audited_spaces""
            }}
        }}";
            var userPayload = string.Format(userFixture, expUser.Id, expUser.Name);

            var converter = new CloudFoundryUserPayloadConverter();
            var user = converter.ConvertUser(userPayload);

            Assert.AreEqual(expUser.Id, user.Id);
            Assert.AreEqual(expUser.Name, user.Name);
            Assert.AreEqual(expUser.CreatedDate, DateTime.MinValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertUserWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertUserWithEmptyPayload()
        {
            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertUserWithNullPayload()
        {
            var converter = new CloudFoundryUserPayloadConverter();
            converter.ConvertUser(null);
        }

        #endregion
    }
}
