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
using System.Threading;
using System.Threading.Tasks;
using cf_net_sdk;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using CloudFoundry.Common.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryClientTests
    {
        internal TestCloudFoundryPocoClient Client;
        internal Uri authEndpoint = new Uri("http://aok.auth.server.com");
        internal Uri tokenEndpoint = new Uri("http://aok.token.server.com");
        internal Uri serverEndpoint = new Uri("http://api.token.server.com");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.Client = new TestCloudFoundryPocoClient();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(ICloudFoundryPocoClientFactory), new TestCloudFoundryPocoClientFactory(this.Client));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.Client = new TestCloudFoundryPocoClient();
            this.ServiceLocator = new ServiceLocator();
        }

        [TestMethod]
        public async Task CanGetInstanceInfo()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, password);
            this.Client.Credential = creds;

            var expInfo = new InstanceInfo("MyInstance", "111", "222", this.authEndpoint, this.tokenEndpoint);
            this.Client.GetInstanceInfo = () => Task.Factory.StartNew(() => expInfo);

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var info = await client.GetInstanceInfoAsync();

            Assert.AreEqual(expInfo.Name, info.Name);
            Assert.AreEqual(expInfo.Build, info.Build);
            Assert.AreEqual(expInfo.Version, info.Version);
            Assert.AreEqual(expInfo.AuthorizationEndpoint.AbsoluteUri, info.AuthorizationEndpoint.AbsoluteUri);
            Assert.AreEqual(expInfo.TokenEndpoint.AbsoluteUri, info.TokenEndpoint.AbsoluteUri);
        }

        [TestMethod]
        public async Task CanAuthenticate()
        {
            var userName = "test";
            var expPassword = "password";
            var accessToken = Guid.NewGuid().ToString();

            var creds = new CloudFoundryPasswordCredential(this.authEndpoint, userName, expPassword);

            var expInfo = new InstanceInfo("MyInstance", "111", "222", this.authEndpoint, this.tokenEndpoint);
            this.Client.GetInstanceInfo = () => Task.Factory.StartNew(() => expInfo);

            this.Client.Credential = creds;
            this.Client.Authenticate = (tokenUri, username, password) =>
            {
                Assert.AreEqual(creds.Username, username);
                Assert.AreEqual(creds.Password, password);
                Assert.AreEqual(this.tokenEndpoint, tokenUri.AbsoluteUri);
                var cred = new CloudFoundryPasswordCredential(creds.ServerEndpoint, userName, password)
                {
                    AccessToken = accessToken
                };
                return Task.Factory.StartNew(() => (IPasswordAuthCredential)cred);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.ConnectAsync();

            Assert.AreEqual(accessToken, resp.AccessToken);
        }

        [TestMethod]
        public async Task CanGetOrganizations()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expOrg = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);
            var expOrgs = new List<Organization>() {expOrg};
            this.Client.GetOrganizations = () => Task.Factory.StartNew(() => (IEnumerable<Organization>)expOrgs);

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetOrganizationsAsync();
            var orgs = resp.ToList();
            Assert.AreEqual(1, orgs.Count);

            Assert.AreEqual(expOrg.Id, orgs[0].Id);
            Assert.AreEqual(expOrg.Name, orgs[0].Name);
            Assert.AreEqual(expOrg.Status, orgs[0].Status);
            Assert.AreEqual(expOrg.CreatedDate, orgs[0].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetOrganization()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expOrg = new Organization("12345", "MyOrg", "Active", DateTime.MinValue);
            this.Client.GetOrganization = (orgId) =>
            {
                Assert.AreEqual(expOrg.Id, orgId);
                return Task.Factory.StartNew(() => expOrg);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var org = await client.GetOrganizationAsync(expOrg.Id);

            Assert.AreEqual(expOrg.Id, org.Id);
            Assert.AreEqual(expOrg.Name, org.Name);
            Assert.AreEqual(expOrg.Status, org.Status);
            Assert.AreEqual(expOrg.CreatedDate, org.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetOrganizationWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetOrganizationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetOrganizationWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetOrganizationAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetSpaces()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expSpc = new Space("12345", "MyOrg", DateTime.MinValue);
            var expSpcs = new List<Space>() { expSpc };
            this.Client.GetSpaces = () => Task.Factory.StartNew(() => (IEnumerable<Space>)expSpcs);

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetSpacesAsync();
            var orgs = resp.ToList();
            Assert.AreEqual(1, orgs.Count);

            Assert.AreEqual(expSpc.Id, orgs[0].Id);
            Assert.AreEqual(expSpc.Name, orgs[0].Name);
            Assert.AreEqual(expSpc.CreatedDate, orgs[0].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetSpacesWithOrgId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expSpc = new Space("12345", "MyOrg", DateTime.MinValue);
            var expSpcs = new List<Space>() { expSpc };
            this.Client.GetSpacesWithId = (orgId) =>
            {
                Assert.AreEqual("12345", orgId);
                return Task.Factory.StartNew(() => (IEnumerable<Space>) expSpcs);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetSpacesAsync("12345");
            var orgs = resp.ToList();
            Assert.AreEqual(1, orgs.Count);

            Assert.AreEqual(expSpc.Id, orgs[0].Id);
            Assert.AreEqual(expSpc.Name, orgs[0].Name);
            Assert.AreEqual(expSpc.CreatedDate, orgs[0].CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetSpacesWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetSpacesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetSpacesWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetSpacesAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetSpace()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expSpc = new Space("12345", "MyOrg", DateTime.MinValue);
            this.Client.GetSpace = (spaceId) =>
            {
                Assert.AreEqual(expSpc.Id, spaceId);
                return Task.Factory.StartNew(() => expSpc);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var spc = await client.GetSpaceAsync(expSpc.Id);

            Assert.AreEqual(expSpc.Id, spc.Id);
            Assert.AreEqual(expSpc.Name, spc.Name);
            Assert.AreEqual(expSpc.CreatedDate, spc.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetSpaceWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetSpaceAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetSpaceWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetSpaceAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetJob()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expJob = new Job("12345", "12345", JobState.Queued, DateTime.MinValue);
            this.Client.GetJob = (jobId) =>
            {
                Assert.AreEqual(expJob.Id, jobId);
                return Task.Factory.StartNew(() => expJob);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var job = await client.GetJobAsync(expJob.Id);

            Assert.AreEqual(expJob.Id, job.Id);
            Assert.AreEqual(expJob.Name, job.Name);
            Assert.AreEqual(expJob.State, job.State);
            Assert.AreEqual(expJob.CreatedDate, job.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetJobWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetJobAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetJobWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetJobAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetUsers()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expUsr = new User("12345", "User1", DateTime.MinValue);
            var expUsers = new List<User>() { expUsr };
            this.Client.GetUsers = () => Task.Factory.StartNew(() => (IEnumerable<User>)expUsers);

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetUsersAsync();
            var usrs = resp.ToList();
            Assert.AreEqual(1, usrs.Count);

            Assert.AreEqual(expUsr.Id, usrs[0].Id);
            Assert.AreEqual(expUsr.Name, usrs[0].Name);
            Assert.AreEqual(expUsr.CreatedDate, usrs[0].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetUsersWithOrgId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expUsr = new User("12345", "User1", DateTime.MinValue);
            var expUsers = new List<User>() { expUsr };
            this.Client.GetUsersWithId = (orgId) =>
            {
                Assert.AreEqual("12345", orgId);
                return Task.Factory.StartNew(() => (IEnumerable<User>)expUsers);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetUsersAsync("12345");
            var usrs = resp.ToList();
            Assert.AreEqual(1, usrs.Count);

            Assert.AreEqual(expUsr.Id, usrs[0].Id);
            Assert.AreEqual(expUsr.Name, usrs[0].Name);
            Assert.AreEqual(expUsr.CreatedDate, usrs[0].CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetUsersWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetUsersAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetUsersWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetUsersAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetUser()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expUsr = new User("12345", "USer1", DateTime.MinValue);
            this.Client.GetUser = (userId) =>
            {
                Assert.AreEqual(expUsr.Id, userId);
                return Task.Factory.StartNew(() => expUsr);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var usr = await client.GetUserAsync(expUsr.Id);

            Assert.AreEqual(expUsr.Id, usr.Id);
            Assert.AreEqual(expUsr.Name, usr.Name);
            Assert.AreEqual(expUsr.CreatedDate, usr.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetUserWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetUserAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetUserWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetUserAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetApplications()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var expApps = new List<Application>() { expApp };
            this.Client.GetApplications = () => Task.Factory.StartNew(() => (IEnumerable<Application>)expApps);

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetApplicationsAsync();
            var apps = resp.ToList();
            Assert.AreEqual(1, apps.Count);

            Assert.AreEqual(expApp.Id, apps[0].Id);
            Assert.AreEqual(expApp.Name, apps[0].Name);
            Assert.AreEqual(expApp.CreatedDate, apps[0].CreatedDate);
        }

        [TestMethod]
        public async Task CanGetApplicationsWithSpaceId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expApp = new Application("12345", "User1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            var expApps = new List<Application>() { expApp };
            this.Client.GetApplicationsWithId = (spcId) =>
            {
                Assert.AreEqual("12345", spcId);
                return Task.Factory.StartNew(() => (IEnumerable<Application>)expApps);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetApplicationsAsync("12345");
            var apps = resp.ToList();
            Assert.AreEqual(1, apps.Count);

            Assert.AreEqual(expApp.Id, apps[0].Id);
            Assert.AreEqual(expApp.Name, apps[0].Name);
            Assert.AreEqual(expApp.CreatedDate, apps[0].CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetApplicationsWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetApplicationsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetApplicationsWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetApplicationsAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expApp = new Application("12345", "USer1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            this.Client.GetApplication = (appId) =>
            {
                Assert.AreEqual(expApp.Id, appId);
                return Task.Factory.StartNew(() => expApp);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.GetApplicationAsync(expApp.Id);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetApplicationWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetApplicationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetApplicationWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetApplicationAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanCreateAnApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            this.Client.CreateApplication = (name, spaceId, memory, instances, disk) =>
            {
                Assert.AreEqual(expApp.Name, name);
                Assert.AreEqual(expApp.Instances, instances);
                Assert.AreEqual(expApp.MemoryLimit, memory);
                Assert.AreEqual(2048, disk);
                Assert.AreEqual("54321", spaceId);
                // app model disk quota?
                // app model spaceId?
                return Task.Factory.StartNew(() => expApp);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.CreateApplicationAsync(expApp.Name, "54321", expApp.MemoryLimit, expApp.Instances, 2048);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateAnApplicationWithNullSpaceId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateApplicationAsync("App1", null, 512, 2, 2048);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateAnApplicationWithEmptySpaceId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateApplicationAsync("App1", string.Empty, 512, 2, 2048);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateAnApplicationWithNullName()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateApplicationAsync(null, "54321", 512, 2, 2048);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateAnApplicationWithEmptyName()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateApplicationAsync(string.Empty, "54321", 512, 2, 2048);
        }

        [TestMethod]
        public async Task CanUploadAnApplicationPackage()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var packageContent = "Some content";
            var expJob = new Job("12345", "12345", JobState.Queued, DateTime.MinValue);
            this.Client.UploadApplicationPackage = (id, package) =>
            {
                Assert.AreEqual(expJob.Id, id);
                Assert.AreEqual(packageContent, TestHelper.GetStringFromStream(package));
                return Task.Factory.StartNew(() => expJob);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var job = await client.UploadApplicationPackageAsync(expJob.Id, packageContent.ConvertToStream());

            Assert.AreEqual(expJob.Id, job.Id);
            Assert.AreEqual(expJob.Name, job.Name);
            Assert.AreEqual(expJob.State, job.State);
            Assert.AreEqual(expJob.CreatedDate, job.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotUploadAnApplicationPackageWithNullAppId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.UploadApplicationPackageAsync(null, "test".ConvertToStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotUploadAnApplicationPackageWithEmptyappId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.UploadApplicationPackageAsync(string.Empty, "test".ConvertToStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotUploadAnApplicationPackageWithNullPackage()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.UploadApplicationPackageAsync("12345", null);
        }

        [TestMethod]
        public async Task CanStartApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expApp = new Application("12345", "App1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            this.Client.StartApplication = (appId) =>
            {
                Assert.AreEqual(expApp.Id, appId);
                return Task.Factory.StartNew(() => expApp);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.StartApplicationAsync(expApp.Id);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotStartApplicationWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.StartApplicationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotStartApplicationWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.StartApplicationAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanStopApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expApp = new Application("12345", "USer1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            this.Client.StopApplication = (appId) =>
            {
                Assert.AreEqual(expApp.Id, appId);
                return Task.Factory.StartNew(() => expApp);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.StopApplicationAsync(expApp.Id);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotStopApplicationWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.StopApplicationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotStopApplicationWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.StopApplicationAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanScaleApplication()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var instances = 5;
            var expApp = new Application("12345", "USer1", ApplicationStatus.Started, 1, 256, DateTime.MinValue);
            this.Client.ScaleApplication = (appId, ints) =>
            {
                Assert.AreEqual(expApp.Id, appId);
                Assert.AreEqual(instances, ints);
                return Task.Factory.StartNew(() => expApp);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.ScaleApplicationAsync(expApp.Id, instances);

            Assert.AreEqual(expApp.Id, app.Id);
            Assert.AreEqual(expApp.Name, app.Name);
            Assert.AreEqual(expApp.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotScaleApplicationWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.ScaleApplicationAsync(null, 5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotScaleApplicationWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.ScaleApplicationAsync(string.Empty, 5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotScaleApplicationWithNegativeInstances()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.ScaleApplicationAsync("54321", -5);
        }

        [TestMethod]
        public async Task CanGetRoutesWithAppId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expRt = new Route("12345", "Route1", "Domain.com", DateTime.MinValue);
            var expRoutes = new List<Route>() { expRt };
            this.Client.GetRoutesWithId = (appId) =>
            {
                Assert.AreEqual("12345", appId);
                return Task.Factory.StartNew(() => (IEnumerable<Route>)expRoutes);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetRoutesAsync("12345");
            var routes = resp.ToList();
            Assert.AreEqual(1, routes.Count);

            Assert.AreEqual(expRt.Id, routes[0].Id);
            Assert.AreEqual(expRt.Name, routes[0].Name);
            Assert.AreEqual(expRt.DomainName, routes[0].DomainName);
            Assert.AreEqual(expRt.CreatedDate, routes[0].CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetRoutesWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetRoutesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetRoutesWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetRoutesAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanCreateARoute()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expRoute = new Route("98765", "Route1", "Blah.com", DateTime.MinValue);
            this.Client.CreateRoute = (name, domainId, spaceId) =>
            {
                Assert.AreEqual(expRoute.Name, name);
                Assert.AreEqual("54321", domainId);
                Assert.AreEqual("12345", spaceId);
                return Task.Factory.StartNew(() => expRoute);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.CreateRouteAsync(expRoute.Name, "54321", "12345");

            Assert.AreEqual(expRoute.Id, app.Id);
            Assert.AreEqual(expRoute.Name, app.Name);
            Assert.AreEqual(expRoute.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateARouteWithNullSpaceId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateRouteAsync("blah","12345", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateARouteWithEmptySpaceId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateRouteAsync("blah", "12345", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateARouteWithNullDomainId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateRouteAsync("blah", null, "54321");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateARouteWithEmptyDomainId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateRouteAsync("blah", string.Empty, "54321");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateARouteWithNullHostName()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateRouteAsync(null, "12345", "54321");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateARouteWithEmptyHostName()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.CreateRouteAsync(string.Empty, "12345", "54321");
        }

        [TestMethod]
        public async Task CanMapARoute()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expRoute = new Route("98765", "Route1", "Blah.com", DateTime.MinValue);
            this.Client.MapRoute = (routeId, appId) =>
            {
                Assert.AreEqual(expRoute.Id, routeId);
                Assert.AreEqual("54321", appId);
                return Task.Factory.StartNew(() => expRoute);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var app = await client.MapRouteAsync(expRoute.Id, "54321");

            Assert.AreEqual(expRoute.Id, app.Id);
            Assert.AreEqual(expRoute.Name, app.Name);
            Assert.AreEqual(expRoute.CreatedDate, app.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotMapARouteWithNullRouteId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.MapRouteAsync("blah", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotMapARouteWithEmptyRouteId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.MapRouteAsync("blah", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotMapARouteWithNullApplicationId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.MapRouteAsync("blah", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotMapARouteWithEmptyApplicationId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.MapRouteAsync("blah", string.Empty);
        }

        [TestMethod]
        public async Task CanGetInstancesWithAppId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var int1 = new Instance("12345", "Instance1", InstanceState.Running, 555, 512, 45, 128, 512, 256, 60, DateTime.MinValue);
            var expInst = new List<Instance>() { int1 };
            this.Client.GetInstancesWithId = (appId) =>
            {
                Assert.AreEqual("12345", appId);
                return Task.Factory.StartNew(() => (IEnumerable<Instance>)expInst);
            };

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetInstancesAsync("12345");
            var ints = resp.ToList();
            Assert.AreEqual(1, ints.Count);

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
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetInstancesWithNullId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetInstancesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetInstancesWithEmptyId()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            await client.GetInstancesAsync(string.Empty);
        }

        [TestMethod]
        public async Task CanGetDomains()
        {
            var userName = "test";
            var password = "password";
            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);
            this.Client.Credential = creds;

            var expDom1 = new Domain("12345", "My.com", DateTime.MinValue);
            var expDoms = new List<Domain>() { expDom1 };
            this.Client.GetDomains = () => Task.Factory.StartNew(() => (IEnumerable<Domain>)expDoms);

            var client = new CloudFoundryClient(this.ServiceLocator, creds, CancellationToken.None);
            var resp = await client.GetDomainsAsync();
            var domains = resp.ToList();
            Assert.AreEqual(1, domains.Count);

            Assert.AreEqual(expDom1.Id, domains[0].Id);
            Assert.AreEqual(expDom1.Name, domains[0].Name);
            Assert.AreEqual(expDom1.CreatedDate, domains[0].CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithTokenAndNullCredential()
        {
            CloudFoundryClient.Create(null, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithNullCredentials()
        {
            CloudFoundryClient.Create(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithNullServiceLocator()
        {
            var userName = "test";
            var password = "password";

            var creds = new CloudFoundryPasswordCredential(this.serverEndpoint, userName, password);

            new CloudFoundryClient(null, creds, CancellationToken.None);
        }
    }
}
