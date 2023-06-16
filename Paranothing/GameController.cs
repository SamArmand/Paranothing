using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class GameController
{
    static GameController _instance;
    internal Bruce Bruce;

    internal Camera Camera;

    internal GameState GameState { get; set; } = GameState.Game;
    internal Level Level => _level;

    internal TimePeriod TimePeriod = TimePeriod.Present;

    readonly Dictionary<string, Level> _levels = new();
    readonly SoundManager _soundManager = SoundManager.Instance;
    bool _soundTriggered, _showingDialogue;
    int _dialogueTimer;
    Level _level;

    List<ICollideable> _collideables = new();
    List<IDrawable> _drawables = new();
    List<IResetable> _resetables = new();
    List<IUpdatable> _updatables = new();
    string _dialogue = string.Empty;
    Vector2 _soundPosition;

    public static GameController Instance => _instance ??= new();

    public bool NextLevel
    {
        get
        {
            var nextLevel = Level.NextLevel;
            if (!_levels.ContainsKey(nextLevel)) return false;

            _levels.TryGetValue(nextLevel, out _level);
            return true;
        }
    }

    public void AddLevel(Level level) => _levels.Add(level.Name, level);

    public bool CollidingWithSolid(Rectangle box, bool includePlayer = true) =>
        _collideables.Any(collideable =>
            (includePlayer || collideable is not Paranothing.Bruce) && collideable is not Stairs && collideable.IsSolid &&
            box.Intersects(collideable.Bounds));

    public void DrawObjects(SpriteBatch spriteBatch)
    {
        _showingDialogue &= _dialogueTimer < 3000;

        if (_showingDialogue)
        {
            var gameFont = ParanothingGame.GameFont;
            var (x, y) = gameFont.MeasureString(_dialogue);
            var cameraPosition = Camera.Position;
            var cameraScale = Camera.Scale;

            spriteBatch.DrawString(gameFont, _dialogue,
                new(cameraPosition.X + Camera.Width / 2f / cameraScale - x / 2,
                    cameraPosition.Y + Camera.Height / cameraScale - y - 10), Color.White);
        }

        var tint = Color.White;
        tint.A = TimePeriod switch
        {
            TimePeriod.Past => 32,
            TimePeriod.FarPast => 4,
            _ => tint.A
        };

        foreach (var drawable in _drawables) drawable.Draw(spriteBatch, tint);

        Bruce.Draw(spriteBatch, tint);
    }

    public void GoToLevel(string levelName)
    {
        if (_levels.ContainsKey(levelName)) _levels.TryGetValue(levelName, out _level);
    }

    public void InitLevel(bool preserveTime)
    {
        _showingDialogue = false;
        _updatables = new();
        _drawables = new();
        _collideables = new();
        _resetables = new();
        Bruce.Reset();
        Bruce.Position = new(Level.PlayerX, Level.PlayerY);
        AddObject(Bruce);
        AddObject(Bruce.ActionBubble);
        AddObject(Camera);

        foreach (var levelObject in Level.LevelObjects)
        {
            if (levelObject is IResetable resetable) resetable.Reset();
            AddObject(levelObject);
        }

        if (!preserveTime)
            TimePeriod = Level.StartTime;
    }

    public void ResetLevel()
    {
        _showingDialogue = false;
        Bruce.Position = new(Level.PlayerX, Level.PlayerY);
        foreach (var resetable in _resetables) resetable.Reset();
        TimePeriod = Level.StartTime;
    }

    public void SetCamera(Camera camera)
    {
        Camera = camera;
        AddObject(camera);
    }

    public void SetPlayer(Bruce player)
    {
        Bruce = player;
        AddObject(player);
        AddObject(player.ActionBubble);
    }

    public void ShowDialogue(string text)
    {
        _showingDialogue = true;
        _dialogue = text;
    }

    public void UpdateObjects(GameTime time)
    {
        _dialogueTimer = _showingDialogue ? _dialogueTimer + time.ElapsedGameTime.Milliseconds : 0;

        foreach (var updatable in _updatables) updatable.Update(time);

        var actionBubble = Bruce.ActionBubble;
        actionBubble.IsVisible = false;

        _soundTriggered = Math.Abs(_soundPosition.X) > 0 && Math.Abs(_soundPosition.Y) > 0;

        foreach (var collideable in _collideables)
        {
            var colliding = collideable.Bounds.Intersects(Bruce.Bounds);
            var brucePosition = Bruce.Position;
            var brucePositionX = brucePosition.X;
            var brucePositionY = brucePosition.Y;
            var bruceState = Bruce.State;
            var bruceDirection = Bruce.Direction;

            switch (collideable)
            {
                case Shadow shadow:
                    var shadowPositionX = shadow.Position.X;
                    var shadowPositionY = shadow.Position.Y;
                    var soundPositionY = _soundPosition.Y;
                    if (_soundTriggered && TimePeriod == TimePeriod.Present && soundPositionY >= shadowPositionY &&
                        soundPositionY <= shadowPositionY + 81)
                        shadow.SoundPosition = _soundPosition;

                    if (!colliding || TimePeriod != TimePeriod.Present || bruceState == Bruce.BruceState.StairsLeft ||
                        bruceState == Bruce.BruceState.StairsRight) break;

                    Bruce.Direction = shadowPositionX > brucePositionX ? Direction.Right : Direction.Left;
                    Bruce.State = Bruce.BruceState.Die;

                    _soundManager.PlaySound("Death");
                    shadow.State = Shadow.ShadowState.Idle;
                    break;
                case Dialogue dialogue:
                    if (colliding) dialogue.Play();
                    break;
                case DoorKey doorKey:
                    if (!colliding) continue;

                    var key = DoorKey.GetKey(doorKey.Name);
                    if (!key.RestrictTime || TimePeriod == key.InTime)
                        key.PickedUp = true;
                    break;
                case Button button:
                    var pressed = _collideables.Any(
                        c =>
                            (c is Bruce || TimePeriod == TimePeriod.Present &&
                                c is Shadow)
                            && button.Bounds.Intersects(c.Bounds));
                    if (!button.StepOn && pressed)
                        _soundManager.PlaySound("Button Press");
                    button.StepOn = pressed;
                    break;
                case Stairs stairs:
                    if (!stairs.IsSolid || !colliding) break;

                    var stairsPositionX = stairs.Position.X;
                    var stairsPositionY = stairs.Position.Y;
                    var stairsDirection = stairs.Direction;
                    var stairsBounds = stairs.Bounds;
                    var stairsBoundsWidth = stairsBounds.Width;
                    var stairsBoundsHeight = stairsBounds.Height;
                    var stairsSmallBoundsY = stairs.SmallBounds.Y;

                    if (brucePositionX + 30 >= stairsPositionX && brucePositionX + 8 <= stairsPositionX)
                    {
                        if ((stairsDirection == Direction.Left &&
                             Math.Abs(brucePositionY + 58 - stairs.SmallBounds.Y) <= 0
                             || stairsDirection == Direction.Right &&
                             Math.Abs(brucePositionY + 58 - (stairsPositionY + stairsBoundsHeight)) <= 0)
                            && bruceState is Bruce.BruceState.Idle or Bruce.BruceState.Walk)
                        {
                            actionBubble.SetAction(ActionBubble.BubbleAction.Stairs, false);
                            Bruce.Interactor = stairs;
                            actionBubble.IsVisible = true;
                        }
                    }
                    else if (brucePositionX + 30 >= stairsPositionX + stairsBoundsWidth &&
                             brucePositionX + 8 <= stairsPositionX + stairsBoundsWidth
                             &&
                             (stairsDirection == Direction.Right &&
                              Math.Abs(brucePositionY + 58 - stairsSmallBoundsY) <= 0
                              || stairsDirection == Direction.Left &&
                              Math.Abs(brucePositionY + 58 - (stairsPositionY + stairsBoundsHeight)) <= 0)
                             && bruceState is Bruce.BruceState.Idle or Bruce.BruceState.Walk)
                    {
                        actionBubble.SetAction(ActionBubble.BubbleAction.Stairs, false);
                        Bruce.Interactor = stairs;
                        actionBubble.IsVisible = true;
                    }

                    if (bruceState != Bruce.BruceState.StairsLeft && bruceState != Bruce.BruceState.StairsRight)
                        break;

                    switch (stairsDirection)
                    {
                        case Direction.Left:
                            if (bruceDirection == Direction.Left && (int)brucePositionY + 58 == stairsSmallBoundsY
                                || bruceDirection == Direction.Right &&
                                Math.Abs(brucePositionY + 58 - (stairsPositionY + stairsBoundsHeight)) <= 0)
                                Bruce.State = Bruce.BruceState.Walk;
                            break;
                        case Direction.Right:
                            if (bruceDirection == Direction.Right && (int)brucePositionY + 58 == stairsSmallBoundsY
                                || bruceDirection == Direction.Left &&
                                Math.Abs(brucePositionY + 58 - (stairsPositionY + stairsBoundsHeight)) <= 0)
                                Bruce.State = Bruce.BruceState.Walk;
                            break;
                    }

                    break;
                case Chair chair:
                    if (chair.State != Chair.ChairState.Falling) break;

                    foreach (var floorBounds in _collideables.OfType<Floor>().Where(c => c.Bounds.Intersects(chair.Bounds)).Select(static floor => floor.Bounds))
                    {
                        while (floorBounds.Intersects(chair.Bounds))
                            chair.Move(-Vector2.UnitY);
                        chair.State = Chair.ChairState.Idle;
                        _soundPosition = chair.Position;

                        _soundManager.PlaySound("Chair Drop");
                    }

                    break;
                case Wardrobe wardrobe:

                    var wardrobePosition = wardrobe.Position;
                    var wardrobePositionX = wardrobePosition.X;
                    var wardrobeBounds = wardrobe.Bounds;

                    if (wardrobe.State == Wardrobe.WardrobeState.Opening)
                        _soundPosition = new(wardrobePositionX + wardrobeBounds.Width / 2f,
                            wardrobePosition.Y + wardrobeBounds.Height / 2f);
                    if (!colliding || !(brucePositionX + (bruceDirection == Direction.Left ? 8 : 32) >
                                        wardrobePositionX))
                        break;

                    bool negated;

                    if (wardrobe.EnterBox.Intersects(Bruce.Bounds))
                    {
                        var linkedWardrobe = wardrobe.LinkedWardrobe;
                        negated = wardrobe.IsLocked || !linkedWardrobe?.IsLocked != true ||
                                  CollidingWithSolid(linkedWardrobe.EnterBox);
                        if (bruceState != Bruce.BruceState.Idle && bruceState != Bruce.BruceState.Walk) break;

                        actionBubble.SetAction(ActionBubble.BubbleAction.Wardrobe, negated);
                    }
                    else
                    {
                        negated = CollidingWithSolid(wardrobeBounds, false) || TooCloseToSolid(wardrobeBounds);
                        if (bruceState != Bruce.BruceState.Idle && bruceState != Bruce.BruceState.Walk) break;

                        actionBubble.SetAction(ActionBubble.BubbleAction.Push, negated);
                    }

                    if (Bruce.Position.X > wardrobe.EnterBox.X && Bruce.Direction is Direction.Right) break;

                    actionBubble.IsVisible = true;
                    if (!negated)
                        Bruce.Interactor = wardrobe;
                    break;
                case Bookcase bookcase:
                    if (!colliding || bookcase.State != Bookcase.BookcaseState.Open) continue;

                    actionBubble.SetAction(ActionBubble.BubbleAction.Bookcase, false);
                    actionBubble.IsVisible = true;
                    Bruce.Interactor = bookcase;
                    break;
                case Portrait portrait:
                    if (portrait.WasMoved && portrait.InTime != TimePeriod || !colliding ||
                        !(brucePositionX + Bruce.Width - 10 > portrait.X) ||
                        bruceState != Bruce.BruceState.Idle && bruceState != Bruce.BruceState.Walk) break;

                    actionBubble.SetAction(
                        portrait.SendTime == TimePeriod.FarPast
                            ? ActionBubble.BubbleAction.OldPortrait
                            : ActionBubble.BubbleAction.Portrait, false);
                    actionBubble.IsVisible = true;
                    Bruce.Interactor = portrait;
                    break;
                case Floor floor:
                    if (bruceState is Bruce.BruceState.StairsLeft or Bruce.BruceState.StairsRight)
                        continue;

                    Bruce.StandOn(floor);
                    break;
                case Door door:
                    if (!door.IsLocked || !colliding || bruceState != Bruce.BruceState.Walk) continue;

                    var doorBoundsX = door.Bounds.X;

                    if (bruceDirection == Direction.Left && brucePositionX > doorBoundsX
                        || bruceDirection == Direction.Right && brucePositionX < doorBoundsX)
                        Bruce.State = Bruce.BruceState.PushingStill;
                    break;
                default:
                    if (!actionBubble.IsVisible && !(Bruce.Interactor is Wardrobe &&
                                                           bruceState is Bruce.BruceState.PushingStill
                                                               or Bruce.BruceState.PushWalk))
                        Bruce.Interactor = null;
                    if (!colliding || bruceState != Bruce.BruceState.Walk || !collideable.IsSolid) continue;

                    var collideableBoundsX = collideable.Bounds.X;

                    if (bruceDirection == Direction.Left && brucePositionX > collideableBoundsX
                        || bruceDirection == Direction.Right && brucePositionX < collideableBoundsX)
                        Bruce.State = Bruce.BruceState.PushingStill;
                    break;
            }
        }

        if (_soundTriggered)
            _soundPosition = Vector2.Zero;
        if (Bruce.Bounds.Intersects(new(0, 0, Level.Width, Level.Height))) return;

        if (NextLevel)
            InitLevel(true);
        else
        {
            //TODO: CHANGE THIS STUFF
            GoToLevel("Level1");
            InitLevel(false);
            GameState = GameState.MainMenu;
        }
    }

    void AddObject(object obj)
    {
        if (obj is IDrawable drawable) _drawables.Add(drawable);
        if (obj is IUpdatable updatable) _updatables.Add(updatable);
        if (obj is ICollideable collideable) _collideables.Add(collideable);
        if (obj is IResetable resetable) _resetables.Add(resetable);
    }

    bool TooCloseToSolid(Rectangle box) => _collideables.Any(col =>
        col is not Paranothing.Bruce and not Stairs and not Floor && col.IsSolid &&
        new Rectangle(box.X - Bruce.Width, box.Y, box.Width + 2 * Bruce.Width, box.Height).Intersects(col.Bounds) &&
        (box.Center.X < col.Bounds.X && Bruce.Direction == Direction.Left ||
         box.Center.X > col.Bounds.X && Bruce.Direction == Direction.Right));
}