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
    public class CloudFoundryAuthenticationPayloadConverterTests
    {
        [TestMethod]
        public void CanConvertAccessTokenWithValidPayload()
        {
            var expectedToken = Guid.NewGuid().ToString();

            var payloadFixture = @"{{
                                        ""access_token"": ""{0}"",
                                        ""refresh_token"": ""R-gfBVF-6zb5WUU8RXnQyPctzyCTfFTGhqfts69F84JaVYSdj8o4e6LDW4V8DLkf-EONjzVoDzUafgnUdnXxlQ"",
                                        ""token_type"": ""bearer"",
                                        ""expires_in"": 86399,
                                        ""scope"": ""openid password.write cloud_controller.admin cloud_controller.read cloud_controller.write scim.read scim.write""
                                    }}";

            var payload = string.Format(payloadFixture, expectedToken);
            var converter = new CloudFoundryAuthenticationPayloadConverter();
            var token = converter.ConvertAccessToken(payload);

            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAccessTokenWithMissingTokenNode()
        {
            var payloadFixture = @"{
                                        ""refresh_token"": ""R-gfBVF-6zb5WUU8RXnQyPctzyCTfFTGhqfts69F84JaVYSdj8o4e6LDW4V8DLkf-EONjzVoDzUafgnUdnXxlQ"",
                                        ""token_type"": ""bearer"",
                                        ""expires_in"": 86399,
                                        ""scope"": ""openid password.write cloud_controller.admin cloud_controller.read cloud_controller.write scim.read scim.write""
                                    }";

            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAccessTokenWithInvalidJson()
        {
            var payloadFixture = @"{ [
                                        ""refresh_token"": ""R-gfBVF-6zb5WUU8RXnQyPctzyCTfFTGhqfts69F84JaVYSdj8o4e6LDW4V8DLkf-EONjzVoDzUafgnUdnXxlQ"",
                                        ""token_type"": ""bearer"",
                                        ""expires_in"": 86399,
                                        ""scope"": ""openid password.write cloud_controller.admin cloud_controller.read cloud_controller.write scim.read scim.write""
                                    }";

            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAccessTokenWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAccessTokenWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertAccessTokenWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertAccessTokenWithEmptyPayload()
        {
            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertAccessTokenWithNullPayload()
        {
            var converter = new CloudFoundryAuthenticationPayloadConverter();
            converter.ConvertAccessToken(null);
        }
    }
}
