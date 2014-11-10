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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Client that can be used to connect and interact with a remote Cloud Foundry service.
    /// </summary>
    public interface ICloudFoundryClient
    {
        /// <summary>
        /// Gets information about a remote Cloud Foundry instance.
        /// </summary>
        /// <returns>A IntanceInfo object.</returns>
        Task<InstanceInfo> GetInstanceInfoAsync();

        /// <summary>
        /// Connects and authenticates the client with a remote Cloud Foundry service.
        /// </summary>
        /// <returns>A credential that includes the current access token.</returns>
        Task<IPasswordAuthCredential> ConnectAsync();

        /// <summary>
        /// Gets a list of organizations from the remote Cloud Foundry service.
        /// </summary>
        /// <returns>An enumerable list of Organizations.</returns>
        Task<IEnumerable<Organization>> GetOrganizationsAsync();

        /// <summary>
        /// Get an Organization from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="organizationId">The Id of the organization.</param>
        /// <returns>The Organization object.</returns>
        Task<Organization> GetOrganizationAsync(string organizationId);

        /// <summary>
        /// Gets a list of spaces from the remote Cloud Foundry service.
        /// </summary>
        /// <returns>An enumerable list of Spaces.</returns>
        Task<IEnumerable<Space>> GetSpacesAsync();

        /// <summary>
        /// Gets a list of spaces for a given organization from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="organizationId">The Id of the organization.</param>
        /// <returns>An enumerable list of Spaces.</returns>
        Task<IEnumerable<Space>> GetSpacesAsync(string organizationId);

        /// <summary>
        /// Gets a Space from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="spaceId">The Id of the Space.</param>
        /// <returns>The Space object.</returns>
        Task<Space> GetSpaceAsync(string spaceId);

        /// <summary>
        /// Gets a list of users from the remote Cloud Foundry service.
        /// </summary>
        /// <returns>An enumerable list of Users.</returns>
        Task<IEnumerable<User>> GetUsersAsync();

        /// <summary>
        /// Gets a list of users for a given organization from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="organizationId">The Id of the organization.</param>
        /// <returns>An enumerable list of Users.</returns>
        Task<IEnumerable<User>> GetUsersAsync(string organizationId);

        /// <summary>
        /// Gets a User from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="userId">The Id of the User.</param>
        /// <returns>The User object.</returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Gets a list of applications from the remote Cloud Foundry service.
        /// </summary>
        /// <returns>An enumerable list of Applications.</returns>
        Task<IEnumerable<Application>> GetApplicationsAsync();

        /// <summary>
        /// Gets a list of applications for a given space from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="spaceId">The Id of the space.</param>
        /// <returns>An enumerable list of Applications.</returns>
        Task<IEnumerable<Application>> GetApplicationsAsync(string spaceId);

        /// <summary>
        /// Gets an Application from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The Application object.</returns>
        Task<Application> GetApplicationAsync(string appId);

        /// <summary>
        /// Creates an Application on the remote Cloud Foundry service.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="spaceId">The id of the associated space.</param>
        /// <param name="memoryLimit">The memory limit for the application.</param>
        /// <param name="instances">The number of instances to create.</param>
        /// <param name="diskQuota">The disk quota for the application.</param>
        /// <returns></returns>
        Task<Application> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota);

        /// <summary>
        /// Uploads an application package to the remote CloudFoundry service.
        /// </summary>
        /// <param name="applicationId">The id of the application to upload to.</param>
        /// <param name="package">The package to upload.</param>
        /// <returns>The resulting Job.</returns>
        Task<Job> UploadApplicationPackageAsync(string applicationId, Stream package);

        /// <summary>
        /// Starts an Application on the remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The Application object.</returns>
        Task<Application> StartApplicationAsync(string appId);

        /// <summary>
        /// Stops an Application on the remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The Application object.</returns>
        Task<Application> StopApplicationAsync(string appId);

        /// <summary>
        /// Scales an Application on the remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <param name="instances">The number of instances to scale to.</param>
        /// <returns>The Application object.</returns>
        Task<Application> ScaleApplicationAsync(string appId, int instances);

        /// <summary>
        /// Gets a list of routes for an Application from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>A list of Route objects.</returns>
        Task<IEnumerable<Route>> GetRoutesAsync(string appId);

        /// <summary>
        /// Creates a route on the remote CloudFoundry service. 
        /// </summary>
        /// <param name="hostName">The host name for the route.</param>
        /// <param name="domainId">The id of the target domain.</param>
        /// <param name="spaceId">The id of the target space.</param>
        /// <returns>The resulting Route.</returns>
        Task<Route> CreateRouteAsync(string hostName, string domainId, string spaceId);

        /// <summary>
        /// Maps a route to an application on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="routeId">The id of the target route.</param>
        /// <param name="applicationId">The id of the target application.</param>
        /// <returns>The mapped Route.</returns>
        Task<Route> MapRouteAsync(string routeId, string applicationId);

        /// <summary>
        /// Gets a list of instances for an Application from the remote Cloud Foundry service.
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>A list of Instance objects.</returns>
        Task<IEnumerable<Instance>> GetInstancesAsync(string appId);

        /// <summary>
        /// Gets a list of domains from the remote Cloud Foundry service.
        /// </summary>
        /// <returns>A list of Domain objects.</returns>
        Task<IEnumerable<Domain>> GetDomainsAsync();

        /// <summary>
        /// Gets the details of a job from the remote Cloud Foundry service.
        /// </summary>
        /// <returns>A Job object.</returns>
        Task<Job> GetJobAsync(string jobId);
    }
}
