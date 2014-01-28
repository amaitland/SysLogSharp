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
using Syslog.Server;
using System.Text.RegularExpressions;

namespace Syslog.BarracudaSpamFilter
{
    /// <summary>
    /// Contains methods for parsing a syslog message from a Barracuda E-mail Spam Filter.
    /// </summary>
    public class Parser : IParser
    {
        // Regex's are from Barracuda's developer specification for their Spam Filter.
        Regex headerRegex = new Regex(@"\]:\s+(?<IP>[^\s]+)\s?(?<ID>[^\s]+)\s?(?<START_TIME>\d+)\s?(?<END_TIME>\d+)\s?(?<NAME>(RECV|SCAN|SEND))\s?(?<INFO>.*)$", RegexOptions.Compiled);
        Regex recvRegex = new Regex(@"(?<SENDER>[^\s]+)\s?(?<RECIPIENT>[^\s]+)\s?(?<ACTION>\d+)\s?(?<REASON>\d+)\s?(?<EXTRA>.*)$", RegexOptions.Compiled);
        Regex scanRegex = new Regex(@"(?<ENCRYPTION>[^\s]+)\s?(?<SENDER>[^\s]+)\s?(?<RECIPIENT>[^\s]+)\s?(?<SCORE>[-\.\d+]+)\s?(?<ACTION>\d+)\s?(?<REASON>\d+)\s?(?<REASON_EXTRA>.*)\sSUBJ:(?<SUBJECT>.*)$", RegexOptions.Compiled);
        Regex sendRegex = new Regex(@"(?<ENCRYPTION>[^\s]+)\s?(?<ACTION>\d+)\s?(?<QUEUE_ID>[^\s]+)\s?(?<REASON>.*)$", RegexOptions.Compiled);

        #region IParser Members

        /// <summary>
        /// Parses the <see cref="SyslogMessage"/> into its individual data fields.
        /// </summary>
        /// <param name="message">The <see cref="SyslogMessage"/> to process.</param>
        /// <returns>Returns a string array of the parsed fields.  Returns <see cref="null"/> if there is an error processing the messages.</returns>
        string[] IParser.Parse(SyslogMessage message)
        {
            string[] msgParts = new string[13];

            if (message == null || message.Message == null)
            {
                return null;
            }

            Match headerMatches = headerRegex.Match(message.Message);

            if (headerMatches.Groups.Count > 0)
            {
                msgParts[0] = message.Timestamp.ToString();  //MsgDateTime

                switch (headerMatches.Groups["NAME"].Value)
                {
                    case "SCAN":
                        Match scanMatches = scanRegex.Match(headerMatches.Groups["INFO"].Value);

                        //Header info
                        msgParts[1] = headerMatches.Groups["IP"].Value;
                        msgParts[2] = headerMatches.Groups["ID"].Value;
                        msgParts[3] = headerMatches.Groups["START_TIME"].Value;
                        msgParts[4] = headerMatches.Groups["END_TIME"].Value;

                        //Scan info
                        msgParts[5] = scanMatches.Groups["ENCRYPTION"].Value;
                        msgParts[6] = scanMatches.Groups["SENDER"].Value;
                        msgParts[7] = scanMatches.Groups["RECIPIENT"].Value;

                        if (scanMatches.Groups["SCORE"].Value != "-")
                        {
                            msgParts[8] = scanMatches.Groups["SCORE"].Value;
                        }
                        else
                        {
                            msgParts[8] = "0";
                        }

                        msgParts[9] = scanMatches.Groups["ACTION"].Value;
                        msgParts[10] = scanMatches.Groups["REASON"].Value;
                        msgParts[11] = scanMatches.Groups["REASON_EXTRA"].Value;
                        msgParts[12] = scanMatches.Groups["SUBJECT"].Value;

                        break;

                    case "SEND":
                    case "RECV":
                    default:
                        msgParts = null;
                        break;
                }
            }
           
            return msgParts;
        }

        #endregion
    }
}
