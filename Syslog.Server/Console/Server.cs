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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace Syslog.Server.Console
{
    public delegate void MessageReceivedCallback(SyslogMessage message);

    /// <summary>
    /// Methods for setting up the services remoting communication channel.
    /// </summary>
    public class Server
    {
        IpcChannel channel = null;

        /// <summary>
        /// Creates a new instace of the class.
        /// </summary>
        public Server()
        {

        }
        
        /// <summary>
        /// Sets up the services remoting channel
        /// </summary>
        public void Start()
        {
            try
            {
                System.Collections.Hashtable props = new System.Collections.Hashtable();
                props["typeFilterLevel"] = "Full";

                // Both formatters only use the typeFilterLevel property
                BinaryClientFormatterSinkProvider cliFormatter = new BinaryClientFormatterSinkProvider(props, null);
                BinaryServerFormatterSinkProvider srvFormatter = new BinaryServerFormatterSinkProvider(props, null);

                // The channel requires these to be set that it can found by name by clients
                props["name"] = "SyslogConsole";
                props["portName"] = "SyslogConsole";
                props["authorizedGroup"] = "Everyone";

                // Create the channel
                channel = new IpcChannel(props, cliFormatter, srvFormatter);
                channel.IsSecured = false;

                // Register the channel in the Windows IPC list
                ChannelServices.RegisterChannel(channel, false);

                // Register the channel for remoting use
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientMethods), "Server", WellKnownObjectMode.Singleton);

                // Assign the event to a handler
                Listener.MessageReceived += new Listener.MessageReceivedEventHandler(Listener_MessageReceived);
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Could not create a named pipe because: " + ex.Message + Environment.NewLine + "Communication with the GUI console will be disabled.",
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Raise the message received event for listening clients.
        /// </summary>
        /// <param name="e">Event data.</param>
        void Listener_MessageReceived(MessageReceivedEventArgs e)
        {
            ClientMethods.FireNewMessageReceived(e.SyslogMessage);
        }

        /// <summary>
        /// Unregisters and tears down the services remoting channel.
        /// </summary>
        public void Stop()
        {
            if (channel != null)
            {
                try
                {
                    ChannelServices.UnregisterChannel(channel);
                }
                catch (Exception ex)
                {
                    EventLogger.LogEvent("Could not unregister IPC channel because: " + ex.Message, System.Diagnostics.EventLogEntryType.Warning);
                }
            }
        }
    }
}
