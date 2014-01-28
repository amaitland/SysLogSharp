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
using System.Data.SqlClient;
using Syslog.Server;

namespace Syslog.BarracudaWebFilter
{
    /// <summary>
    /// Class used to store messages to a Microsoft SQL Server
    /// </summary>
    public class SqlClient : AbstractDatabaseDataStore, IDataStore
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="connectionString">A valid SQL Server connection string.</param>
        public SqlClient(string connectionString) : base(connectionString)
        {

        }

        /// <summary>
        /// Processes an enumerable list of messages to a data store.
        /// </summary>
        /// <param name="messages">An enumerable type of messages.</param>
        /// <returns>Return true if processes was successful.</returns>
        public override bool StoreMessages(IEnumerable<string[]> messages)
        {
            bool ok = true;

            IEnumerator<string[]> enumer = null;
            SqlConnection conn = new SqlConnection(base.ConnectionString);
            SqlCommand cmd = new SqlCommand(string.Empty, conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "dbo.InsertBarracudaWebFilterMessage";
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
                                cmd.Parameters.AddWithValue("@SourceIP", enumer.Current[1]);
                                cmd.Parameters.AddWithValue("@DestIPOrDns", enumer.Current[2]);
                                cmd.Parameters.AddWithValue("@ContentType", enumer.Current[3]);
                                cmd.Parameters.AddWithValue("@URL", enumer.Current[4]);
                                cmd.Parameters.AddWithValue("@Action", enumer.Current[5]);
                                cmd.Parameters.AddWithValue("@Reason", enumer.Current[6]);
                                cmd.Parameters.AddWithValue("@FormatVersion", enumer.Current[7]);
                                cmd.Parameters.AddWithValue("@MatchFlag", enumer.Current[8]);
                                cmd.Parameters.AddWithValue("@TQFlag", enumer.Current[9]);
                                cmd.Parameters.AddWithValue("@ActionType", enumer.Current[10]);
                                cmd.Parameters.AddWithValue("@SrcType", enumer.Current[11]);
                                cmd.Parameters.AddWithValue("@SrcDetail", enumer.Current[12]);
                                cmd.Parameters.AddWithValue("@DestType", enumer.Current[13]);
                                cmd.Parameters.AddWithValue("@SpyType", enumer.Current[14]);
                                cmd.Parameters.AddWithValue("@MatchedPart", enumer.Current[15]);
                                cmd.Parameters.AddWithValue("@MatchedCategory", enumer.Current[16]);
                                cmd.Parameters.AddWithValue("@LocalUser", enumer.Current[17]);

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
    }
}
