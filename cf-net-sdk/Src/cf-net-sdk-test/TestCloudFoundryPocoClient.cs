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
using cf_net_sdk;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common.ServiceLocation;

namespace CloudFoundry.Test
{
    internal class TestCloudFoundryPocoClient : ICloudFoundryPocoClient
    {
        internal IPasswordAuthCredential Credential { get; set; }

        public Func<Uri, string, string, Task<IPasswordAuthCredential>> Authenticate { get; set; }

        public Func<Task<IEnumerable<Organization>>> GetOrganizations { get; set; }

        public Func<string, Task<Organization>> GetOrganization { get; set; }

        public Func<Task<IEnumerable<Space>>> GetSpaces { get; set; }

        public Func<string, Task<IEnumerable<Space>>> GetSpacesWithId { get; set; }

        public Func<string, Task<Space>> GetSpace { get; set; }

        public Func<Task<InstanceInfo>> GetInstanceInfo { get; set; }

        public Func<Task<IEnumerable<User>>> GetUsers { get; set; }

        public Func<string, Task<IEnumerable<User>>> GetUsersWithId { get; set; }

        public Func<string, Task<User>> GetUser { get; set; }

        public Func<Task<IEnumerable<Application>>> GetApplications { get; set; }

        public Func<string, Task<IEnumerable<Application>>> GetApplicationsWithId { get; set; }

        public Func<string, Task<Application>> GetApplication { get; set; }

        public Func<string, string, int, int, int, Task<Application>> CreateApplication { get; set; }

        public Func<string, Stream, Task<Job>> UploadApplicationPackage { get; set; }

        public Func<string, Task<Application>> StartApplication { get; set; }

        public Func<string, Task<Application>> StopApplication { get; set; }

        public Func<string, int, Task<Application>> ScaleApplication { get; set; }

        public Func<string, Task<IEnumerable<Route>>> GetRoutesWithId { get; set; }

        public Func<string, string, string, Task<Route>> CreateRoute { get; set; }

        public Func<string, string, Task<Route>> MapRoute { get; set; }

        public Func<string, Task<IEnumerable<Instance>>> GetInstancesWithId { get; set; }

        public Func<Task<IEnumerable<Domain>>> GetDomains { get; set; }

        public Func<string, Task<Job>> GetJob { get; set; }

        public async Task<InstanceInfo> GetInstanceInfoAsync()
        {
            return await this.GetInstanceInfo();
        }

        public async Task<IPasswordAuthCredential> AuthenticateAsync(Uri tokenEndpoint, string username, string password)
        {
            return await this.Authenticate(tokenEndpoint, username, password);
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsAsync()
        {
            return await this.GetOrganizations();
        }

        public async Task<Organization> GetOrganizationAsync(string organizationId)
        {
            return await this.GetOrganization(organizationId);
        }

        public async Task<IEnumerable<Space>> GetSpacesAsync()
        {
            return await this.GetSpaces();
        }

        public async Task<IEnumerable<Space>> GetSpacesAsync(string organizationId)
        {
            return await this.GetSpacesWithId(organizationId);
        }

        public async Task<Space> GetSpaceAsync(string spaceId)
        {
            return await this.GetSpace(spaceId);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await this.GetUsers();
        }

        public async Task<IEnumerable<User>> GetUsersAsync(string organizationId)
        {
            return await this.GetUsersWithId(organizationId);
        }

        public async Task<User> GetUserAsync(string userId)
        {
            return await this.GetUser(userId);
        }

        public async Task<IEnumerable<Application>> GetApplicationsAsync()
        {
            return await this.GetApplications();
        }

        public async Task<IEnumerable<Application>> GetApplicationsAsync(string spaceId)
        {
            return await this.GetApplicationsWithId(spaceId);
        }

        public async Task<Application> GetApplicationAsync(string appId)
        {
            return await this.GetApplication(appId);
        }

        public async Task<Application> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota)
        {
            return await this.CreateApplication(name, spaceId, memoryLimit, instances, diskQuota);
        }

        public async Task<Job> UploadApplicationPackageAsync(string applicationId, Stream package)
        {
            return await this.UploadApplicationPackage(applicationId, package);
        }

        public async Task<Application> StartApplicationAsync(string appId)
        {
            return await this.StartApplication(appId);
        }

        public async Task<Application> StopApplicationAsync(string appId)
        {
            return await this.StopApplication(appId);
        }

        public async Task<Application> ScaleApplicationAsync(string appId, int instances)
        {
            return await this.ScaleApplication(appId, instances);
        }

        public async Task<IEnumerable<Route>> GetRoutesAsync(string appId)
        {
            return await this.GetRoutesWithId(appId);
        }

        public async Task<Route> CreateRouteAsync(string hostName, string domainId, string spaceId)
        {
            return await this.CreateRoute(hostName, domainId, spaceId);
        }

        public async Task<Route> MapRouteAsync(string routeId, string applicationId)
        {
            return await this.MapRoute(routeId, applicationId);
        }

        public async Task<IEnumerable<Instance>> GetInstancesAsync(string appId)
        {
            return await this.GetInstancesWithId(appId);
        }

        public async Task<IEnumerable<Domain>> GetDomainsAsync()
        {
            return await this.GetDomains();
        }

        public async Task<Job> GetJobAsync(string jobId)
        {
            return await this.GetJob(jobId);
        }
    }

    internal class TestCloudFoundryPocoClientFactory : ICloudFoundryPocoClientFactory
    {
        internal ICloudFoundryPocoClient Client;
        public TestCloudFoundryPocoClientFactory(ICloudFoundryPocoClient client)
        {
            this.Client = client;
        }

        public ICloudFoundryPocoClient Create(IServiceLocator serviceLocator, IPasswordAuthCredential credential,
            CancellationToken cancellationToken)
        {
            return this.Client;
        }
    }
}
