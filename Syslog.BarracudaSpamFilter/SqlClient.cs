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
using System.Data.SqlClient;
using Syslog.Server;

namespace Syslog.BarracudaSpamFilter
{
    /// <summary>
    /// Class used to store messages to a Microsoft SQL Server
    /// </summary>
    public class SqlClient : DatabaseStorer, IStorer
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="connectionString">A valid SQL Server connection string.</param>
        public SqlClient(string connectionString) : base(connectionString)
        {

        }

        /// <summary>
        /// Processes the queue of messages to a data store.
        /// </summary>
        /// <param name="buffer">The <see cref="Queue<string[]>"/> of messages.</param>
        internal void ProcessBuffer(Queue<string[]> buffer)
        {
            ((IStorer)this).StoreMessages(buffer);
        }


        #region IStorer Members
        /// <summary>
        /// Processes an enumerable list of messages to a data store.
        /// </summary>
        /// <param name="messages">An enumerable type of messages.</param>
        /// <returns>Return true if processes was successful.</returns>
        bool IStorer.StoreMessages(IEnumerable<string[]> messages)
        {
            bool ok = true;

            IEnumerator<string[]> enumer = null;
            SqlConnection conn = new SqlConnection(base.ConnectionString);
            SqlCommand cmd = new SqlCommand(string.Empty, conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "dbo.InsertBarracudaSpamFilterMessage";
            try
            {
                conn.Open();

                try
                {
                    using (enumer = messages.GetEnumerator())
                    {

                        while (enumer.MoveNext())
                        {
                            try
                            {
                                cmd.Parameters.Clear();

                                cmd.Parameters.AddWithValue("@MsgDateTime", enumer.Current[0]);
                                cmd.Parameters.AddWithValue("@IP", enumer.Current[1]);
                                cmd.Parameters.AddWithValue("@ID", enumer.Current[2]);
                                cmd.Parameters.AddWithValue("@StartTime", enumer.Current[3]);
                                cmd.Parameters.AddWithValue("@EndTime", enumer.Current[4]);
                                cmd.Parameters.AddWithValue("@Encryption", enumer.Current[5]);
                                cmd.Parameters.AddWithValue("@Sender", enumer.Current[6]);
                                cmd.Parameters.AddWithValue("@Recipient", enumer.Current[7]);
                                cmd.Parameters.AddWithValue("@Score", enumer.Current[8]);
                                cmd.Parameters.AddWithValue("@Action", enumer.Current[9]);
                                cmd.Parameters.AddWithValue("@Reason", enumer.Current[10]);
                                cmd.Parameters.AddWithValue("@ReasonExtra", enumer.Current[11]);
                                cmd.Parameters.AddWithValue("@Subject", enumer.Current[12]);

                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                ok = false;
                            }
                        }
                    }
                }
                finally
                {
                    if (conn != null)
                    {
                        conn.Close();
                    }

                    if (enumer != null)
                    {
                        enumer.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
            }

            return ok;
        }

        #endregion
    }
}
