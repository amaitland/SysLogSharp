using System.ComponentModel;
using System.ServiceProcess;

namespace Syslog.Service.Installer
{
	[RunInstaller(true)]
	public sealed class SysLogServiceInstaller : ServiceInstaller
	{
		public SysLogServiceInstaller()
		{
			Description = "SysLog Service implemented in C#";
			DisplayName = "SysLog Service";
			ServiceName = "SysLog Service";
			StartType = ServiceStartMode.Automatic;
		}
	}
}
