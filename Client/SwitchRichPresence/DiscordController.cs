using System;

namespace SwitchPresence
{
    public class DiscordController
    {
        public DiscordRpc.RichPresence presence;
        DiscordRpc.EventHandlers handlers;
        public string optionalSteamId;

        /// <summary>
        ///     Initializes Discord RPC
        /// </summary>
        public void Initialize(string appId)
        {
            presence = new DiscordRpc.RichPresence();
            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            DiscordRpc.Initialize(appId, ref handlers, true, optionalSteamId);
        }

        public void ReadyCallback()
        {
            Console.WriteLine("Discord RPC is ready!");
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            Console.WriteLine($"Error: {errorCode} - {message}");
        }

        public void ErrorCallback(int errorCode, string message)
        {
            Console.WriteLine($"Error: {errorCode} - {message}");
        }
    }
}
