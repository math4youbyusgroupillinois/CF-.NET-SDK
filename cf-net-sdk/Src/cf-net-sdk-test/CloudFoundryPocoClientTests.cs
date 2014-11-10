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
using System.Threading;
using System.Threading.Tasks;
using cf_net_sdk;
using cf_net_sdk.Interfaces;
using cf_net_sdk_test;
using CloudFoundry.Common;
using CloudFoundry.Common.Http;
using CloudFoundry.Common.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryPocoClientTests
    {
        internal TestCloudFoundryRestClient Client;
        internal Uri authEndpoint = new Uri("http://aok.example.server.com");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.Client = new TestCloudFoundryRestClient();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(ICloudFoundryRestClientFactory), new TestCloudFoundryRestClientFactory(this.Client));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.Client = new TestCloudFoundryRestClient();
            this.ServiceLocator = new ServiceLocator();
        }

        #region InstanceInfo Tests

        [TestMethod]
        public async Task CanGetIntanceInfo()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expInfo = new InstanceInfo("MyInstance", "111","222", this.authEndpoint, this.authEndpoint);
            var converter = new TestCloudFoundryInstanceInfoPayloadConverter(expInfo);

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IInstanceInfoPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var info = await pocoClient.GetInstanceInfoAsync();

            Assert.AreEqual(expInfo.Name, info.Name);
            Assert.AreEqual(expInfo.Build, info.Build);
            Assert.AreEqual(expInfo.Version, info.Version);
            Assert.AreEqual(expInfo.AuthorizationEndpoint.AbsoluteUri, info.AuthorizationEndpoint.AbsoluteUri);
            Assert.AreEqual(expInfo.TokenEndpoint.AbsoluteUri, info.TokenEndpoint.AbsoluteUri);
        }

        #endregion

        #region Authentication Tests

        [TestMethod]
        public async Task CanAuthenticate()
        {
            var userName = "test";
            var password = "password";
            var accessToken = Guid.NewGuid().ToString();

            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IAuthenticationPayloadConverter), new TestCloudFoundryAuthPayloadConverter(accessToken));
            
            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(),new HttpHeadersAbstraction(),HttpStatusCode.OK));

            var restClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await restClient.AuthenticateAsync(this.authEndpoint, userName, password);

            Assert.AreEqual(accessToken, resp.AccessToken);
            Assert.AreEqual(userName, resp.Username);
            Assert.AreEqual(password, resp.Password);
            Assert.AreEqual(authEndpoint, resp.ServerEndpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AuthenticationFailesWhenUserIsNotAuthorized()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            //TODO: add the error payload content, so that conversion can be tested.
            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized));

            var restClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            await restClient.AuthenticateAsync(this.authEndpoint, userName, password);
        }

        #endregion

        #region Organization Tests

        [TestMethod]
        public async Task CanGetOrganizations()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var org1 = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);
            var org2 = new Organization("54321", "OrgMy", "Inactive", DateTime.MaxValue);
            var converter = new TestCloudFoundryOrganizationPayloadConverter(new List<Organization>() { org1, org2 });
            
            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IOrganizationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetOrganizationsAsync();

            var orgs = resp.ToList();
            Assert.AreEqual(2, orgs.Count());

            Assert.AreEqual(org1.Id, orgs[0].Id);
            Assert.AreEqual(org1.Name, orgs[0].Name);
            Assert.AreEqual(org1.Status, orgs[0].Status);
            Assert.AreEqual(org1.CreatedDate, orgs[0].CreatedDate);

            Assert.AreEqual(org2.Id, orgs[1].Id);
            Assert.AreEqual(org2.Name, orgs[1].Name);
            Assert.AreEqual(org2.Status, orgs[1].Status);
            Assert.AreEqual(org2.CreatedDate, orgs[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetOrganization()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expOrg = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);
            var converter = new TestCloudFoundryOrganizationPayloadConverter(new List<Organization>() { expOrg });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IOrganizationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var org = await pocoClient.GetOrganizationAsync("12345");

            Assert.AreEqual(expOrg.Id, org.Id);
            Assert.AreEqual(expOrg.Name, org.Name);
            Assert.AreEqual(expOrg.Status, org.Status);
            Assert.AreEqual(expOrg.CreatedDate, org.CreatedDate);
        }

        #endregion

        #region Space Tests

        [TestMethod]
        public async Task CanGetSpaces()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var spc1 = new Space("12345", "MySpc", DateTime.MinValue);
            var spc2 = new Space("54321", "SpcMy", DateTime.MaxValue);
            var converter = new TestCloudFoundrySpacePayloadConverter(new List<Space>() { spc1, spc2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(ISpacePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetSpacesAsync();

            var spaces = resp.ToList();
            Assert.AreEqual(2, spaces.Count());

            Assert.AreEqual(spc1.Id, spaces[0].Id);
            Assert.AreEqual(spc1.Name, spaces[0].Name);
            Assert.AreEqual(spc1.CreatedDate, spaces[0].CreatedDate);

            Assert.AreEqual(spc2.Id, spaces[1].Id);
            Assert.AreEqual(spc2.Name, spaces[1].Name);
            Assert.AreEqual(spc2.CreatedDate, spaces[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetSpacesWithOrgId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var spc1 = new Space("12345", "MySpc", DateTime.MinValue);
            var spc2 = new Space("54321", "SpcMy", DateTime.MaxValue);
            var converter = new TestCloudFoundrySpacePayloadConverter(new List<Space>() { spc1, spc2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(ISpacePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetSpacesAsync("12345");

            var spaces = resp.ToList();
            Assert.AreEqual(2, spaces.Count());

            Assert.AreEqual(spc1.Id, spaces[0].Id);
            Assert.AreEqual(spc1.Name, spaces[0].Name);
            Assert.AreEqual(spc1.CreatedDate, spaces[0].CreatedDate);

            Assert.AreEqual(spc2.Id, spaces[1].Id);
            Assert.AreEqual(spc2.Name, spaces[1].Name);
            Assert.AreEqual(spc2.CreatedDate, spaces[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetSpace()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expSpc = new Space("12345", "MySpc", DateTime.MinValue);
            var converter = new TestCloudFoundrySpacePayloadConverter(new List<Space>() { expSpc });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(ISpacePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var spc = await pocoClient.GetSpaceAsync("12345");

            Assert.AreEqual(expSpc.Id, spc.Id);
            Assert.AreEqual(expSpc.Name, spc.Name);
            Assert.AreEqual(expSpc.CreatedDate, spc.CreatedDate);
        }

        #endregion

        #region User Tests

        [TestMethod]
        public async Task CanGetUsers()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var usr1 = new User("12345", "User1", DateTime.MinValue);
            var usr2 = new User("54321", "User2", DateTime.MaxValue);
            var converter = new TestCloudFoundryUserPayloadConverter(new List<User>() { usr1, usr2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IUserPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetUsersAsync();

            var users = resp.ToList();
            Assert.AreEqual(2, users.Count());

            Assert.AreEqual(usr1.Id, users[0].Id);
            Assert.AreEqual(usr1.Name, users[0].Name);
            Assert.AreEqual(usr1.CreatedDate, users[0].CreatedDate);

            Assert.AreEqual(usr2.Id, users[1].Id);
            Assert.AreEqual(usr2.Name, users[1].Name);
            Assert.AreEqual(usr2.CreatedDate, users[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetUsersWithOrgId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var usr1 = new User("12345", "User1", DateTime.MinValue);
            var usr2 = new User("54321", "User2", DateTime.MaxValue);
            var converter = new TestCloudFoundryUserPayloadConverter(new List<User>() { usr1, usr2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IUserPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetUsersAsync("12345");

            var users = resp.ToList();
            Assert.AreEqual(2, users.Count());

            Assert.AreEqual(usr1.Id, users[0].Id);
            Assert.AreEqual(usr1.Name, users[0].Name);
            Assert.AreEqual(usr1.CreatedDate, users[0].CreatedDate);

            Assert.AreEqual(usr2.Id, users[1].Id);
            Assert.AreEqual(usr2.Name, users[1].Name);
            Assert.AreEqual(usr2.CreatedDate, users[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetUser()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expUsr = new User("12345", "User1", DateTime.MinValue);
            var converter = new TestCloudFoundryUserPayloadConverter(new List<User>() { expUsr });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IUserPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var usr = await pocoClient.GetUserAsync("12345");

            Assert.AreEqual(expUsr.Id, usr.Id);
            Assert.AreEqual(expUsr.Name, usr.Name);
            Assert.AreEqual(expUsr.CreatedDate, usr.CreatedDate);
        }

        #endregion

        #region App Tests

        [TestMethod]
        public async Task CanGetApplications()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var app1 = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var app2 = new Application("54321", "App2", ApplicationStatus.Started, 1, 256, DateTime.MaxValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { app1, app2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetApplicationsAsync();

            var apps = resp.ToList();
            Assert.AreEqual(2, apps.Count());

            Assert.AreEqual(app1.Id, apps[0].Id);
            Assert.AreEqual(app1.Name, apps[0].Name);
            Assert.AreEqual(app1.CreatedDate, apps[0].CreatedDate);

            Assert.AreEqual(app2.Id, apps[1].Id);
            Assert.AreEqual(app2.Name, apps[1].Name);
            Assert.AreEqual(app2.CreatedDate, apps[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetApplicationsWithSpaceId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var app1 = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var app2 = new Application("54321", "App2", ApplicationStatus.Started, 1, 256, DateTime.MaxValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { app1, app2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetApplicationsAsync("12345");

            var apps = resp.ToList();
            Assert.AreEqual(2, apps.Count());

            Assert.AreEqual(app1.Id, apps[0].Id);
            Assert.AreEqual(app1.Name, apps[0].Name);
            Assert.AreEqual(app1.CreatedDate, apps[0].CreatedDate);

            Assert.AreEqual(app2.Id, apps[1].Id);
            Assert.AreEqual(app2.Name, apps[1].Name);
            Assert.AreEqual(app2.CreatedDate, apps[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { expApp });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.GetApplicationAsync("12345");

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        public async Task CanCreateAnApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { expApp });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Created));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.CreateApplicationAsync("someApp", "1235", 512, 2, 2048);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        public async Task CanUploadAnApplicationPackage()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expJob = new Job("12345", "12345", JobState.Queued, DateTime.MinValue);
            var converter = new TestCloudFoundryJobPayloadConverter(expJob);

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IJobPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Created));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var job = await pocoClient.UploadApplicationPackageAsync("someApp", "some package".ConvertToStream());

            Assert.AreEqual(expJob.Id, job.Id);
            Assert.AreEqual(expJob.Name, job.Name);
            Assert.AreEqual(expJob.State, job.State);
            Assert.AreEqual(expJob.CreatedDate, job.CreatedDate);
        }

        [TestMethod]
        public async Task CanStartApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { expApp });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.StartApplicationAsync("12345");

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        public async Task CanStopApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { expApp });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.StopApplicationAsync("12345");

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        public async Task CanScaleApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var converter = new TestCloudFoundryApplicationPayloadConverter(new List<Application>() { expApp });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Created));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.ScaleApplicationAsync("12345", 2);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        #endregion

        #region Route Tests

        [TestMethod]
        public async Task CanGetRoutesWithAppId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var rt1 = new Route("12345", "Route1", "Domain1.com", DateTime.MinValue);
            var rt2 = new Route("54321", "Route2", "Domain2.com", DateTime.MaxValue);
            var converter = new TestCloudFoundryRoutePayloadConverter(new List<Route>() { rt1, rt2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IRoutePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetRoutesAsync("12345");

            var rts = resp.ToList();
            Assert.AreEqual(2, rts.Count());

            Assert.AreEqual(rt1.Id, rts[0].Id);
            Assert.AreEqual(rt1.Name, rts[0].Name);
            Assert.AreEqual(rt1.CreatedDate, rts[0].CreatedDate);

            Assert.AreEqual(rt2.Id, rts[1].Id);
            Assert.AreEqual(rt2.Name, rts[1].Name);
            Assert.AreEqual(rt2.CreatedDate, rts[1].CreatedDate);
        }

        [TestMethod]
        public async Task CanCreateARoute()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expRoute = new Route("12345", "Route1", "blah.com", DateTime.MinValue);
            var converter = new TestCloudFoundryRoutePayloadConverter(new List<Route>() { expRoute });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IRoutePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Created));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.CreateRouteAsync("Rotue1","54321","12345");

            Assert.AreEqual(expRoute.Id, app.Id);
            Assert.AreEqual(expRoute.Name, app.Name);
            Assert.AreEqual(expRoute.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        public async Task CanMapARoute()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expRoute = new Route("12345", "Route1", "blah.com", DateTime.MinValue);
            var converter = new TestCloudFoundryRoutePayloadConverter(new List<Route>() { expRoute });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IRoutePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Created));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await pocoClient.MapRouteAsync(expRoute.Id, "54321");

            Assert.AreEqual(expRoute.Id, app.Id);
            Assert.AreEqual(expRoute.Name, app.Name);
            Assert.AreEqual(expRoute.CreatedDate, app.CreatedDate);
        }

        #endregion

        #region Instance Tests

        [TestMethod]
        public async Task CanGetInstancesWithAppId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var int1 = new Instance("12345", "Instance1", InstanceState.Running, 555, 512, 45, 128, 512, 256, 60, DateTime.MinValue);
            var int2 = new Instance("54321", "Instance2", InstanceState.Unknown, 666, 1024, 23, 256, 1024, 512, 120, DateTime.MaxValue);
            var converter = new TestCloudFoundryInstancePayloadConverter(new List<Instance>() { int1, int2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IInstancePayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetInstancesAsync("12345");

            var ints = resp.ToList();
            Assert.AreEqual(2, ints.Count());

            Assert.AreEqual(int1.Id, ints[0].Id);
            Assert.AreEqual(int1.Name, ints[0].Name);
            Assert.AreEqual(int1.State, ints[0].State);
            Assert.AreEqual(int1.Port, ints[0].Port);
            Assert.AreEqual(int1.MemoryUsage, ints[0].MemoryUsage);
            Assert.AreEqual(int1.CpuUsage, ints[0].CpuUsage);
            Assert.AreEqual(int1.DiskUsage, ints[0].DiskUsage);
            Assert.AreEqual(int1.MemoryQuota, ints[0].MemoryQuota);
            Assert.AreEqual(int1.DiskQuota, ints[0].DiskQuota);
            Assert.AreEqual(int1.UpTime, ints[0].UpTime);
            Assert.AreEqual(int1.CreatedDate, ints[0].CreatedDate);

            Assert.AreEqual(int2.Id, ints[1].Id);
            Assert.AreEqual(int2.Name, ints[1].Name);
            Assert.AreEqual(int2.State, ints[1].State);
            Assert.AreEqual(int2.Port, ints[1].Port);
            Assert.AreEqual(int2.MemoryUsage, ints[1].MemoryUsage);
            Assert.AreEqual(int2.CpuUsage, ints[1].CpuUsage);
            Assert.AreEqual(int2.DiskUsage, ints[1].DiskUsage);
            Assert.AreEqual(int2.MemoryQuota, ints[1].MemoryQuota);
            Assert.AreEqual(int2.DiskQuota, ints[1].DiskQuota);
            Assert.AreEqual(int2.UpTime, ints[1].UpTime);
            Assert.AreEqual(int2.CreatedDate, ints[1].CreatedDate);
        }

        #endregion

        #region Domain Tests

        [TestMethod]
        public async Task CanGetDomains()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var dom1 = new Domain("12345", "My.com", DateTime.MinValue);
            var dom2 = new Domain("54321", "Dom.com", DateTime.MaxValue);
            var converter = new TestCloudFoundryDomainPayloadConverter(new List<Domain>() { dom1, dom2 });

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IDomainPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await pocoClient.GetDomainsAsync();

            var domains = resp.ToList();
            Assert.AreEqual(2, domains.Count());

            Assert.AreEqual(dom1.Id, domains[0].Id);
            Assert.AreEqual(dom1.Name, domains[0].Name);
            Assert.AreEqual(dom1.CreatedDate, domains[0].CreatedDate);

            Assert.AreEqual(dom2.Id, domains[1].Id);
            Assert.AreEqual(dom2.Name, domains[1].Name);
            Assert.AreEqual(dom2.CreatedDate, domains[1].CreatedDate);
        }

        #endregion

        #region Job Tests

        [TestMethod]
        public async Task CanGetJob()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            var expJob = new Job("12345", "12345", JobState.Queued, DateTime.MinValue);
            var converter = new TestCloudFoundryJobPayloadConverter( expJob );

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IJobPayloadConverter), converter);

            this.Client.Responses.Enqueue(new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK));

            var pocoClient = new CloudFoundryPocoClient(this.ServiceLocator, creds, CancellationToken.None);
            var job = await pocoClient.GetJobAsync("12345");

            Assert.AreEqual(expJob.Id, job.Id);
            Assert.AreEqual(expJob.Name, job.Name);
            Assert.AreEqual(expJob.State, job.State);
            Assert.AreEqual(expJob.CreatedDate, job.CreatedDate);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatingClientWithNullServiceLocatorThrows()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);

            new CloudFoundryPocoClient(null, creds, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatingClientWithNullCredentialsThrows()
        {
            new CloudFoundryPocoClient(this.ServiceLocator, null, CancellationToken.None);
        }
    }
}
