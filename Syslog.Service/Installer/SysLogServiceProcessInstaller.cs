using System.ComponentModel;
using System.ServiceProcess;

namespace Syslog.Service.Installer
{
	[RunInstaller(true)]
	public sealed class SysLogServiceProcessInstaller : ServiceProcessInstaller
	{
		public SysLogServiceProcessInstaller()
		{
			Account = ServiceAccount.LocalSystem;
		}
	}
}
