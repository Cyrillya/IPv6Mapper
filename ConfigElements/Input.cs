using IPv6Mapper.ConfigElements;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace MachineTranslate.ConfigElements;

public class Input : ConfigElement<string>
{
    public override void OnBind()
    {
        base.OnBind();

        var textBoxBackground = new UIPanel {
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent
        };
        textBoxBackground.SetPadding(0);
        textBoxBackground.Top.Set(0f, 0f);
        textBoxBackground.Left.Set(-320, 1f);
        textBoxBackground.Width.Set(310, 0f);
        textBoxBackground.Height.Set(30, 0f);

        Append(textBoxBackground);
        
        textBoxBackground.Append(new UIHorizontalSeparator {
            Top = new StyleDimension(-8f, 1f),
            Width = StyleDimension.FromPercent(1f),
            Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
        });

        var uIInputTextField = new UIFocusInputTextField();
        uIInputTextField.SetText(Value);
        uIInputTextField.Top.Set(5, 0f);
        uIInputTextField.Left.Set(0, 0f);
        uIInputTextField.Width.Set(-20, 1f);
        uIInputTextField.Height.Set(20, 0);
        uIInputTextField.OnTextChange += (a, b) => {
            Value = uIInputTextField.CurrentString;
        };

        textBoxBackground.Append(uIInputTextField);
    }
}