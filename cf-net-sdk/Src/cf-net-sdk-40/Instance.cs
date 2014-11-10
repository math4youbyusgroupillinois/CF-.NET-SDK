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

namespace cf_net_sdk
{
    /// <summary>
    /// Represents an instance of an application on the remote Cloud Foundry service.
    /// </summary>
    public class Instance : Entity
    {
        /// <summary>
        /// The current state of the instance.
        /// </summary>
        public InstanceState State { get; internal set; }

        /// <summary>
        /// The host name for this instance.
        /// </summary>
        public string HostName { get { return this.Name; }}

        /// <summary>
        /// The port of this instance.
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// The current memory usage for this instance.
        /// </summary>
        public long MemoryUsage { get; internal set; }

        /// <summary>
        /// The current CPU usage of this instance.
        /// </summary>
        public long CpuUsage { get; internal set; }

        /// <summary>
        /// The current disk usage of this instance.
        /// </summary>
        public long DiskUsage { get; internal set; }

        /// <summary>
        /// The current memory usage for this instance.
        /// </summary>
        public long MemoryQuota { get; internal set; }

        /// <summary>
        /// The current disk usage of this instance.
        /// </summary>
        public long DiskQuota { get; internal set; }

        /// <summary>
        /// The total up-time for this instance.
        /// </summary>
        public TimeSpan UpTime { get; internal set; }

        /// <summary>
        /// Creates a new instance of the Instance class.
        /// </summary>
        /// <param name="id">The id of the instance.</param>
        /// <param name="name">The name of the instance.</param>
        /// <param name="state">The state of the instance.</param>
        /// <param name="port">The port number for the instance.</param>
        /// <param name="memoryUsage">The memory usage of the instance.</param>
        /// <param name="cpuUsage">The current CPU usage of the instance.</param>
        /// <param name="diskUsage">The current disk usage of the instance.</param>
        /// <param name="memoryQuota">The current memory quota for the instance.</param>
        /// <param name="upTime">The current up-time for the instance.</param>
        /// <param name="createDate">The time at when these details were gathered.</param>
        /// <param name="diskQuota">The current disk quota for the instance.</param>
        public Instance(string id, string name, InstanceState state, int port, long memoryUsage, long cpuUsage, long diskUsage, long diskQuota, long memoryQuota, int upTime, DateTime createDate)
            : base(id, name, createDate)
        {
            this.State = state;
            this.Port = port;
            this.MemoryUsage = memoryUsage;
            this.CpuUsage = cpuUsage;
            this.DiskUsage = diskUsage;
            this.DiskQuota = diskQuota;
            this.MemoryQuota = memoryQuota;
            this.UpTime = TimeSpan.FromSeconds(upTime);
        }
    }
}
