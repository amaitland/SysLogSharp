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
using Syslog.Server;

namespace Syslog.GenericFilter
{
    /// <summary>
    /// Contains methods for parsing a syslog message from a Barracuda Web Filter.
    /// </summary>
    public class GenericParser : IParser
    {
        /// <summary>
        /// Parses the <see cref="SyslogMessage"/> into its individual data fields.
        /// </summary>
        /// <param name="message">The <see cref="SyslogMessage"/> to process.</param>
        /// <returns>Returns a string array of the parsed fields.  Returns <see cref="null"/> if there is an error processing the messages.</returns>
        public string[] Parse(SyslogMessage message)
        {
            if (message == null || message.Message == null)
            {
                return null;
            }

            string[] msgParts = message.Message.Split(' ');
            string[] msg = null;

			Console.WriteLine(message.Message);

	        return msgParts;

			//if (msgParts.Length >= 26)
			//{
			//	msg = new string[18];

			//	msg[0] = message.Timestamp.ToString();  //MsgDateTime
			//	msg[1] = msgParts[3];   //SourceIP
			//	msg[2] = msgParts[4];   //DestIP
			//	msg[3] = msgParts[5];   //ContentType
			//	msg[4] = msgParts[7];   //URL
			//	msg[5] = msgParts[10];  //Action
			//	msg[6] = msgParts[11];  //Reason
			//	msg[7] = msgParts[13];  //FormatVersion
			//	msg[8] = msgParts[14];  //MatchFlag
			//	msg[9] = msgParts[15];  //TQFlag
			//	msg[10] = msgParts[16]; //ActionType
			//	msg[11] = msgParts[17]; //SrcType

			//	//SrcDetail
			//	int srcDetailPartsCount;
			//	if (msgParts[18].Contains("(") && msgParts[18].Contains(")"))
			//	{
			//		srcDetailPartsCount = 0;
			//		msg[12] = msgParts[18].Replace("(", string.Empty).Replace(")", string.Empty);
			//	}
			//	else
			//	{
			//		srcDetailPartsCount = -1;
			//		do
			//		{
			//			srcDetailPartsCount++;
			//			msg[12] += msgParts[18 + srcDetailPartsCount] + " ";
			//		} while (!msgParts[18 + srcDetailPartsCount].Contains(")"));

			//		msg[12] = msg[12].TrimEnd(' ');
			//		msg[12] = msg[12].Replace("(", string.Empty).Replace(")", string.Empty);
			//		msg[12] = msg[12].Substring(msg[12].IndexOf(':') + 1,
			//			msg[12].Length - 1 - msg[12].IndexOf(':'));
			//	}

			//	msg[13] = msgParts[19 + srcDetailPartsCount];   //DestType
			//	msg[14] = msgParts[21 + srcDetailPartsCount];   //SpyType
			//	msg[15] = msgParts[24 + srcDetailPartsCount];   //MatchedPart
			//	msg[16] = msgParts[25 + srcDetailPartsCount];   //MatchedCategory
			//	//UserInfo
			//	msg[17] = msgParts[26 + srcDetailPartsCount].Substring(msgParts[26 + srcDetailPartsCount].IndexOf(':') + 1,
			//		msgParts[26 + srcDetailPartsCount].Length - 1 - msgParts[26 + srcDetailPartsCount].IndexOf(':') - 1);
			//}

            return msg;
        }
    }
}
