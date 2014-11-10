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

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Provides password based credentials for authenticating against a remote service.
    /// </summary>
    public interface IPasswordAuthCredential
    {
        /// <summary>
        /// Gets the endpoint that the credential is valid against. 
        /// </summary>
        Uri ServerEndpoint { get; }

        /// <summary>
        /// Gets the name of the user for authentication.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password used for authentication.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the current access token.
        /// </summary>
        string AccessToken { get; }
    }
}
