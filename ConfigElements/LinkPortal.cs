using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace IPv6Mapper.ConfigElements;

public class IPv6Tutorial : LinkPortal
{
    public override string GetUrl() => Language.ActiveCulture.CultureInfo.Name is "zh-Hans"
        ? "https://ipw.cn/doc/ipv6/user/enable_ipv6.html"
        : "https://www.google.com/search?q=how+to+enable+ipv6";
}

public class IPv6Test3 : LinkPortal
{
    public override string GetUrl() => "https://ipw.cn/";
}

public class IPv6Test2 : LinkPortal
{
    public override string GetUrl() => "https://testipv6.cn/";
}

public class IPv6Test1 : LinkPortal
{
    public override string GetUrl() => "https://test-ipv6.com/";
}

public abstract class LinkPortal : ConfigElement
{
    public abstract string GetUrl();

    public override void LeftClick(UIMouseEvent evt) {
        base.LeftClick(evt);

        Utils.OpenToURL(GetUrl());
    }

    public override void OnBind() {
        base.OnBind();
        Height.Set(32f, 0f);
        DrawLabel = false;

        Append(new UIText(Label, 0.35f, true) {
            TextOriginX = 0.5f,
            TextOriginY = 0.5f,
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        });
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        var dimensions = GetDimensions();
        float num = dimensions.Width + 1f;
        var pos = new Vector2(dimensions.X, dimensions.Y);
        var color = IsMouseHovering
            ? UICommon.DefaultUIBlue
            : UICommon.DefaultUIBlue.MultiplyRGBA(new Color(180, 180, 180));
        DrawPanel2(spriteBatch, pos, TextureAssets.SettingsPanel.Value, num, dimensions.Height, color);

        base.DrawSelf(spriteBatch);
    }
}