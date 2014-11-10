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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;
using CloudFoundry.Common.Http;
using CloudFoundry.Common.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    internal class CloudFoundryRestClient : ICloudFoundryRestClient
    {
        /// <summary>
        /// A service locator that can be used to locate dependent services.
        /// </summary>
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// A credential to use when making requests.
        /// </summary>
        internal IPasswordAuthCredential Credential;

        /// <summary>
        /// A cancellation token to use when making requests.
        /// </summary>
        internal CancellationToken CancellationToken;

        /// <summary>
        /// Creates a new instance of the CloudFoundryRestClient class.
        /// </summary>
        /// <param name="serviceLocator">A service locator that can be used to locate dependent services.</param>
        /// <param name="credential">A credential to use when making requests.</param>
        /// <param name="cancellationToken">A cancellation token to use when making requests.</param>
        public CloudFoundryRestClient(IServiceLocator serviceLocator, IPasswordAuthCredential credential, CancellationToken cancellationToken)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a rest client with a null service locator.");
            credential.AssertIsNotNull("credential","Cannot create a rest client with a null credential.");
            
            this.ServiceLocator = serviceLocator;
            this.Credential = credential;
            this.CancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetInstanceInfoAsync()
        {
            var infoEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/info";
            var client = this.GetHttpClient();
            client.Uri = new Uri(infoEndpoint);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> AuthenticateAsync(Uri tokenEndpoint, string username, string password)
        {
            username.AssertIsNotNullOrEmpty("username","Cannot authenticate with a null or empty username.");
            password.AssertIsNotNullOrEmpty("password", "Cannot authenticate with a null or empty password.");

            var parameters = new Dictionary<string, string>();
            parameters["username"] = username;
            parameters["password"] = password;
            parameters["grant_type"] = "password";

            var queryString = BuildQueryString(parameters);

            var authUri = tokenEndpoint.AbsoluteUri.TrimEnd('/') + "/oauth/token";
            var requestUri = new UriBuilder(authUri);
            requestUri.Query = queryString;

            var client = this.GetHttpClient();
            client.Uri = requestUri.Uri;
            client.Headers.Add("Authorization","Basic Y2Y6");
            client.Method = HttpMethod.Post;

            return await client.SendAsync();
        }

        #region Organization Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetOrganizationsAsync()
        {
            var orgEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/organizations";
            var client = this.GetHttpClient();
            client.Uri = new Uri(orgEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetOrganizationAsync(string orgId)
        {
            var orgEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/organizations/" +orgId;
            var client = this.GetHttpClient();
            client.Uri = new Uri(orgEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        #region Space Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetSpacesAsync()
        {
            var spaceEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/spaces";
            var client = this.GetHttpClient();
            client.Uri = new Uri(spaceEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetSpacesAsync(string orgId)
        {
            orgId.AssertIsNotNullOrEmpty("orgId","Cannot get spaces for an organization with a null or empty id.");

            var orgEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/organizations/" +orgId +"/spaces";
            var client = this.GetHttpClient();
            client.Uri = new Uri(orgEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetSpaceAsync(string spaceId)
        {
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot get a space with a null or empty id.");

            var spaceEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/spaces/" + spaceId;
            var client = this.GetHttpClient();
            client.Uri = new Uri(spaceEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        #region User Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetUsersAsync()
        {
            var userEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/users";
            var client = this.GetHttpClient();
            client.Uri = new Uri(userEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetUsersAsync(string orgId)
        {
            orgId.AssertIsNotNullOrEmpty("orgId", "Cannot get users for an organization with a null or empty id.");

            var orgEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/organizations/" + orgId + "/users";
            var client = this.GetHttpClient();
            client.Uri = new Uri(orgEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetUserAsync(string userId)
        {
            userId.AssertIsNotNullOrEmpty("userId", "Cannot get a user with a null or empty id.");

            var userEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/users/" + userId;
            var client = this.GetHttpClient();
            client.Uri = new Uri(userEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        #region App Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetApplicationsAsync()
        {
            var appEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps";
            var client = this.GetHttpClient();
            client.Uri = new Uri(appEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetApplicationsAsync(string spaceId)
        {
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot get apps for a space with a null or empty id.");

            var appEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/spaces/" + spaceId + "/apps";
            var client = this.GetHttpClient();
            client.Uri = new Uri(appEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetApplicationAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get an app with a null or empty id.");

            var appsEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps/" + appId;
            var client = this.GetHttpClient();
            client.Uri = new Uri(appsEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota)
        {
            name.AssertIsNotNullOrEmpty("name", "Cannot create an application with a null or empty name.");
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot create an application with a null or empty space id.");

            dynamic body = new System.Dynamic.ExpandoObject();
            body.name = name;
            body.memory = memoryLimit;
            body.instances = instances;
            body.disk_quota = diskQuota;
            body.space_guid = spaceId;
            string requestBody = JToken.FromObject(body).ToString();

            var appEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps";
            var client = this.GetHttpClient();
            client.Uri = new Uri(appEndpoint);
            client.Method = HttpMethod.Post;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));
            client.Content = requestBody.ConvertToStream();

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> UpdateApplicationAsync(string appId, IDictionary<string, object> properties )
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get an app with a null or empty id.");
            properties.AssertIsNotNull("properties", "Cannot update an app with a null collection of properties.");

            if (properties.Count == 0)
            {
                throw new ArgumentException("Cannot update an app with an empty collection of properties.");
            }

            var appsEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps/" + appId;
            var client = this.GetHttpClient();
            client.Uri = new Uri(appsEndpoint);
            client.Method = HttpMethod.Put;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));
            client.Content = BuildApplicationUpdateBody(properties).ConvertToStream();
            
            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> UploadApplicationPackageAsync(string appId, Stream package)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get an app with a null or empty id.");
            package.AssertIsNotNull("package", "Cannot upload a null or empty application package.");

            var parameters = new Dictionary<string, string>();
            parameters["async"] = "true";
            var queryString = BuildQueryString(parameters);

            var bitsEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps/" + appId +"/bits";
            var requestUri = new UriBuilder(bitsEndpoint);
            requestUri.Query = queryString;

            var client = this.GetHttpClient();
            client.Uri = requestUri.Uri;
            client.Method = HttpMethod.Put;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            var uploadContent = BuildApplicationPackageBody(package);

            return await client.SendAsync(uploadContent);
        }

        #endregion

        #region Domain Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetDomainsAsync()
        {
            var domainsEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/shared_domains";
            var client = this.GetHttpClient();
            client.Uri = new Uri(domainsEndpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        #region Job Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetJobAsync(string jobId)
        {
            jobId.AssertIsNotNullOrEmpty("jobId", "Cannot get a job with a null or empty id.");

            var jobEnpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/jobs/" + jobId;
            var client = this.GetHttpClient();
            client.Uri = new Uri(jobEnpoint);
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        #region Route Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetRoutesAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get routes for an application with a null or empty id.");

            var parameters = new Dictionary<string, string>();
            parameters["inline-relations-depth"] = "1";
            var queryString = BuildQueryString(parameters);

            var routesEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps/" + appId + "/routes";
            var requestUri = new UriBuilder(routesEndpoint);
            requestUri.Query = queryString;

            var client = this.GetHttpClient();
            client.Uri = requestUri.Uri;
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateRouteAsync(string hostName, string domainId, string spaceId)
        {
            hostName.AssertIsNotNullOrEmpty("hostName", "Cannot create a route with a null or empty host name.");
            spaceId.AssertIsNotNullOrEmpty("spaceId", "Cannot create a route with a null or empty space id.");
            domainId.AssertIsNotNullOrEmpty("domainId", "Cannot create a route with a null or empty domain id.");

            dynamic body = new System.Dynamic.ExpandoObject();
            body.host = hostName;
            body.domain_guid = domainId;
            body.space_guid = spaceId;
            string requestBody = JToken.FromObject(body).ToString();

            var parameters = new Dictionary<string, string>();
            parameters["inline-relations-depth"] = "1";
            var queryString = BuildQueryString(parameters);

            var routesEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/routes";
            var requestUri = new UriBuilder(routesEndpoint);
            requestUri.Query = queryString;

            var client = this.GetHttpClient();
            client.Uri = requestUri.Uri;
            client.Method = HttpMethod.Post;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));
            client.Content = requestBody.ConvertToStream();

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> MapRouteAsync(string routeId, string applicationId)
        {
            routeId.AssertIsNotNullOrEmpty("routeId", "Cannot map a route to an application with a null or empty  route id.");
            applicationId.AssertIsNotNullOrEmpty("applicationId", "Cannot map a route to an application with a null or empty  application id.");

            var parameters = new Dictionary<string, string>();
            parameters["inline-relations-depth"] = "1";
            var queryString = BuildQueryString(parameters);

            var routesEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/routes/" + routeId + "/apps/" + applicationId;
            var requestUri = new UriBuilder(routesEndpoint);
            requestUri.Query = queryString;

            var client = this.GetHttpClient();
            client.Uri = requestUri.Uri;
            client.Method = HttpMethod.Put;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetInstancesAsync(string appId)
        {
            appId.AssertIsNotNullOrEmpty("appId", "Cannot get instances for an application with a null or empty id.");

            var statsEndpoint = this.Credential.ServerEndpoint.AbsoluteUri.TrimEnd('/') + "/v2/apps/" + appId + "/stats";
            var requestUri = new Uri(statsEndpoint);
            
            var client = this.GetHttpClient();
            client.Uri = requestUri;
            client.Method = HttpMethod.Get;
            client.Headers.Add(BuildAuthenticationHeader(this.Credential.AccessToken));

            return await client.SendAsync();
        }

        #endregion

        /// <summary>
        /// Gets an http client to use when making requests.
        /// </summary>
        /// <returns>An client capable of making http requests.</returns>
        internal IHttpAbstractionClient GetHttpClient()
        {
            return this.ServiceLocator.Locate<IHttpAbstractionClientFactory>().Create(this.CancellationToken);
        }

        /// <summary>
        /// Builds a properly formatted authentication header.
        /// </summary>
        /// <param name="accessToken">The access token to be used in the header.</param>
        /// <returns>A key value pair representing the header key and its value.</returns>
        internal KeyValuePair<string,string> BuildAuthenticationHeader(string accessToken)
        {
            return new KeyValuePair<string, string>("Authorization", "bearer " + accessToken);
        }

        /// <summary>
        /// Builds a Uri query string. 
        /// </summary>
        /// <param name="parameters">A collection of key value pairs.</param>
        /// <returns>A Uri query string.</returns>
        internal string BuildQueryString(IDictionary<string, string> parameters)
        {
            var queryArray =
                (from item in parameters
                    select string.Format("{0}={1}", Uri.EscapeUriString(item.Key), Uri.EscapeUriString(item.Value)))
                    .ToArray();
            return string.Join("&", queryArray);
        }

        /// <summary>
        /// Builds a request body for updating an application.
        /// </summary>
        /// <param name="properties">The list of properties to update.</param>
        /// <returns>A Json request body.</returns>
        internal string BuildApplicationUpdateBody(IDictionary<string, object> properties)
        {
            return JsonConvert.SerializeObject(properties, Formatting.None);
        }

        internal IEnumerable<IHttpMultiPartFormDataAbstraction> BuildApplicationPackageBody(Stream package)
        {
            var parts = new List<IHttpMultiPartFormDataAbstraction>();
            parts.Add(new HttpMultiPartFormDataAbstraction("async", string.Empty, string.Empty, "true".ConvertToStream()));
            parts.Add(new HttpMultiPartFormDataAbstraction("resources", string.Empty, string.Empty, "[]".ConvertToStream()));
            parts.Add(new HttpMultiPartFormDataAbstraction("application", "application.zip", "application/zip", package));
            return parts;
        }
    }
}
