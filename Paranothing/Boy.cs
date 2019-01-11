using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing
{
    class Boy : IDrawable, IUpdatable, ICollideable
    {
        private GameController _control = GameController.GetInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.GetInstance();
        private SoundManager soundMan = SoundManager.Instance();
        private SpriteSheet sheet;
        private int pushSoundTimer;
        private int frame;
        private int frameLength;
        private int frameTime;
        private string animName;
        private List<int> animFrames;
        public string Animation
        {
            get { return animName; }
            set
            {
                if (sheet.HasAnimation(value) && animName != value)
                {
                    animName = value;
                    animFrames = sheet.GetAnimation(animName);
                    frame = 0;
                    frameTime = 0;
                }
            }
        }
        public float drawLayer;
        private float moveSpeedX, moveSpeedY; // Pixels per animation frame
        private Vector2 position;
        public int Width, Height;

        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }

        public enum BoyState { Idle, Walk, StairsLeft, StairsRight, PushWalk, PushingStill, Teleport, TimeTravel, ControllingChair, Die }
        public BoyState State;
        public Direction Direction { get; set; }
        public ActionBubble ActionBubble { get; set; }
        private Vector2 teleportTo;
        private TimePeriod timeTravelTo;
        public Chair NearestChair { get; set; }
        public IInteractable Interactor;

        public Boy(float X, float Y, ActionBubble actionBubble)
        {
            sheet = sheetMan.GetSheet("boy");
            frame = 0;
            frameTime = 0;
            frameLength = 60;
            position = new Vector2(X, Y);
            Width = 38;
            Height = 58;
            State = BoyState.Idle;
            Animation = "stand";
            Direction = Direction.Right;
            ActionBubble = actionBubble;
            actionBubble.Player = this;
            actionBubble.Show();
            teleportTo = new Vector2();
            drawLayer = DrawLayer.Player;
        }

        public void Reset()
        {
            frame = 0;
            frameTime = 0;
            frameLength = 60;
            position = new Vector2(X, Y);
            Width = 38;
            Height = 58;
            State = BoyState.Idle;
            Animation = "stand";
            Direction = Direction.Right;
            ActionBubble.Player = this;
            ActionBubble.Show();
            teleportTo = new Vector2();
            NearestChair = null;
        }

        public Texture2D GetImage() => sheet.Image;

        public void Draw(SpriteBatch renderer, Color tint)
        {
            var flip = SpriteEffects.None;
            if (Direction == Direction.Left)
                flip = SpriteEffects.FlipHorizontally;
            var sprite = sheet.GetSprite(animFrames.ElementAt(frame));
            renderer.Draw(sheet.Image, position, sprite, tint, 0f, new Vector2(), 1f, flip, drawLayer);
        }

        private void CheckInput(GameController control)
        {
            if (State != BoyState.Die)
            {
                if (control.KeyState.IsKeyDown(Keys.Space) || control.PadState.IsButtonDown(Buttons.A))
                {
                    if ((((State == BoyState.Walk || State == BoyState.Idle) && control.KeyState.IsKeyUp(Keys.Right) && control.KeyState.IsKeyUp(Keys.Left)
                    && control.PadState.IsButtonUp(Buttons.LeftThumbstickRight) && control.PadState.IsButtonUp(Buttons.LeftThumbstickLeft)) || State == BoyState.PushWalk) && null != Interactor)
                    {
                        Interactor.Interact();
                    }


                }
                else if ((State == BoyState.PushingStill || State == BoyState.PushWalk) && Interactor != null)
                {
                    State = BoyState.Idle;
                }
                if (control.KeyState.IsKeyDown(Keys.LeftControl) || control.PadState.IsButtonDown(Buttons.RightTrigger))
                {
                    if (NearestChair != null)
                    {
                        State = BoyState.ControllingChair;
                        NearestChair.State = Chair.ChairsState.Moving;
                    }
                }
                else
                {
                    if (NearestChair != null && NearestChair.State == Chair.ChairsState.Moving)
                    {
                        NearestChair.State = Chair.ChairsState.Falling;
                        State = BoyState.Idle;
                    }
                }
                if (control.KeyState.IsKeyUp(Keys.Left) && control.KeyState.IsKeyUp(Keys.Right)
                    && control.PadState.IsButtonUp(Buttons.LeftThumbstickLeft) && control.PadState.IsButtonUp(Buttons.LeftThumbstickRight)
                    && State != BoyState.Teleport && State != BoyState.TimeTravel)
                {
                    if (State != BoyState.ControllingChair)
                    {
                        if (Direction == Direction.Right)
                        {
                            if (State != BoyState.StairsRight && State != BoyState.StairsLeft)
                            {
                                if ((State == BoyState.PushWalk || State == BoyState.PushingStill) && Interactor != null)
                                    State = BoyState.PushingStill;
                                else
                                    State = BoyState.Idle;
                            }
                        }
                        else
                        {
                            if (State != BoyState.StairsRight && State != BoyState.StairsLeft)
                            {
                                if ((State == BoyState.PushWalk || State == BoyState.PushingStill) && Interactor != null)
                                    State = BoyState.PushingStill;
                                else
                                    State = BoyState.Idle;
                            }
                        }
                    }
                }
                else
                {
                    if (State != BoyState.ControllingChair)
                    {
                        if (control.KeyState.IsKeyDown(Keys.Right) || control.PadState.IsButtonDown(Buttons.LeftThumbstickRight))
                        {
                            if ((State != BoyState.PushWalk && State != BoyState.PushingStill) || (Interactor != null && ((Wardrobe)Interactor).X > X))
                                Direction = Direction.Right;
                            if (State == BoyState.Idle)
                                State = BoyState.Walk;
                            if (State == BoyState.PushingStill && Direction == Direction.Right && Interactor != null)
                                State = BoyState.PushWalk;
                        }
                        else if (control.KeyState.IsKeyDown(Keys.Left) || control.PadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                        {
                            if ((State != BoyState.PushWalk && State != BoyState.PushingStill) || (Interactor != null && ((Wardrobe)Interactor).X < X))
                                Direction = Direction.Left;
                            if (State == BoyState.Idle)
                                State = BoyState.Walk;
                            if (State == BoyState.PushingStill && Direction == Direction.Left && Interactor != null)
                                State = BoyState.PushWalk;
                        }
                    }
                }
                if (State == BoyState.ControllingChair)
                {

                    if (NearestChair != null && NearestChair.State == Chair.ChairsState.Moving)
                    {
                        if (control.KeyState.IsKeyDown(Keys.Right) || control.PadState.IsButtonDown(Buttons.LeftThumbstickRight))
                        {
                            NearestChair.Move(Direction.Right);
                        }
                        else if (control.KeyState.IsKeyDown(Keys.Left) || control.PadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                        {
                            NearestChair.Move(Direction.Left);
                        }
                        if (control.KeyState.IsKeyDown(Keys.Up) || control.PadState.IsButtonDown(Buttons.LeftThumbstickUp))
                        {
                            NearestChair.Move(Direction.Up);
                        }
                        else if (control.KeyState.IsKeyDown(Keys.Down) || control.PadState.IsButtonDown(Buttons.LeftThumbstickDown))
                        {
                            NearestChair.Move(Direction.Down);
                        }
                    }
                    else
                    {
                        State = BoyState.Idle;
                    }
                }
            }
        }

        public void Update(GameTime time)
        {
            int elapsed = time.ElapsedGameTime.Milliseconds;
            frameTime += elapsed;
            CheckInput(_control);
            drawLayer = DrawLayer.Player;

            switch (State)
            {
                case BoyState.Idle:
                    if (Animation == "pushstill" || Animation == "startpush" || Animation == "push")
                    {
                        Animation = "endpush";

                        soundMan.StopSound("Pushing Wardrobe");
                    }
                    if (Animation == "control" || Animation == "controlstart")
                    {
                        Animation = "controlend";
                    }
                    if ((Animation == "endpush" || Animation == "controlend") && frame == 2 || Animation == "walk")
                    {
                        Animation = "stand";
                    }

                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    break;
                case BoyState.Walk:
                    Animation = "walk";
                    moveSpeedX = 3;
                    moveSpeedY = 0;
                    break;
                case BoyState.StairsLeft:
                    Animation = "walk";
                    moveSpeedX = 3;
                    moveSpeedY = 2;
                    drawLayer = DrawLayer.PlayerBehindStairs;
                    break;
                case BoyState.StairsRight:
                    Animation = "walk";
                    moveSpeedX = 3;
                    moveSpeedY = -2;
                    drawLayer = DrawLayer.PlayerBehindStairs;
                    break;
                case BoyState.PushingStill:
                    soundMan.StopSound("Pushing Wardrobe");

                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    if (Animation == "walk" || Animation == "stand")
                        Animation = "startpush";
                    if (Animation == "startpush" && frame == 3 || Animation == "push")
                        Animation = "pushstill";
                    break;
                case BoyState.PushWalk:
                    moveSpeedY = 0;
                    if (Animation == "walk" || Animation == "stand")
                    {
                        moveSpeedX = 0;
                        Animation = "startpush";
                    }
                    if (Animation == "startpush" && frame == 3 || Animation == "pushstill")
                    {
                        Animation = "push";
                        pushSoundTimer = 201;
                    }
                    if (Animation == "push")
                        moveSpeedX = 3;
                    break;

                case BoyState.Teleport:
                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    if (Animation == "walk" || Animation == "stand")
                    {
                        Animation = "enterwardrobe";
                        Wardrobe targetWR = ((Wardrobe)Interactor).GetLinkedWr();
                        if (targetWR != null)
                        {
                            Rectangle target = targetWR.GetBounds();
                            teleportTo = new Vector2(target.X + 16, target.Y + 24);
                            Interactor = null;
                        }
                    }
                    if (Animation == "enterwardrobe" && frame == 6)
                    {
                        position = new Vector2(teleportTo.X, teleportTo.Y);
                        Animation = "leavewardrobe";
                    }
                    if (Animation == "leavewardrobe" && frame == 7)
                    {
                        Animation = "stand";
                        State = BoyState.Idle;
                    }
                    break;
                case BoyState.TimeTravel:
                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    if (Animation == "walk" || Animation == "stand")
                    {
						var p = (Portrait)Interactor;
                        if (p.WasMoved)
                            teleportTo = p.MovedPos;
                        else
                            teleportTo = new Vector2();
                        Animation = "enterportrait";
                        if (_control.TimePeriod == TimePeriod.Present)
                            timeTravelTo = ((Portrait)Interactor).SendTime;
                        Interactor = null;
                    }
                    if (Animation == "enterportrait" && frame == 7)
                    {
                        if (_control.TimePeriod != TimePeriod.Present)
                            _control.TimePeriod = TimePeriod.Present;
                        else
                            _control.TimePeriod = timeTravelTo;
                        Animation = "leaveportrait";
                        if (teleportTo.X != 0 && teleportTo.Y != 0)
                        {
                            X = teleportTo.X;
                            Y = teleportTo.Y;
                        }
                    }
                    if (Animation == "leaveportrait" && frame == 7)
                    {
                        Animation = "stand";
                        State = BoyState.Idle;
                    }
                    break;
                case BoyState.ControllingChair:
                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    if (Animation != "control")
                    {
                        Animation = "controlstart";
                    }
                    if (Animation == "controlstart" && frame == 2)
                        Animation = "control";
                    break;
                case BoyState.Die:
                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    Animation = "disappear";
                    if (frame == 7)
                    {
                        Reset();
                        _control.ResetLevel();
                    }
                    break;
            }
            if (frameTime >= frameLength)
            {
                int flip = 1;
                if (Direction == Direction.Left)
                    flip = -1;
                X += moveSpeedX * flip;
                if (State == BoyState.PushWalk && Animation == "push" && Interactor != null)
                {
                    pushSoundTimer += elapsed;
                    Wardrobe w = (Wardrobe)Interactor;
                    if (!_control.CollidingWithSolid(w.PushBox, false))
                    {
                        w.X += (int)(moveSpeedX * flip);
                        if (pushSoundTimer > 200)
                        {
                            soundMan.PlaySound("Pushing Wardrobe", true);

                            pushSoundTimer = 0;
                        }
                    }
                    else
                        X -= moveSpeedX * flip;
                }
                if (moveSpeedY == 0)
                {
                    moveSpeedY = 1;
                    flip = 1;
                }
                Y += moveSpeedY * flip;
                frameTime = 0;
                frame = (frame + 1) % animFrames.Count;
            }
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)(position.X), (int)(position.Y), Width, Height);
        }

        public bool IsSolid()
        {
            return true;
        }
    }
}