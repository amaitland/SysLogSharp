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

namespace Syslog.Server
{
    /// <summary>
    /// Methods and properties used by all handlers (modules).
    /// </summary>
    internal class MessageHandlerBase
    {
        private Assembly _assemblyRef;
        private Type _parserType;
        private IParser _parser;

        private Type _storerType;
        private IDataStore _dataStore;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="assemblyName">The full compiled name of the handler assembly.</param>
        /// <param name="parserClassName">The name of the class that implements the <see cref="IParser"/> interface in the assembly.</param>
        /// <param name="storerClassName">The name of the class that implements the <see cref="IDataStore"/> interface in the assembly.</param>
        /// <param name="connectionString">The connection string, if required, for the storer class.  This parameter can be null.</param>
        public MessageHandlerBase(string assemblyName, string parserClassName, string storerClassName, string connectionString)
        {
            AssemblyName = assemblyName;
            ParserClassName = parserClassName;
            StorerClassName = storerClassName;
            ConnectionString = connectionString;
        }

	    /// <summary>
	    /// Gets or sets the full compiled name of the handler assembly.
	    /// </summary>        
	    public string AssemblyName { get; set; }

	    /// <summary>
	    /// Gets or sets the name of the class that implements the <see cref="IParser"/> interface in the assembly.
	    /// </summary>
	    public string ParserClassName { get; set; }

	    /// <summary>
	    /// Gets or sets the name of the class that implements the <see cref="IDataStore"/> interface in the assembly.
	    /// </summary>
	    public string StorerClassName { get; set; }

	    /// <summary>
	    /// Gets or sets the connection string for the storer class.
	    /// </summary>
	    public string ConnectionString { get; set; }

	    /// <summary>
        /// Gets the assembly by the name defined in the parameter <see cref="AssemblyName"/>.
        /// </summary>
        /// <returns>Returns the found assembly.</returns>
        public Assembly GetAssembly()
        {
            if (_assemblyRef == null)
            {
                if (AssemblyName != null)
                {
                    _assemblyRef = Assembly.Load(AssemblyName);
                }
            }
            return _assemblyRef;
        }

        /// <summary>
        /// Gets a reference to the class in the <see cref="AssemblyName"/> that matches <see cref="ParserClassName"/>.
        /// </summary>
        /// <returns>Returns the <see cref="IParser"/> class reference.</returns>
        public IParser GetParser()
        {
            if (_parser == null)
            {
                if (_parserType == null)
                {
                    if (AssemblyName != null && ParserClassName != null)
                    {
                        if (_assemblyRef == null)
                        {
                            GetAssembly();
                        }

                        if (_assemblyRef != null)
                        {
                            _parserType = _assemblyRef.GetType(AssemblyName + "." + ParserClassName);
                        }
                    }
                }

                if (_parserType != null && _parser == null)
                {
                    _parser = (IParser)Activator.CreateInstance(_parserType);
                }
            }

            return _parser;
        }

        /// <summary>
        /// Gets a reference to the class in the <see cref="AssemblyName"/> that matches <see cref="StorerClassName"/>.
        /// </summary>
        /// <returns>Returns the <see cref="IDataStore"/> class reference.</returns>
        public IDataStore GetStorer()
        {
            if (_dataStore == null)
            {
                if (_storerType == null)
                {
                    if (AssemblyName != null && StorerClassName != null)
                    {
                        if (_assemblyRef == null)
                        {
                            GetAssembly();
                        }

                        if (_assemblyRef != null)
                        {
                            _storerType = _assemblyRef.GetType(AssemblyName + "." + StorerClassName);
                        }
                    }
                }

                if (_storerType != null && _dataStore == null)
                {
                    if (_storerType.BaseType == typeof(AbstractDatabaseDataStore))
                    {
                        _dataStore = (IDataStore)Activator.CreateInstance(_storerType, ConnectionString);
                    }
                    else
                    {
                        _dataStore = (IDataStore)Activator.CreateInstance(_storerType);
                    }
                }
            }
            return _dataStore;
        }
    }
}
