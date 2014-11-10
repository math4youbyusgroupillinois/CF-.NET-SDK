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

using cf_net_sdk;
using cf_net_sdk.Interfaces;
using CloudFoundry.Common.Http;
using CloudFoundry.Common.ServiceLocation;

namespace CloudFoundry
{
    /// <inheritdoc/>
    public class ServiceRegistrar : IServiceLocationRegistrar
    {
        /// <inheritdoc/>
        public void Register(IServiceLocationManager manager, IServiceLocator locator)
        {
            //Common
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new HttpAbstractionClientFactory());
            
            //clients
            manager.RegisterServiceInstance(typeof(ICloudFoundryPocoClientFactory), new CloudFoundryPocoClientFactory());
            manager.RegisterServiceInstance(typeof(ICloudFoundryRestClientFactory), new CloudFoundryRestClientFactory());

            //converters
            manager.RegisterServiceInstance(typeof(IAuthenticationPayloadConverter), new CloudFoundryAuthenticationPayloadConverter());
            manager.RegisterServiceInstance(typeof(IOrganizationPayloadConverter), new CloudFoundryOrganizationPayloadConverter());
            manager.RegisterServiceInstance(typeof(ISpacePayloadConverter), new CloudFoundrySpacePayloadConverter());
            manager.RegisterServiceInstance(typeof(IDomainPayloadConverter), new CloudFoundryDomainPayloadConverter());
            manager.RegisterServiceInstance(typeof(IInstanceInfoPayloadConverter), new CloudFoundryInstanceInfoPayloadConverter());
            manager.RegisterServiceInstance(typeof(IUserPayloadConverter), new CloudFoundryUserPayloadConverter());
            manager.RegisterServiceInstance(typeof(IApplicationPayloadConverter), new CloudFoundryApplicationPayloadConverter());
            manager.RegisterServiceInstance(typeof(IJobPayloadConverter), new CloudFoundryJobPayloadConverter());
            manager.RegisterServiceInstance(typeof(IRoutePayloadConverter), new CloudFoundryRoutePayloadConverter());
            manager.RegisterServiceInstance(typeof(IInstancePayloadConverter), new CloudFoundryInstancePayloadConverter());
        }
    }
}
