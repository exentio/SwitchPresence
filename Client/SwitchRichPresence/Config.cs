using System;
using System.Collections.Generic;
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
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private const string CONFIG_PATH = "config.txt";

        public string ClientID { get; set; } = "464720851976060940";
        public string IP { get; set; } = "192.168.0.XX";
        public bool ShowUser { get; set; } = true;
        public bool ShowTimer { get; set; } = true;
        public string SIcon { get; set; } = "";
        public string LIcon { get; set; } = "";
        public string Detail { get; set; } = "";

        public Config()
        {
            if (File.Exists(CONFIG_PATH))
            {
                string[] lines = File.ReadAllLines(CONFIG_PATH);

                foreach (var line in lines)
                {
                    string[] parts = line.Replace(" ", "").Replace("\t", "").Split('=');

                    if (parts.Length == 2)
                    {
                        try
                        {
                            switch (parts[0].ToLower())
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
                                    Detail = Base64Decode(parts[1]);
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
            List<string> lines = new List<string>()
            {
                "client_id=" + ClientID,
                "ip=" + IP,
                "show_user=" + (ShowUser ? "true" : "false"),
                "show_timer=" + (ShowTimer ? "true" : "false"),
                "sicon=" +  SIcon,
                "licon=" + LIcon,
                "detail=" + Base64Encode(Detail),
            };

            File.WriteAllLines(CONFIG_PATH, lines);
        }
    }
}
