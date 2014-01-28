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
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Syslog.Server.Config;

namespace Syslog.Server
{
	/// <summary>
	/// Contains methods for receiving syslog messages on a UDP socket.
	/// </summary>
    /// <remarks>The listener creates sockets that listen on a UDP port for syslog messages and calls the
    /// parser method, if any, that is associated with the IP address that sent the syslog message.
    /// Messages that do not match the RFC 3164 specification are discarded and the next message is processed.
    /// An instance of the <see cref="LogBuffer"/> is also maintained by the listener.
    /// </remarks>
	public sealed class Listener
	{
		public delegate void MessageReceivedEventHandler(MessageReceivedEventArgs e);
		public static event MessageReceivedEventHandler MessageReceived;

		private static Listener _instance = null;

		private LogBuffer buffer;
        private int logBufferFlushFrequency = 30;
		private Dictionary<string, MessageHandler> ipFilters = new Dictionary<string, MessageHandler>();
		private Dictionary<string, string[]> ipForwards = new Dictionary<string, string[]>();

		private IPAddress listenAddress;
		private Socket socket;
		private Socket sendSocket;
		private readonly int SYSLOG_PORT;
		private const int RECEIVE_BUFFER_SIZE = 1024;

		private byte[] receiveBuffer = new Byte[RECEIVE_BUFFER_SIZE];
		private EndPoint remoteEndpoint = null;
		private Regex msgRegex = new Regex(@"
(\<(?<PRI>\d{1,3})\>){0,1}
(?<HDR>
  (?<TIMESTAMP>
	(?<MMM>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s
	(?<DD>[ 0-9][0-9])\s
	(?<HH>[0-9]{2})\:(?<MM>[0-9]{2})\:(?<SS>[0-9]{2})
  )\s
  (?<HOSTNAME>
	[^ ]+?
  )\s
){0,1}
(?<MSG>.*)
", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Creates an instance of the listener
        /// </summary>
        /// <param name="listenIPAddress">A valid IPv4 address.</param>
        /// <param name="listenPort">A valid port value from 1 to 65535</param>
        private Listener(string listenIPAddress, int listenPort, int logFlushFrequency)
        {
            if (listenIPAddress == null)
            {
                throw new ArgumentNullException("listenIPAddress", "An IP Address is required.");
            }

            IPAddress tempIP = null;

            if (listenIPAddress.ToUpper() != "ANY" && !IPAddress.TryParse(listenIPAddress, out tempIP))
            {
                throw new ArgumentException("IP address is not valid.", "listenIPAddress");
            }

            if (listenPort < 0 || listenPort > 65535)
            {
                throw new ArgumentOutOfRangeException("listenPort", "listenPort must be between 1 and 65535.");
            }

            if (logFlushFrequency <= 0)
            {
                throw new ArgumentOutOfRangeException("logFlushFrequency", "logFlushFrequency must be greater than 0.");
            }

            if (tempIP == null)
            {
                tempIP = IPAddress.Any;
            }

            this.listenAddress = tempIP;
            this.SYSLOG_PORT = listenPort;
            this.logBufferFlushFrequency = logFlushFrequency;
		}

        /// <summary>
        /// Creates an instance of the Listener if one does not already exist.
        /// </summary>
        /// <param name="listenIPAddress">A valid IPv4 address.</param>
        /// <param name="listenPort">A valid port value from 1 to 65535.</param>
        /// <returns>Returns a Listner object.</returns>
        public static Listener CreateInstance(string listenIPAddress, int listenPort, int logFlushFrequency)
        {
            if (_instance == null)
            {
                _instance = new Listener(listenIPAddress, listenPort, logFlushFrequency);
            }

            return _instance;
        }

        /// <summary>
        /// Gets an instance of the Listener.
        /// </summary>
        /// <returns>Returns a Listner object.</returns>
		public static Listener GetInstance()
		{
			return _instance;
		}

        /// <summary>
        /// Creates a socket that waits for traffic and sets up the handler modules
        /// </summary>
        /// <returns>Returns true if the listener starts successfully.  Otherwise, false.</returns>
		public bool Start()
		{
			HandlerSection handlers;

            // Get the handlers collection
			try
			{
				handlers = (HandlerSection)System.Configuration.ConfigurationManager.GetSection("handlerSection");
			}
			catch (Exception ex)
			{
				EventLogger.LogEvent("Could not load configuration because: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);

				return false;
			}

            // Ensure that a socket needs to be initialized.
            if (this.socket != null) { return true; }

            // Ensure that a socket needs to be initialized and bound.
            if (this.socket != null && this.socket.IsBound) { return true; }

			try
			{
				this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			catch (Exception ex)
			{
				EventLogger.LogEvent("Could not create socket because: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);

				return false;
			}

            // Bind the socket to the specificed IP and port
			try
			{
				this.socket.Bind(new IPEndPoint(this.listenAddress, SYSLOG_PORT));
			}
			catch (Exception ex)
			{
				this.socket.Close();
				this.socket = null;

				EventLogger.LogEvent("Could not bind socket because: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);

				return false;
			}

            if (this.socket == null) { return false; }

            // Create a new LogBuffer that will be used to store messages until they are discarded or flushed to a persistent store.
			buffer = new LogBuffer(this.logBufferFlushFrequency);

			if (handlers != null)
			{
                // Load each module (handler) from the configuration file
				foreach (HandlerConfiguration handler in handlers.Handlers)
				{
					MessageHandler msgHandler = new MessageHandler(handler.AssemblyName, handler.ParserClassName,
								handler.StorageClassName, handler.ConnectionString);

                    // If the handler has a storage class then setup the buffer to temporarily store the messages
					if (handler.StorageClassName != null)
					{
						buffer.InitializeBuffer(msgHandler);
					}

                    // If the handler is configured for specific IP addresses, setup the IP handler lookup list
					if (handler.FilterIPAddresses != null)
					{
						string[] filters = handler.FilterIPAddresses.Split(',', ';');

						for (int i = 0; i < filters.Length; i++)
						{
							ipFilters.Add(filters[i], msgHandler);

                            // If the handler also has IP forwards set, add them to the IP Forwards lookup list
							if (handler.IPForwards != null && handler.IPForwards.Length > 0)
							{
								ipForwards.Add(filters[i], handler.IPForwards.Split(',', ';'));
							}
						}
					}
				}
			}

            // If any handler has an IP forward setup, create a send socket that will be used forward messages
			if (ipForwards.Count > 0)
			{
				try
				{
					sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					sendSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
				}
				catch (Exception ex)
				{

				}
			}

			try
			{
                // Start the listen operation on the socket
				RegisterReceiveOperation();
			}
			catch (Exception ex)
			{
				EventLogger.LogEvent("Could not register socket on data received event because: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
				return false;
			}

			return true;
		}

        /// <summary>
        /// Creates a callback for the socket receive event
        /// </summary>
        /// <returns>Returns bool if the callback is setup successfully.</returns>
		public bool RegisterReceiveOperation()
		{
            // Ensure that the listener socket is still alive
            if (this.socket == null) { return false; }

			try
			{
                // receive from anybody
				this.remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

				EndPoint ep = (EndPoint)this.remoteEndpoint;

                // Setup the receive buffer to be used when a message is received
				this.receiveBuffer = new byte[RECEIVE_BUFFER_SIZE]; // nice and big receive buffer

                // Setup the receive callback
				this.socket.BeginReceiveFrom(receiveBuffer, 0, RECEIVE_BUFFER_SIZE, SocketFlags.None, ref this.remoteEndpoint,
					new AsyncCallback(ReceiveCallback), this.socket);
			}
			catch (Exception ex)
			{
				this.socket.Close();
				this.socket = null;

				EventLogger.LogEvent("Could not add callback method to the socket because: " + ex.Message,
					 System.Diagnostics.EventLogEntryType.Warning);
			}

			return true;
		}

        /// <summary>
        /// Handles the result of send (message forward) operation.
        /// </summary>
        /// <param name="result">The result of a send operatoin.</param>
		private void SendCallback(IAsyncResult result)
		{
			Socket sock = (Socket)result.AsyncState;

			if (sock != null)
			{
				sock.EndSendTo(result);
			}
		}

        /// <summary>
        /// Processes a message received event.
        /// </summary>
        /// <param name="result">The result of a receive event.</param>
		private void ReceiveCallback(IAsyncResult result)
		{
			// get a reference to the socket on which the message was received
			Socket sock = (Socket)result.AsyncState;

			EndPoint ep = null;
            IPEndPoint remoteEP = null;

            // variable to store received data length
            int inlen;

			remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

            // Gather information about the message and the sender
			try
			{
				ep = (EndPoint)remoteEndpoint;
				inlen = sock.EndReceiveFrom(result, ref ep);
				remoteEP = (IPEndPoint)ep;
			}
			catch (Exception ex)
			{
				// only post messages if class socket reference is not null
				// in all other cases, the socket has been terminated
				if (this.socket != null)
				{
					EventLogger.LogEvent("Receive operation failed with message: " + ex.Message,
						System.Diagnostics.EventLogEntryType.Warning);
				}
				inlen = -1;
			}

			// if socket has been closed, ignore received data and return
            if (this.socket == null) { return; }

			// check that received data is long enough
			if (inlen <= 0)
			{
				// request next packet
				RegisterReceiveOperation();
				return;
			}

            // If an IP forward is defined for the source of this message, forward the message to the specified IP's
			if (ipForwards.ContainsKey(remoteEP.Address.ToString()))
			{
				if (this.socket != null)
				{
					foreach (string ipAddress in ipForwards[remoteEP.Address.ToString()])
					{
						byte[] sendBuffer = new byte[this.receiveBuffer.Length];
						this.receiveBuffer.CopyTo(sendBuffer, 0);

						this.sendSocket.BeginSendTo(sendBuffer, 0, inlen, SocketFlags.None,
						   new IPEndPoint(IPAddress.Parse(ipAddress), 514), new AsyncCallback(SendCallback), sendSocket);
					}
				}
			}

			string packet = null;

            // Get the human readable text of the message to process
			try
			{
				packet = System.Text.Encoding.ASCII.GetString(receiveBuffer, 0, inlen);
			}
			catch (Exception ex)
			{
				EventLogger.LogEvent("Could not parse packet to string because: " + ex.Message,
						System.Diagnostics.EventLogEntryType.Warning);
			}

            // Run the regular expression against the message text to extract the groups
			Match m = msgRegex.Match(packet);

			//If a match is not found the message is not valid
			if (m != null && !string.IsNullOrEmpty(packet))
			{
				//parse PRI section into a priority value
				int pri;
				int priority = int.TryParse(m.Groups["PRI"].Value, out pri) ? pri : 0;

				//parse the HEADER section - contains TIMESTAMP and HOSTNAME
				string hostname = null;
				Nullable<DateTime> timestamp = null;

                // Get the timestamp and hostname from the header of the message
				if (!string.IsNullOrEmpty(m.Groups["HDR"].Value))
				{
					if (!string.IsNullOrEmpty(m.Groups["TIMESTAMP"].Value))
					{
						try
						{
							timestamp = new DateTime(
							  DateTime.Now.Year,
							  MonthNumber(m.Groups["MMM"].Value),
							  int.Parse(m.Groups["DD"].Value),
							  int.Parse(m.Groups["HH"].Value),
							  int.Parse(m.Groups["MM"].Value),
							  int.Parse(m.Groups["SS"].Value)
							  );
						}
						catch (ArgumentException)
						{
							//Ignore bad timestamp args.
						}
					}

					if (!string.IsNullOrEmpty(m.Groups["HOSTNAME"].Value))
					{
						hostname = m.Groups["HOSTNAME"].Value;
					}
				}

				if (!timestamp.HasValue)
				{
					//add timestamp as per RFC3164
					timestamp = DateTime.Now;
				}

				if (string.IsNullOrEmpty(hostname))
				{
					hostname = ep.ToString();
				}

				string message = null;

                // Get the message text part of the message if it was found
				if ((m.Groups["MSG"].Value) != null)
				{
					message = m.Groups["MSG"].Value;

					try
					{
						SyslogMessage sm = new SyslogMessage(priority, timestamp.Value, hostname, message);

                        // Ensure that a handler is defined for the MessageReceived event
						if (MessageReceived != null)
						{
							MessageReceived(new MessageReceivedEventArgs(sm));
						}

						//If the message is from an IP not listed in any filter do not process it
						if (!ipFilters.ContainsKey(remoteEP.Address.ToString()))
						{
							RegisterReceiveOperation();
							return;
						}

						string[] parsedMsg = null;
						if (ipFilters[remoteEP.Address.ToString()].ParserClassName != null)
						{
							try
							{
                                // Parse the message using the parser defined for IP from where the message came
								parsedMsg = ipFilters[remoteEP.Address.ToString()].GetParser().Parse(sm);
							}
							catch (Exception ex)
							{
								EventLogger.LogEvent("Could not get parser or parse message for ip " + remoteEP.Address.ToString()
									+ " because: " + ex.Message, System.Diagnostics.EventLogEntryType.Warning);
							}
						}

                        // Add the message to the LogBuffer if a storage Class is defined and message was parsed successfully.
						if (parsedMsg != null && buffer != null && ipFilters[remoteEP.Address.ToString()].StorerClassName != null)
						{
							buffer.AddEntry(ipFilters[remoteEP.Address.ToString()].AssemblyName, parsedMsg);
						}
					}
					catch (Exception ex)
					{
						EventLogger.LogEvent("Could not create new SyslogMessage because: " + ex.Message,
						System.Diagnostics.EventLogEntryType.Warning);
					}
				}

			}

            // Return the socket to the listen state
			RegisterReceiveOperation();
		}

        /// <summary>
        /// Stop listening on the socket and flush the <see cref="LogBuffer"/>
        /// </summary>
		public void Stop()
		{
			if (this.socket != null)
			{
				this.socket.Close();
				this.socket = null;
			}

			if (this.sendSocket != null)
			{
				this.sendSocket.Close();
				this.sendSocket = null;
			}

			if (buffer != null)
			{
				buffer.Flush();
			}
		}

        /// <summary>
        /// Convert a month short name into a number.
        /// </summary>
        /// <param name="monthName">Short name of a month.</param>
        /// <returns>The number of a month.</returns>
		private static int MonthNumber(string monthName)
		{
			switch (monthName.ToLower().Substring(0, 3))
			{
				case "jan": return 1;
				case "feb": return 2;
				case "mar": return 3;
				case "apr": return 4;
				case "may": return 5;
				case "jun": return 6;
				case "jul": return 7;
				case "aug": return 8;
				case "sep": return 9;
				case "oct": return 10;
				case "nov": return 11;
				case "dec": return 12;
				default:
					throw new Exception("Unrecognised month name: " + monthName);
			}
		}
	}
}
