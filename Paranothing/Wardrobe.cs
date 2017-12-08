using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Wardrobe : Collideable, Updatable, Drawable, Interactive, Lockable, Saveable
    {
        # region Attributes
        private static Dictionary<string, Wardrobe> wardrobeDict = new Dictionary<string, Wardrobe>();
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SoundManager soundMan = SoundManager.getInstance();
        //Collidable
        private Vector2 startPos;
        private Vector2 positionPres;
        private Vector2 positionPast1;
        private Vector2 positionPast2;
        private string name;
        private string keyName = "";
        public int X
        {
            get
            {
                switch (control.timePeriod)
                {
                    case TimePeriod.FarPast:
                        return (int)positionPast2.X;
                    case TimePeriod.Past:
                        return (int)positionPast1.X;
                    case TimePeriod.Present:
                        return (int)positionPres.X;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (control.timePeriod)
                {
                    case TimePeriod.FarPast:
                        positionPast2.X = value;
                        positionPast1.X = value;
                        positionPres.X = value;
                        break;
                    case TimePeriod.Past:
                        positionPast1.X = value;
                        positionPres.X = value;
                        break;
                    case TimePeriod.Present:
                        positionPres.X = value;
                        break;
                    default:
                        break;
                }
            }
        }
        public int Y
        {
            get
            {
                switch (control.timePeriod)
                {
                    case TimePeriod.FarPast:
                        return (int)positionPast2.Y;
                    case TimePeriod.Past:
                        return (int)positionPast1.Y;
                    case TimePeriod.Present:
                        return (int)positionPres.Y;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (control.timePeriod)
                {
                    case TimePeriod.FarPast:
                        positionPast2.Y = value;
                        positionPast1.Y = value;
                        positionPres.Y = value;
                        break;
                    case TimePeriod.Past:
                        positionPast1.Y = value;
                        positionPres.Y = value;
                        break;
                    case TimePeriod.Present:
                        positionPres.Y = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public Rectangle enterBox
        {
            get { return new Rectangle(X + 24, Y + 9, 23, 73); }
        }

        public Rectangle pushBox
        {
            get { return new Rectangle(X+2, Y+2, 65, 78); }
        }

        //Drawable
        private SpriteSheet sheet;
        private string linkedName;
        private bool locked;
        private bool startLocked;
        private int frameTime;
        private int frameLength;
        private int frame;
        private string animName;
        private List<int> animFrames;
        public enum WardrobeState { Closed, Opening, Open }
        public WardrobeState state;
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

        # region Constructor

        public Wardrobe(int x, int y, string name)
        {
            this.sheet = sheetMan.getSheet("wardrobe");
            startPos = new Vector2(x, y);
            positionPres = new Vector2(x, y);
            positionPast1 = new Vector2(x, y);
            positionPast2 = new Vector2(x, y);
            this.startLocked = false;
            locked = startLocked;
            if (startLocked)
            {
                Animation = "wardrobeclosed";
                state = WardrobeState.Closed;
            }
            else
            {
                Animation = "wardrobeopening";
                state = WardrobeState.Open;
            }
            frameLength = 80;
            this.name = name;
            if (wardrobeDict.ContainsKey(name))
                wardrobeDict.Remove(name);
            wardrobeDict.Add(name, this);
        }

        public Wardrobe(int x, int y, string name, bool startLocked)
        {
            this.sheet = sheetMan.getSheet("wardrobe");
            positionPres = new Vector2(x, y);
            positionPast1 = new Vector2(x, y);
            positionPast2 = new Vector2(x, y);
            this.startLocked = startLocked;
            locked = startLocked;
            if (startLocked)
            {
                Animation = "wardrobeclosed";
                state = WardrobeState.Closed;
            }
            else
            {
                Animation = "wardrobeopening";
                state = WardrobeState.Open;
            }
            frameLength = 160;
            this.name = name;
            if (wardrobeDict.ContainsKey(name))
                wardrobeDict.Remove(name);
            wardrobeDict.Add(name, this);
        }

        public Wardrobe(string saveString)
        {
            this.sheet = sheetMan.getSheet("wardrobe");
            int x = 0;
            int y = 0;
            startLocked = false;
            name = "WR";
            string link = "WR";
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndWardrobe") && lineNum < lines.Length)
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
                if (line.StartsWith("name:"))
                {
                    name = line.Substring(5).Trim();
                }
                if (line.StartsWith("locked:"))
                {
                    try { startLocked = bool.Parse(line.Substring(7)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("link:"))
                {
                    link = line.Substring(5).Trim();
                }
                if (line.StartsWith("keyName:"))
                {
                    keyName = line.Substring(8).Trim();
                }
                lineNum++;
            }
            locked = startLocked;
            if (startLocked)
            {
                Animation = "wardrobeclosed";
                state = WardrobeState.Closed;
            }
            else
            {
                Animation = "wardrobeopening";
                state = WardrobeState.Open;
            }
            startPos = new Vector2(x, y);
            positionPres = new Vector2(x, y);
            positionPast1 = new Vector2(x, y);
            positionPast2 = new Vector2(x, y);
            if (wardrobeDict.ContainsKey(name))
                wardrobeDict.Remove(name);
            wardrobeDict.Add(name, this);
            setLinkedWR(link);
        }

        public void reset()
        {
            positionPres = new Vector2(startPos.X, startPos.Y);
            positionPast1 = new Vector2(startPos.X, startPos.Y);
            positionPast2 = new Vector2(startPos.X, startPos.Y);
            locked = startLocked;

            if (startLocked)
            {
                Animation = "wardrobeclosed";
                state = WardrobeState.Closed;
            }
            else
            {
                Animation = "wardrobeopening";
                state = WardrobeState.Open;
            }
        }

        # endregion

        # region Methods

        //Collideable
        public Rectangle getBounds()
        {
            return pushBox;
        }
        public bool isSolid()
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

            if (keyName != "")
            {
                DoorKeys k = DoorKeys.getKey(keyName);
                if (k != null)
                {
                    if (k.pickedUp && state == WardrobeState.Closed)
                    {
                        state = WardrobeState.Opening;
                    }
                }
            }

            switch (state)
            {
                case WardrobeState.Open:
                    Animation = "wardrobeopen";
                    break;
                case WardrobeState.Opening:
                    frameLength = 100;
                    if (frame == 2)
                    {
                        Animation = "wardrobeopen";
                        state = WardrobeState.Open;
                        unlockObj();
                    }
                    else
                        Animation = "wardrobeopening";
                    break;
                case WardrobeState.Closed:
                    Animation = "wardrobeclosed";
                    break;
            }
            if (frameTime >= frameLength)
            {   
                frameTime = 0;
                frame = (frame + 1) % animFrames.Count;
            }
        }

        public void setLinkedWR(string linkedName)
        {
            this.linkedName = linkedName;
        }

        public Wardrobe getLinkedWR()
        {
            Wardrobe w;
            if (wardrobeDict.ContainsKey(linkedName))
                wardrobeDict.TryGetValue(linkedName, out w);
            else
                w = null;
            return w;
        }

        public void lockObj()
        {
            locked = true;
        }

        public void unlockObj()
        {
            locked = false;

            if (GameTitle.toggleSound)
            {
                soundMan.playSound("Wardrobe Unlock");
            }
        }

        public bool isLocked()
        {
            return locked;
        }

        public void Interact()
        {
            Boy player = control.player;
            if (Rectangle.Intersect(player.getBounds(), enterBox).Width != 0)
            {
                Wardrobe linkedWR = getLinkedWR();
                if (!locked && linkedWR != null && !linkedWR.isLocked() && !control.collidingWithSolid(linkedWR.enterBox))
                {
                    player.state = Boy.BoyState.Teleport;
                    player.X = X + 16;

                    if (GameTitle.toggleSound)
                    {
                        soundMan.playSound("Wardrobe Travel");
                    }
                }
            }
            else
            {
                if (control.collidingWithSolid(pushBox, false))
                    player.state = Boy.BoyState.PushingStill;
                else
                {
                    player.state = Boy.BoyState.PushWalk;
                }
                if (player.X > X)
                {
                    player.X = X + 67;
                    player.direction = Direction.Left;
                }
                else
                {
                    player.X = X - 36;
                    player.direction = Direction.Right;
                }
            }
        }

        public string saveData()
        {
            return "StartWardrobe\nx:" + X + "\ny:" + Y + "\nname:" + name + "\nlocked:" + startLocked + "\nlink:" + linkedName + "\nkeyName:" + keyName + "\nEndWardrobe";
        }

        # endregion


        public void setKeyName(string keyName)
        {
            this.keyName = keyName;
        }
    }
}
