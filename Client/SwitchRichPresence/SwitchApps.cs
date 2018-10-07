using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SwitchPresence
{

    [Serializable]
    public class ServerVersionException : Exception
    {
        public ServerVersionException() { }
        public ServerVersionException(string message) : base(message) { }
        public ServerVersionException(string message, Exception inner) : base(message, inner) { }
        protected ServerVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class SwitchApps : IDisposable
    {
        private const int CLIENT_VERSION = 1 << 16 | 0 << 8 | 3; //different than the app version
        private Socket client;
        public List<TitleInfo> Applications { get; private set; } = new List<TitleInfo>();

        public SwitchApps(string ip)
        {
            if (!IPAddress.TryParse(ip, out IPAddress addr))
                throw new Exception($"Invalid IP address: \"{ip}\"");

            client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client.Connect(addr, 0xCAFE);

            int ver = GetServerVersion();
            if (ver != CLIENT_VERSION)
            {
                Dispose();

                var VerExceptionText = "Client and server versions don't match: The client is outdated.\nPlease download the latest update of both client and server.";

                if (ver > CLIENT_VERSION)
                    throw new ServerVersionException(VerExceptionText);
                else
                    throw new ServerVersionException(VerExceptionText);
            }

            GetApplicationList();
        }
        public void Dispose()
        {
            try
            {
                TcpCommand.SendCommand(client, TcpCommand.SendCommandType.Disconnect);
            }
            catch { }
        }

        private void GetApplicationList()
        {
            //get the list
            List<ApplicationRecord> apps = new List<ApplicationRecord>();

            TcpCommand.SendCommand(client, TcpCommand.SendCommandType.ListApps);
            int count = TcpCommand.ReceiveInt32(client);
            TcpCommand.SendCommand(client, TcpCommand.SendCommandType.Confirm);
            byte[] buff = TcpCommand.ReceiveBuffer(client, count * 0x18);

            using (MemoryStream ms = new MemoryStream(buff))
            {
                BinaryReader br = new BinaryReader(ms);
                for (int i = 0; i < count; i++)
                    apps.Add(new ApplicationRecord(br));
            }

            Applications = new List<TitleInfo>();
            for (int i = 0; i < count; i++)
            {
                Applications.Add(new TitleInfo(client, apps[i].TitleID));
            }
        }
        private int GetServerVersion()
        {
            TcpCommand.SendCommand(client, TcpCommand.SendCommandType.GetVersion);
            int ver = TcpCommand.ReceiveInt32(client);
            return ver;
        }
        
        public TitleInfo GetPlaying()
        {
            TcpCommand.SendCommand(client, TcpCommand.SendCommandType.GetCurrentApp);
            ulong tid = TcpCommand.ReceiveUInt64(client);

            foreach (var app in Applications)
            {
                if (app.TitleID == tid)
                {
                    return app;
                }
            }
            return null;
        }
        public string GetCurrentUser()
        {
            TcpCommand.SendCommand(client, TcpCommand.SendCommandType.GetActiveUser);
            bool selected = TcpCommand.ReceiveBool(client);
            if (!selected)
                return null;
            else
            {
                TcpCommand.SendCommand(client, TcpCommand.SendCommandType.Confirm);
                byte[] buffer = TcpCommand.ReceiveBuffer(client, 0x20);
                return Encoding.UTF8.GetString(buffer).Replace("\0", "");
            }
        }
    }
}
