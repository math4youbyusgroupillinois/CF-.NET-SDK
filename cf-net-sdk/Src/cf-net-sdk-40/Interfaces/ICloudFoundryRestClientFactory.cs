﻿// /* ============================================================================
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

using System.Threading;
using CloudFoundry.Common.ServiceLocation;

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Creates REST clients that can be used to make requests to a remote Cloud Foundry service.
    /// </summary>
    public interface ICloudFoundryRestClientFactory
    {
        /// <summary>
        /// Creates a REST client that can be used to make requests to a remote Cloud Foundry service.
        /// </summary>
        /// <param name="serviceLocator">A service locator that can be used to locate dependent services.</param>
        /// <param name="credential">The credential to use when making requests.</param>
        /// <param name="cancellationToken">A cancellation token to use when making requests.</param>
        /// <returns>A client capable of making REST requests to a remote Cloud Foundry service.</returns>
        ICloudFoundryRestClient Create(IServiceLocator serviceLocator, IPasswordAuthCredential credential, CancellationToken cancellationToken);
    }
}
