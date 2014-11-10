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
using System.Threading;
using System.Threading.Tasks;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using CloudFoundry.Common.ServiceLocation;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    public class CloudFoundryClient : ICloudFoundryClient
    {
        /// <summary>
        /// Credentials for authentication.
        /// </summary>
        internal IPasswordAuthCredential Credential;

        /// <summary>
        /// Service locator used to locate dependent services.
        /// </summary>
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// CancellationToken for requests made by the client.
        /// </summary>
        internal CancellationToken CancellationToken;

        /// <summary>
        /// Creates a new instance of the CloudFoundryClient class.
        /// </summary>
        /// <param name="serviceLocator">Service locator used to locate dependent services.</param>
        /// <param name="credential">Credential to be used for authenticating the client.</param>
        /// <param name="cancellationToken">A cancellationToken for requests made by the client.</param>
        internal CloudFoundryClient(IServiceLocator serviceLocator, IPasswordAuthCredential credential, CancellationToken cancellationToken)
        {
            credential.AssertIsNotNull("credential", "Cannot create a Cloud Foundry client with a null credential.");
            serviceLocator.AssertIsNotNull("serviceLocator","Cannot create a Cloud Foundry client with a null service locator.");

            this.ServiceLocator = serviceLocator;
            this.Credential = credential;
            this.CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates a new instance of the CloudFoundryClient class.
        /// </summary>
        /// <param name="credential">Credential to be used for authenticating the client.</param>
        internal CloudFoundryClient(IPasswordAuthCredential credential) : this(new ServiceLocator(), credential, CancellationToken.None)
        {
        }

        /// <summary>
        /// Creates a new instance of the CloudFoundryClient class.
        /// </summary>
        /// <param name="credential">Credential to be used for authenticating the client.</param>
        /// <param name="cancellationToken">A cancellation token for requests made by the client.</param>
        internal CloudFoundryClient(IPasswordAuthCredential credential, CancellationToken cancellationToken)
            : this(new ServiceLocator(), credential, cancellationToken)
        {
        }

        /// <summary>
        /// Creates a Cloud Foundry client.
        /// </summary>
        /// <param name="credential">Credential used for authenticating the client.</param>
        /// <returns>A Cloud Foundry client.</returns>
        public static CloudFoundryClient Create(IPasswordAuthCredential credential)
        {
            return new CloudFoundryClient(credential);
        }

        /// <summary>
        /// Creates a Cloud Foundry client.
        /// </summary>
        /// <param name="credential">Credential used for authenticating the client.</param>
        /// <param name="cancellationToken">A cancellation token for requests made by the client.</param>
        /// <returns>A Cloud Foundry client.</returns>
        public static CloudFoundryClient Create(IPasswordAuthCredential credential, CancellationToken cancellationToken)
        {
            return new CloudFoundryClient(credential, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<InstanceInfo> GetInstanceInfoAsync()
        {
            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetInstanceInfoAsync();
        }

        /// <inheritdoc/>
        public async Task<IPasswordAuthCredential> ConnectAsync()
        {
            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            var info = await pocoClient.GetInstanceInfoAsync();
            var cred = await pocoClient.AuthenticateAsync(info.TokenEndpoint, this.Credential.Username, this.Credential.Password);
            this.Credential = cred;

            return this.Credential;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationsAsync()
        {
            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetOrganizationsAsync();
        }

        /// <inheritdoc/>
        public async Task<Organization> GetOrganizationAsync(string organizationId)
        {
            organizationId.AssertIsNotNullOrEmpty("organizationId","Cannot get an organization with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetOrganizationAsync(organizationId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Space>> GetSpacesAsync()
        {
            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetSpacesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Space>> GetSpacesAsync(string organizationId)
        {
            organizationId.AssertIsNotNullOrEmpty("organizationId", "Cannot get spaces for an organization with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetSpacesAsync(organizationId);
        }

        /// <inheritdoc/>
        public async Task<Space> GetSpaceAsync(string spaceId)
        {
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot get a space with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetSpaceAsync(spaceId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetUsersAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync(string organizationId)
        {
            organizationId.AssertIsNotNullOrEmpty("organizationId", "Cannot get users for an organization with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetUsersAsync(organizationId);
        }

        /// <inheritdoc/>
        public async Task<User> GetUserAsync(string userId)
        {
            userId.AssertIsNotNullOrEmpty("userId", "Cannot get a user with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetUserAsync(userId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetApplicationsAsync()
        {
            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetApplicationsAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetApplicationsAsync(string spaceId)
        {
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot get applications for a space with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetApplicationsAsync(spaceId);
        }

        /// <inheritdoc/>
        public async Task<Application> GetApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get an application with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetApplicationAsync(appId);
        }

        /// <inheritdoc/>
        public async Task<Application> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota)
        {
            name.AssertIsNotNullOrEmpty("name", "Cannot create an application with a null or empty name.");
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot create an application with a null or empty space id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.CreateApplicationAsync(name, spaceId, memoryLimit, instances, diskQuota);
        }

        /// <inheritdoc/>
        public async Task<Job> UploadApplicationPackageAsync(string applicationId, Stream package)
        {
            applicationId.AssertIsNotNullOrEmpty("applicationId", "Cannot upload a package to an application with a null or empty Id.");
            package.AssertIsNotNull("package", "Cannot upload a null application package.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.UploadApplicationPackageAsync(applicationId, package);
        }

        /// <inheritdoc/>
        public async Task<Application> StartApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot start an application with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.StartApplicationAsync(appId);
        }

        /// <inheritdoc/>
        public async Task<Application> StopApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot stop an application with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.StopApplicationAsync(appId);
        }

        /// <inheritdoc/>
        public async Task<Application> ScaleApplicationAsync(string appId, int instances)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot scale an application with a null or empty Id.");
            if (instances < 0)
            {
                throw new ArgumentException("Cannot scale an application to a negative number of instances.","instances");
            }

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.ScaleApplicationAsync(appId, instances);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Route>> GetRoutesAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get routes for an application with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetRoutesAsync(appId);
        }

        /// <inheritdoc/>
        public async Task<Route> CreateRouteAsync(string hostName, string domainId, string spaceId)
        {
            hostName.AssertIsNotNullOrEmpty("hostName", "Cannot create a route with a null or empty host name.");
            domainId.AssertIsNotNullOrEmpty("domainId", "Cannot create a route with a null or empty domain id.");
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot create a route with a null or empty space id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.CreateRouteAsync(hostName, domainId, spaceId);
        }

        /// <inheritdoc/>
        public async Task<Route> MapRouteAsync(string routeId, string applicationId)
        {
            routeId.AssertIsNotNullOrEmpty("routeId", "Cannot map a route with a null or empty route id.");
            applicationId.AssertIsNotNullOrEmpty("applicationId", "Cannot map a route with a null or empty application id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.MapRouteAsync(routeId, applicationId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Instance>> GetInstancesAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get instances for an application with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetInstancesAsync(appId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Domain>> GetDomainsAsync()
        {
           var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetDomainsAsync();
        }

        /// <inheritdoc/>
        public async Task<Job> GetJobAsync(string jobId)
        {
            jobId.AssertIsNotNullOrEmpty("jobId", "Cannot get job details for a job with a null or empty Id.");

            var pocoClient = this.ServiceLocator.Locate<ICloudFoundryPocoClientFactory>()
                .Create(this.ServiceLocator, this.Credential, this.CancellationToken);

            return await pocoClient.GetJobAsync(jobId);
        }
    }
}
