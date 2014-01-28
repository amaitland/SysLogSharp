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
using System.Configuration;

namespace Syslog.Server.Config
{
    /// <summary>
    /// Definition of the <see cref="HandlerSection"/> configuration section.
    /// </summary>
    public class HandlerSection : ConfigurationSection
    {
        public HandlerSection() : base()
        { }

        [ConfigurationProperty("handlers", IsDefaultCollection=false),
       ConfigurationCollection(typeof(HandlerCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "handlerSection")]
        public HandlerCollection Handlers
        {
            get { return base["handlers"] as HandlerCollection; }
        }
    }
}
