using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Shadows : ICollideable, Updatable, IDrawable, Saveable
    {
        # region Attributes
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SoundManager soundMan = SoundManager.getInstance();
        //Drawable
        private SpriteSheet sheet;
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
        //Collideable
        private int moveSpeedX, moveSpeedY; // Pixels per animation frame
        private Vector2 startPos;
        private Vector2 position;
        private Vector2 soundPos;
        public int patrolDistance;
        private int distMoved = 0;
        private Rectangle bounds { get { return new Rectangle(X, Y+7, 32, 74); } }
        public int Width, Height;
        public enum ShadowState { Idle, Walk, SeekSound }
        public ShadowState state;
        public Direction direction;

        # endregion

        # region Constructor

        public Shadows(float X, float Y, int distance)
        {
            this.sheet = sheetMan.getSheet("shadow");
            frame = 0;
            frameTime = 0;
            frameLength = 70;
            startPos = new Vector2(X, Y);
            position = new Vector2(X, Y);
            soundPos = new Vector2(X, Y);
            Width = 38;
            Height = 58;
            patrolDistance = distance;
            if (patrolDistance < 0)
                patrolDistance = -patrolDistance;
            distMoved = patrolDistance;
            state = ShadowState.Idle;
            Animation = "stand";
            direction = Direction.Right;
        }

        public Shadows(string saveString)
        {
            this.sheet = sheetMan.getSheet("shadow");
            Animation = "walk";
            state = ShadowState.Walk;
            X = 0;
            Y = 0;
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndShadow") && lineNum < lines.Length)
            {
                line = lines[lineNum];
                if (line.StartsWith("x:"))
                {
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("y:"))
                {
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("patrolDist:"))
                {
                    try { patrolDistance = int.Parse(line.Substring(11)); }
                    catch (FormatException) { }
                }
                lineNum++;
            }
            if (patrolDistance < 0)
                patrolDistance = -patrolDistance;
            distMoved = patrolDistance;
            startPos = new Vector2(X, Y);
            soundPos = new Vector2(X, Y);
        }

        public void reset()
        {
            frame = 0;
            frameTime = 0;
            position = new Vector2(startPos.X, startPos.Y);
            soundPos = new Vector2(startPos.X, startPos.Y);
            distMoved = patrolDistance;
            state = ShadowState.Walk;
            Animation = "walk";
            direction = Direction.Right;
        }
        
        # endregion

        # region Methods

        //Accessors & Mutators
        public int X { get { return (int)position.X; } set { position.X = value; } }
        public int Y { get { return (int)position.Y; } set { position.Y = value; } }

        //Updatable
        public void update(GameTime time)
        {
            if (control.timePeriod == TimePeriod.Present)
            {
                int elapsed = time.ElapsedGameTime.Milliseconds;
                frameTime += elapsed;
                switch (state)
                {
                    case ShadowState.Idle:
                        if (Animation == "walk")
                            Animation = "stopwalk";
                        if (Animation == "stopwalk" && frame == 2)
                            Animation = "stand";
                        moveSpeedX = 0;
                        moveSpeedY = 0;
                        frameLength = 80;
                        break;
                    case ShadowState.Walk:
                        if (patrolDistance != 0)
                        {
                            if ((Animation == "stopwalk" && frame == 2) || Animation == "stand" || Animation == "walk")
                            {
                                frameLength = 80;
                                Animation = "walk";
                                moveSpeedX = 3;
                                moveSpeedY = 0;
                            }
                            else
                            {
                                moveSpeedX = 0;
                            }
                        }
                        else
                            state = ShadowState.Idle;
                        break;
                    case ShadowState.SeekSound:
                        Animation = "walk";
                        moveSpeedX = 3;
                        moveSpeedY = 0;
                        if (soundPos.X > X)
                            direction = Direction.Right;
                        else
                            direction = Direction.Left;
                        if (Math.Abs(soundPos.X - X) < 3)
                            state = ShadowState.Idle;
                        break;
                }
                if (frameTime >= frameLength)
                {
                    int flip = 1;
                    if (direction == Direction.Left)
                        flip = -1;
                    X += moveSpeedX * flip;
                    Y += moveSpeedY * flip;
                    frameTime = 0;
                    frame = (frame + 1) % animFrames.Count;
                    if (state == ShadowState.Walk && patrolDistance != 0)
                    {
                        distMoved += moveSpeedX;
                        if (distMoved >= patrolDistance * 2)
                        {
                            Animation = "stopwalk";
                            X -= (patrolDistance * 2 - distMoved) * flip;
                            if (direction == Direction.Left)
                                direction = Direction.Right;
                            else
                                direction = Direction.Left;
                            distMoved = 0;
                        }
                    }
                    if (control.collidingWithSolid(GetBounds(), false))
                    {
                        if (state == ShadowState.SeekSound)
                        {
                            state = ShadowState.Idle;
                            X -= moveSpeedX * flip;
                            Y -= moveSpeedY * flip;
                        }
                        else if (state == ShadowState.Walk)
                        {
                            distMoved = patrolDistance * 2 - distMoved;
                            distMoved -= moveSpeedX;
                            Animation = "stopwalk";
                            if (direction == Direction.Left)
                            {
                                X += moveSpeedX;
                                direction = Direction.Right;
                            }
                            else
                            {
                                X -= moveSpeedX;
                                direction = Direction.Left;
                            }
                        }
                    }
                }
            }
        }

        public void stalkNoise(int X, int Y)
        {
            soundPos = new Vector2(X, Y);
            state = ShadowState.SeekSound;
            if (Animation == "walk")
                Animation = "stopwalk";

            if (GameTitle.toggleSound)
            {
                soundMan.playSound("Shadow");
            }
        }

        //Drawable
        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            if (control.timePeriod == TimePeriod.Present)
            {
                SpriteEffects flip = SpriteEffects.None;
                if (direction == Direction.Left)
                    flip = SpriteEffects.FlipHorizontally;
                Rectangle sprite = sheet.getSprite(animFrames.ElementAt(frame));
                renderer.Draw(sheet.image, position, sprite, tint, 0f, new Vector2(), 1f, flip, DrawLayer.Player + 0.005f);
            }
        }

        //Collideable
        public Rectangle GetBounds()
        {
            return bounds;
        }

        public bool IsSolid()
        {
            return false;
        }

        public string saveData()
        {
            return "StartShadow\nx:" + X + "\ny:" + Y + "\npatrolDist:" + patrolDistance + "\nEndShadow";
        }

        # endregion
    }
}
