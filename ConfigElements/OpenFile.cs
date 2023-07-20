using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace IPv6Mapper.ConfigElements;

public class OpenNetworkInfo : OpenFile
{
    protected override void OnClick() {
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c ipconfig & pause",
            CreateNoWindow = false, // 设置为 true 表示不创建新窗口
            UseShellExecute = true, // 设置为 true 表示使用操作系统的 Shell 来启动进程
            WindowStyle = ProcessWindowStyle.Normal // 设置窗口样式为普通显示
        });
    }
}

public class OpenNetworkControl : OpenFile
{
    protected override void OnClick() {
        Process.Start("control", "ncpa.cpl");
    }
}

public class OpenFirewall : OpenFile
{
    protected override void OnClick() {
        Process.Start("control", "firewall.cpl");
    }
}

public class OpenFile : ConfigElement
{
    public override void OnBind() {
        base.OnBind();
        Height.Set(36f, 0f);
        DrawLabel = false;
        
        Append(new UIText(Label, 0.4f, true) {
            TextOriginX = 0.5f,
            TextOriginY = 0.5f,
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        });
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();
        float num = dimensions.Width + 1f;
        var pos = new Vector2(dimensions.X, dimensions.Y);
        var color = IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlue.MultiplyRGBA(new Color(180, 180, 180));
        DrawPanel2(spriteBatch, pos, TextureAssets.SettingsPanel.Value, num, dimensions.Height, color);

        base.DrawSelf(spriteBatch);
    }

    public override void LeftClick(UIMouseEvent evt) {
        base.LeftClick(evt);
        OnClick();
    }

    protected virtual void OnClick() {
        var fullPath = Path.Combine(ConfigManager.ModConfigPath, "IPv6Mapper_Config.json");
        if (!File.Exists(fullPath)) return;
        Process.Start(new ProcessStartInfo(fullPath)
        {
            UseShellExecute = true
        });
    }
}