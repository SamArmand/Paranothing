using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing
{
    class Boy : IDrawable, Updatable, ICollideable
    {
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SoundManager soundMan = SoundManager.getInstance();
        private SpriteSheet sheet;
        private int pushSoundTimer = 0;
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
               
                if (sheet.hasAnimation(value) && animName != value)
                {
                    animName = value;
                    animFrames = sheet.getAnimation(animName);
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
        public BoyState state;
        public Direction direction;
        public ActionBubble actionBubble;
        private Vector2 teleportTo;
        private TimePeriod timeTravelTo;
        public Chairs nearestChair;
        public IInteractable interactor;

        public Boy(float X, float Y, ActionBubble actionBubble)
        {
            this.sheet = sheetMan.getSheet("boy");
            frame = 0;
            frameTime = 0;
            frameLength = 60;
            position = new Vector2(X, Y);
            Width = 38;
            Height = 58;
            state = BoyState.Idle;
            Animation = "stand";
            direction = Direction.Right;
            this.actionBubble = actionBubble;
            actionBubble.Player = this;
            actionBubble.show();
            teleportTo = new Vector2();
            drawLayer = DrawLayer.Player;
        }

        public void reset()
        {
            frame = 0;
            frameTime = 0;
            frameLength = 60;
            position = new Vector2(X, Y);
            Width = 38;
            Height = 58;
            state = BoyState.Idle;
            Animation = "stand";
            direction = Direction.Right;
            actionBubble.Player = this;
            actionBubble.show();
            teleportTo = new Vector2();
            nearestChair = null;
        }

        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            SpriteEffects flip = SpriteEffects.None;
            if (direction == Direction.Left)
                flip = SpriteEffects.FlipHorizontally;
            Rectangle sprite = sheet.getSprite(animFrames.ElementAt(frame));
            renderer.Draw(sheet.image, position, sprite, tint, 0f, new Vector2(), 1f, flip, drawLayer);
        }

        private void checkInput(GameController control)
        {
            if (state != BoyState.Die)
            {
                if ((control.keyState.IsKeyDown(Keys.Space) || control.padState.IsButtonDown(Buttons.A)))
                {
                    if ((((state == BoyState.Walk || state == BoyState.Idle) && control.keyState.IsKeyUp(Keys.Right) && control.keyState.IsKeyUp(Keys.Left)
                    && control.padState.IsButtonUp(Buttons.LeftThumbstickRight) && control.padState.IsButtonUp(Buttons.LeftThumbstickLeft)) || state == BoyState.PushWalk) && null != interactor)
                    {
                        interactor.Interact();
                    }


                }
                else if ((state == BoyState.PushingStill ||state == BoyState.PushWalk) && interactor != null)
                {
                    state = BoyState.Idle;
                }
                if (control.keyState.IsKeyDown(Keys.LeftControl) || control.padState.IsButtonDown(Buttons.RightTrigger))                {
                    if (nearestChair != null)
                    {
                        state = BoyState.ControllingChair;
                        nearestChair.state = Chairs.ChairsState.Moving;
                    }
                }
                else
                {
                    if (nearestChair != null && nearestChair.state == Chairs.ChairsState.Moving)
                    {
                        nearestChair.state = Chairs.ChairsState.Falling;
                        state = BoyState.Idle;
                    }
                }
                if (control.keyState.IsKeyUp(Keys.Left) && control.keyState.IsKeyUp(Keys.Right)
                    && control.padState.IsButtonUp(Buttons.LeftThumbstickLeft) && control.padState.IsButtonUp(Buttons.LeftThumbstickRight)
                    && state != BoyState.Teleport && state != BoyState.TimeTravel)
                {
                    if (state != BoyState.ControllingChair)
                    {
                        if (direction == Direction.Right)
                        {
                            if (state != BoyState.StairsRight && state != BoyState.StairsLeft)
                            {
                                if ((state == BoyState.PushWalk || state == BoyState.PushingStill) && interactor != null)
                                    state = BoyState.PushingStill;
                                else
                                    state = BoyState.Idle;
                            }
                        }
                        else
                        {
                            if (state != BoyState.StairsRight && state != BoyState.StairsLeft)
                            {
                                if ((state == BoyState.PushWalk || state == BoyState.PushingStill) && interactor != null)
                                    state = BoyState.PushingStill;
                                else
                                    state = BoyState.Idle;
                            }
                        }
                    }
                }
                else
                {
                    if (state != BoyState.ControllingChair)
                    {
                        if (control.keyState.IsKeyDown(Keys.Right) || control.padState.IsButtonDown(Buttons.LeftThumbstickRight))
                        {
                            if ((state != BoyState.PushWalk && state != BoyState.PushingStill) || (interactor != null && ((Wardrobe)interactor).X > X))
                                direction = Direction.Right;
                            if (state == BoyState.Idle)
                                state = BoyState.Walk;
                            if (state == BoyState.PushingStill && direction == Direction.Right && interactor != null)
                                state = BoyState.PushWalk;
                        }
                        else if (control.keyState.IsKeyDown(Keys.Left) || control.padState.IsButtonDown(Buttons.LeftThumbstickLeft))
                        {
                            if ((state != BoyState.PushWalk && state != BoyState.PushingStill) || (interactor != null && ((Wardrobe)interactor).X < X))
                                direction = Direction.Left;
                            if (state == BoyState.Idle)
                                state = BoyState.Walk;
                            if (state == BoyState.PushingStill && direction == Direction.Left && interactor != null)
                                state = BoyState.PushWalk;
                        }
                    }
                }
                if (state == BoyState.ControllingChair)
                {

                    if (nearestChair != null && nearestChair.state == Chairs.ChairsState.Moving)
                    {
                        if (control.keyState.IsKeyDown(Keys.Right) || control.padState.IsButtonDown(Buttons.LeftThumbstickRight))
                        {
                            nearestChair.move(Direction.Right);
                        }
                        else if (control.keyState.IsKeyDown(Keys.Left) || control.padState.IsButtonDown(Buttons.LeftThumbstickLeft))
                        {
                            nearestChair.move(Direction.Left);
                        }
                        if (control.keyState.IsKeyDown(Keys.Up) || control.padState.IsButtonDown(Buttons.LeftThumbstickUp))
                        {
                            nearestChair.move(Direction.Up);
                        }
                        else if (control.keyState.IsKeyDown(Keys.Down) || control.padState.IsButtonDown(Buttons.LeftThumbstickDown))
                        {
                            nearestChair.move(Direction.Down);
                        }
                    }
                    else
                    {
                        state = BoyState.Idle;
                    }
                }
            }
        }

        public void update(GameTime time)
        {
            int elapsed = time.ElapsedGameTime.Milliseconds;
            frameTime += elapsed;
            checkInput(control);
            drawLayer = DrawLayer.Player;

            switch (state)
            {
                case BoyState.Idle:
                    if (Animation == "pushstill" || Animation == "startpush" || Animation == "push")
                    {
                        Animation = "endpush";
                    }
                    if (Animation == "control")
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
                    control.hideDialogue();
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
                        Wardrobe targetWR = ((Wardrobe)interactor).getLinkedWR();
                        if (targetWR != null)
                        {
                            Rectangle target = targetWR.GetBounds();
                            teleportTo = new Vector2(target.X + 16, target.Y + 24);
                            interactor = null;
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
                        state = BoyState.Idle;
                    }
                    break;
                case BoyState.TimeTravel:
                    moveSpeedX = 0;
                    moveSpeedY = 0;
                    if (Animation == "walk" || Animation == "stand")
                    {
                        Portrait p = (Portrait)interactor;
                        if (p.wasMoved)
                            teleportTo = p.movedPos;
                        else
                            teleportTo = new Vector2();
                        Animation = "enterportrait";
                        if (control.timePeriod == TimePeriod.Present) 
                            timeTravelTo = ((Portrait)interactor).sendTime;
                        interactor = null;
                    }
                    if (Animation == "enterportrait" && frame == 7)
                    {
                        if (control.timePeriod != TimePeriod.Present)
                            control.timePeriod = TimePeriod.Present;
                        else
                            control.timePeriod = timeTravelTo;
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
                        state = BoyState.Idle;
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
                        reset();
                        control.resetLevel();
                    }
                    break;
            }
            if (frameTime >= frameLength)
            {
                int flip = 1;
                if (direction == Direction.Left)
                    flip = -1;
                X += moveSpeedX * flip;
                if (state == BoyState.PushWalk && Animation == "push" && interactor != null)
                {
                    pushSoundTimer += elapsed;
                    Wardrobe w = (Wardrobe)interactor;
                    if (!control.collidingWithSolid(w.pushBox, false))
                    {
                        w.X += (int)(moveSpeedX * flip);
                        if (pushSoundTimer > 200 && GameTitle.toggleSound)
                        {
                            soundMan.playSound("Pushing Wardrobe");
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
