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
using System.Threading.Tasks;

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Client for making calls to a remote Cloud Foundry service.
    /// </summary>
    public interface ICloudFoundryPocoClient
    {
        /// <summary>
        /// Gets information about a remote Cloud Foundry Instance.
        /// </summary>
        /// <returns>The resulting info.</returns>
        Task<InstanceInfo> GetInstanceInfoAsync();

        /// <summary>
        /// Authenticates against a remote CloudFoundry service. 
        /// </summary>
        /// <param name="tokenEndpoint">An endpoint that can issue tokens.</param>
        /// <param name="username">The name of the user to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <returns>The resulting credential.</returns>
        Task<IPasswordAuthCredential> AuthenticateAsync(Uri tokenEndpoint, string username, string password);

        /// <summary>
        /// Gets a list of Organizations from the remote CloudFoundry service. 
        /// </summary>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<Organization>> GetOrganizationsAsync();

        /// <summary>
        /// Gets an Organization from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="organizationId">The Id of the Organization.</param>
        /// <returns>The resulting Organization.</returns>
        Task<Organization> GetOrganizationAsync(string organizationId);

        /// <summary>
        /// Gets a list of Spaces from the remote CloudFoundry service. 
        /// </summary>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<Space>> GetSpacesAsync();

        /// <summary>
        /// Gets a list of Spaces for a given organization from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="organizationId">The Id of the Organization.</param>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<Space>> GetSpacesAsync(string organizationId);

        /// <summary>
        /// Gets a Spaces from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="spaceId">The Id of the Space.</param>
        /// <returns>The resulting Space.</returns>
        Task<Space> GetSpaceAsync(string spaceId);

        /// <summary>
        /// Gets a list of Users from the remote CloudFoundry service. 
        /// </summary>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<User>> GetUsersAsync();

        /// <summary>
        /// Gets a list of Users for a given organization from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="organizationId">The Id of the Organization.</param>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<User>> GetUsersAsync(string organizationId);

        /// <summary>
        /// Gets a User from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="userId">The Id of the User.</param>
        /// <returns>The resulting User.</returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Gets a list of Applications from the remote CloudFoundry service. 
        /// </summary>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<Application>> GetApplicationsAsync();

        /// <summary>
        /// Gets a list of Applications for a given space from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="spaceId">The Id of the Space.</param>
        /// <returns>The resulting list.</returns>
        Task<IEnumerable<Application>> GetApplicationsAsync(string spaceId);

        /// <summary>
        /// Gets a Application from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The resulting Application.</returns>
        Task<Application> GetApplicationAsync(string appId);

        /// <summary>
        /// Creates an application on the remote CloudFoundry service.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="spaceId">The Id of the associated space.</param>
        /// <param name="memoryLimit">The memory limit for the application.</param>
        /// <param name="instances">The number of instances for the application.</param>
        /// <param name="diskQuota">The disk quota for the application.</param>
        /// <returns>The resulting Application.</returns>
        Task<Application> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota);

        /// <summary>
        /// Uploads an application package to the remote CloudFoundry service.
        /// </summary>
        /// <param name="applicationId">The id of the application to upload to.</param>
        /// <param name="package">The package to upload.</param>
        /// <returns>The resulting Job.</returns>
        Task<Job> UploadApplicationPackageAsync(string applicationId, Stream package);

        /// <summary>
        /// Starts an Application on the remote CloudFoundry service. 
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The resulting Application.</returns>
        Task<Application> StartApplicationAsync(string appId);

        /// <summary>
        /// Stops an Application on the remote CloudFoundry service. 
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The resulting Application.</returns>
        Task<Application> StopApplicationAsync(string appId);

        /// <summary>
        /// Updates the number of instances of an Application on the remote CloudFoundry service. 
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <param name="instances">The number of instances.</param>
        /// <returns>The resulting Application.</returns>
        Task<Application> ScaleApplicationAsync(string appId, int instances);

        /// <summary>
        /// Gets a list of Routes for an Application from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The resulting Routes.</returns>
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
        /// Gets a list of Instances for an Application from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="appId">The Id of the Application.</param>
        /// <returns>The resulting Instances.</returns>
        Task<IEnumerable<Instance>> GetInstancesAsync(string appId);

        /// <summary>
        /// Gets a list of Domains from the remote CloudFoundry service. 
        /// </summary>
        /// <returns>The resulting Domains.</returns>
        Task<IEnumerable<Domain>> GetDomainsAsync();

        /// <summary>
        /// Gets the details of a job from the remote CloudFoundry service. 
        /// </summary>
        /// <param name="jobId">The Id of the Job.</param>
        /// <returns>The resulting Job.</returns>
        Task<Job> GetJobAsync(string jobId);
    }
}
