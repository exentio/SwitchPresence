using System;
using DiscordRPC;

namespace SwitchPresence
{
    public class DiscordController
    {
        public DiscordRpcClient rpcClient;
        public string optionalSteamId;

        /// <summary>
        ///     Initializes Discord RPC
        /// </summary>
        public void Initialize(string appId)
        {
            rpcClient = new DiscordRpcClient(appId);
            rpcClient.Initialize();
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