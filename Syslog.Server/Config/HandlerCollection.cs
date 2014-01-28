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
    /// Collection of handlers defined in the configuration file
    /// </summary>
    public class HandlerCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets the type of configuration collection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        /// <summary>
        /// Creates a new <see cref="HandlerConfiguration"/>.
        /// </summary>
        /// <returns>Returns a new <see cref="HandlerConfiguration"/>.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new HandlerConfiguration();
        }

        /// <summary>
        /// Gets the AssemblyName from the <see cref="HandlerConfiguration"/>.
        /// </summary>
        /// <param name="element">The <see cref="HandlerConfiguration"/>.</param>
        /// <returns>Returns the AssemblyName.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HandlerConfiguration)element).AssemblyName;
        }

        /// <summary>
        /// Gets a <see cref="HandlerConfiguration"/> at the given index.
        /// </summary>
        /// <param name="index">The index of the element to retrieve.</param>
        /// <returns>Returns a <see cref="HandlerConfiguration"/>.</returns>
        public HandlerConfiguration this[int index]
        {
            get { return (HandlerConfiguration)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Removes the given element from the collection.
        /// </summary>
        /// <param name="element">The elemnt to remove.</param>
        public void Remove(HandlerConfiguration element)
        {
            BaseRemove(element.AssemblyName);
        }

        /// <summary>
        /// Removes the element with the given AssemblyName.
        /// </summary>
        /// <param name="name">The AssemblyName to remove.</param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        /// <summary>
        /// Removes the element the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }
}
