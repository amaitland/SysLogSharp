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

namespace Syslog.Server
{
    /// <summary>
    /// Event data for the <see cref="MessageReceivedEvent"/>.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly SyslogMessage _syslogMessage;
        /// <summary>
        /// Returns the syslog message as received from the remote host.
        /// </summary>
        public SyslogMessage SyslogMessage
        {
            get { return _syslogMessage; }
        }

        /// <summary>
        /// Creates a new instance of the MessageReceivedEventArgs class.
        /// </summary>
        /// <param name="sm">The <see cref="SyslogMessage"/> of the event.</param>
        public MessageReceivedEventArgs(SyslogMessage sm)
        {
            _syslogMessage = sm;
        }
    }
}
