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

namespace Syslog.Server.Console
{
    /// <summary>
    /// Base class for all subscribers of the <see cref=" MessageReceivedCallback"/> event.
    /// </summary>
    /// <remarks>This class is used to communicate via remoting using events.</remarks>
    public abstract class MessageReceivedCallbackSink : MarshalByRefObject
    {
        /// <summary>
        /// Method called after a message received event.
        /// </summary>
        /// <param name="message">The <see cref="SyslogMessage"/> used with the event.</param>
        /// <remarks>This method must be overridden.</remarks>
        protected abstract void OnMessageReceived(SyslogMessage message);

        /// <summary>
        /// Raise the OnMessageReceived method.
        /// </summary>
        /// <param name="message">The <see cref="SyslogMessage"/> used with the event.</param>
        public void FireMessageReceived(SyslogMessage message)
        {
            try
            {
                OnMessageReceived(message);
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Could not fire OnMessageReceived because: " + ex.Message,
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }
    }
}
