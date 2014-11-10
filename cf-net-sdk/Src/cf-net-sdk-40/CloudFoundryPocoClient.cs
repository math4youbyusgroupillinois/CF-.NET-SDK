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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using CloudFoundry.Common.ServiceLocation;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    internal class CloudFoundryPocoClient : ICloudFoundryPocoClient
    {
        /// <summary>
        /// A service locator that can be used to locate dependent services.
        /// </summary>
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Cancellation token used when making requests.
        /// </summary>
        internal CancellationToken CancellationToken;

        /// <summary>
        /// Credential used when making requests.
        /// </summary>
        internal IPasswordAuthCredential Credential;

        /// <summary>
        /// Create a new instance of the CloudFoundryPocoClient class.
        /// </summary>
        /// <param name="serviceLocator">A service locator that can be used to locate dependent services.</param>
        /// <param name="credential">The credential to use when making requests.</param>
        /// <param name="cancellationToken">The cancellation token to use when making requests.</param>
        public CloudFoundryPocoClient(IServiceLocator serviceLocator, IPasswordAuthCredential credential, CancellationToken cancellationToken)
        {
            serviceLocator.AssertIsNotNull("serviceLocator","Cannot create a poco client with a null service locator.");
            credential.AssertIsNotNull("credential","Cannot create a poco client with a null credential.");

            this.ServiceLocator = serviceLocator;
            this.CancellationToken = cancellationToken;
            this.Credential = credential;
        }

        /// <inheritdoc/>
        public async Task<InstanceInfo> GetInstanceInfoAsync()
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetInstanceInfoAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get instance information. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IInstanceInfoPayloadConverter>();
            var info = converter.Convert(await resp.ReadContentAsStringAsync());

            return info;
        }

        /// <inheritdoc/>
        public async Task<IPasswordAuthCredential> AuthenticateAsync(Uri tokenEndpoint, string username, string password)
        {
            username.AssertIsNotNullOrEmpty("username", "Cannot authenticate with a null or empty username.");
            password.AssertIsNotNullOrEmpty("password", "Cannot authenticate with a null or empty password.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.AuthenticateAsync(tokenEndpoint, username, password);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to authenticate. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IAuthenticationPayloadConverter>();
            var token = converter.ConvertAccessToken(await resp.ReadContentAsStringAsync());
            ((CloudFoundryPasswordCredential)this.Credential).AccessToken = token;
            
            return this.Credential;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationsAsync()
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetOrganizationsAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get organizations. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IOrganizationPayloadConverter>();
            var orgs = converter.ConvertOrganizations(await resp.ReadContentAsStringAsync());

            return orgs;
        }

        /// <inheritdoc/>
        public async Task<Organization> GetOrganizationAsync(string organizationId)
        {
            organizationId.AssertIsNotNullOrEmpty("organizationId","Cannot get an organization with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetOrganizationAsync(organizationId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get organization. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IOrganizationPayloadConverter>();
            var orgs = converter.ConvertOrganization(await resp.ReadContentAsStringAsync());

            return orgs;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Space>> GetSpacesAsync()
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetSpacesAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get spaces. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<ISpacePayloadConverter>();
            var spaces = converter.ConvertSpaces(await resp.ReadContentAsStringAsync());

            return spaces;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Space>> GetSpacesAsync(string organizationId)
        {
            organizationId.AssertIsNotNullOrEmpty("organizationId", "Cannot get spaces for an organization with a with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetSpacesAsync(organizationId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get spaces. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<ISpacePayloadConverter>();
            var spaces = converter.ConvertSpaces(await resp.ReadContentAsStringAsync());

            return spaces;
        }

        /// <inheritdoc/>
        public async Task<Space> GetSpaceAsync(string spaceId)
        {
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot get a space with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetSpaceAsync(spaceId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get space. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<ISpacePayloadConverter>();
            var spc = converter.ConvertSpace(await resp.ReadContentAsStringAsync());

            return spc;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetUsersAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get users. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IUserPayloadConverter>();
            var users = converter.ConvertUsers(await resp.ReadContentAsStringAsync());

            return users;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync(string organizationId)
        {
            organizationId.AssertIsNotNullOrEmpty("organizationId", "Cannot get users for an organization with a with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetUsersAsync(organizationId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get users. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IUserPayloadConverter>();
            var users = converter.ConvertUsers(await resp.ReadContentAsStringAsync());

            return users;
        }

        /// <inheritdoc/>
        public async Task<User> GetUserAsync(string userId)
        {
            userId.AssertIsNotNullOrEmpty("userId", "Cannot get a user with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetUserAsync(userId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get user. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IUserPayloadConverter>();
            var usr = converter.ConvertUser(await resp.ReadContentAsStringAsync());

            return usr;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetApplicationsAsync()
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetApplicationsAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get applications. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IApplicationPayloadConverter>();
            var apps = converter.ConvertApplications(await resp.ReadContentAsStringAsync());

            return apps;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetApplicationsAsync(string spaceId)
        {
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot get applications for a space with a with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetApplicationsAsync(spaceId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get applications. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IApplicationPayloadConverter>();
            var apps = converter.ConvertApplications(await resp.ReadContentAsStringAsync());

            return apps;
        }

        /// <inheritdoc/>
        public async Task<Application> GetApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get an application with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetApplicationAsync(appId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get application. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IApplicationPayloadConverter>();
            var app = converter.ConvertApplication(await resp.ReadContentAsStringAsync());

            return app;
        }

        /// <inheritdoc/>
        public async Task<Application> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota)
        {
            name.AssertIsNotNullOrEmpty("name", "Cannot create an application with a null or empty name.");
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot create an application with a null or empty space id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.CreateApplicationAsync(name, spaceId, memoryLimit, instances, diskQuota);

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to create an application. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IApplicationPayloadConverter>();
            var app = converter.ConvertApplication(await resp.ReadContentAsStringAsync());

            return app;
        }

        /// <inheritdoc/>
        public async Task<Job> UploadApplicationPackageAsync(string applicationId, Stream package)
        {
            applicationId.AssertIsNotNullOrEmpty("applicationId", "Cannot upload a package to an application with a null or empty id.");
            package.AssertIsNotNull("package", "Cannot upload a null application package.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.UploadApplicationPackageAsync(applicationId, package);

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to upload an application package. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IJobPayloadConverter>();
            var job = converter.Convert(await resp.ReadContentAsStringAsync());

            return job;
        }

        /// <inheritdoc/>
        public async Task<Application> StartApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot start an application with a null or empty Id.");

            return await this.UpdateApplication(appId, new Dictionary<string, object>() {{"state", "STARTED"}});
        }

        /// <inheritdoc/>
        public async Task<Application> StopApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot stop an application with a null or empty Id.");

            return await this.UpdateApplication(appId, new Dictionary<string, object>() {{"state", "STOPPED"}});
        }

        /// <inheritdoc/>
        public async Task<Application> ScaleApplicationAsync(string appId, int instances)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot scale an application with a null or empty Id.");
            if (instances < 0)
            {
                throw new ArgumentException(
                    "Cannot scale an application with a number of instances that is less then zero.", "instances");
            }

            return await this.UpdateApplication(appId, new Dictionary<string, object>() { { "instances", instances } });
        }

        /// <summary>
        /// Updates an application on the Remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id for the application.</param>
        /// <param name="properties">A collection of properties to update.</param>
        /// <returns>The resulting Application.</returns>
        internal async Task<Application> UpdateApplication(string appId, Dictionary<string, object> properties)
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.UpdateApplicationAsync(appId, properties);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to update application. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IApplicationPayloadConverter>();
            var app = converter.ConvertApplication(await resp.ReadContentAsStringAsync());

            return app;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Route>> GetRoutesAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get routes with an application id that is null or empty.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetRoutesAsync(appId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get routes. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IRoutePayloadConverter>();
            var routes = converter.ConvertRoutes(await resp.ReadContentAsStringAsync());

            return routes;
        }

        /// <inheritdoc/>
        public async Task<Route> CreateRouteAsync(string hostName, string domainId, string spaceId)
        {
            hostName.AssertIsNotNullOrEmpty("hostName", "Cannot create a route with a null or empty name.");
            domainId.AssertIsNotNullOrEmpty("domainId", "Cannot create a route with a null or empty domain id.");
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot create a route with a null or empty space id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.CreateRouteAsync(hostName, domainId, spaceId);

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to create a route. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IRoutePayloadConverter>();
            var route = converter.ConvertRoute(await resp.ReadContentAsStringAsync());

            return route;
        }

        /// <inheritdoc/>
        public async Task<Route> MapRouteAsync(string routeId, string applicationId)
        {
            routeId.AssertIsNotNullOrEmpty("routeId", "Cannot map a route to an application with a null or empty route id.");
            applicationId.AssertIsNotNullOrEmpty("applicationId", "Cannot map a route to an application with a null or empty application id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.MapRouteAsync(routeId, applicationId);

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to map a route. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IRoutePayloadConverter>();
            var route = converter.ConvertRoute(await resp.ReadContentAsStringAsync());

            return route;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Instance>> GetInstancesAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get instances with an application id that is null or empty.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetInstancesAsync(appId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get instances. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IInstancePayloadConverter>();
            var routes = converter.ConvertInstances(await resp.ReadContentAsStringAsync());

            return routes;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Domain>> GetDomainsAsync()
        {
            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetDomainsAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get domains. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IDomainPayloadConverter>();
            var domains = converter.ConvertDomains(await resp.ReadContentAsStringAsync());

            return domains;
        }

        /// <inheritdoc/>
        public async Task<Job> GetJobAsync(string jobId)
        {
            jobId.AssertIsNotNullOrEmpty("jobId", "Cannot get a job with a null or empty Id.");

            var client = this.ServiceLocator.Locate<ICloudFoundryRestClientFactory>().Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var resp = await client.GetJobAsync(jobId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get job. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IJobPayloadConverter>();
            var spc = converter.Convert(await resp.ReadContentAsStringAsync());

            return spc;
        }
    }
}
