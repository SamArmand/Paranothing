using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing
{
    sealed class GameController
    {
        readonly SoundManager _soundMan = SoundManager.Instance();
        public KeyboardState KeyState;
        public GamePadState PadState;

        List<IUpdatable> _updatableObjs;
        List<IDrawable> _drawableObjs;
        List<ICollideable> _collideableObjs;
        public Boy Player;
        public GameState State;
        public TimePeriod TimePeriod;
        public Level Level;
        public Camera Camera;

        bool _soundTriggered;
        Vector2 _soundPos;
        bool _showingDialogue;
        string _dialogue = "";
        int _dialogueTimer;

        readonly Dictionary<string, Level> _levels;

        static GameController _instance;

        public static GameController GetInstance() => _instance ?? (_instance = new GameController());

        GameController()
        {
            _updatableObjs = new List<IUpdatable>();
            _drawableObjs = new List<IDrawable>();
            _collideableObjs = new List<ICollideable>();
            _levels = new Dictionary<string, Level>();
            State = GameState.Game;
            TimePeriod = TimePeriod.Present;
        }

        public void AddLevel(Level level) => _levels.Add(level.Name, level);

        public void GoToLevel(string levelName)
        {
            if (_levels.ContainsKey(levelName)) _levels.TryGetValue(levelName, out Level);
        }

        public bool NextLevel()
        {
            var nextLevel = Level.NextLevel;
            if (!_levels.ContainsKey(nextLevel)) return false;

            _levels.TryGetValue(nextLevel, out Level);
            return true;
        }

        public void SetPlayer(Boy player)
        {
            Player = player;
            AddObject(player);
            AddObject(player.ActionBubble);
        }

        public void SetCamera(Camera camera)
        {
            Camera = camera;
            AddObject(camera);
        }

        public void InitLevel(bool preserveTime)
        {
            _showingDialogue = false;
            _updatableObjs = new List<IUpdatable>();
            _drawableObjs = new List<IDrawable>();
            _collideableObjs = new List<ICollideable>();
            Player.Reset();
            Player.X = Level.PlayerX;
            Player.Y = Level.PlayerY;
            AddObject(Player);
            AddObject(Player.ActionBubble);
            AddObject(Camera);
            foreach (var obj in Level.GetObjs())
            {
                obj.Reset();
                AddObject(obj);
            }
            if (!preserveTime)
                TimePeriod = Level.StartTime;
        }

        public void ResetLevel()
        {
            _showingDialogue = false;
            Player.X = Level.PlayerX;
            Player.Y = Level.PlayerY;
            foreach (var obj in Level.GetObjs()) obj.Reset();
            TimePeriod = Level.StartTime;
        }

        public void UpdateObjs(GameTime time)
        {
            KeyState = Keyboard.GetState();
            PadState = GamePad.GetState(PlayerIndex.One);

            if (_showingDialogue)
                _dialogueTimer += time.ElapsedGameTime.Milliseconds;
            else
                _dialogueTimer = 0;

            foreach (var obj in _updatableObjs) obj.Update(time);

            Player.ActionBubble.Hide();

            if (Math.Abs(_soundPos.X) > 0 && Math.Abs(_soundPos.Y) > 0)
                _soundTriggered = true;
            else _soundTriggered = false;

            foreach (var obj in _collideableObjs)
            {
                var colliding = Collides(obj.GetBounds(), Player.GetBounds());
                switch (obj)
                {
                    case Shadows shadow:
                        UpdateShadow(shadow, colliding);
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
                    case Button button1:
                        var button = button1;
                        var pressed = _collideableObjs.Any(
                            c => (c is Boy || TimePeriod == TimePeriod.Present && c is Shadows)
                                 && Collides(button.GetBounds(), c.GetBounds()));
                        if (!button.StepOn && pressed)
                            _soundMan.PlaySound("Button Press");
                        button.StepOn = pressed;
                        break;
                    case Stairs stairs:
                        UpdateStairs(stairs, colliding);
                        break;
                    case Chair chair:
                        UpdateChair(chair);
                        break;
                    case Wardrobe wardrobe:
                        UpdateWardrobe(wardrobe, colliding);
                        break;
                    case Bookcase interactor:
                        var bookcase = interactor;
                        if (!colliding || bookcase.State != Bookcase.BookcasesState.Open) continue;

                        Player.ActionBubble.SetAction(ActionBubble.BubbleAction.Bookcase, false);
                        Player.ActionBubble.Show();
                        Player.Interactor = interactor;
                        break;
                    case Portrait portrait:
                        UpdatePortrait(portrait, colliding);
                        break;
                    case Floor floor:
                        if (Player.State == Boy.BoyState.StairsLeft || Player.State == Boy.BoyState.StairsRight) continue;

                        while (Collides(Player.GetBounds(), floor.GetBounds())) --Player.Y;
                        break;
                    case Door door1:
                        var door = door1;
                        if (!door.IsLocked || !colliding || Player.State != Boy.BoyState.Walk) continue;

                        if (Player.Direction == Direction.Left && Player.X > door.GetBounds().X
                            || Player.Direction == Direction.Right && Player.X < door.GetBounds().X)
                            Player.State = Boy.BoyState.PushingStill;
                        break;
                    default:
                        if (!Player.ActionBubble.IsVisible && !(Player.Interactor is Wardrobe && (Player.State == Boy.BoyState.PushingStill || Player.State == Boy.BoyState.PushWalk)))
                            Player.Interactor = null;
                        var collider = obj;
                        if (!colliding || Player.State != Boy.BoyState.Walk || !collider.IsSolid()) continue;

                        if (Player.Direction == Direction.Left && Player.X > collider.GetBounds().X
                            || (Player.Direction == Direction.Right && Player.X < collider.GetBounds().X))
                            Player.State = Boy.BoyState.PushingStill;
                        break;
                }
            }
            if (_soundTriggered)
                _soundPos = new Vector2();
            if (Collides(Player.GetBounds(), new Rectangle(0, 0, Level.Width, Level.Height))) return;

            if (NextLevel())
            {
                InitLevel(true);
            }
            else
            {
                //TODO: CHANGE THIS STUFF
                GoToLevel("Level1");
                InitLevel(false);
                State = GameState.MainMenu;
            }
        }

        void UpdatePortrait(Portrait portrait, bool colliding)
        {
            if (portrait.WasMoved && portrait.InTime != TimePeriod || !colliding ||
                !(Player.X + Player.Width - 10 > portrait.X) ||
                Player.State != Boy.BoyState.Idle && Player.State != Boy.BoyState.Walk) return;

            Player.ActionBubble.SetAction(
                portrait.SendTime == TimePeriod.FarPast
                    ? ActionBubble.BubbleAction.OldPortrait
                    : ActionBubble.BubbleAction.Portrait, false);
            Player.ActionBubble.Show();
            Player.Interactor = portrait;
        }

        void UpdateChair(Chair chair)
        {
            if (chair.State != Chair.ChairsState.Falling) return;

            foreach (var c in _collideableObjs.OfType<Floor>().Where(c => Collides(c.GetBounds(), chair.GetBounds())))
            {
                while (Collides(c.GetBounds(), chair.GetBounds()))
                    --chair.Y;
                chair.State = Chair.ChairsState.Idle;
                _soundPos = new Vector2(chair.X, chair.Y);

                _soundMan.PlaySound("Chair Drop");
            }
        }

        void UpdateWardrobe(Wardrobe wardrobe, bool colliding)
        {
            if (wardrobe.State == Wardrobe.WardrobeState.Opening)
                _soundPos = new Vector2(wardrobe.X + wardrobe.GetBounds().Width / 2,
                    wardrobe.Y + wardrobe.GetBounds().Height / 2);
            if (!colliding || !(Player.X + (Player.Direction == Direction.Left ? 8 : 32) > wardrobe.X))
                return;

            bool negated;
            if (Collides(wardrobe.EnterBox, Player.GetBounds()))
            {
                var linkedWr = wardrobe.GetLinkedWr();
                negated = wardrobe.IsLocked() || !linkedWr?.IsLocked() != true || CollidingWithSolid(linkedWr.EnterBox);
                if (Player.State != Boy.BoyState.Idle && Player.State != Boy.BoyState.Walk) return;

                Player.ActionBubble.SetAction(ActionBubble.BubbleAction.Wardrobe, negated);
            }
            else
            {
                negated = CollidingWithSolid(wardrobe.GetBounds(), false);
                if (Player.State != Boy.BoyState.Idle && Player.State != Boy.BoyState.Walk) return;

                Player.ActionBubble.SetAction(ActionBubble.BubbleAction.Push, negated);
            }

            Player.ActionBubble.Show();
            if (!negated)
                Player.Interactor = wardrobe;
        }

        void UpdateStairs(Stairs stair, bool colliding)
        {
            if (!stair.IsSolid() || !colliding) return;

            if (Player.X + 30 >= stair.X && Player.X + 8 <= stair.X)
            {
                if ((stair.Direction == Direction.Left && Math.Abs(Player.Y + 58 - stair.GetSmallBounds().Y) <= 0
                     || stair.Direction == Direction.Right &&
                     Math.Abs(Player.Y + 58 - (stair.Y + stair.GetBounds().Height)) <= 0)
                    && (Player.State == Boy.BoyState.Idle || Player.State == Boy.BoyState.Walk))
                {
                    Player.ActionBubble.SetAction(ActionBubble.BubbleAction.Stair, false);
                    Player.Interactor = stair;
                    Player.ActionBubble.Show();
                }
            }
            else if (Player.X + 30 >= stair.X + stair.GetBounds().Width && Player.X + 8 <= stair.X + stair.GetBounds().Width)
            {
                if ((stair.Direction == Direction.Right && Math.Abs(Player.Y + 58 - stair.GetSmallBounds().Y) <= 0
                     || stair.Direction == Direction.Left &&
                     Math.Abs(Player.Y + 58 - (stair.Y + stair.GetBounds().Height)) <= 0)
                    && (Player.State == Boy.BoyState.Idle || Player.State == Boy.BoyState.Walk))
                {
                    Player.ActionBubble.SetAction(ActionBubble.BubbleAction.Stair, false);
                    Player.Interactor = stair;
                    Player.ActionBubble.Show();
                }
            }

            if (Player.State != Boy.BoyState.StairsLeft && Player.State != Boy.BoyState.StairsRight)
                return;

            switch (stair.Direction)
            {
                case Direction.Left:
                    if (Player.Direction == Direction.Left && (int) Player.Y + 58 == stair.GetSmallBounds().Y
                        || Player.Direction == Direction.Right && (int) Player.Y + 58 == stair.Y + stair.GetBounds().Height)
                        Player.State = Boy.BoyState.Walk;
                    break;
                case Direction.Right:
                    if (Player.Direction == Direction.Right && (int) Player.Y + 58 == stair.GetSmallBounds().Y
                        || Player.Direction == Direction.Left && (int) Player.Y + 58 == stair.Y + stair.GetBounds().Height)
                        Player.State = Boy.BoyState.Walk;
                    break;
                case Direction.Up:
                    break;
                case Direction.Down:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void UpdateShadow(Shadows shadow, bool colliding)
        {
            if (_soundTriggered && TimePeriod == TimePeriod.Present && _soundPos.Y >= shadow.Y && _soundPos.Y <= shadow.Y + 81)
                shadow.StalkNoise((int) _soundPos.X, (int) _soundPos.Y);

            if (!colliding || TimePeriod != TimePeriod.Present || Player.State == Boy.BoyState.StairsLeft ||
                Player.State == Boy.BoyState.StairsRight) return;

            Player.Direction = shadow.X > Player.X ? Direction.Right : Direction.Left;
            Player.State = Boy.BoyState.Die;

            _soundMan.PlaySound("Death");
            shadow.State = Shadows.ShadowState.Idle;
        }

        public void DrawObjs(SpriteBatch renderer)
        {

            if (_showingDialogue)
            {
                var textDim = Game1.GameFont.MeasureString(_dialogue);
                renderer.DrawString(Game1.GameFont, _dialogue, new Vector2
                {
                    X = Camera.X + Camera.Width / 2f / Camera.Scale - textDim.X / 2,
                    Y = Camera.Y + Camera.Height / Camera.Scale - textDim.Y - 10
                }, Color.White);
            }
            var tint = Color.White;
            switch (TimePeriod)
            {
                case TimePeriod.Past:
                    tint.A = 32;
                    break;
                case TimePeriod.FarPast:
                    tint.A = 4;
                    break;
                case TimePeriod.Present:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var obj in _drawableObjs) obj.Draw(renderer, tint);

            Player.Draw(renderer, tint);

        }

        static bool Collides(Rectangle box1, Rectangle box2)
        {
            var i = Rectangle.Intersect(box1, box2);
                return i.Width != 0;
        }

        void AddObject(object obj)
        {
            if (obj is IDrawable drawable) _drawableObjs.Add(drawable);
            if (obj is IUpdatable updatable) _updatableObjs.Add(updatable);
            if (obj is ICollideable collideable) _collideableObjs.Add(collideable);
        }

        public bool CollidingWithSolid(Rectangle box, bool includePlayer = true)
        {
            return _collideableObjs.Where(col => includePlayer || !(col is Boy)).Where(col => !(col is Stairs)).Any(col => col.IsSolid() && Collides(box, col.GetBounds()));
        }

        public void ShowDialogue(string text)
        {
            _showingDialogue = true;
            _dialogue = text;
        }

        public void HideDialogue()
        {
            if (_dialogueTimer >= 3000)
                _showingDialogue = false;
        }
    }
}