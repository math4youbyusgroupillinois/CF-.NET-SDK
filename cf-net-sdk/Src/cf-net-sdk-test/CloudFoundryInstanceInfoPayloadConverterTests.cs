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
using cf_net_sdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryInstanceInfoPayloadConverterTests
    {
        internal string InfoFixture = @"{{
            ""name"": ""{0}"",
            ""build"": ""{1}"",
            ""support"": ""stackato-support@activestate.com"",
            ""version"": {2},
            ""description"": ""ActiveState Stackato"",
            ""authorization_endpoint"": ""{3}"",
            ""token_endpoint"": ""{4}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";

        public string CreateaInfoPayload(InstanceInfo info)
        {
            return string.Format(InfoFixture, info.Name, info.Build, info.Version,
                info.AuthorizationEndpoint.AbsoluteUri, info.TokenEndpoint.AbsoluteUri);
        }

        #region Convert Tests

        [TestMethod]
        public void CanConvertInfoWithValidPayload()
        {
            var expInfo = new InstanceInfo("MyInstance","111", "2", new Uri("http://server.com"), new Uri("Http://someotherserver.com"));
            var payload = CreateaInfoPayload(expInfo);

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            var info = converter.Convert(payload);

            Assert.AreEqual(expInfo.Name, info.Name);
            Assert.AreEqual(expInfo.Build, info.Build);
            Assert.AreEqual(expInfo.Version, info.Version);
            Assert.AreEqual(expInfo.AuthorizationEndpoint.AbsoluteUri, info.AuthorizationEndpoint.AbsoluteUri);
            Assert.AreEqual(expInfo.TokenEndpoint.AbsoluteUri, info.TokenEndpoint.AbsoluteUri);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingName()
        {
            string infoFixture = @"{{
            ""build"": ""{0}"",
            ""support"": ""stackato-support@activestate.com"",
            ""version"": {1},
            ""description"": ""ActiveState Stackato"",
            ""authorization_endpoint"": ""{2}"",
            ""token_endpoint"": ""{3}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";
            var infoPayload = string.Format(infoFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(infoPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingBuild()
        {
            string infoFixture = @"{{
            ""name"": ""{0}"",
            ""support"": ""stackato-support@activestate.com"",
            ""version"": {1},
            ""description"": ""ActiveState Stackato"",
            ""authorization_endpoint"": ""{2}"",
            ""token_endpoint"": ""{3}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";
            var infoPayload = string.Format(infoFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(infoPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingVersion()
        {
            string infoFixture = @"{{
            ""name"": ""{0}"",
            ""build"": ""{1}"",
            ""support"": ""stackato-support@activestate.com"",
            ""description"": ""ActiveState Stackato"",
            ""authorization_endpoint"": ""{2}"",
            ""token_endpoint"": ""{3}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";
            var infoPayload = string.Format(infoFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(infoPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingAuthUrl()
        {
            string infoFixture = @"{{
            ""name"": ""{0}"",
            ""build"": ""{1}"",
            ""support"": ""stackato-support@activestate.com"",
            ""version"": {2},
            ""description"": ""ActiveState Stackato"",
            ""token_endpoint"": ""{3}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";
            var infoPayload = string.Format(infoFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(infoPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInstanceWithMissingTokenUrl()
        {
            string infoFixture = @"{{
            ""name"": ""{0}"",
            ""build"": ""{1}"",
            ""support"": ""stackato-support@activestate.com"",
            ""version"": {2},
            ""description"": ""ActiveState Stackato"",
            ""authorization_endpoint"": ""{3}"",
            ""allow_debug"": true,
            ""vendor_version"": ""v3.2.1-21-gccecf32"",
            ""stackato"": {{
                ""license_accepted"": true,
                ""UUID"": ""E58A1A62-078F-11E4-881D-FA163ECD6706""
            }},
            ""user"": ""c3b17dac-aae2-4abe-a1a0-7c3791d6f2b4"",
            ""limits"": {{
                ""memory"": 32768,
                ""app_uris"": 16,
                ""services"": 32,
                ""apps"": 200
            }},
            ""cc_nginx"": false
        }}";
            var infoPayload = string.Format(infoFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(infoPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSpaceWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertSpaceWithEmptyPayload()
        {
            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertSpaceWithNullPayload()
        {
            var converter = new CloudFoundryInstanceInfoPayloadConverter();
            converter.Convert(null);
        }

        #endregion
    }
}
