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

namespace cf_net_sdk.Interfaces
{
    /// <summary>
    /// Converts http authentication responses.
    /// </summary>
    public interface IAuthenticationPayloadConverter
    {
        /// <summary>
        /// Converts an http authentication response payload into an access token.
        /// </summary>
        /// <param name="payload">An http response payload.</param>
        /// <returns>An access token.</returns>
        string ConvertAccessToken(string payload);
    }
}
