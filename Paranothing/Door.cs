using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Doors : ICollideable, Updatable, IDrawable, IInteractable, Lockable, Saveable
    {
        # region Attributes

        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SoundManager soundMan = SoundManager.getInstance();
        //Collidable
        private Vector2 position;
        private Rectangle bounds { get { return new Rectangle(X + 25, Y, 8, 75); } }
        //Drawable
        private SpriteSheet sheet;
        private bool startLocked;
        private bool locked;
        private int frameTime;
        private int frameLength;
        private int frame;
        private string animName;
        private List<int> animFrames;
        private enum DoorsState { Closed, Opening, Open }
        private DoorsState state;
        private string keyName;

        # endregion

        # region Constructor

        public Doors(int x, int y, int width, int height, int frameLength, bool startLocked)
        {
            this.sheet = sheetMan.getSheet("door");
            position = new Vector2(x, y);
            locked = startLocked;
            this.frameLength = frameLength;
            if (locked)
            {
                if (control.timePeriod == TimePeriod.Present)
                    Animation = "doorclosedpresent";
                else
                    Animation = "doorclosed";
                state = DoorsState.Closed;
            }
            else
            {
                if (control.timePeriod == TimePeriod.Present)
                    Animation = "dooropeningpresent";
                else
                    Animation = "dooropeningpast";
                state = DoorsState.Open;
            }
        }

        public Doors(string saveString)
        {
            this.sheet = sheetMan.getSheet("door");
            X = 0;
            Y = 0;
            startLocked = false;
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndDoor") && lineNum < lines.Length)
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
                if (line.StartsWith("locked:"))
                {
                    try { startLocked = bool.Parse(line.Substring(7)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("keyName:"))
                {
                    keyName = line.Substring(8).Trim();
                }
                lineNum++;
            }
            locked = startLocked;
            
            if (locked)
            {
                if (control.timePeriod == TimePeriod.Present)
                    Animation = "doorclosedpresent";
                else
                    Animation = "doorclosedpast";
                state = DoorsState.Closed;
            }
            else
            {
                if (control.timePeriod == TimePeriod.Present)
                    Animation = "dooropeningpresent";
                else
                    Animation = "dooropeningpast";
                state = DoorsState.Open;
            }

        }

        # endregion

        # region Methods

        public void reset()
        {
            locked = startLocked;

            if (locked)
            {
                if (control.timePeriod == TimePeriod.Present)
                    Animation = "doorclosedpresent";
                else
                    Animation = "doorclosedpast";
                state = DoorsState.Closed;
            }
            else
            {
                if (control.timePeriod == TimePeriod.Present)
                    Animation = "dooropeningpresent";
                else
                    Animation = "dooropeningpast";
                state = DoorsState.Open;
            }
        }

        //Accessors & Mutators
        public int X
        {
            get { return (int)position.X; }
            set { position.X = value; }
        }
        public int Y
        {
            get { return (int)position.Y; }
            set { position.Y = value; }
        }
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

        //Lockable
        public void lockObj()
        {
            locked = true;
        }

        public void unlockObj()
        {
            locked = false;

            if (GameTitle.toggleSound)
            {
                soundMan.playSound("Door Unlock");
            }
        }

        public bool isLocked()
        {
            return locked;
        }

        //Collideable
        public Rectangle GetBounds()
        {
            return bounds;
        }
        public bool IsSolid()
        {
            return locked;
        }

        //Drawable
        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            Rectangle sprite = sheet.getSprite(animFrames.ElementAt(frame));
            renderer.Draw(sheet.image, position, sprite, tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Background);
        }

        //Updatable
        public void update(GameTime time)
        {
            int elapsed = time.ElapsedGameTime.Milliseconds;
            frameTime += elapsed;
            if (keyName != null && keyName != "")
            {
                DoorKeys k = DoorKeys.getKey(keyName);
                if (k != null)
                {
                    if (k.pickedUp && state == DoorsState.Closed)
                    {
                        state = DoorsState.Opening;
                        frameLength = 100;
                        unlockObj();
                    }
                }
            }

            string timeP = "past";
            if (control.timePeriod == TimePeriod.Present)
                timeP = "present";
            switch (state)
            {
                case DoorsState.Open:
                    Animation = "dooropen" + timeP;
                    break;
                case DoorsState.Opening:
                    if (frameTime >= frameLength)
                    {
                        Animation = "dooropen" + timeP;
                        state = DoorsState.Open;
                    }
                    else
                        Animation = "dooropening" + timeP;
                    break;
                case DoorsState.Closed:
                    Animation = "doorclosed" + timeP;
                    break;
            }
            if (frameTime >= frameLength)
            {
                frameTime = 0;
                frame = (frame + 1) % animFrames.Count;
            }
        }

        public void setKeyName(string keyName)
        {
            this.keyName = keyName;
        }

        //Interactive
        public void Interact()
        {
        }

        # endregion

        public string saveData()
        {
            return "StartDoor\nx:" + X + "\ny:" + Y + "\nlocked:" + startLocked + "\nkeyName:" + keyName + "\nEndDoor";
        }
    }
}
