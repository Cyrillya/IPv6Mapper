using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;

namespace IPv6Mapper.ConfigElements;

public class UIFocusInputTextField : UIElement
{
	internal bool Focused = false;
	internal string CurrentString = "";

	private int _textBlinkerCount;
	private int _textBlinkerState;
	public bool UnfocusOnTab { get; internal set; } = false;

	public delegate void EventHandler(object sender, EventArgs e);

	public event EventHandler OnTextChange;
	public event EventHandler OnUnfocus;
	public event EventHandler OnTab;

	public UIFocusInputTextField()
	{
	}

	public void SetText(string text)
	{
		if (text == null)
			text = "";

		if (CurrentString != text) {
			CurrentString = text;
			OnTextChange?.Invoke(this, new EventArgs());
		}
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		Main.clrInput();
		Focused = true;
	}

	public override void Update(GameTime gameTime)
	{
		var mousePosition = new Vector2(Main.mouseX, Main.mouseY);
		if (!ContainsPoint(mousePosition) && Main.mouseLeft)
		{
			Focused = false;
			OnUnfocus?.Invoke(this, new EventArgs());
		}
		base.Update(gameTime);
	}
	private static bool JustPressed(Keys key)
	{
		return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (Focused) {
			PlayerInput.WritingText = true;
			Main.instance.HandleIME();
			string newString = Main.GetInputText(CurrentString);
			if (!newString.Equals(CurrentString)) {
				CurrentString = newString;
				OnTextChange?.Invoke(this, EventArgs.Empty);
			}
			else {
				CurrentString = newString;
			}
			if (JustPressed(Keys.Tab)) {
				if (UnfocusOnTab) {
					Focused = false;
					OnUnfocus?.Invoke(this, EventArgs.Empty);
				}
				OnTab?.Invoke(this, EventArgs.Empty);
			}
			if (++_textBlinkerCount >= 20) {
				_textBlinkerState = (_textBlinkerState + 1) % 2;
				_textBlinkerCount = 0;
			}
		}
		string displayString = CurrentString;
		if (_textBlinkerState == 1 && Focused) {
			displayString += "|";
		}
		var space = GetDimensions();
		if (CurrentString.Length == 0 && !Focused) {
			Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.IPv6Mapper.TypeHere"), new Vector2(space.X, space.Y), Color.Gray, 0.75f);
		}
		else {
			float scale = 310f / FontAssets.MouseText.Value.MeasureString(CurrentString).X;
			scale = Math.Clamp(scale, 0.62f, 0.8f);
			Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y + 2), Color.White, scale);
		}
	}
}