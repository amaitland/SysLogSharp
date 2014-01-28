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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using Syslog.Server.Console;

namespace Syslog.Console
{
    /// <summary>
    /// Methods for communicating with the service's remoting channel.
    /// </summary>
    public class ConsoleServerClient
    {
        IpcChannel channel;
        MessageReceivedCallbackSink sink;
        ClientMethods remoter;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public ConsoleServerClient()
        {
        }

        private bool isConnected;
        /// <summary>
        /// Gets or sets the connected state of the client.
        /// </summary>
        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }

        private bool isPaused;
        /// <summary>
        /// Gets or sets the paused state of client
        /// </summary>
        /// <remarks>If <see cref="IsPaused"/> is true then the client does not receive event from the service.</remarks>
        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        /// <summary>
        /// Makes a connection to the service's remoting channel.
        /// </summary>
        public void Connect()
        {
            if (this.isConnected) { return; }

            try
            {
                System.Collections.Hashtable props = new System.Collections.Hashtable();
                props["typeFilterLevel"] = "Full";

                // Both formatters only use the typeFilterLevel property
                BinaryClientFormatterSinkProvider cliFormatter = new BinaryClientFormatterSinkProvider(props, null);
                BinaryServerFormatterSinkProvider srvFormatter = new BinaryServerFormatterSinkProvider(props, null);

                // The channel requires these to be set that it can found by name by the service
                props["name"] = "ConsoleClient";
                props["portName"] = "ConsoleClient";
                props["authorizedGroup"] = "Everyone";

                // Create the channel
                channel = new IpcChannel(props, cliFormatter, srvFormatter);

                // Register the channel for remoting use
                System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);

                // Create the refence to the service's remoting channel
                remoter = (ClientMethods)Activator.GetObject(typeof(ClientMethods), "ipc://SyslogConsole/Server");

                // Register the MessageReceivedCallback event on the handler
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(MessageReceivedCallbackSink),
                    "ServerEvents", WellKnownObjectMode.Singleton);

                // Setup the event subscription
                sink = new MessageReceivedSink();
                remoter.MessageHandled += new MessageReceivedCallback(sink.FireMessageReceived);

                this.isConnected = true;
            }
            catch
            {
                this.isConnected = false;
            }
        }

        /// <summary>
        /// Removes the <see cref="MessageReceivedCallback"/> event subscription.
        /// </summary>
        public void PauseReceiver()
        {
            try
            {
                remoter.MessageHandled -= new MessageReceivedCallback(sink.FireMessageReceived);
            }
            catch (Exception)
            {

            }
            finally
            {
                this.isPaused = true;
            }
        }

        /// <summary>
        /// Adds the <see cref="MessageReceivedCallback"/> event subscription.
        /// </summary>
        public void ResumeReceiver()
        {
            try
            {
                remoter.MessageHandled += new MessageReceivedCallback(sink.FireMessageReceived);
            }
            catch (Exception)
            {

            }
            finally
            {
                this.isPaused = false;
            }
        }

        /// <summary>
        /// Disconnects the client from the services remoting channel.
        /// </summary>
        public void Disconnect()
        {
            if (!this.isConnected) return;
            try
            {
                try
                {
                    // Unregister the event subscription
                    remoter.MessageHandled -= new MessageReceivedCallback(sink.FireMessageReceived);
                }
                catch (RemotingException)
                {

                }

                try
                {
                    if (channel != null)
                    {
                        ChannelServices.UnregisterChannel(channel);
                        channel = null;
                    }
                }
                catch (RemotingException)
                {

                }
            }
            finally
            {
                this.isConnected = false;
            }
        }
    }
}
