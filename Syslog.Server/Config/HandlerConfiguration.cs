/*
 * Copyright 2010 Andrew Draut
 * 
 * This file is part of Syslog Sharp.
 * 
 * Syslog Sharp is free software: you can redistribute it and/or modify it under the terms of the GNU General 
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at 
 * your option) any later version.
 * 
 * Syslog Sharp is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even 
 * the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with Syslog Sharp. If not, see http://www.gnu.org/licenses/.
*/

using System.Configuration;

namespace Syslog.Server.Config
{
    /// <summary>
    /// Represents a Handler configuration element.
    /// </summary>
    public class HandlerConfiguration : ConfigurationElement
    {
        public HandlerConfiguration() { }

        /// <summary>
        /// Creates a new <see cref="HandlerConfiguration"/>.
        /// </summary>
        /// <param name="assemblyName">The fully compiled name of the assembly.</param>
        /// <param name="storageClassName">The name of class that implements the <see cref="IStorer"/> interface.</param>
        /// <param name="parserClassName">The name of the class that implements the <see cref="IParser"/> interface.</param>
        /// <param name="filterIPAddresses">A comma or semi-colon seperated list of IPv4 addresses to listen for messages from.</param>
        /// <param name="connectionString">The connection string to connect to the data store used by the <paramref name="storageClassName"/>, if required.</param>
        /// <param name="ipForwards">A comma or semi-colon seperated list of IPv4 addresses to foward raw messages to.</param>
        public HandlerConfiguration(string assemblyName, string storageClassName,
            string parserClassName, string filterIPAddresses, string connectionString,
            string ipForwards)
        {
            this.AssemblyName = assemblyName;
            this.StorageClassName = storageClassName;
            this.ParserClassName = parserClassName;
            this.FilterIPAddresses = filterIPAddresses;
            this.ConnectionString = connectionString;
            this.IPForwards = ipForwards;
        }

        /// <summary>
        /// Gets or sets the fully compiled name of the assembly.
        /// </summary>
        [ConfigurationProperty("assemblyName", IsRequired = true, IsKey = true)]
        public string AssemblyName
        {
            get { return this["assemblyName"] as string; }
            set { this["assemblyName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of class that implements the <see cref="IStorer"/> interface.
        /// </summary>
        [ConfigurationProperty("storageClassName", IsRequired = false)]
        public string StorageClassName
        {
            get { return this["storageClassName"] as string; }
            set { this["storageClassName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the class that implements the <see cref="IParser"/> interface.
        /// </summary>
        [ConfigurationProperty("parserClassName", IsRequired = true)]
        public string ParserClassName
        {
            get { return this["parserClassName"] as string; }
            set { this["parserClassName"] = value; }
        }

        /// <summary>
        /// Gets or sets a comma or semi-colon seperated list of IPv4 addresses to listen for messages from.
        /// </summary>
        [ConfigurationProperty("filterIPAdresses", IsRequired = true)]
        public string FilterIPAddresses
        {
            get { return this["filterIPAdresses"] as string; }
            set { this["filterIPAdresses"] = value; }
        }

        /// <summary>
        /// Gets or sets the connection string to connect to the data store used by the <paramref name="storageClassName"/>, if required.
        /// </summary>
        [ConfigurationProperty("connectionString", IsRequired = false)]
        public string ConnectionString
        {
            get { return this["connectionString"] as string; }
            set { this["connectionString"] = value; }
        }

        /// <summary>
        /// Gets or sets a comma or semi-colon seperated list of IPv4 addresses to foward raw messages to.
        /// </summary>
        [ConfigurationProperty("ipForwards", IsRequired = false)]
        public string IPForwards
        {
            get { return this["ipForwards"] as string; }
            set { this["ipForwards"] = value; }
        }
    }
}
