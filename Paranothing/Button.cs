using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Button : IDrawable, ICollideable
{
    static readonly Dictionary<string, Button> Buttons = new();

    internal bool StepOn = false;

    readonly SpriteSheet _sheet = SpriteSheetManager.Instance.GetSheet("button");
    readonly Vector2 _position;

    internal Button(string saveString)
    {
        var name = "BT";
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndButton", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("name:", StringComparison.Ordinal)) name = line[5..].Trim();
        }

        Buttons[name] = this;
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, 16, 5);

    public bool IsSolid => false;

    internal static Button GetKey(string name)
    {
        Button button;
        if (Buttons.ContainsKey(name))
            Buttons.TryGetValue(name, out button);
        else
            button = null;
        return button;
    }

    public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, Bounds,
        StepOn ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
        new(), SpriteEffects.None, DrawLayer.Key);
}