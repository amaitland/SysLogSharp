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

using System;
using System.Reflection;
using System.Threading;

namespace Syslog.Server
{
    /// <summary>
    /// Methods and properties used by all handlers (modules).
    /// </summary>
    internal class MessageHandlerBase
    {
        private Assembly assemblyRef;
        private Type parserType;
        IParser parser;

        private Type storerType;
        IStorer storer;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="assemblyName">The full compiled name of the handler assembly.</param>
        /// <param name="parserClassName">The name of the class that implements the <see cref="IParser"/> interface in the assembly.</param>
        /// <param name="storerClassName">The name of the class that implements the <see cref="IStorer"/> interface in the assembly.</param>
        /// <param name="connectionString">The connection string, if required, for the storer class.  This parameter can be null.</param>
        public MessageHandlerBase(string assemblyName, string parserClassName, string storerClassName, string connectionString)
        {
            AssemblyName = assemblyName;
            ParserClassName = parserClassName;
            StorerClassName = storerClassName;
            this.ConnectionString = connectionString;
        }

        private string assemblyName;
        /// <summary>
        /// Gets or sets the full compiled name of the handler assembly.
        /// </summary>        
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }

        private string parserClassName;
        /// <summary>
        /// Gets or sets the name of the class that implements the <see cref="IParser"/> interface in the assembly.
        /// </summary>
        public string ParserClassName
        {
            get { return parserClassName; }
            set { parserClassName = value; }
        }

        private string storerClassName;
        /// <summary>
        /// Gets or sets the name of the class that implements the <see cref="IStorer"/> interface in the assembly.
        /// </summary>
        public string StorerClassName
        {
            get { return storerClassName; }
            set { storerClassName = value; }
        }

        private string connectionString;
        /// <summary>
        /// Gets or sets the connection string for the storer class.
        /// </summary>
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        /// <summary>
        /// Gets the assembly by the name defined in the parameter <see cref="AssemblyName"/>.
        /// </summary>
        /// <returns>Returns the found assembly.</returns>
        public Assembly GetAssembly()
        {
            if (assemblyRef == null)
            {
                if (AssemblyName != null)
                {
                    assemblyRef = Assembly.Load(this.AssemblyName);
                }
            }
            return assemblyRef;
        }

        /// <summary>
        /// Gets a reference to the class in the <see cref="AssemblyName"/> that matches <see cref="ParserClassName"/>.
        /// </summary>
        /// <returns>Returns the <see cref="IParser"/> class reference.</returns>
        public IParser GetParser()
        {
            if (parser == null)
            {
                if (parserType == null)
                {
                    if (AssemblyName != null && ParserClassName != null)
                    {
                        if (assemblyRef == null)
                        {
                            GetAssembly();
                        }

                        if (assemblyRef != null)
                        {
                            parserType = assemblyRef.GetType(AssemblyName + "." + ParserClassName);
                        }
                    }
                }

                if (parserType != null && parser == null)
                {
                    parser = (IParser)Activator.CreateInstance(parserType);
                }
            }

            return parser;
        }

        /// <summary>
        /// Gets a reference to the class in the <see cref="AssemblyName"/> that matches <see cref="StorerClassName"/>.
        /// </summary>
        /// <returns>Returns the <see cref="IStorer"/> class reference.</returns>
        public IStorer GetStorer()
        {
            if (storer == null)
            {
                if (storerType == null)
                {
                    if (AssemblyName != null && StorerClassName != null)
                    {
                        if (assemblyRef == null)
                        {
                            GetAssembly();
                        }

                        if (assemblyRef != null)
                        {
                            storerType = assemblyRef.GetType(AssemblyName + "." + StorerClassName);
                        }
                    }
                }

                if (storerType != null && storer == null)
                {
                    if (storerType.BaseType == typeof(DatabaseStorer))
                    {
                        storer = (IStorer)Activator.CreateInstance(storerType, this.ConnectionString);
                    }
                    else
                    {
                        storer = (IStorer)Activator.CreateInstance(storerType);
                    }
                }
            }
            return storer;
        }
    }
}
