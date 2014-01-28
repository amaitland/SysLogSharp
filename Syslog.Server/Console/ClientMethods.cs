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

namespace Syslog.Server.Console
{
    /// <summary>
    /// Methods for communicating to the GUI console via remoting
    /// </summary>
    public class ClientMethods : MarshalByRefObject, IConsole
    {
        private static event MessageReceivedCallback _eventSubscribers;

        #region IConsole Members

        /// <summary>
        /// Adds or removes event subscribers
        /// </summary>
        public event MessageReceivedCallback MessageHandled
        {
            add { _eventSubscribers += value; }
            remove { _eventSubscribers -= value; }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="MessageReceivedCallback"/> event for all subscribers
        /// </summary>
        /// <param name="message">The syslog message to use when raising the event.</param>
        public static void FireNewMessageReceived(SyslogMessage message)
        {
            if (_eventSubscribers != null)
            {
                try
                {
                    _eventSubscribers(message);
                }
                catch (System.Runtime.Remoting.RemotingException)
                {
                    //Traps an exception when the client IPC channel has been closed
                    //and pending events were still being written to the channel
                }
                catch (Exception ex)
                {
                    EventLogger.LogEvent("Could not send new messages to IPC subscribers because: " + ex.Message,
                        System.Diagnostics.EventLogEntryType.Warning);
                }
            }
        }
    }
}
