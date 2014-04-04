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

namespace Syslog.Server
{
    /// <summary>
    /// Message source / use
    /// </summary>
    [Serializable]
    public enum FacilityCode
    {
        None = -1,
        KernelMessage = 0,
        UserLevelMessage = 1,
        MailSystem = 2,
        System = 3,
        SecurityAuthMessage = 4,
        InternalSyslogGeneratedMessage = 5,
        LinePrinter = 6,
        NetworkNews = 7,
        UUCP = 8,
        Clock = 9,
        SecurityAuthMessage2 = 10,
        FTP = 11,
        NTP = 12,
        LogAudit = 13,
        LogAlert = 14,
        Clock2 = 15,
        LocalUse0 = 16,
        LocalUse1 = 17,
        LocalUse2 = 18,
        LocalUse3 = 19,
        LocalUse4 = 20,
        LocalUse5 = 21,
        LocalUse6 = 22,
        LocalUse7 = 23
    }
}
