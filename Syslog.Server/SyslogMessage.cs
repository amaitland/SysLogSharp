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

/// Based on code from Fantail.SyslogServer.  Copyright included below.
/// ====================
/// 
/// Fantail Technology Ltd ( www.fantail.net.nz )
/// 
/// Designed by Chris Guthrey & David Husselmann
/// Developed by David Husselmann for Fantail Technology Ltd
///
/// chris@fantail.net.nz
/// david@tamix.com
/// 
/// Copyright (c) 2007, Fantail Technology Ltd
/// 
/// All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Syslog.Server
{
    /// <summary>
    /// Encapsulates a single syslog message, as received from a remote host.
    /// </summary>
    [Serializable]
    public class SyslogMessage : MarshalByRefObject 
    {
        /// <summary>
        /// Creates a new instance of the SyslogMessage class.
        /// </summary>
        /// <param name="priority">Specifies the encoded PRI field, containing the facility and severity values.</param>
        /// <param name="timestamp">Specifies the timestamp, if present in the packet.</param>
        /// <param name="hostname">Specifies the hostname, if present in the packet.  The hostname can only be present if the timestamp is also present (RFC3164).</param>
        /// <param name="message">Specifies the textual content of the message.</param>
        public SyslogMessage(int priority, DateTime timestamp, string hostname, string message)
        {
            if (priority > 0)
            {
                // The facility code is the nearest whole number of the priority value divided by 8
                this.facility = (FacilityCode)(int)Math.Floor((double)priority / 8);
                // The severity code is the remainder of the priority value divided by 8
                this.severity = (SeverityCode) (priority % 8);
            }
            else
            {
                this.facility = FacilityCode.None;
                this.severity = SeverityCode.None;
            }

            this.timestamp = timestamp;
            this.hostname = hostname;
            this.message = message;
        }

        private FacilityCode facility;
        /// <summary>
        /// Returns an integer specifying the facility.
        /// </summary>
        public FacilityCode Facility
        {
            get { return facility; }
        }

        private SeverityCode severity;
        /// <summary>
        /// Returns an integer number specifying the severity.
        /// </summary>
        public SeverityCode Severity
        {
            get { return severity; }
        }

        private DateTime timestamp;
        /// <summary>
        /// Returns a DateTime specifying the moment at which the event is known to have happened.  As per RFC3164,
        /// if the host does not send this value, it may be added by a relay.
        /// </summary>
        public DateTime Timestamp
        {
            get { return timestamp; }
        }

        private string hostname;
        /// <summary>
        /// Returns the DNS hostname where the message originated, or the IP address if the hostname is unknown.
        /// </summary>
        public string Hostname
        {
            get { return hostname; }
            //set { hostname = value; }
        }

        private string message;
        /// <summary>
        /// Returns a string indicating the textual content of the message.
        /// </summary>
        public string Message
        {
            get { return message; }
            //set { message = value; }
        }

        /// <summary>
        /// Returns a textual representation of the syslog message, for debugging purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat("Facility: ", this.facility.ToString(), "\nSeverity: ", this.severity.ToString(),
              "\nTimestamp: ", this.timestamp.ToString(), "\nHostname: ", this.hostname, "\nMessage: ", this.message);
        }
    }
}
