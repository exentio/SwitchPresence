using System;
using System.IO;
using System.Text;

namespace SwitchRichPresence
{
    public class Config
    {
        public static string Base64Decode(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private const string CONFIG_PATH = "config.txt";

        public string ClientID { get; set; } = "464720851976060940";
        public string IP { get; set; } = "192.168.0.XX";
        public bool ShowUser { get; set; } = true;
        public bool ShowTimer { get; set; } = true;
        public string SIcon { get; set; } = null;
        public string LIcon { get; set; } = null;
        public string Detail { get; set; } = null;

        public Config()
        {
            if (File.Exists(CONFIG_PATH))
            {
                string[] lines = File.ReadAllLines(CONFIG_PATH);

                foreach (var line in lines)
                {
                    string[] parts = line.Replace("\t", "").Split('=');

                    if (parts.Length == 2)
                    {
                        try
                        {
                            parts[1] = parts[1].TrimStart(' ');
                            switch (parts[0].TrimEnd(' ').ToLower())
                            {
                                case "client_id":
                                    ClientID = parts[1];
                                    break;

                                case "ip":
                                    IP = parts[1];
                                    break;

                                case "show_user":
                                    ShowUser = bool.Parse(parts[1]);
                                    break;

                                case "show_timer":
                                    ShowTimer = bool.Parse(parts[1]);
                                    break;

                                case "sicon":
                                    SIcon = parts[1];
                                    break;

                                case "licon":
                                    LIcon = parts[1];
                                    break;

                                case "detail":
                                    Detail = parts[1];
                                    break;
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public void Save()
        {
            var lines = new string[]
            {
                $"client_id={ClientID}",
                $"ip={IP}",
                $"show_user={(ShowUser ? "true" : "false")}",
                $"show_timer={(ShowTimer ? "true" : "false")}",
                $"sicon={SIcon}",
                $"licon={LIcon}",
                $"detail={Detail}",
            };

            File.WriteAllLines(CONFIG_PATH, lines);
        }
    }
}