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
using Syslog.GenericFilter;
using Syslog.Server;
using Syslog.Server.Console;

namespace Syslog.Test
{
    public class Program
    {
		private static readonly GenericParser Parser = new GenericParser();

		public static void Main(string[] args)
		{
			var listener = new Listener(System.Configuration.ConfigurationManager.AppSettings["listenIPAddress"],
			                            Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["listenPort"]),
			                            Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["bufferFlushFrequency"]));

			listener.MessageReceived += MessageReceived;

			var consoleServer = new SysLogServer(listener);
			if (consoleServer.Start())
			{
				Console.WriteLine("Listener Started.  Press any key to stop listener");
				Console.WriteLine("Console server started.");

				Console.ReadLine();
			}
			
			consoleServer.Stop();
		}

	    private static void MessageReceived(MessageReceivedEventArgs e)
        {
            Console.WriteLine(Parser.Parse(e.SyslogMessage));
        }
    }
}
