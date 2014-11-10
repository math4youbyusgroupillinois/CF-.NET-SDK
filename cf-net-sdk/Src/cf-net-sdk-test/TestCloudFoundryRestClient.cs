// /* ============================================================================
// Copyright 2014 Hewlett Packard
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
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
using CloudFoundry.Common.Http;
using CloudFoundry.Common.ServiceLocation;

namespace CloudFoundry.Test
{
    public class TestCloudFoundryRestClient : ICloudFoundryRestClient
    {
        public TestCloudFoundryRestClient()
        {
            this.Responses = new Queue<IHttpResponseAbstraction>();
        }

        public Queue<IHttpResponseAbstraction> Responses { get; set; }

        public Task<IHttpResponseAbstraction> GetInstanceInfoAsync()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> AuthenticateAsync(Uri tokenEndpoint, string username, string password)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetOrganizationsAsync()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetOrganizationAsync(string orgId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetSpacesAsync()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetSpacesAsync(string orgId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetSpaceAsync(string spaceId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetUsersAsync()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetUsersAsync(string orgId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetUserAsync(string userId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetApplicationsAsync()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetApplicationsAsync(string spaceId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetApplicationAsync(string appId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> CreateApplicationAsync(string name, string spaceId, int memoryLimit, int instances, int diskQuota)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetRoutesAsync(string appId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> CreateRouteAsync(string hostName, string domainId, string spaceId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> MapRouteAsync(string routeId, string applicationId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetInstancesAsync(string appId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> UpdateApplicationAsync(string appId, IDictionary<string, object> properties )
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> UploadApplicationPackageAsync(string appId, Stream package)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetDomainsAsync()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetJobAsync(string jobId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }
    }

    public class TestCloudFoundryRestClientFactory : ICloudFoundryRestClientFactory
    {
        internal ICloudFoundryRestClient Client;
        public TestCloudFoundryRestClientFactory(ICloudFoundryRestClient client)
        {
            this.Client = client;
        }

        public ICloudFoundryRestClient Create(IServiceLocator serviceLocator, IPasswordAuthCredential credential,  CancellationToken cancellationToken)
        {
            return Client;
        }
    }
}
