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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Syslog.Server;

namespace Syslog.Console
{
    public partial class Main : Form
    {
        public delegate void HandleMessageReceived(SyslogMessage e);

        ConsoleServerClient client;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            client = new ConsoleServerClient();
            client.Connect();
            // Register the remoted event handler
            MessageReceivedSink.OnServerMessageReceived += MessageReceivedSink_OnServerMessageReceived;
        }

        void MessageReceivedSink_OnServerMessageReceived(SyslogMessage message)
        {
            if (!IsDisposed)
            {
                if (!Disposing)
                {
                    // Use Invoke to ensure that the event is fired on the forms thread.
                    Invoke(new HandleMessageReceived(Listener_MessageReceived), message);
                }
                else
                {
                    client.Disconnect();
                }
            }
        }

        void Listener_MessageReceived(SyslogMessage e)
        {
            if (e != null)
            {
                // Add syslog message to the top of the grid.
                dataGridView1.Rows.Insert(0,e.Timestamp.ToShortDateString(),
                    e.Timestamp.ToShortTimeString(),
                    e.Hostname,
                    e.Message);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                client.Disconnect();
            }
            catch (Exception)
            {

            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!client.IsPaused)
            {
                client.PauseReceiver();
                ((ToolStripButton)sender).Text = "Unlock Screen";
            }
            else
            {
                client.ResumeReceiver();
                ((ToolStripButton)sender).Text = "Lock Screen";
            }
        }

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
    }
}