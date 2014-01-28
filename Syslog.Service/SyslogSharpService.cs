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
using System.ServiceProcess;
using Syslog.Server;
using Syslog.Server.Console;

namespace Syslog.Service
{
    public partial class SyslogSharpService : ServiceBase
    {
        private Listener _listener;
        private SysLogServer _sysLogServer;

        public SyslogSharpService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_listener == null)
            {

				_listener = new Listener(System.Configuration.ConfigurationManager.AppSettings["listenIPAddress"],
                    Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["listenPort"]),
                    Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["bufferFlushFrequency"]));
            }

            if (!_listener.Start())
            {
                OnStop();

                return;
            }

            if (_sysLogServer == null)
            {
				_sysLogServer = new SysLogServer(_listener);
            }

            _sysLogServer.Start();
        }

        protected override void OnStop()
        {
            if (_listener != null)
            {
                _listener.Stop();
            }

            if (_sysLogServer != null)
            {
                _sysLogServer.Stop();
            }
        }
    }
}
