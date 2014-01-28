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
using System.Collections.Specialized;
using System.Text;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Syslog.Server
{
    /// <summary>
    /// Temporary in-memory store of parsed messages
    /// </summary>
    internal class LogBuffer
    {
        // List of buffers with a list of messages to store
        private Dictionary<string, List<string[]>> buffers = new Dictionary<string, List<string[]>>();
        // List of message handlers
        private Dictionary<string, MessageHandler> handlers = new Dictionary<string, MessageHandler>();
        private Timer flushTimer = null;

        /// <summary>
        /// Creates a new instance of the log buffer and starts the flush timer
        /// </summary>
        /// <param name="flushFrequency">The time, in seconds, between buffer flushes.</param>
        /// <remarks>
        /// The flush timer activates every 30 seconds by default.
        /// </remarks>
        public LogBuffer(int flushFrequency)
        {
            if (flushFrequency > 0)
            {
                FlushFrequency = flushFrequency;
            }
            else
            {
                FlushFrequency = 30000;
            }

            flushTimer = new Timer(new TimerCallback(FlushTimer_Tick), null, this.flushFrequency, this.flushFrequency);
        }

        /// <summary>
        /// Creates a new instance of the log buffer and starts the flush timer
        /// </summary>
        /// <remarks>
        /// The flush timer activates every 30 seconds by default.
        /// </remarks>
        public LogBuffer()
            : this(30)
        {

        }

        private int flushFrequency;
        /// <summary>
        /// Gets or sets the time, in seconds, bewteen log flushes.
        /// </summary>
        public int FlushFrequency
        {
            get { return flushFrequency; }
            set
            {
                flushFrequency = value * 1000;
                if (flushTimer != null)
                {
                    flushTimer.Change(0, flushFrequency);
                }
            }
        }


        /// <summary>
        /// Sets up the buffer for the given <paramref name="handler"/>.
        /// </summary>
        /// <param name="assembly">Name of the assembly the buffer will handle </param>
        public void InitializeBuffer(MessageHandler handler)
        {
            if (!buffers.ContainsKey(handler.AssemblyName))
            {
                buffers.Add(handler.AssemblyName, new List<string[]>());
            }

            if (!handlers.ContainsKey(handler.AssemblyName))
            {
                handlers.Add(handler.AssemblyName, handler);
            }
        }

        /// <summary>
        /// Adds parsed message to the specified hanlder buffer.
        /// </summary>
        /// <param name="bufferName">The name of the buffer for the handler.</param>
        /// <param name="entry">The parsed message.</param>
        public void AddEntry(string bufferName, string[] entry)
        {
            try
            {
                // Lock the buffer while adding data so that other threads do not try to write to the buffer
                lock (buffers[bufferName])
                {
                    if (buffers.ContainsKey(bufferName))
                    {
                        buffers[bufferName].Add(entry);
                    }
                    // Notify waiting threads that this object is available
                    Monitor.Pulse(buffers[bufferName]);
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogEvent("Could not add entry to the buffer because: " + ex.Message,
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Handles the Timer.Tick event.
        /// </summary>
        /// <param name="state">The state of the timer.</param>
        private void FlushTimer_Tick(object state)
        {
            Flush();
        }

        /// <summary>
        /// Flushes all buffers that contain data by calling each buffer handler's StoreMessages method.
        /// </summary>
        public void Flush()
        {
            // Get a fast, one-way, read-only enumer of the buffers' keys
            Dictionary<string, List<string[]>>.KeyCollection.Enumerator enumer = buffers.Keys.GetEnumerator();
            List<string[]> currentVal, copiedList = null;

            // Ensure that a buffer is defined
            if (enumer.Current == null)
            { enumer.MoveNext(); }

            while (enumer.Current != null)
            {
                currentVal = buffers[enumer.Current];

                // Lock the buffer while a copy is made so that new entries being added do not block or fail the copy.
                lock (currentVal)
                {
                    try
                    {
                        if (currentVal.Count > 0)
                        {
                            copiedList = DeepCopy<List<string[]>>(currentVal);

                            // Ensure that the copied object contains the same number of messages.
                            if (copiedList.Count != currentVal.Count)
                            {
                                EventLogger.LogEvent("An error occured while storing messages for " + enumer.Current,
                                            System.Diagnostics.EventLogEntryType.Error);
                            }

                            // Clear the buffer.
                            currentVal.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogEvent("Could not store messages from " + enumer.Current + " because: " + ex.Message,
                             System.Diagnostics.EventLogEntryType.Error);
                    }
                    finally
                    {
                        Monitor.Pulse(currentVal);
                    }
                }

                if (copiedList != null && copiedList.Count > 0)
                {
                    try
                    {
                        // Get a refence to the storer interface of the handler
                        var storer = handlers[enumer.Current].GetStorer();

                        if (storer != null)
                        {
                            //Process the list of messages
                            if (!storer.StoreMessages(copiedList))
                            {
                                EventLogger.LogEvent("An error occured while storing messages for " + enumer.Current,
                                    System.Diagnostics.EventLogEntryType.Warning);
                            }
                            else
                            {
                                copiedList.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogEvent("Could not store message from " + enumer.Current + " because: " + ex.Message,
                             System.Diagnostics.EventLogEntryType.Warning);
                    }
                }

                enumer.MoveNext();
            }
        }

        /// <summary>
        /// Performs a deep copy of an object.
        /// </summary>
        /// <typeparam name="T">The type of object to copy.</typeparam>
        /// <param name="obj">The object to copy.</param>
        /// <returns>A copy of the object.</returns>
        /// <remarks>Creates an actual copy of memory and not just a copy of memory references using binary serialization in memory
        /// as it is quick and simple.</remarks>
        public static T DeepCopy<T>(T obj)
        {
            object result = null;

            if (obj != null)
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    ms.Position = 0;

                    result = (T)formatter.Deserialize(ms);
                    ms.Close();
                    ms.Dispose();
                }
            }

            return (T)result;
        }
    }
}
