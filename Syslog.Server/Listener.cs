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
	public sealed class Listener : IListener
	{
		public event Action<MessageReceivedEventArgs> MessageReceived;

		private const int ReceiveBufferSize = 1024;

		private LogBuffer _buffer;
        private readonly int _logBufferFlushFrequency = 30;
		private readonly Dictionary<string, MessageHandler> _ipFilters = new Dictionary<string, MessageHandler>();
		private readonly Dictionary<string, string[]> _ipForwards = new Dictionary<string, string[]>();

		private readonly IPAddress _listenAddress;
		private Socket _socket;
		private Socket _sendSocket;
		private readonly int _listenPort;
		
		private byte[] _receiveBuffer = new Byte[ReceiveBufferSize];
		private EndPoint _remoteEndpoint;
		private readonly Regex _msgRegex = new Regex(@"
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
		/// <param name="listenIpAddress">A valid IPv4 address.</param>
		/// <param name="listenPort">A valid port value from 1 to 65535</param>
		/// <param name="logFlushFrequency">Frequence to flush the log file default 30</param>
		public Listener(IPAddress listenIpAddress, int listenPort, int logFlushFrequency)
        {
            if (listenIpAddress == null)
            {
                throw new ArgumentNullException("listenIpAddress", "An IP Address is required.");
            }
            if (listenIpAddress == null)
            {
                throw new ArgumentException("IP address is not valid.", "listenIpAddress");
            }

            if (listenPort < 0 || listenPort > 65535)
            {
                throw new ArgumentOutOfRangeException("listenPort", "listenPort must be between 1 and 65535.");
            }

            if (logFlushFrequency <= 0)
            {
                throw new ArgumentOutOfRangeException("logFlushFrequency", "logFlushFrequency must be greater than 0.");
            }

			_listenAddress = listenIpAddress;
            _listenPort = listenPort;
            _logBufferFlushFrequency = logFlushFrequency;
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
            if (_socket != null) { return true; }

            // Ensure that a socket needs to be initialized and bound.
            if (_socket != null && _socket.IsBound) { return true; }

			try
			{
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			catch (Exception ex)
			{
				EventLogger.LogEvent("Could not create socket because: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);

				return false;
			}

            // Bind the socket to the specificed IP and port
			try
			{
				_socket.Bind(new IPEndPoint(_listenAddress, _listenPort));
			}
			catch (Exception ex)
			{
				_socket.Close();
				_socket = null;

				EventLogger.LogEvent("Could not bind socket because: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);

				return false;
			}

            if (_socket == null) { return false; }

            // Create a new LogBuffer that will be used to store messages until they are discarded or flushed to a persistent store.
			_buffer = new LogBuffer(_logBufferFlushFrequency);

			if (handlers != null)
			{
                // Load each module (handler) from the configuration file
				foreach (HandlerConfiguration handler in handlers.Handlers)
				{
					var msgHandler = new MessageHandler(handler.AssemblyName, handler.ParserClassName, handler.StorageClassName, handler.ConnectionString);

                    // If the handler has a storage class then setup the buffer to temporarily store the messages
					if (handler.StorageClassName != null)
					{
						_buffer.InitHandler(msgHandler);
					}

                    // If the handler is configured for specific IP addresses, setup the IP handler lookup list
					if (!string.IsNullOrEmpty(handler.FilterIPAddresses))
					{
						var filters = handler.FilterIPAddresses.Split(',', ';');

						foreach (var filter in filters)
						{
							_ipFilters.Add(filter, msgHandler);

							// If the handler also has IP forwards set, add them to the IP Forwards lookup list
							if (!string.IsNullOrEmpty(handler.IPForwards))
							{
								_ipForwards.Add(filter, handler.IPForwards.Split(',', ';'));
							}
						}
					}
				}
			}

            // If any handler has an IP forward setup, create a send socket that will be used forward messages
			if (_ipForwards.Count > 0)
			{
				try
				{
					_sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					_sendSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
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
            if (_socket == null) { return false; }

			try
			{
                // receive from anybody
				_remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

				var ep = _remoteEndpoint;

                // Setup the receive buffer to be used when a message is received
				_receiveBuffer = new byte[ReceiveBufferSize]; // nice and big receive buffer

                // Setup the receive callback
				_socket.BeginReceiveFrom(_receiveBuffer, 0, ReceiveBufferSize, SocketFlags.None, ref ep, ReceiveCallback, _socket);
			}
			catch (Exception ex)
			{
				_socket.Close();
				_socket = null;

				EventLogger.LogEvent("Could not add callback method to the socket because: " + ex.Message, System.Diagnostics.EventLogEntryType.Warning);
			}

			return true;
		}

        /// <summary>
        /// Handles the result of send (message forward) operation.
        /// </summary>
        /// <param name="result">The result of a send operatoin.</param>
		private static void SendCallback(IAsyncResult result)
		{
			var sock = (Socket)result.AsyncState;

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
            IPEndPoint remoteEp = null;

            // variable to store received data length
            int inlen;

			_remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

            // Gather information about the message and the sender
			try
			{
				ep = _remoteEndpoint;
				inlen = sock.EndReceiveFrom(result, ref ep);
				remoteEp = (IPEndPoint)ep;
			}
			catch (Exception ex)
			{
				// only post messages if class socket reference is not null
				// in all other cases, the socket has been terminated
				if (_socket != null)
				{
					EventLogger.LogEvent("Receive operation failed with message: " + ex.Message,
						System.Diagnostics.EventLogEntryType.Warning);
				}
				inlen = -1;
			}

			// if socket has been closed, ignore received data and return
            if (_socket == null) { return; }

			// check that received data is long enough
			if (inlen <= 0)
			{
				// request next packet
				RegisterReceiveOperation();
				return;
			}

            // If an IP forward is defined for the source of this message, forward the message to the specified IP's
			if (_ipForwards.ContainsKey(remoteEp.Address.ToString()))
			{
				if (_socket != null)
				{
					foreach (string ipAddress in _ipForwards[remoteEp.Address.ToString()])
					{
						var sendBuffer = new byte[_receiveBuffer.Length];
						_receiveBuffer.CopyTo(sendBuffer, 0);

						_sendSocket.BeginSendTo(sendBuffer, 0, inlen, SocketFlags.None,
						   new IPEndPoint(IPAddress.Parse(ipAddress), 514), new AsyncCallback(SendCallback), _sendSocket);
					}
				}
			}

			string packet = null;

            // Get the human readable text of the message to process
			try
			{
				packet = System.Text.Encoding.ASCII.GetString(_receiveBuffer, 0, inlen);
			}
			catch (Exception ex)
			{
				EventLogger.LogEvent("Could not parse packet to string because: " + ex.Message,
						System.Diagnostics.EventLogEntryType.Warning);
			}

            // Run the regular expression against the message text to extract the groups
			var m = _msgRegex.Match(packet);

			//If a match is not found the message is not valid
			if (m != null && !string.IsNullOrEmpty(packet))
			{
				//parse PRI section into a priority value
				int pri;
				var priority = int.TryParse(m.Groups["PRI"].Value, out pri) ? pri : 0;

				//parse the HEADER section - contains TIMESTAMP and HOSTNAME
				string hostname = null;
				DateTime? timestamp = null;

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

				// Get the message text part of the message if it was found
				if ((m.Groups["MSG"].Value) != null)
				{
					var message = m.Groups["MSG"].Value;

					try
					{
						var sm = new SyslogMessage(priority, timestamp.Value, hostname, message);

                        // Ensure that a handler is defined for the MessageReceived event
						if (MessageReceived != null)
						{
							MessageReceived(new MessageReceivedEventArgs(sm));
						}

						//If the message is from an IP not listed in any filter do not process it
						if (!_ipFilters.ContainsKey(remoteEp.Address.ToString()))
						{
							RegisterReceiveOperation();
							return;
						}

						string[] parsedMsg = null;
						if (_ipFilters[remoteEp.Address.ToString()].ParserClassName != null)
						{
							try
							{
                                // Parse the message using the parser defined for IP from where the message came
								parsedMsg = _ipFilters[remoteEp.Address.ToString()].GetParser().Parse(sm);
							}
							catch (Exception ex)
							{
								EventLogger.LogEvent("Could not get parser or parse message for ip " + remoteEp.Address.ToString()
									+ " because: " + ex.Message, System.Diagnostics.EventLogEntryType.Warning);
							}
						}

                        // Add the message to the LogBuffer if a storage Class is defined and message was parsed successfully.
						if (parsedMsg != null && _buffer != null && _ipFilters[remoteEp.Address.ToString()].StorerClassName != null)
						{
							_buffer.AddEntry(_ipFilters[remoteEp.Address.ToString()].AssemblyName, parsedMsg);
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
			if (_socket != null)
			{
				_socket.Close();
				_socket = null;
			}

			if (_sendSocket != null)
			{
				_sendSocket.Close();
				_sendSocket = null;
			}

			if (_buffer != null)
			{
				_buffer.Flush();
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
