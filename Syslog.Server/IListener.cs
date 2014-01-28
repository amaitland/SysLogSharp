using System;

namespace Syslog.Server
{
	public interface IListener
	{
		/// <summary>
		/// Creates a socket that waits for traffic and sets up the handler modules
		/// </summary>
		/// <returns>Returns true if the listener starts successfully.  Otherwise, false.</returns>
		bool Start();

		/// <summary>
		/// Stop listening on the socket and flush the <see cref="LogBuffer"/>
		/// </summary>
		void Stop();

		/// <summary>
		/// Event triggered when a message is recieved
		/// </summary>
		event Action<MessageReceivedEventArgs> MessageReceived;
	}
}