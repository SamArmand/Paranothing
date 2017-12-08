﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Bookcases : ICollideable, Updatable, Drawable, Interactive, Saveable
    {
        # region Attributes

        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SoundManager soundMan = SoundManager.getInstance();
        //Collidable
        private Vector2 position;
        private string button1, button2;
        private int unlockTimer;
        public int X
        {
            get
            {
                return (int)position.X;
            }
            set
            {
                position.X = value;
            }
        }
        public int Y
        {
            get
            {
                return (int)position.Y;
            }
            set
            {
                position.Y = value;
            }
        }
        private Rectangle bounds { get { return new Rectangle(X, Y, 37, 75); } }

        //Drawable
        private SpriteSheet sheet;
        private bool locked;
        private bool startLocked;
        private int frameTime;
        private int frameLength;
        private int frame;
        private string animName;
        private List<int> animFrames;
        public enum BookcasesState { Closed, Closing, Opening, Open }
        public BookcasesState state;
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

        # endregion

        # region Constructors

        public Bookcases(int x, int y, string button1, string button2)
        {
            bool startLocked = false;
            this.sheet = sheetMan.getSheet("bookcase");
            position = new Vector2(x, y);
            this.startLocked = startLocked;
            locked = startLocked;
            if (startLocked)
            {
                Animation = "bookcaseclosed";
                state = BookcasesState.Closed;
            }
            else
            {
                Animation = "bookcaseopening";
                state = BookcasesState.Open;
            }
            frameLength = 100;
            this.button1 = button1;
            this.button2 = button2;
        }

        public Bookcases(string saveString)
        {
            this.sheet = sheetMan.getSheet("bookcase");
            int x = 0;
            int y = 0;
            startLocked = false;
            button1 = "";
            button2 = "";
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndBookcase") && lineNum < lines.Length)
            {
                line = lines[lineNum];
                if (line.StartsWith("x:"))
                {
                    try { x = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("y:"))
                {
                    try { y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("button1:"))
                {
                    button1 = line.Substring(8).Trim();
                }
                if (line.StartsWith("button2:"))
                {
                    button2 = line.Substring(8).Trim();
                }
                lineNum++;
            }
            locked = startLocked;
            if (startLocked)
            {
                Animation = "bookcaseclosed";
                state = BookcasesState.Closed;
            }
            else
            {
                Animation = "bookcaseopening";
                state = BookcasesState.Open;
            }
            position = new Vector2(x, y);
            frameLength = 100;
        }

        # endregion

        # region Methods

        //Collideable
        public Rectangle GetBounds()
        {
            return bounds;
        }
        public bool IsSolid()
        {
            return false;
        }

        //Drawable
        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void draw(SpriteBatch renderer, Color tint)
        {
            Rectangle sprite = sheet.getSprite(animFrames.ElementAt(frame));
            renderer.Draw(sheet.image, new Vector2(X, Y), sprite, tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Wardrobe);
        }

        //Updatable
        public void update(GameTime time)
        {
            int elapsed = time.ElapsedGameTime.Milliseconds;
            frameTime += elapsed;
            if (state == BookcasesState.Opening)
                unlockTimer += elapsed;
            if (unlockTimer >= 450)
            {
                if (GameTitle.toggleSound)
                {
                    soundMan.playSound("Final Door Part 2");
                }
                unlockTimer = 0;
            }
            bool b1Pushed = false;
            bool b2Pushed = false;
            if (button1 != "")
            {
                Button b = Button.getKey(button1);
                if (b != null && b.stepOn)
                {
                    b1Pushed = true;
                }
            }
            else
                b1Pushed = true;

            if (button2 != "")
            {
                Button b = Button.getKey(button2);
                if (b != null && b.stepOn)
                {
                    b2Pushed = true;
                }
            }
            else
                b2Pushed = true;

            if (b1Pushed && b2Pushed)
            {
                if (state == BookcasesState.Closed)
                {
                    state = BookcasesState.Opening;
                    if (unlockTimer == 0 && GameTitle.toggleSound)
                    {
                        soundMan.playSound("Final Door Part 1");
                    }
                }
            }
            else
            {
                unlockTimer = 0;
                if (state != BookcasesState.Closed)
                    state = BookcasesState.Closing;
                else
                    state = BookcasesState.Closed;
            }

            switch (state)
            {
                case BookcasesState.Open:
                    Animation = "bookcaseopen";
                    break;
                case BookcasesState.Opening:
                    if (frame == 4)
                    {
                        Animation = "bookcaseopen";
                        state = BookcasesState.Open;
                    }
                    else
                        Animation = "bookcaseopening";
                    break;
                case BookcasesState.Closing:
                    unlockTimer = 0;
                    if (Animation == "bookcaseclosing" && frame == 4)
                    {
                        Animation = "close";
                        state = BookcasesState.Closed;
                    }
                    else
                        Animation = "bookcaseclosing";
                    break;
                case BookcasesState.Closed:
                    unlockTimer = 0;
                    Animation = "bookcaseclosed";
                    break;
            }
            if (frameTime >= frameLength)
            {
                frameTime = 0;
                frame = (frame + 1) % animFrames.Count;
            }
        }

        public void Interact()
        {
            Boy player = control.player;

            if (state == BookcasesState.Open)
                Game1.EndGame = true;

        }

        public string saveData()
        {
            return "StartBookcases\nx:" + X + "\ny:" + Y + "\nbutton1:" + button1 + "\nbutton2:" + button2 + "\nEndBookcases";
        }

        //reset
        public void reset()
        {
            if (control.nextLevel())
                control.initLevel(true);
        }

        # endregion
    }
}
