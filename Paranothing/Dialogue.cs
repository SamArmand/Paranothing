using System;
using Microsoft.Xna.Framework;

namespace Paranothing;

sealed class Dialogue : ICollideable, IResetable
{
    readonly GameController _gameController = GameController.Instance;
    readonly string _text = string.Empty;
    readonly Vector2 _position;

    internal Dialogue(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndDialogue", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("text:", StringComparison.Ordinal)) _text = line[5..].Trim();
        }
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, 20, 20);

    public bool IsSolid => false;

    bool Played { get; set; }

    public void Reset() => Played = false;

    internal void Play()
    {
        if (Played) return;

        _gameController.ShowDialogue(_text);
        Played = true;
    }
}