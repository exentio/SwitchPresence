using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace SwitchPresence
{
    public partial class MainForm : Form
    {
        public static string TEMP_PATH = Path.GetTempPath() + @"\SwitchRichPresence\";

        string appID;
        DiscordController discord = new DiscordController();
        Thread UpdatePlaying;
        SwitchApps apps = null;
        TitleInfo CurrentPlaying = null;
        ulong CurrentTid = 0;
        DateTime startTime;
        string CurrentUser = null;


        private void UpdateInfo()
        {
            bool newTitle = (CurrentPlaying != null && CurrentPlaying.TitleID != CurrentTid);

            if (CurrentPlaying != null && !AFS.Checked)
            {
                if (newTitle)
                {
                    if (pictureBox_icon.Image != null)
                        pictureBox_icon.Image.Dispose();
                    pictureBox_icon.Image = new Bitmap(CurrentPlaying.Icon);

                    startTime = DateTime.UtcNow;
                    CurrentTid = CurrentPlaying.TitleID;

                    discord.rpcClient.SetPresence(new DiscordRPC.RichPresence()
                    {
                        Details = $"{CurrentPlaying.Metadata.GetLanguage().ApplicationName}",
                        Assets = new DiscordRPC.Assets()
                        {
                            SmallImageKey = "icon",
                            SmallImageText = "SwitchPresence Sysmodule",
                            LargeImageKey = $"{CurrentPlaying.TitleID:x16}",
                            LargeImageText = CurrentPlaying.Metadata.GetLanguage().ApplicationName
                        },
                        Timestamps = new DiscordRPC.Timestamps()
                        {
                            Start = startTime
                        }
                    });
                    OverrideDetail();
                    OverrideLargeIcon();
                    OverrideSmallIcon();
                }

                //update user
                CheckBox_showUser_CheckedChanged(null, null);

                //update time
                CheckBox_showTime_CheckedChanged(null, null);

                //layout
                label_game.Text = string.Format("Game: {0}\nTitle ID: {1}\nVersion: {2}\n{3}",
                CurrentPlaying.Metadata.GetLanguage().ApplicationName,
                CurrentPlaying.Metadata.TitleId,
                CurrentPlaying.Metadata.AppVersion,
                discord.rpcClient.CurrentPresence.State
                );

            }
            else
            {
                if (pictureBox_icon.Image != null)
                    pictureBox_icon.Image.Dispose();
                pictureBox_icon.Image = null;

                discord.rpcClient.SetPresence(new DiscordRPC.RichPresence()
                {
                    Details = "Not Playing",
                    Assets = new DiscordRPC.Assets()
                    {
                        SmallImageKey = null,
                        SmallImageText = null,
                        LargeImageKey = null,
                        LargeImageText = null
                    },
                    State = null
                });
                label_game.Text = "Not playing";
                CurrentTid = 0;
            }
            discord.rpcClient.Invoke();
        }


        private void SaveConfig()
        {
            Config config = new Config()
            {
                IP = textBox_ip.Text,
                ClientID = textBox_clientId.Text,
                ShowTimer = checkBox_showTime.Checked,
                ShowUser = checkBox_showUser.Checked,
                SIcon = textBox_overridesicon.Text,
                LIcon = textBox_overridelicon.Text,
                Detail = textBox_overridedetail.Text
            };
            config.Save();
        }
        private void LoadConfig()
        {
            Config config = new Config();
            appID = config.ClientID;
            textBox_ip.Text = config.IP;
            textBox_clientId.Text = config.ClientID;
            checkBox_showTime.Checked = config.ShowTimer;
            checkBox_showUser.Checked = config.ShowUser;
            textBox_overridesicon.Text = config.SIcon;
            textBox_overridelicon.Text = config.LIcon;
            textBox_overridedetail.Text = config.Detail;
        }

        public MainForm()
        {
            InitializeComponent();

            LoadConfig();

            Updater.GetUpdate();

            if (!Directory.Exists(TEMP_PATH))
            {
                Directory.CreateDirectory(TEMP_PATH);
            }

            discord.Initialize(appID);

            textBox_overridedetail.TextChanged += (o, e) => OverrideDetail();
            textBox_overridelicon.TextChanged += (o, e) => OverrideLargeIcon();
            textBox_overridesicon.TextChanged += (o, e) => OverrideSmallIcon();

        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (button_connect.Text != "Connect")
            {
                //close safely
                Button_connect_Click(null, null);
                discord.rpcClient.ClearPresence();
                discord.rpcClient.Dispose();
            }
        }

        private void Button_connect_Click(object sender, EventArgs e)
        {
            if (button_connect.Text == "Connect")
            {
                try
                {
                    apps = new SwitchApps(textBox_ip.Text);
                }
                catch (SocketException)
                {
                    MessageBox.Show("No switch found!\nMake sure that the IP address you entered is correct.");
                    return;
                }
                catch (ServerVersionException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                SaveConfig();

                AFS.Visible = true;
                warn.Visible = true;
                textBox_ip.Enabled = false;
                textBox_clientId.Enabled = false;
                utilsToolStripMenuItem.Visible = true;
                button_connect.Text = "Abort";

                UpdatePlaying = new Thread(() =>
                {
                    while (true)
                    {
                        CurrentPlaying = apps.GetPlaying();
                        CurrentUser = apps.GetCurrentUser();
                        Invoke(new Action(() => UpdateInfo()));

                        Thread.Sleep(1000 * 2);
                    }
                })
                {
                    IsBackground = true
                };
                UpdatePlaying.Start();
            }
            else //abort
            {
                AFS.Visible = false;
                AFS.Checked = false;
                warn.Visible = false;
                textBox_ip.Enabled = true;
                textBox_clientId.Enabled = true;
                utilsToolStripMenuItem.Visible = false;
                button_connect.Text = "Connect";

                UpdatePlaying.Abort();
                apps.Dispose();
                UpdatePlaying = null;
                CurrentPlaying = null;
                apps = null;
                UpdateInfo();
                discord.rpcClient.ClearPresence();
                discord.rpcClient.Dispose();
            }
        }

        private void ExportIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderSelectDialog fbox = new FolderSelectDialog();
            if (fbox.ShowDialog() == DialogResult.OK)
            {
                foreach (var app in apps.Applications)
                {
                    using (Bitmap bmp = new Bitmap(app.Icon, 512, 512))
                    {
                        bmp.Save(fbox.SelectedPath + $@"\{app.TitleID:x16}.png");
                    }
                }

                MessageBox.Show("Done!");
            }
        }

        private void CheckBox_showUser_CheckedChanged(object sender, EventArgs e)
        {
            if (apps != null)
            {

                if (checkBox_showUser.Checked && CurrentUser != null)
                    discord.rpcClient.UpdateState($"User: {CurrentUser}");

                else
                    discord.rpcClient.UpdateState(null);
            }
        }

        private void OverrideDetail()
        {
            if (string.IsNullOrWhiteSpace(textBox_overridedetail.Text) && (CurrentPlaying != null))
            {
                discord.rpcClient.UpdateDetails($"Playing {CurrentPlaying.Metadata.GetLanguage().ApplicationName}");
            }
            else if (CurrentPlaying != null)
            {
                discord.rpcClient.UpdateDetails(textBox_overridedetail.Text);
            }
            SaveConfig();
        }

        private void OverrideSmallIcon()
        {
            if (string.IsNullOrWhiteSpace(textBox_overridesicon.Text))
            {
                discord.rpcClient.UpdateSmallAsset("SmallImageKey", "icon");
            }
            else
            {
                discord.rpcClient.UpdateSmallAsset("SmallImageKey", textBox_overridesicon.Text);
            }
            SaveConfig();
        }

        private void OverrideLargeIcon()
        {
            if (string.IsNullOrWhiteSpace(textBox_overridelicon.Text) && (CurrentPlaying != null))
            {
                discord.rpcClient.UpdateLargeAsset("LargeImageKey", $"{CurrentPlaying.TitleID:x16}");
            }
            else if (CurrentPlaying != null)
            {
                discord.rpcClient.UpdateLargeAsset("LargeImageKey", textBox_overridelicon.Text);
            }
            SaveConfig();
        }

        private void CheckBox_showTime_CheckedChanged(object sender, EventArgs e)
        {
            if (apps != null)
            {
                DateTime time;
                if (checkBox_showTime.Checked)
                    time = startTime;
                else
                    time = default(DateTime);
                discord.rpcClient.UpdateStartTime(time);
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog();
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start($"https://discordapp.com/developers/applications/{textBox_clientId.Text}");
    }
}
