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

namespace Syslog.GenericFilter
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
			throw new NotImplementedException();

            using(var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(string.Empty, conn) {CommandType = System.Data.CommandType.StoredProcedure, CommandText = "dbo.InsertBarracudaWebFilterMessage"})
            {
	            try
	            {
		            conn.Open();

		            foreach (var message in messages)
		            {
			            cmd.Parameters.AddWithValue("@MsgDateTime", message[0]);
			            cmd.Parameters.AddWithValue("@SourceIP", message[1]);
			            cmd.Parameters.AddWithValue("@DestIPOrDns", message[2]);
			            cmd.Parameters.AddWithValue("@ContentType", message[3]);
			            cmd.Parameters.AddWithValue("@URL", message[4]);
			            cmd.Parameters.AddWithValue("@Action", message[5]);
			            cmd.Parameters.AddWithValue("@Reason", message[6]);
			            cmd.Parameters.AddWithValue("@FormatVersion", message[7]);
			            cmd.Parameters.AddWithValue("@MatchFlag", message[8]);
			            cmd.Parameters.AddWithValue("@TQFlag", message[9]);
			            cmd.Parameters.AddWithValue("@ActionType", message[10]);
			            cmd.Parameters.AddWithValue("@SrcType", message[11]);
			            cmd.Parameters.AddWithValue("@SrcDetail", message[12]);
			            cmd.Parameters.AddWithValue("@DestType", message[13]);
			            cmd.Parameters.AddWithValue("@SpyType", message[14]);
			            cmd.Parameters.AddWithValue("@MatchedPart", message[15]);
			            cmd.Parameters.AddWithValue("@MatchedCategory", message[16]);
			            cmd.Parameters.AddWithValue("@LocalUser", message[17]);

			            cmd.ExecuteNonQuery();
		            }
		            return true;
	            }
	            catch (Exception ex)
	            {
		            return false;
	            }
            }
        }
    }
}
