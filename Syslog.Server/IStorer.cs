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

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Syslog.Server
{
    /// <summary>
    /// Base interface for writing messages to a data store.
    /// </summary>
    public interface IStorer
    {
        /// <summary>
        /// Processes an enumerable object of parsed messages.
        /// </summary>
        /// <param name="messages">An enumerable of parsed messages.</param>
        /// <returns>Returns true if processing was successful.  False otherwise.</returns>
        bool StoreMessages(IEnumerable<string[]> messages);
    }
}
