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
using System.Collections.Generic;
using System.Text;

namespace Syslog.Server
{
    /// <summary>
    /// Methods and properties implementation of the <see cref="MessageHandlerBase"/> class.
    /// </summary>
    internal class MessageHandler : MessageHandlerBase
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="assemblyName">The full compiled name of the handler assembly.</param>
        /// <param name="parserClassName">The name of the class that implements the <see cref="IParser"/> interface in the assembly.</param>
        /// <param name="storerClassName">The name of the class that implements the <see cref="IStorer"/> interface in the assembly.</param>
        /// <param name="connectionString">The connection string, if required, for the storer class.  This parameter can be null.</param>
        public MessageHandler(string assemblyName, string parserClassName, string storerClassName, string connectionString)
            : base(assemblyName, parserClassName, storerClassName, connectionString)
        {

        }
    }
}
