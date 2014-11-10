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
using cf_net_sdk.Interfaces;
using CloudFoundry.Common;

namespace cf_net_sdk
{
    /// <inheritdoc/>
    public class CloudFoundryPasswordCredential : IPasswordAuthCredential
    {
        /// <inheritdoc/>
        public Uri ServerEndpoint { get; internal set; }

        /// <inheritdoc/>
        public string Username { get; internal set; }

        /// <inheritdoc/>
        public string Password { get; internal set; }

        /// <inheritdoc/>
        public string AccessToken { get; internal set; }


        /// <summary>
        /// Creates a new instance of the CloudFoundryPasswordCredential class.
        /// </summary>
        /// <param name="authEndpoint">The endpoint to use when authenticating this credential.</param>
        /// <param name="username">The name of the user.</param>
        /// <param name="password">The password for the user.</param>
        public CloudFoundryPasswordCredential(Uri authEndpoint, string username, string password)
        {
            authEndpoint.AssertIsNotNull("authEndpoint", "Cannot create a credential with a null endpoint.");
            username.AssertIsNotNullOrEmpty("username", "Cannot create a credential with a null or empty user name.");
            password.AssertIsNotNull("password", "Cannot create a credential with a null password.");

            this.ServerEndpoint = authEndpoint;
            this.Username = username;
            this.Password = password;
            this.AccessToken = string.Empty;
        }
    }
}
