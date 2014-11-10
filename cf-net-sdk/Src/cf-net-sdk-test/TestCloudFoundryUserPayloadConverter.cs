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
using System.Linq;
using cf_net_sdk;
using cf_net_sdk.Interfaces;

namespace cf_net_sdk_test
{
    internal class TestCloudFoundryUserPayloadConverter : IUserPayloadConverter
    {
        ICollection<User> Spaces { get; set; }

        public TestCloudFoundryUserPayloadConverter(string id, string name, DateTime createDate)
        {
          this.Spaces = new List<User>() { new User(id, name, createDate)};
        }

        public TestCloudFoundryUserPayloadConverter(ICollection<User> spaces)
        {
            this.Spaces = spaces;
        }

        public IEnumerable<User> ConvertUsers(string payload)
        {
            return this.Spaces;
        }

        public User ConvertUser(string payload)
        {
            return this.Spaces.First();
        }
    }
}
