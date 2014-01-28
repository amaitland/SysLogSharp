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
using System.ServiceProcess;
using Syslog.Service.Installer;

namespace Syslog.Service
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		public static void Main(string[] args)
        {
			Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

			if (args.Length > 0)
			{
				// Parse the command line args
				var install = ParseCommandLine(args);
				if (install == true)
				{
					InstallationManager.Install(args);
				}
				else if (install == false)
				{
					InstallationManager.Uninstall(args);
				}
				else
				{
					Console.WriteLine("Invalid command line args -i for install or -u for uninstall");
				}
			}
			else
			{
				var servicesToRun = new ServiceBase[] { new SyslogSharpService() };
				ServiceBase.Run(servicesToRun);
			}
        }

		private static bool? ParseCommandLine(IEnumerable<string> commandLine)
		{
			foreach (var arg in commandLine)
			{
				var argument = arg.ToLower();
				switch (argument)
				{
					case "-i":
					case "-install":
					{
						return true;
					}
					case "-u":
					case "-uninstall":
					{
						return false;
					}
				}
			}

			return null;
		}
    }
}