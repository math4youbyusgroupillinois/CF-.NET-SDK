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
    internal class TestCloudFoundryDomainPayloadConverter : IDomainPayloadConverter
    {
        ICollection<Domain> Domains { get; set; }

        public TestCloudFoundryDomainPayloadConverter(string id, string name, DateTime createDate)
        {
          this.Domains = new List<Domain>() { new Domain(id, name, createDate)};
        }

        public TestCloudFoundryDomainPayloadConverter(ICollection<Domain> domains)
        {
            this.Domains = domains;
        }

        public IEnumerable<Domain> ConvertDomains(string payload)
        {
            return this.Domains;
        }

        public Domain ConvertDomain(string payload)
        {
            return this.Domains.First();
        }
    }
}
