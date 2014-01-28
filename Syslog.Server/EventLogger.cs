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
using System.Diagnostics;

namespace Syslog.Server
{
    /// <summary>
    /// Wrapper class for writing to the Windows Event Log
    /// </summary>
    public static class EventLogger
    {
        private static readonly string source = "Syslog Service";
        private static readonly string logName = "Application";

        /// <summary>
        /// Writes the <paramref name="message"/> to the Application event log with the given severity (<paramref name="entryType"/>).
        /// </summary>
        /// <param name="message">The message text to log.</param>
        /// <param name="entryType">The severity of the message.</param>
        public static void LogEvent(string message, EventLogEntryType entryType)
        {
            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, logName);
                }

                EventLog.WriteEntry(source, message, entryType);
            }
            catch (Exception)
            {
                //Make sure no errors are thrown here to avoid application crash.
            }
        }
    }
}
