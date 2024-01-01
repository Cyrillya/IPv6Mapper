using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;
using static System.Int32;

namespace IPv6Mapper;

public class IPv6Mapper : Mod
{
    internal static MethodInfo OnSubmitServerPortInfo;
    internal static FieldInfo SelectedMenuInfo;
    internal static Config Config;
    internal static bool IPv6Copied;
    internal static string IPv6Address;
    internal static int IPv6DetectTimer;
    internal static bool IsInIPv6Server;
    private static Process _tinyMapper;

    public override void Unload() {
        Config = null;
        OnSubmitServerPortInfo = null;
        Netplay.OnDisconnect -= CloseTinyMapper;
    }

    public override void Load() {
        Config = ModContent.GetInstance<Config>();

        On_Main.TryDisposingEverything += orig => {
            orig.Invoke();

            // Kill tinyMapper
            if (_tinyMapper is not null) {
                _tinyMapper.Kill();
                _tinyMapper = null;
            }
        };

        On_Main.DrawMenu += (orig, self, time) => {
            // Kill tinyMapper
            if (Main.menuMode is MenuID.Title && _tinyMapper is not null) {
                _tinyMapper.Kill();
                _tinyMapper = null;
            }

            // Select IP from list
            if (Main.menuMode is MenuID.ServerIP) {
                IsInIPv6Server = false;

                SelectedMenuInfo ??=
                    typeof(Main).GetField("selectedMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                if (SelectedMenuInfo != null) {
                    int selectedMenu = SelectedMenuInfo.GetValue(Main.instance) as int? ?? 0;
                    if (selectedMenu is >= 2 and < 9) {
                        Main.autoPass = false;
                        int num45 = selectedMenu - 2;

                        var realPort = Main.recentPort[num45];
                        var realIP = Main.recentIP[num45];
                        if (IPAddress.TryParse(realIP, out var ipAddress) &&
                            ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&
                            TryParse(Config.CustomMappedLocalPort, out int port)) {
                            Netplay.ListenPort = port;
                            Main.getIP = "127.0.0.1";
                            Netplay.SetRemoteIPAsync(Main.getIP, Main.StartClientGameplay);
                            Main.menuMode = 14;
                            Main.statusText = Language.GetTextValue("Net.ConnectingTo", Main.getIP);

                            var arguments = $"-l127.0.0.1:{Main.getPort} -r[{realIP}]:{realPort} -t";
                            OpenTinyMapper(arguments);

                            IPv6Address = realIP;
                            IsInIPv6Server = true;
                        }
                        else {
                            Netplay.ListenPort = realPort;
                            Main.getIP = realIP;
                            Netplay.SetRemoteIPAsync(Main.getIP, Main.StartClientGameplay);
                            Main.menuMode = 14;
                            Main.statusText = Language.GetTextValue("Net.ConnectingTo", Main.getIP);
                        }
                    }
                }
            }

            if (Main.menuMode is not MenuID.SteamMultiplayerOptions) {
                orig.Invoke(self, time);
                IPv6Copied = false;
                return;
            }

            // Draw IPv6 text
            var address = Config.CustomIPv6Address;
            if (string.IsNullOrWhiteSpace(address)) {
                IPv6DetectTimer++;
                if (IPv6DetectTimer > 300 || string.IsNullOrWhiteSpace(IPv6Address)) {
                    IPv6DetectTimer = 0;
                    // 检测以输出地址更新信息
                    var newIPv6Address = GetIPv6Address();
                    if (newIPv6Address != IPv6Address) {
                        IPv6Address = newIPv6Address;
                        WriteIPv6Info();
                    }
                }

                address = IPv6Address;
            }

            const float textScale = 0.5f;
            var color = Color.LightGray;
            var font = FontAssets.DeathText.Value;
            string keyAddress = IPv6Copied ? "IPAddressCopied" : "IPAddress";
            string textAddress = Language.GetTextValue(GetLocalizationKey(keyAddress), address);
            string textTip = Language.GetTextValue(GetLocalizationKey("ConfigTip"));
            var position = new Vector2(Main.screenWidth / 2f, 700f);
            var sizeAddress = font.MeasureString(textAddress) * textScale;
            var posAddress = position - sizeAddress / 2f;
            var posTip = position - font.MeasureString(textTip) * textScale / 2f;
            posTip.Y += 36f;

            Utils.DrawBorderStringBig(Main.spriteBatch, textAddress, posAddress, color, scale: textScale);
            Utils.DrawBorderStringBig(Main.spriteBatch, textTip, posTip, color, scale: textScale);

            var boxAddress = new Rectangle((int) posAddress.X, (int) posAddress.Y, (int) sizeAddress.X,
                (int) sizeAddress.Y);
            if (boxAddress.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && !IPv6Copied) {
                Platform.Get<IClipboard>().Value = address;
                IPv6Copied = true;
            }

            // Vanilla draw
            orig.Invoke(self, time);
        };

        // 主机
        On_Netplay.StartServer += orig => {
            orig.Invoke();

            var address = Config.CustomIPv6Address;
            if (string.IsNullOrWhiteSpace(address) && string.IsNullOrWhiteSpace(IPv6Address)) {
                IPv6Address = GetIPv6Address();
                address = IPv6Address;
                WriteIPv6Info();
            }

            if (!IPAddress.TryParse(address, out var ipAddress) ||
                ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6) {
                return;
            }

            var arguments = $"-l[{address}]:{Config.CustomMappedRemotePort} -r127.0.0.1:{Netplay.ListenPort} -t";
            OpenTinyMapper(arguments);
        };

        // 客机
        On_Main.OnSubmitServerIP += (orig, ip) => {
            IsInIPv6Server = false;

            OnSubmitServerPortInfo ??=
                typeof(Main).GetMethod("OnSubmitServerPort", BindingFlags.NonPublic | BindingFlags.Static);

            if (!IPAddress.TryParse(ip, out var ipAddress) ||
                ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6 ||
                OnSubmitServerPortInfo == null) {
                orig.Invoke(ip);
                return;
            }

            Main.getIP = "127.0.0.1";
            Main.getPort = Config.CustomMappedLocalPort;

            OnSubmitServerPortInfo.Invoke(null, new object[] {Main.getPort});
            var arguments = $"-l127.0.0.1:{Main.getPort} -r[{ip}]:{Config.CustomMappedRemotePort} -t";
            OpenTinyMapper(arguments);

            IPv6Address = ip;
            IsInIPv6Server = true;
        };

        // 保存到最近服务器
        On_Netplay.AddCurrentServerToRecentList += orig => {
            if (!IsInIPv6Server || !IPAddress.TryParse(IPv6Address, out var ipAddress) ||
                ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6) {
                orig.Invoke();
                return;
            }

            string serverIPText = Netplay.ServerIPText;
            int listenPort = Netplay.ListenPort;
            Netplay.ServerIPText = IPv6Address;
            TryParse(Config.CustomMappedRemotePort, out Netplay.ListenPort);

            orig.Invoke();

            Netplay.ServerIPText = serverIPText;
            Netplay.ListenPort = listenPort;
        };

        // Close client-side tinymapper process on disconnect
        Netplay.OnDisconnect += CloseTinyMapper;

        // Close server-side tinymapper process when server shuts down.
        // Yes, I agree that hooking to a messaging system seem weird, but this
        // is what the game uses to check server liveliness
        On_NetMessage.EnsureLocalPlayerIsPresent += orig => {
            orig.Invoke();
            if (Netplay.Disconnect) {
                CloseTinyMapper();
            }
        };
    }

