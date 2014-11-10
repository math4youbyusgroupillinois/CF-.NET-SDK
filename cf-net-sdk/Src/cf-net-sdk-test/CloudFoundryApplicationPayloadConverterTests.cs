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
    public class CloudFoundryApplicationPayloadConverterTests
    {
        internal string ApplicationsFixture = @"{{
                                                        ""total_results"": {0},
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {1}
                                                        ]
                                                    }}";

        internal string ApplicationFixtrue = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": {4},
        ""instances"": {5},
        ""disk_quota"": 1024,
        ""state"": ""{3}"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";

        public string CreateaApplicationPayload(Application app)
        {
            return string.Format(ApplicationFixtrue, app.Id, app.Name, app.CreatedDate, app.Status, app.MemoryLimit, app.Instances);
        }

        public string CreateApplicationsPayload(IEnumerable<Application> apps)
        {
            var isFirst = true;
            var appsPayload = string.Empty;
            foreach (var app in apps)
            {
                if (!isFirst)
                {
                    appsPayload += ",";
                }

                appsPayload += CreateaApplicationPayload(app);

                isFirst = false;
            }

            return string.Format(ApplicationsFixture, apps.Count(), appsPayload);
        }

        #region ConvertApplications Tests

        [TestMethod]
        public void CanConvertAppsWithValidPayload()
        {
            var app1 = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var app2 = new Application("54321", "App2", ApplicationStatus.Started, 1, 256, DateTime.MaxValue);
            var payload = CreateApplicationsPayload(new List<Application>() { app1, app2 });

            var converter = new CloudFoundryApplicationPayloadConverter();
            var apps = converter.ConvertApplications(payload).ToList();

            Assert.AreEqual(2, apps.Count());

            Assert.AreEqual(app1.Id, apps[0].Id);
            Assert.AreEqual(app1.Name, apps[0].Name);
            Assert.AreEqual(app1.CreatedDate, apps[0].CreatedDate);

            Assert.AreEqual(app2.Id, apps[1].Id);
            Assert.AreEqual(app2.Name, apps[1].Name);
            Assert.AreEqual(app2.CreatedDate.ToLongTimeString(), apps[1].CreatedDate.ToLongTimeString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAppsWithInvalidOrganizationJson()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);
            var appsPayload = string.Format(ApplicationsFixture, "1", appPayload);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(appsPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAppsWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAppsWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAppsWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAppsWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertAppsWithEmptyPayload()
        {
            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertAppsWithNullPayload()
        {
            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplications(null);
        }

        #endregion

        #region ConvertUser Tests

        [TestMethod]
        public void CanConvertApplicationWithValidPayload()
        {
            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var payload = CreateaApplicationPayload(expApp);

            var converter = new CloudFoundryApplicationPayloadConverter();
            var app = converter.ConvertApplication(payload);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithMissingMetadataNode()
        {
            var appFixture = @"{{
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithMissingEntityNode()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{1}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertApplicationWithMissingId()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithMissingName()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{1}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithMissingState()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithMissingMemoryLimit()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithEmptyState()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": """",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        public void CanConvertApplicationWithUnknownState()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""BADORUNKNOWNSTATE"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            var app = converter.ConvertApplication(appPayload);
            Assert.AreEqual(ApplicationStatus.Unknown, app.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithMissingInstanceCount()
        {
            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""{2}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(appPayload);
        }

        [TestMethod]
        public void CanConvertApplicationWithMissingCreateDate()
        {
            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);

            var appFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""updated_at"": ""2014-07-10T00:29:03+00:00""
      }},
      ""entity"": {{
        ""name"": ""{1}"",
        ""production"": false,
        ""space_guid"": ""b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_guid"": ""4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""buildpack"": null,
        ""detected_buildpack"": null,
        ""environment_json"": null,
        ""memory"": 1024,
        ""instances"": 1,
        ""disk_quota"": 1024,
        ""state"": ""STOPPED"",
        ""version"": ""b3d16354-e6ec-4e2f-9421-457995dbbc38"",
        ""command"": null,
        ""console"": false,
        ""debug"": null,
        ""staging_task_id"": null,
        ""package_state"": ""STAGED"",
        ""health_check_timeout"": null,
        ""staging_failed_reason"": null,
        ""space_url"": ""/v2/spaces/b130f0b7-9d4c-4d39-b42a-75072cd24dd0"",
        ""stack_url"": ""/v2/stacks/4347e2ff-d6b7-424e-9e9e-b65af6d7d715"",
        ""events_url"": ""/v2/apps/{0}/events"",
        ""service_bindings_url"": ""/v2/apps/{0}/service_bindings"",
        ""routes_url"": ""/v2/apps/{0}/routes""
      }}
    }}";
            var appPayload = string.Format(appFixture, expApp.Id, expApp.Name);

            var converter = new CloudFoundryApplicationPayloadConverter();
            var app = converter.ConvertApplication(appPayload);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, DateTime.MinValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertApplicationWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertApplicationWithEmptyPayload()
        {
            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertApplicationWithNullPayload()
        {
            var converter = new CloudFoundryApplicationPayloadConverter();
            converter.ConvertApplication(null);
        }

        #endregion
    }
}
