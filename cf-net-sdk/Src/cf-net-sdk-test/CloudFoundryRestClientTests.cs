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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using cf_net_sdk;
using cf_net_sdk_test;
using CloudFoundry.Common;
using CloudFoundry.Common.Http;
using CloudFoundry.Common.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryRestClientTests
    {
        internal CloudFoundryRestSimulator simulator;
        internal Uri serverEndpoint = new Uri("http://api.example.server.com");
        internal Uri tokenEndpoint = new Uri("http://aok.example.server.com/uaa");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.simulator = new CloudFoundryRestSimulator();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new CloudFoundryRestSimulatorFactory(simulator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.simulator = new CloudFoundryRestSimulator();
            this.ServiceLocator = new ServiceLocator();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatingRestClientWithNullServiceLocatorThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            new CloudFoundryRestClient(null, cred, CancellationToken.None);
        }

        #region Get Info Tests

        [TestMethod]
        public async Task CanGetInstanceInfo()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;
            this.simulator.AccessToken = accessToken;
            this.simulator.InstanceInfo = new TestInstanceInfo()
            {
                Name = "MyServer",
                AuthEndpoint = "someuri",
                Build = "222",
                TokenEndpoint = "someendpoint",
                Version = "2"
            };

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetInstanceInfoAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingInstanceInfoFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetInstanceInfoAsync();

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/info"));
        }

        [TestMethod]
        public async Task GettingInstanceInfoDoesNotIncludeAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetInstanceInfoAsync();

            Assert.IsFalse(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
        }

        #endregion

        #region Authentication Tests

        [TestMethod]
        public async Task CanAuthenticate()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            this.simulator.Users.Add(Guid.NewGuid().ToString(),new KeyValuePair<string, string>(userName,password));

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.AuthenticateAsync(this.tokenEndpoint, userName, password);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task AuthenticatingFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.AuthenticateAsync(this.tokenEndpoint, userName, password);

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/uaa/oauth/token"));

            var queryArgs = HttpUtility.ParseQueryString(this.simulator.Uri.Query);
            Assert.IsNotNull(queryArgs["username"]);
            Assert.IsNotNull(queryArgs["password"]);
            Assert.IsNotNull(queryArgs["grant_type"]);
            Assert.AreEqual(userName, queryArgs["username"]);
            Assert.AreEqual(password, queryArgs["password"]);
            Assert.AreEqual("password", queryArgs["grant_type"]);
        }

        [TestMethod]
        public async Task AuthenticatingAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";

            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.AuthenticateAsync(this.serverEndpoint, userName, password);

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("Basic Y2Y6", header.Value);
        }

        #endregion

        #region Get Organizations Tests

        [TestMethod]
        public async Task CanGetOrganizations()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Organizations.Add("12345", new KeyValuePair<string, string>("MyOrg","54321"));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetOrganizationsAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingOrganizationsFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetOrganizationsAsync();

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/organizations"));
        }

        [TestMethod]
        public async Task GettingOrganizationsAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetOrganizationsAsync();

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " +accessToken, header.Value);
        }

        #endregion

        #region Get Organization Tests

        [TestMethod]
        public async Task CanGetOrganization()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Organizations.Add("12345", new KeyValuePair<string, string>("MyOrg", "54321"));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetOrganizationAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingOrganizationFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetOrganizationAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/organizations/12345"));
        }

        [TestMethod]
        public async Task GettingOrganizationAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetOrganizationAsync("12345");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Sapces Tests

        [TestMethod]
        public async Task CanGetSpaces()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Spaces.Add("12345", new KeyValuePair<string, string>("MySpace", "54321"));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetSpacesAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetSpacesWithOrgId()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Spaces.Add("12345", new KeyValuePair<string, string>("MySpace", "54321"));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetSpacesAsync("45678");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingSpacesFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpacesAsync();

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/spaces"));
        }

        [TestMethod]
        public async Task GettingSpacesWithOrgIdFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpacesAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/organizations/12345/spaces"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingSpacesWithNullOrgIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpacesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingSpacesWithEmptyOrgIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpacesAsync(string.Empty);
        }

        [TestMethod]
        public async Task GettingSpacesAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpacesAsync();

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        [TestMethod]
        public async Task GettingSpacesWithOrgIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpacesAsync("23456");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Space Tests

        [TestMethod]
        public async Task CanGetSpace()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Spaces.Add("12345", new KeyValuePair<string, string>("MySpace", "54321"));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetSpaceAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingSpaceFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpaceAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/spaces/12345"));
        }

        [TestMethod]
        public async Task GettingSpaceAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetSpaceAsync("12345");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Users Tests

        [TestMethod]
        public async Task CanGetUsers()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Users.Add("12345", new KeyValuePair<string, string>(userName, password));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetUsersAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetUsersWithOrgId()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Users.Add("12345", new KeyValuePair<string, string>(userName, password));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetUsersAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingUsersFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUsersAsync();

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/users"));
        }

        [TestMethod]
        public async Task GettingUsersWithOrgIdFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUsersAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/organizations/12345/users"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingUsersWithNullOrgIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUsersAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingUsersWithEmptyOrgIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUsersAsync(string.Empty);
        }

        [TestMethod]
        public async Task GettingUsersAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUsersAsync();

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        [TestMethod]
        public async Task GettingUsersWithOrgIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUsersAsync("23456");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get User Tests

        [TestMethod]
        public async Task CanGetUser()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Users.Add("12345", new KeyValuePair<string, string>(userName, password));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetUserAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingUserFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUserAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/users/12345"));
        }

        [TestMethod]
        public async Task GettingUserAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetUserAsync("12345");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Apps Tests

        [TestMethod]
        public async Task CanGetApps()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Apps.Add("12345", "SomeApp");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetApplicationsAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetAppsWithSpaceId()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Apps.Add("12345", "SomeApp");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetApplicationsAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingAppsFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationsAsync();

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps"));
        }

        [TestMethod]
        public async Task GettingAppsWithSpaceIdFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationsAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/spaces/12345/apps"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingAppsWithNullSpaceIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingAppsWithEmptySpaceIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationsAsync(string.Empty);
        }

        [TestMethod]
        public async Task GettingAppsAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationsAsync();

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        [TestMethod]
        public async Task GettingAppsWithOrgIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationsAsync("23456");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get App Tests

        [TestMethod]
        public async Task CanGetApp()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Apps.Add("12345", "SomeApp");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetApplicationAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingAppFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps/12345"));
        }

        [TestMethod]
        public async Task GettingAppAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetApplicationAsync("12345");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Create App Tests

        [TestMethod]
        public async Task CanCreateAnApp()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var resp = await restClient.CreateApplicationAsync(name, "12345", 512, 2, 2048);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.AreEqual(name, this.simulator.Apps.First().Value);
        }

        [TestMethod]
        public async Task CreatingAnAppFormsTheCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var memory = 512;
            var spaceId = "12345";
            var instances = 2;
            var disk = 2048;
            await restClient.CreateApplicationAsync(name, spaceId, memory, instances, disk);

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps"));
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);
        }

        [TestMethod]
        public async Task CreatingAnAppFormsTheCorrectBody()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var memory = 512;
            var spaceId = "12345";
            var instances = 2;
            var disk = 2048;
            await restClient.CreateApplicationAsync(name, spaceId, memory, instances, disk);

            this.simulator.Content.Position = 0;
            var body = JObject.Parse(TestHelper.GetStringFromStream(this.simulator.Content));
            Assert.AreEqual(name, (string)body["name"]);
            Assert.AreEqual(memory, (int)body["memory"]);
            Assert.AreEqual(instances, (int)body["instances"]);
            Assert.AreEqual(disk, (int)body["disk_quota"]);
            Assert.AreEqual(spaceId, (string)body["space_guid"]);
        }

        [TestMethod]
        public async Task CreatingAnAppAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var memory = 512;
            var spaceId = "12345";
            var instances = 2;
            var disk = 2048;
            await restClient.CreateApplicationAsync(name, spaceId, memory, instances, disk);

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Update App Tests

        [TestMethod]
        public async Task CanUpdateApp()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Apps.Add("54321", "App1");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var props = new Dictionary<string, object>() {{"state", "STOPPED"}};
            var resp = await restClient.UpdateApplicationAsync("54321", props);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task UpdatingAppFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var props = new Dictionary<string, object>() { { "state", "STOPPED" } };
            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync("12345", props);

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps/12345"));
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);
        }

        [TestMethod]
        public async Task UpdatingAppFormsCorrectJsonBody()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var setting1 = new KeyValuePair<string, object>("state", "STOPPED");
            var setting2 = new KeyValuePair<string, object>("instances", 2);
            var settings = new Dictionary<string, object>()
            {
                {setting1.Key, setting1.Value},
                {setting2.Key, setting2.Value}
            };

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync("12345", settings);

            var requestJson = TestHelper.GetStringFromStream(this.simulator.Content);
            var obj = JObject.Parse(requestJson);
            Assert.IsNotNull(obj[setting1.Key]);
            Assert.AreEqual(2, obj.Properties().Count());
            Assert.AreEqual(setting1.Value, (string)obj[setting1.Key]);
            Assert.IsNotNull(obj[setting1.Key]);
            Assert.AreEqual(setting2.Value, (int)obj[setting2.Key]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatingAppWithNullAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var props = new Dictionary<string, object>() { { "state", "STOPPED" } };
            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync(null, props);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatingAppWithNullAppSettingsThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync("54321", null );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdatingAppWithEmptyAppSettingsThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync("54321", new Dictionary<string, object>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdatingAppWithEmptyAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var props = new Dictionary<string, object>() { { "state", "STOPPED" } };
            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync(string.Empty, props);
        }

        [TestMethod]
        public async Task UpdatingAppWithAppIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var props = new Dictionary<string, object>() { { "state", "STOPPED" } };
            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UpdateApplicationAsync("23456", props);

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Upload Package Tests

        [TestMethod]
        public async Task CanUploadAnApp()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;
            
            this.simulator.Apps.Add("54321", "App1");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var applicationStream = "Test Application Data";
            var resp = await restClient.UploadApplicationPackageAsync("54321", applicationStream.ConvertToStream());

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
        }

        [TestMethod]
        public async Task UploadingAPackageFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var applicationStream = "Test Application Data";
            await restClient.UploadApplicationPackageAsync("54321", applicationStream.ConvertToStream());

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps/54321/bits"));
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);

            var queryArgs = HttpUtility.ParseQueryString(this.simulator.Uri.Query);
            Assert.IsNotNull(queryArgs["async"]);
            Assert.AreEqual("true", queryArgs["async"]);
        }

        [TestMethod]
        public async Task UploadingAPackageFormsCorrectBody()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var applicationStream = "Test Application Data";
            await restClient.UploadApplicationPackageAsync("54321", applicationStream.ConvertToStream());

            Assert.IsNotNull(this.simulator.MultiPartRequestData);

            Assert.AreEqual(3, this.simulator.MultiPartRequestData.Count());
            
            var asyncField = this.simulator.MultiPartRequestData.FirstOrDefault(i => i.Name == "async");
            Assert.IsNotNull(asyncField);
            Assert.AreEqual("true", TestHelper.GetStringFromStream(asyncField.Content));

            var resourcesField = this.simulator.MultiPartRequestData.FirstOrDefault(i => i.Name == "resources");
            Assert.IsNotNull(resourcesField);
            Assert.AreEqual("[]", TestHelper.GetStringFromStream(resourcesField.Content));

            var appField = this.simulator.MultiPartRequestData.FirstOrDefault(i => i.Name == "application");
            Assert.IsNotNull(appField);
            Assert.AreEqual(applicationStream, TestHelper.GetStringFromStream(appField.Content));
            Assert.AreEqual("application.zip", appField.FileName);
            Assert.AreEqual("application/zip", appField.ContentType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UploadingAPackageWithNullAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var applicationStream = "Test Application Data";
            await restClient.UploadApplicationPackageAsync(null, applicationStream.ConvertToStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UploadingAPackageWithNullStreamThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.UploadApplicationPackageAsync("54321", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UploadingAPackageWithEmptyAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var applicationStream = "Test Application Data";
            await restClient.UploadApplicationPackageAsync(string.Empty, applicationStream.ConvertToStream());
        }

        [TestMethod]
        public async Task UploadingAPackageWithAppIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var applicationStream = "Test Application Data";
            await restClient.UploadApplicationPackageAsync("54321", applicationStream.ConvertToStream());

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Routes with Id Tests

        [TestMethod]
        public async Task CanGetRoutesWithAppId()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Routes.Add("12345", new TestRouteInfo("SomeRoute","54321", "67890"));
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetRoutesAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingRoutesWithAppIdFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetRoutesAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps/12345/routes"));

            var queryArgs = HttpUtility.ParseQueryString(this.simulator.Uri.Query);
            Assert.IsNotNull(queryArgs["inline-relations-depth"]);
            Assert.AreEqual("1", queryArgs["inline-relations-depth"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingRoutesWithNullAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetRoutesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingRoutesWithEmptyAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetRoutesAsync(string.Empty);
        }

        [TestMethod]
        public async Task GettingRoutesWithAppIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetRoutesAsync("23456");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Routes Tests

        [TestMethod]
        public async Task CanGetInstancesWithAppId()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Instances.Add("0", "192.168.0.1");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetInstancesAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingInstancesWithAppIdFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetInstancesAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/apps/12345/stats"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingInstancesWithNullAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetInstancesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingInstancesWithEmptyAppIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetInstancesAsync(string.Empty);
        }

        [TestMethod]
        public async Task GettingInstancesWithAppIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetInstancesAsync("23456");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Create Route Tests

        [TestMethod]
        public async Task CanCreateARoute()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var domainId = "54321";
            var spaceId = "12345";
            var resp = await restClient.CreateRouteAsync(name, domainId, spaceId);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.AreEqual(name, this.simulator.Routes.First().Value.HostName);
            Assert.AreEqual(domainId, this.simulator.Routes.First().Value.DomainId);
            Assert.AreEqual(spaceId, this.simulator.Routes.First().Value.SpaceId);
        }

        [TestMethod]
        public async Task CreatingARouteFormsTheCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var domainId = "54321";
            var spaceId = "12345";

            await restClient.CreateRouteAsync(name, domainId, spaceId);

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/routes"));
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);

            var queryArgs = HttpUtility.ParseQueryString(this.simulator.Uri.Query);
            Assert.IsNotNull(queryArgs["inline-relations-depth"]);
            Assert.AreEqual("1", queryArgs["inline-relations-depth"]);
        }

        [TestMethod]
        public async Task CreatingARouteFormsTheCorrectBody()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var domainId = "54321";
            var spaceId = "12345";
            await restClient.CreateRouteAsync(name, domainId, spaceId);

            this.simulator.Content.Position = 0;
            var body = JObject.Parse(TestHelper.GetStringFromStream(this.simulator.Content));
            Assert.AreEqual(name, (string)body["host"]);
            Assert.AreEqual(domainId, (string)body["domain_guid"]);
            Assert.AreEqual(spaceId, (string)body["space_guid"]);
        }

        [TestMethod]
        public async Task CreatingARouteAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var name = "newApp";
            var domainId = "54321";
            var spaceId = "12345";
            await restClient.CreateRouteAsync(name, domainId, spaceId);

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Map Route Tests

        [TestMethod]
        public async Task CanMapARoute()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.AccessToken = accessToken;

            this.simulator.Routes.Add("54321", new TestRouteInfo("blah","98765","12345"));
            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var routeId = "54321";
            var applicationId = "23456";
            var resp = await restClient.MapRouteAsync(routeId, applicationId);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.AreEqual(routeId, this.simulator.Mappings.Keys.First());
            Assert.AreEqual(applicationId, this.simulator.Mappings.Values.First());
        }

        [TestMethod]
        public async Task MappingARouteFormsTheCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var routeId = "54321";
            var applicationId = "23456";
            await restClient.MapRouteAsync(routeId, applicationId);

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/routes/" +routeId +"/apps/" + applicationId));
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);

            var queryArgs = HttpUtility.ParseQueryString(this.simulator.Uri.Query);
            Assert.IsNotNull(queryArgs["inline-relations-depth"]);
            Assert.AreEqual("1", queryArgs["inline-relations-depth"]);
        }

        [TestMethod]
        public async Task MappingARouteAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);

            var routeId = "54321";
            var applicationId = "23456";
            await restClient.MapRouteAsync(routeId, applicationId);

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Domains Tests

        [TestMethod]
        public async Task CanGetDomains()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Domains.Add("12345", "somwhere.com");
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetDomainsAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingDomainsFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetDomainsAsync();

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/shared_domains"));
        }

        [TestMethod]
        public async Task GettingDomainsAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetDomainsAsync();

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion

        #region Get Job Tests

        [TestMethod]
        public async Task CanGetJobWithId()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "98765";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            this.simulator.Jobs.Add("12345", JobState.Queued.ToString());
            this.simulator.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            var resp = await restClient.GetJobAsync("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task GettingJobWithIdFormsCorrectUrl()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetJobAsync("12345");

            Assert.IsTrue(this.simulator.Uri.AbsolutePath.Equals("/v2/jobs/12345"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingJobWithNullIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetJobAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingJobWithEmptyIdThrows()
        {
            var userName = "test";
            var password = "password";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetJobAsync(string.Empty);
        }

        [TestMethod]
        public async Task GettingJobWithIdAddsCorrectAuthHeader()
        {
            var userName = "test";
            var password = "password";
            var accessToken = "1234567890";
            var cred = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            cred.AccessToken = accessToken;

            var restClient = new CloudFoundryRestClient(this.ServiceLocator, cred, CancellationToken.None);
            await restClient.GetJobAsync("23456");

            Assert.IsTrue(this.simulator.Headers.Any(h => h.Key.ToLowerInvariant() == "authorization"));
            var header = this.simulator.Headers.FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization");
            Assert.AreEqual("bearer " + accessToken, header.Value);
        }

        #endregion
    }
}
