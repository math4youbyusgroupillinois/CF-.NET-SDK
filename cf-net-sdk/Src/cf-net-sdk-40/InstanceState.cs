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

using CloudFoundry.Common;

namespace cf_net_sdk
{
    public enum InstanceState
    {
        Running, 
        Unknown
    }

    public static class InstanceStateExtentions
    {
        public static InstanceState GetInstanceState(this string input)
        {
            input.AssertIsNotNullOrEmpty("input", "Cannot get instance state with null or empty value.");

            switch (input.ToLowerInvariant())
            {
                case "running":
                    return InstanceState.Running;
                default:
                    return InstanceState.Unknown;
            }
        }
    }
}