    public override void PostSetupContent() {
        string targetFilePath = AppDomain.CurrentDomain.BaseDirectory; // 保存目标路径
        const string targetFileName = "tinymapper.exe"; // 保存目标文件名

        string fullPath = Path.Combine(targetFilePath, targetFileName);

        try {
            using var fileStream = GetFileStream("tinymapper.exe");
            using var targetStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

            fileStream.CopyTo(targetStream);

            Console.WriteLine(Language.GetTextValue(GetLocalizationKey("TinyMapperSaved")) + targetFilePath);
        }
        catch (Exception ex) {
            Logger.Warn(Language.GetTextValue(GetLocalizationKey("TinyMapperSaveError")), ex);
        }
    }

    private static void OpenTinyMapper(string arguments) {
        if (_tinyMapper is not null) {
            _tinyMapper.Kill();
            _tinyMapper = null;
        }

        var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tinymapper.exe");
        _tinyMapper = Process.Start(fileName, arguments);
    }

    private static void CloseTinyMapper() {
        Console.WriteLine("CloseTinyMapper");
        if (_tinyMapper is not null) {
            _tinyMapper.Kill();
            _tinyMapper = null;
        }
    }

    private void WriteIPv6Info() {
        if (string.IsNullOrWhiteSpace(IPv6Address) || IPv6Address is "NONE!") {
            Console.WriteLine(Language.GetTextValue(GetLocalizationKey("NotGetIP")));
        }
        else {
            Console.WriteLine(Language.GetTextValue(GetLocalizationKey("IPAddressRaw")) + IPv6Address);
        }
    }

    public string GetIPv6Address() {
        try {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces) {
                // 排除回环接口和非运行状态的接口
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    networkInterface.OperationalStatus != OperationalStatus.Up) continue;

                var ipProperties = networkInterface.GetIPProperties();
                foreach (var ipAddressInfo in ipProperties.UnicastAddresses) {
                    // 判断是否为IPv6地址和临时地址
                    if (ipAddressInfo is {
                            IsDnsEligible: false, Address.IsIPv6LinkLocal: false,
                            Address.AddressFamily: System.Net.Sockets.AddressFamily.InterNetworkV6
                        }) {
                        return ipAddressInfo.Address.ToString();
                    }
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine(Language.GetTextValue(GetLocalizationKey("ErrorGetIP")) + ex.Message);
        }

        return "NONE!";
    }
}