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
using CloudFoundry.Common.Http;

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Client for making REST requests to a remote CloudFoundry instance.
    /// </summary>
    public interface ICloudFoundryRestClient
    {
        /// <summary>
        /// Gets information about the remote Cloud Foundry instance.
        /// </summary>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetInstanceInfoAsync();

        /// <summary>
        /// Authenticates the given user against the remote CloudFoundry instance.
        /// </summary>
        /// <param name="tokenEndpoint">An endpoint that can issue tokens.</param>
        /// <param name="username">The name of the user to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> AuthenticateAsync(Uri tokenEndpoint, string username, string password);

        /// <summary>
        /// Gets a list of all organizations on the remote Cloud Foundry instance.
        /// </summary>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetOrganizationsAsync();

        /// <summary>
        /// Gets an organization on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="orgId">The id for the organization.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetOrganizationAsync(string orgId);

        /// <summary>
        /// Gets a list of all spaces on the remote Cloud Foundry instance.
        /// </summary>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetSpacesAsync();

        /// <summary>
        /// Gets a list of all Spaces in a given organization on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="orgId">The id for the organization.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetSpacesAsync(string orgId);

        /// <summary>
        /// Gets a Space on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="spaceId">The id for the space.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetSpaceAsync(string spaceId);

        /// <summary>
        /// Gets a list of all users on the remote Cloud Foundry instance.
        /// </summary>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetUsersAsync();

        /// <summary>
        /// Gets a list of all Users in a given organization on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="orgId">The id for the organization.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetUsersAsync(string orgId);

        /// <summary>
        /// Gets a User on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="userId">The id for the user.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetUserAsync(string userId);

        /// <summary>
        /// Gets a list of all apps on the remote Cloud Foundry instance.
        /// </summary>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetApplicationsAsync();

        /// <summary>
        /// Gets a list of all apps in a given space on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="spaceId">The id for the space.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetApplicationsAsync(string spaceId);

        /// <summary>
        /// Gets an app on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="appId">The id for the app.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetApplicationAsync(string appId);

        /// <summary>
        /// Creates a new application on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="spaceId">The Id of the associated space.</param>
        /// <param name="memoryLimit">The memory limit for the application.</param>
        /// <param name="instances">The number of instances for the application.</param>
        /// <param name="diskQuota">The size of the disk quota for the application.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota);

        /// <summary>
        /// Gets a list of Routes for an Application on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="appId">The id for the Application.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetRoutesAsync(string appId);

        /// <summary>
        /// Creates a new route on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="hostName">The host name for the route.</param>
        /// <param name="domainId">The Id of the target domain.</param>
        /// <param name="spaceId">The Id of the target space.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> CreateRouteAsync(string hostName, string domainId, string spaceId);

        /// <summary>
        /// Maps a route to an application on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="routeId">The id of the target route.</param>
        /// <param name="applicationId">The id of the target application.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> MapRouteAsync(string routeId, string applicationId);

        /// <summary>
        /// Gets a list of Instances for an Application on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="appId">The id for the Application.</param>` 
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetInstancesAsync(string appId);

        /// <summary>
        /// Updates an application on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="appId">The id for the Application.</param>
        /// <param name="properties">The properties and values to update.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> UpdateApplicationAsync(string appId, IDictionary<string, object> properties );

        /// <summary>
        /// Uploads an application package to the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="appId">The id for the target application.</param>
        /// <param name="package">The binary application package.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> UploadApplicationPackageAsync(string appId, Stream package);

        /// <summary>
        /// Gets a list of domains from the remote Cloud Foundry instance.
        /// </summary>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetDomainsAsync();

        /// <summary>
        /// Gets the details of a job on the remote Cloud Foundry instance.
        /// </summary>
        /// <param name="jobId">The id for the target job.</param>
        /// <returns>An Http response.</returns>
        Task<IHttpResponseAbstraction> GetJobAsync(string jobId);
    }
}
