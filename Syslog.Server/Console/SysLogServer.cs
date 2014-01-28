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
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace Syslog.Server.Console
{
    public delegate void MessageReceivedCallback(SyslogMessage message);

    /// <summary>
    /// Methods for setting up the services remoting communication channel.
    /// </summary>
    public class SysLogServer
    {
		private readonly IListener _listener;
	    private IpcChannel _channel;

        /// <summary>
        /// Creates a new instace of the class.
        /// </summary>
		public SysLogServer(IListener listener)
        {
	        _listener = listener;

			if (_listener == null)
			{
				throw new ArgumentNullException("listener");
			}
        }

	    /// <summary>
        /// Sets up the services remoting channel
        /// </summary>
        public bool Start()
        {
			if (!_listener.Start())
			{
				return false;
			}
			
            try
            {
                var props = new Hashtable();
                props["typeFilterLevel"] = "Full";

                // Both formatters only use the typeFilterLevel property
                var cliFormatter = new BinaryClientFormatterSinkProvider(props, null);
                var srvFormatter = new BinaryServerFormatterSinkProvider(props, null);

                // The channel requires these to be set that it can found by name by clients
                props["name"] = "SyslogConsole";
                props["portName"] = "SyslogConsole";
                props["authorizedGroup"] = "Everyone";

                // Create the channel
                _channel = new IpcChannel(props, cliFormatter, srvFormatter) {IsSecured = false};

	            // Register the channel in the Windows IPC list
                ChannelServices.RegisterChannel(_channel, false);

                // Register the channel for remoting use
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientMethods), "Server", WellKnownObjectMode.Singleton);

                // Assign the event to a handler
				_listener.MessageReceived += Listener_MessageReceived;

	            return true;
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Could not create a named pipe because: " + ex.Message + Environment.NewLine + "Communication with the GUI console will be disabled.",
                    System.Diagnostics.EventLogEntryType.Warning);
            }

		    return false;
        }

        /// <summary>
        /// Raise the message received event for listening clients.
        /// </summary>
        /// <param name="e">Event data.</param>
        private void Listener_MessageReceived(MessageReceivedEventArgs e)
        {
            ClientMethods.FireNewMessageReceived(e.SyslogMessage);
        }

        /// <summary>
        /// Unregisters and tears down the services remoting channel.
        /// </summary>
        public void Stop()
        {
			if (_listener != null)
			{
				_listener.Stop();
			}

            if (_channel != null)
            {
                try
                {
                    ChannelServices.UnregisterChannel(_channel);
                }
                catch (Exception ex)
                {
                    EventLogger.LogEvent("Could not unregister IPC channel because: " + ex.Message, System.Diagnostics.EventLogEntryType.Warning);
                }
            }
        }
    }
}
