using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class ActionBubble : IDrawable
{
    readonly int _negateIndex;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("action");

    bool _isNegated;
    BubbleAction _bubbleAction = BubbleAction.None;
    int _animationIndex;
    string _animationName;

    internal ActionBubble() => _negateIndex = _spriteSheet.GetAnimation("negate").FirstOrDefault();

    internal enum BubbleAction
    {
        None,
        Wardrobe,
        Push,
        Portrait,
        OldPortrait,
        Stairs,
        Chair,
        Bookcase
    }

    internal Bruce Bruce { private get; init; }

    internal Chair Chair { private get; set; }

    internal bool IsVisible { get; set; }

    string Animation
    {
        set
        {
            if (!_spriteSheet.HasAnimation(value) || _animationName == value) return;

            _animationName = value;
            _animationIndex = _spriteSheet.GetAnimation(_animationName).First();
        }
    }

    public void Draw(SpriteBatch spriteBatch, Color tint)
    {
        if (!IsVisible) return;

        Vector2 drawingPosition;

        if (_bubbleAction != BubbleAction.Chair && Bruce is not null)
        {
            var brucePosition = Bruce.Position;
            drawingPosition = new(brucePosition.X + 11, brucePosition.Y - 27);
        }
        else if (Chair is not null)
        {
            var chairPosition = Chair.Position;
            drawingPosition = new(chairPosition.X + 11, chairPosition.Y - 32);
        }
        else
            drawingPosition = Vector2.Zero;

        var image = _spriteSheet.Image;

        spriteBatch.Draw(image, drawingPosition, _spriteSheet.GetSprite(0), tint, 0f, new(), 1f,
            SpriteEffects.None,
            DrawLayer.ActionBubble);
        spriteBatch.Draw(image, drawingPosition, _spriteSheet.GetSprite(_animationIndex), tint, 0f, new(),
            1f,
            SpriteEffects.None, DrawLayer.ActionBubble - 0.001f);
        if (_isNegated)
            spriteBatch.Draw(image, drawingPosition, _spriteSheet.GetSprite(_negateIndex), tint, 0f, new(),
                1f,
                SpriteEffects.None, DrawLayer.ActionBubble - 0.002f);
    }

    internal void SetAction(BubbleAction action, bool negated)
    {
        _bubbleAction = action;
        _isNegated = negated;
        switch (action)
        {
            case BubbleAction.Wardrobe:
                Animation = "wardrobe";
                break;
            case BubbleAction.Portrait:
                Animation = "portrait";
                break;
            case BubbleAction.OldPortrait:
                Animation = "old_portrait";
                break;
            case BubbleAction.Push:
                Animation = "push";
                break;
            case BubbleAction.Chair:
                Animation = "chair";
                break;
            case BubbleAction.Stairs:
                Animation = "stairs";
                break;
            case BubbleAction.Bookcase:
                Animation = "bookcase";
                break;
            case BubbleAction.None:
                break;
            default:
                Animation = "negate";
                break;
        }
    }
}