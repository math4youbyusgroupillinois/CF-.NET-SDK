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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using cf_net_sdk;
using CloudFoundry.Common;
using CloudFoundry.Common.Http;
using CloudFoundry.Test;
using Newtonsoft.Json.Linq;

namespace cf_net_sdk_test
{
    public class TestInstanceInfo
    {
        public string Name { get; set; }
        
        public string Build { get; set; }
        
        public string Version { get; set; }

        public string AuthEndpoint { get; set; }

        public string TokenEndpoint { get; set; }
    }

    public class TestRouteInfo
    {
        public TestRouteInfo(string host, string domainId, string spaceId)
        {
            this.HostName = host;
            this.DomainId = domainId;
            this.SpaceId = spaceId;
        }

        public string HostName { get; set; }

        public string DomainId { get; set; }

        public string SpaceId { get; set; }
    }

    public class CloudFoundryRestSimulator : DisposableClass, IHttpAbstractionClient
    {
        #region response fixtures

        internal string AccessDeniedErrMessage = @"{
                                        ""error"": ""access_denied"",
                                        ""error_description"": ""You are not allowed to access this resource.""
                                      }";

        internal string OrgNotFoundErrMessage = @"{
                                                    ""code"": 30003,
                                                    ""description"": ""The organization could not be found: bda18bed-bb6e-4c7b-a5aa-3b16fe661344"",
                                                    ""error_code"": ""CF-OrganizationNotFound"",
                                                    ""types"": [
                                                        ""OrganizationNotFound"",
                                                        ""Error""
                                                    ]
                                                }";

        internal string AuthResponseFixture = @"{{
                                        ""access_token"": ""{0}"",
                                        ""refresh_token"": ""R-gfBVF-6zb5WUU8RXnQyPctzyCTfFTGhqfts69F84JaVYSdj8o4e6LDW4V8DLkf-EONjzVoDzUafgnUdnXxlQ"",
                                        ""token_type"": ""bearer"",
                                        ""expires_in"": 86399,
                                        ""scope"": ""openid password.write cloud_controller.admin cloud_controller.read cloud_controller.write scim.read scim.write""
                                    }}";

        internal string MetadataFixture = @"{{
                                                        ""total_results"": 1,
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {0}
                                                        ]
                                                    }}";

        internal string OrganizationFixtrue = @"{{
                                                    ""metadata"": {{
                                                        ""guid"": ""{0}"",
                                                        ""url"": ""/v2/organizations/{0}"",
                                                        ""created_at"": ""2014-07-01T19:15:01+00:00"",
                                                        ""updated_at"": null
                                                    }},
                                                    ""entity"": {{
                                                        ""name"": ""{1}"",
                                                        ""billing_enabled"": false,
                                                        ""quota_definition_guid"": ""{2}"",
                                                        ""status"": ""active"",
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

        internal string SpaceFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/spaces/{0}"",
                ""created_at"": ""2014-07-01T19:15:01+00:00"",
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

        internal string InfoFixture = @"{{
            ""name"": ""{0}"",
            ""build"": ""{1}"",
            ""support"": ""stackato-support@activestate.com"",
            ""version"": {2},
            ""description"": ""ActiveState Stackato"",
            ""authorization_endpoint"": ""{3}"",
            ""token_endpoint"": ""{4}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";

        internal string UserFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/users/{0}"",
                ""created_at"": ""2014-07-01T19:15:01+00:00"",
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

        internal string AppFixture = @"{{
      ""metadata"": {{
        ""guid"": ""{0}"",
        ""url"": ""/v2/apps/{0}"",
        ""created_at"": ""2014-07-10T00:29:03+00:00"",
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

        internal string RouteFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""2014-07-10T00:29:03+00:00"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""{2}"",
                ""space_guid"": ""{3}"",
                ""domain_url"": ""/v2/domains/{2}"",
                ""space_url"": ""/v2/spaces/{3}"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";

        internal string StatsFixture = @"
            ""{0}"": {{
                ""state"": ""RUNNING"",
                ""stats"": {{
                    ""name"": ""wordpress-example"",
                    ""uris"": [
                        ""wordpress-example.15.125.123.164.xip.io"",
                        ""wordpress-rocks.15.125.123.164.xip.io""
                    ],
                    ""host"": ""{1}"",
                    ""port"": 53466,
                    ""uptime"": 897,
                    ""mem_quota"": 268435456,
                    ""disk_quota"": 2147483648,
                    ""fds_quota"": 16384,
                    ""usage"": {{
                        ""time"": ""2014-07-15 23:27:36 +0000"",
                        ""cpu"": 0,
                        ""mem"": 26734592,
                        ""disk"": 46776320
                    }}
                }}
            }}
        ";

        internal string DomainFixtrue = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/shared_domains/{0}"",
                ""created_at"": ""2014-07-09T17:41:14+00:00"",
                ""updated_at"": ""2014-07-28T17:47:53+00:00""
            }},
            ""entity"": {{
                ""name"": ""{1}""
            }}
        }}";

        internal string JobFixtrue = @"{{
            ""metadata"": {{
            ""guid"": ""{0}"",
            ""created_at"": ""2014-07-31T15:56:27+00:00"",
            ""url"": ""/v2/jobs/{0}""
            }},
            ""entity"": {{
            ""guid"": ""{0}"",
            ""status"": ""{1}""
            }}
        }}";

        #endregion

        public IEnumerable<IHttpMultiPartFormDataAbstraction> MultiPartRequestData { get; set; }

        public HttpMethod Method { get; set; }
        
        public Uri Uri { get; set; }
        
        public Stream Content { get; set; }
        
        public IDictionary<string, string> Headers { get; private set; }
        
        public string ContentType { get; set; }
        
        public TimeSpan Timeout { get; set; }

        public TimeSpan Delay { get; set; }


        public string AccessToken { get; set; }

        public IDictionary<string, KeyValuePair<string, string>> Users { get; private set; }

        public IDictionary<string, KeyValuePair<string, string>> Organizations { get; private set; }

        public IDictionary<string, KeyValuePair<string, string>> Spaces { get; private set; }

        public IDictionary<string, string> Apps { get; private set; }

        public IDictionary<string, string> Jobs { get; private set; }

        public IDictionary<string, string> Domains { get; private set; }

        public IDictionary<string, TestRouteInfo> Routes { get; private set; }

        public IDictionary<string, string> Mappings { get; private set; } 

        public IDictionary<string, string> Instances { get; private set; }

        public TestInstanceInfo InstanceInfo { get; set; }

        public CloudFoundryRestSimulator()
        {
            this.Instances = new Dictionary<string, string>();
            this.Routes = new Dictionary<string, TestRouteInfo>();
            this.Mappings = new Dictionary<string, string>();
            this.Users = new Dictionary<string, KeyValuePair<string, string>>();
            this.Organizations = new Dictionary<string, KeyValuePair<string, string>>();
            this.Spaces = new Dictionary<string, KeyValuePair<string, string>>();
            this.Apps = new Dictionary<string, string>();
            this.Jobs = new Dictionary<string, string>();
            this.Domains = new Dictionary<string, string>();
            this.InstanceInfo = null;
            this.Headers = new Dictionary<string, string>();
            this.Delay = TimeSpan.FromMilliseconds(0);
            this.AccessToken = string.Empty;
        }

        public CloudFoundryRestSimulator(CancellationToken token) : this()
        {
        }

        public async Task<IHttpResponseAbstraction> SendAsync(IEnumerable<IHttpMultiPartFormDataAbstraction> multipartData)
        {
            this.MultiPartRequestData = multipartData;

            return await this.SendAsync();
        }

        public Task<IHttpResponseAbstraction> SendAsync()
        {
            if (!HasValidAuthHeader())
            {
                return Task.Factory.StartNew(() => TestHelper.CreateResponse(HttpStatusCode.Unauthorized, new Dictionary<string, string>(), AccessDeniedErrMessage.ConvertToStream()));
            }
           
            IHttpResponseAbstraction retVal;
            switch (this.Method.ToString().ToLowerInvariant())
            {
                case "get":
                    retVal = HandleGet();
                    break;
                case "post":
                    retVal = HandlePost();
                    break;
                case "put":
                    retVal = HandlePut();
                    break;
                case "delete":
                    retVal = TestHelper.CreateErrorResponse();
                    break;
                default:
                    retVal = TestHelper.CreateErrorResponse();
                    break;
            }

            Thread.Sleep(Delay);
            return Task.Factory.StartNew(() => retVal);
        }

        internal bool HasValidAuthHeader()
        {
            //if this is a request for instance info, not auth is required.
            if (this.Uri.AbsolutePath == "/info" && this.Method == HttpMethod.Get)
            {
                return true;
            }

            //make sure an auth header exists.
            if (this.Headers.All(h => h.Key.ToLowerInvariant() != "authorization"))
            {
                return false;
            }

            //get the auth header from the collection.
            var authHeader = this.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");

            //if the request is for a token, make sure the auth token has the correct value.
            if (this.Uri.AbsolutePath.EndsWith("/uaa/oauth/token") && this.Method == HttpMethod.Post)
            {
                return authHeader.Value == "Basic Y2Y6";
            }

            //otherwise check to see if the access token matches the stored token.
            return authHeader.Value == "bearer " +AccessToken;
        }

        internal IHttpResponseAbstraction HandlePost()
        {
            if (this.Uri.AbsolutePath.EndsWith("/uaa/oauth/token"))
            {
                return HandleAuthRequest();
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/apps"))
            {
                if (this.Uri.AbsolutePath == "/v2/apps")
                {
                    return HandlePostApplication();
                }
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/routes"))
            {
                if (this.Uri.AbsolutePath == "/v2/routes")
                {
                    return HandlePostRoutes();
                }
            }

            return TestHelper.CreateErrorResponse();
        }

        internal IHttpResponseAbstraction HandlePutApplicationBits()
        {
            var jobPayload = string.Format(JobFixtrue, Guid.NewGuid(), JobState.Queued);
            var responseStream = jobPayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.Created, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandlePostApplication()
        {
            try
            {
                var obj = JObject.Parse(TestHelper.GetStringFromStream(this.Content));
                var name = (string)obj["name"];
                if (name == null)
                {
                    return TestHelper.CreateErrorResponse();
                }

                var newAppId = Guid.NewGuid().ToString();
                this.Apps.Add(newAppId, name);
                var resp =  (HttpResponseAbstraction)HandleGetApp(newAppId);
                resp.StatusCode = HttpStatusCode.Created;
                return resp;
            }
            catch (Exception)
            {
                return TestHelper.CreateErrorResponse();
            }
        }

        internal IHttpResponseAbstraction HandlePostRoutes()
        {
            try
            {
                var obj = JObject.Parse(TestHelper.GetStringFromStream(this.Content));
                var host = (string)obj["host"];
                var domainId = (string)obj["domain_guid"];
                var spaceId = (string)obj["space_guid"];
                if (host == null || domainId == null || spaceId == null)
                {
                    return TestHelper.CreateErrorResponse();
                }

                var newRouteId = Guid.NewGuid().ToString();
                this.Routes.Add(newRouteId, new TestRouteInfo(host, domainId, spaceId));
                var resp = (HttpResponseAbstraction)HandleGetRoute(newRouteId);
                resp.StatusCode = HttpStatusCode.Created;
                return resp;
            }
            catch (Exception)
            {
                return TestHelper.CreateErrorResponse();
            }
        }

        internal IHttpResponseAbstraction HandlePut()
        {
            if (this.Uri.AbsolutePath.StartsWith("/v2/apps"))
            {
                if (this.Uri.AbsolutePath.EndsWith("/bits"))
                {
                    return HandlePutApplicationBits();
                }

                if (this.Apps.ContainsKey(this.Uri.Segments.Last()))
                {
                    return HandleGetApp(this.Uri.Segments.Last());
                }

                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/routes"))
            {
                if (this.Routes.ContainsKey(this.Uri.Segments[3].Trim('/')))
                {
                    this.Mappings.Add(this.Uri.Segments[3].Trim('/'), this.Uri.Segments[5].Trim('/'));
                    var resp = (HttpResponseAbstraction)HandleGetRoute(this.Uri.Segments[3].Trim('/'));
                    resp.StatusCode = HttpStatusCode.Created;
                    return resp;
                }
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }
            return TestHelper.CreateErrorResponse();
        }

        internal IHttpResponseAbstraction HandleGet()
        {
            if (this.Uri.AbsolutePath.StartsWith("/info"))
            {
                return HandleGetInfo();
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/organizations"))
            {
                if (this.Uri.AbsolutePath == "/v2/organizations")
                {
                    return HandleGetOrganizations();
                }

                if (this.Uri.AbsolutePath.EndsWith("/spaces"))
                {
                    return HandleGetSpaces();
                }

                if (this.Uri.AbsolutePath.EndsWith("/users"))
                {
                    return HandleGetUsers();
                }
                return HandleGetOrganization(this.Uri.Segments.Last());
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/spaces"))
            {
                if (this.Uri.AbsolutePath == "/v2/spaces")
                {
                    return HandleGetSpaces();
                }
                if (this.Uri.AbsolutePath.EndsWith("/apps"))
                {
                    return HandleGetApps();
                }
                return HandleGetSpace(this.Uri.Segments.Last());
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/users"))
            {
                if (this.Uri.AbsolutePath == "/v2/users")
                {
                    return HandleGetUsers();
                }
                return HandleGetUser(this.Uri.Segments.Last());
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/apps"))
            {
                if (this.Uri.AbsolutePath == "/v2/apps")
                {
                    return HandleGetApps();
                }
                if (this.Uri.AbsolutePath.EndsWith("/routes"))
                {
                    return HandleGetRoutes();
                }
                if (this.Uri.AbsolutePath.EndsWith("/stats"))
                {
                    return HandleGetInstances();
                }
                return HandleGetApp(this.Uri.Segments.Last());
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/routes"))
            {
                if (this.Uri.AbsolutePath == "/v2/routes")
                {
                    return HandleGetRoutes();
                }
                //return HandleGetRoute();
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/shared_domains"))
            {
                if (this.Uri.AbsolutePath == "/v2/shared_domains")
                {
                    return HandleGetDomains();
                }
            }

            if (this.Uri.AbsolutePath.StartsWith("/v2/jobs"))
            {
                return HandleGetJob(this.Uri.Segments.Last());
            }

            return TestHelper.CreateErrorResponse();
        }

        internal IHttpResponseAbstraction HandleGetJob(string jobId)
        {
            if (!this.Jobs.ContainsKey(jobId))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound, new Dictionary<string, string>(), new MemoryStream());
            }
            var state = this.Jobs[jobId];
            var id = jobId;
            if (state == JobState.Finished.ToString())
            {
                id = "0";
            }
            var jobPayload = string.Format(JobFixtrue, id, state);
            var responseStream = jobPayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);    
        }

        internal IHttpResponseAbstraction HandleGetDomains()
        {
            var isFirst = true;
            var domains = string.Empty;
            foreach (var domain in this.Domains)
            {
                if (!isFirst)
                {
                    domains += ",";
                }

                domains += string.Format(DomainFixtrue, domain.Key, domain.Value);

                isFirst = false;
            }

            var responseStream = string.Format(MetadataFixture, this.Domains.Count, domains).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetInfo()
        {
            if (this.InstanceInfo == null)
            {
                return TestHelper.CreateErrorResponse();
            }

            var infoPayload = string.Format(InfoFixture, this.InstanceInfo.Name, this.InstanceInfo.Build,
                this.InstanceInfo.Version, this.InstanceInfo.AuthEndpoint, this.InstanceInfo.TokenEndpoint);
            var responseStream = infoPayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetOrganizations()
        {
            var isFirst = true;
            var orgs = string.Empty;
            foreach (var org in this.Organizations)
            {
                if (!isFirst)
                {
                    orgs += ",";
                }

                orgs += string.Format(OrganizationFixtrue, org.Key, org.Value.Key, org.Value.Value);

                isFirst = false;
            }

            var responseStream = string.Format(MetadataFixture, this.Organizations.Count, orgs).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetOrganization(string orgId)
        {
            if (!this.Organizations.ContainsKey(orgId))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound, new Dictionary<string, string>(), OrgNotFoundErrMessage.ConvertToStream());
            }
            var org = this.Organizations[orgId]; 
            var orgPayload = string.Format(OrganizationFixtrue, orgId, org.Key, org.Value);
            var responseStream = orgPayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetSpaces()
        {
            var isFirst = true;
            var spaces = string.Empty;
            foreach (var space in this.Spaces)
            {
                if (!isFirst)
                {
                    spaces += ",";
                }

                spaces += string.Format(SpaceFixtrue, space.Key, space.Value.Key, space.Value.Value);

                isFirst = false;
            }

            var responseStream = string.Format(MetadataFixture, this.Spaces.Count, spaces).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetSpace(string spaceId)
        {
            if (!this.Spaces.ContainsKey(spaceId))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }
            var space = this.Spaces[spaceId];
            var spacePayload = string.Format(SpaceFixtrue, spaceId, space.Key, space.Value);
            var responseStream = spacePayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetRoute(string routeId)
        {
            if (!this.Routes.ContainsKey(routeId))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }
            var route = this.Routes[routeId];
            var routePayload = string.Format(RouteFixture, routeId, route.HostName, route.DomainId, route.SpaceId);
            var responseStream = routePayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetApps()
        {
            var isFirst = true;
            var apps = string.Empty;
            foreach (var app in this.Apps)
            {
                if (!isFirst)
                {
                    apps += ",";
                }

                apps += string.Format(AppFixture, app.Key, app.Value);

                isFirst = false;
            }

            var responseStream = string.Format(MetadataFixture, this.Spaces.Count, apps).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetInstances()
        {
            var isFirst = true;
            var instances = string.Empty;
            foreach (var instance in this.Instances)
            {
                if (!isFirst)
                {
                    instances += ",";
                }

                instances += string.Format(StatsFixture, instance.Key, instance.Value);

                isFirst = false;
            }

            var responseStream = string.Format("{{ {0} }}", instances).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetApp(string appId)
        {
            if (!this.Apps.ContainsKey(appId))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }
            var appName = this.Apps[appId];
            var appPayload = string.Format(AppFixture, appId, appName);
            var responseStream = appPayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetUsers()
        {
            var isFirst = true;
            var users = string.Empty;
            foreach (var user in this.Users)
            {
                if (!isFirst)
                {
                    users += ",";
                }

                users += string.Format(UserFixture, user.Key, user.Value.Key);

                isFirst = false;
            }

            var responseStream = string.Format(MetadataFixture, this.Users.Count, users).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetUser(string userId)
        {
            if (!this.Users.ContainsKey(userId))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }
            var user = this.Users[userId];
            var userPayload = string.Format(UserFixture, userId, user.Key);
            var responseStream = userPayload.ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleGetRoutes()
        {
            var isFirst = true;
            var routes = string.Empty;
            foreach (var route in this.Routes)
            {
                if (!isFirst)
                {
                    routes += ",";
                }

                routes += string.Format(RouteFixture, route.Key, route.Value.HostName, route.Value.DomainId, route.Value.SpaceId);

                isFirst = false;
            }

            var responseStream = string.Format(MetadataFixture, this.Routes.Count, routes).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal IHttpResponseAbstraction HandleAuthRequest()
        {
            var userInfo = GetUserInfo(this.Uri);

            if (!CheckUserAuth(userInfo.Key, userInfo.Value))
            {
                TestHelper.CreateResponse(HttpStatusCode.Unauthorized, new Dictionary<string, string>(), AccessDeniedErrMessage.ConvertToStream());
            }

            
            this.AccessToken = Guid.NewGuid().ToString();
            var responseStream = string.Format(this.AuthResponseFixture, AccessToken).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), responseStream);
        }

        internal KeyValuePair<string, string> GetUserInfo(Uri inputUri)
        {
            var queryParams = HttpUtility.ParseQueryString(inputUri.Query);
            if (queryParams["username"] != null && queryParams["username"] != null)
            {
                return new KeyValuePair<string, string>(queryParams["username"], queryParams["username"]);
            }
            return new KeyValuePair<string, string>(string.Empty, string.Empty);
        }

        internal bool CheckUserAuth(string userName, string password)
        {
            return this.Users.Any(u => u.Value.Key == userName && u.Value.Value == password);
        }
    }
}
