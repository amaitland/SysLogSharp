using System;
using System.Collections;
using System.Configuration.Install;

namespace Syslog.Service.Installer
{
	public static class InstallationManager
	{
		public static void Install(string[] args)
		{
			try
			{
				using (var installer = new AssemblyInstaller(typeof(InstallationManager).Assembly, args))
				{
					IDictionary state = new Hashtable();

					// Install the service
					installer.UseNewContext = true;
					try
					{
						installer.Install(state);
						installer.Commit(state);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);

						try
						{
							installer.Rollback(state);
						}
						catch (Exception exception)
						{
							Console.WriteLine(exception.Message);
						}
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("Failed to install service. Error: " + exception.Message);
			}
		}

		public static void Uninstall(string[] args)
		{
			try
			{
				using (var installer = new AssemblyInstaller(typeof(InstallationManager).Assembly, args))
				{
					IDictionary state = new Hashtable();

					// Install the service
					installer.UseNewContext = true;
					try
					{
						installer.Uninstall(state);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());

						try
						{
							installer.Rollback(state);
						}
						catch (Exception exception)
						{
							Console.WriteLine(exception.ToString());
						}
					}
				}
			}
			catch (Exception exception)
			{
				
				Console.WriteLine("Failed to install service. Error: " + exception.Message);
			}
		}
	}
}
