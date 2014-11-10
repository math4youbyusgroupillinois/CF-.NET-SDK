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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.Test
{
    [TestClass]
    public class CloudFoundryRoutePayloadConverterTests
    {
        internal string RoutesFixture = @"{{
                                                        ""total_results"": {0},
                                                        ""total_pages"": 1,
                                                        ""prev_url"": null,
                                                        ""next_url"": null,
                                                        ""resources"": [
                                                            {1}
                                                        ]
                                                    }}";

        internal string RouteFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                        ""name"": ""{2}""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";

        public string CreateRoutePayload(Route route)
        {
            return string.Format(RouteFixture, route.Id, route.Name, route.DomainName, route.CreatedDate);
        }

        public string CreateRoutesPayload(IEnumerable<Route> routes)
        {
            var isFirst = true;
            var routesPayload = string.Empty;
            foreach (var route in routes)
            {
                if (!isFirst)
                {
                    routesPayload += ",";
                }

                routesPayload += CreateRoutePayload(route);

                isFirst = false;
            }

            return string.Format(RoutesFixture, routes.Count(), routesPayload);
        }

        #region ConvertRoutes Tests

        [TestMethod]
        public void CanConvertRoutesWithValidPayload()
        {
            var rt1 = new Route("12345", "Route1", "Domain1.com", DateTime.MinValue);
            var rt2 = new Route("54321", "Route2", "Domain2.com", DateTime.MaxValue);
            var payload = CreateRoutesPayload(new List<Route>() { rt1, rt2 });

            var converter = new CloudFoundryRoutePayloadConverter();
            var routes = converter.ConvertRoutes(payload).ToList();

            Assert.AreEqual(2, routes.Count());

            Assert.AreEqual(rt1.Id, routes[0].Id);
            Assert.AreEqual(rt1.Name, routes[0].Name);
            Assert.AreEqual(rt1.CreatedDate, routes[0].CreatedDate);

            Assert.AreEqual(rt2.Id, routes[1].Id);
            Assert.AreEqual(rt2.Name, routes[1].Name);
            Assert.AreEqual(rt2.DomainName, routes[1].DomainName);
            Assert.AreEqual(rt2.CreatedDate.ToLongTimeString(), routes[1].CreatedDate.ToLongTimeString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRoutesWithInvalidRouteJson()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                        ""name"": ""{2}""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);
            var routesPayload = string.Format(RoutesFixture, "1", routePayload);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(routesPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRoutesWithInvalidDomainJson()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), DateTime.Now);
            var routesPayload = string.Format(RoutesFixture, "1", routePayload);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(routesPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRoutesWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRoutesWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRoutesWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRoutesWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertRoutesWithEmptyPayload()
        {
            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertRoutesWithNullPayload()
        {
            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoutes(null);
        }

        #endregion

        #region ConvertRoute Tests

        [TestMethod]
        public void CanConvertRouteWithValidPayload()
        {
            var expRt = new Route("12345", "Route1", "Domain.com", DateTime.MinValue);
            var payload = CreateRoutePayload(expRt);

            var converter = new CloudFoundryRoutePayloadConverter();
            var rt = converter.ConvertRoute(payload);

            Assert.AreEqual(expRt.Id, rt.Id);
            Assert.AreEqual(expRt.Name, rt.Name);
            Assert.AreEqual(expRt.DomainName, rt.DomainName);
            Assert.AreEqual(expRt.CreatedDate, rt.CreatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithMissingMetadataNode()
        {
            var rtFixture = @"{{
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                        ""name"": ""{2}""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(rtFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithMissingEntityNode()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{1}"",
                ""updated_at"": null
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertRouteWithMissingId()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                        ""name"": ""{2}""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithMissingHostName()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                        ""name"": ""{2}""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithMissingDomainElement()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithMissingDomainEntityElement()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithMissingDomainName()
        {
            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""created_at"": ""{3}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var routePayload = string.Format(routeFixture, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(routePayload);
        }
        
        [TestMethod]
        public void CanConvertRouteWithMissingCreateDate()
        {
            var expRt = new Route("12345", "Route","Domain.com", DateTime.MinValue);

            var routeFixture = @"{{
            ""metadata"": {{
                ""guid"": ""{0}"",
                ""url"": ""/v2/routes/{0}"",
                ""updated_at"": null
            }},
            ""entity"": {{
                ""host"": ""{1}"",
                ""domain_guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                ""space_guid"": ""807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""domain_url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                ""domain"": {{
                    ""metadata"": {{
                        ""guid"": ""40dce779-5081-4471-927b-1655fe172d2c"",
                        ""url"": ""/v2/domains/40dce779-5081-4471-927b-1655fe172d2c"",
                        ""created_at"": ""2014-07-09T17:41:14+00:00"",
                        ""updated_at"": ""2014-07-09T17:47:46+00:00""
                    }},
                    ""entity"": {{
                        ""name"": ""{2}""
                    }}
                }},
                ""space_url"": ""/v2/spaces/807d1dd2-1722-4636-a6ab-67ed8bd8e5d4"",
                ""apps_url"": ""/v2/routes/{0}/apps""
            }}
        }}";
            var appPayload = string.Format(routeFixture, expRt.Id, expRt.Name, expRt.DomainName);

            var converter = new CloudFoundryRoutePayloadConverter();
            var route = converter.ConvertRoute(appPayload);

            Assert.AreEqual(expRt.Id, route.Id);
            Assert.AreEqual(expRt.Name, route.Name);
            Assert.AreEqual(expRt.DomainName, route.DomainName);
            Assert.AreEqual(expRt.CreatedDate, DateTime.MinValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithInvalidJson()
        {
            var payloadFixture = @"{ [ }";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithNonObjectJson()
        {
            var payloadFixture = @"[]";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithEmptyObjectJson()
        {
            var payloadFixture = @"{}";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertRouteWithNonJson()
        {
            var payloadFixture = @"NOT JSON";

            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(payloadFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertRouteWithEmptyPayload()
        {
            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertRouteWithNullPayload()
        {
            var converter = new CloudFoundryRoutePayloadConverter();
            converter.ConvertRoute(null);
        }

        #endregion
    }
}
