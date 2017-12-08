using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    public class Chairs : Collideable, Updatable, Drawable, Interactive, Saveable
    {
        # region Attributes

        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        //Collidable
        private Vector2 startPos;
        private Vector2 positionPres;
        private Vector2 positionPast1;
        private Vector2 positionPast2;
        private int tX = 0, tY = 0;
        private int speed = 3;
        private int moveTime = 0;
        private int movelength = 70;
        private Rectangle bounds { get { return new Rectangle(X, Y, 40, 52); } }
        //Drawable
        private SpriteSheet sheet;
        public enum ChairsState { Idle, Falling, Moving }
        public ChairsState state;
        private ActionBubble bubble = new ActionBubble();

        # endregion

        # region Constructor

        public Chairs(int x, int y, int width, int height, int frameLength, bool startLocked)
        {
            this.sheet = sheetMan.getSheet("chair");
            positionPres = new Vector2(x, y);
            positionPast1 = new Vector2(x, y);
            positionPast2 = new Vector2(x, y);
            startPos = new Vector2(x, y);
            bubble.chair = this;
        }

        public Chairs(string saveString)
        {
            this.sheet = sheetMan.getSheet("chair");
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int x = 0;
            int y = 0;
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndChairs") && lineNum < lines.Length)
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
                lineNum++;
            }
            startPos = new Vector2(x, y);
            positionPres = new Vector2(x, y);
            positionPast1 = new Vector2(x, y);
            positionPast2 = new Vector2(x, y);
            bubble.chair = this;
        }

        # endregion

        # region Methods

        //Accessors & Mutators
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

        //Collideable
        public Rectangle getBounds()
        {
            return bounds;
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
            Vector2 drawPos = new Vector2(X, Y);
            if (control.timePeriod == TimePeriod.Present)
                renderer.Draw(sheet.image, drawPos, sheet.getSprite(1), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Chairs);
            else
                renderer.Draw(sheet.image, drawPos, sheet.getSprite(0), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Chairs);
            bubble.draw(renderer, tint);
        }

        //Updatable
        public void update(GameTime time)
        {
            Boy player = control.player;
            int elapsed = time.ElapsedGameTime.Milliseconds;
            switch (state)
            {
                case ChairsState.Idle:
                    if (player.nearestChair != null && player.nearestChair != this)
                    {
                        if (player.nearestChair.state == ChairsState.Idle)
                        {
                            Vector2 oldDist = new Vector2(player.X - player.nearestChair.X, player.Y - player.nearestChair.Y);
                            Vector2 newDist = new Vector2(player.X - X, player.Y - Y);
                            if (newDist.LengthSquared() < oldDist.LengthSquared())
                            {
                                player.nearestChair = this;
                                bubble.show();
                                bubble.setAction(ActionBubble.BubbleAction.Chair, false);
                            }
                            else
                                bubble.hide();
                        }
                    }
                    else
                    {
                        player.nearestChair = this;
                        bubble.show();
                        bubble.setAction(ActionBubble.BubbleAction.Chair, false);
                    }
                    break;
                case ChairsState.Falling:
                    bubble.hide();
                    Y++;
                    break;
                case ChairsState.Moving:
                    bubble.hide();
                    moveTime += elapsed;
                    if (moveTime >= movelength)
                    {
                        X += tX * speed;
                        Y += tY * speed;
                        moveTime = 0;

                        Rectangle smallerBound = new Rectangle(X + 2, Y + 2, bounds.Width - 4, bounds.Height - 4);
                        if (control.collidingWithSolid(smallerBound, false))
                        {
                            X -= tX * speed;
                            Y -= tY * speed;
                        }
                        tX = 0;
                        tY = 0;
                    }
                    break;
            }
        }

        public void move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    tY = -1;
                    break;
                case Direction.Down:
                    tY = 1;
                    break;
                case Direction.Left:
                    tX = -1;
                    break;
                case Direction.Right:
                    tX = 1;
                    break;
            }
        }

        public void drop()
        {
            state = ChairsState.Falling;
        }

        //Interactive
        public void Interact()
        {
        }

        public string saveData()
        {
            return "StartChairs\nx:" + X + "\ny:" + Y + "\nEndChairs";
        }

        //reset
        public void reset()
        {
            positionPres = new Vector2(startPos.X, startPos.Y);
            positionPast1 = new Vector2(startPos.X, startPos.Y);
            positionPast2 = new Vector2(startPos.X, startPos.Y);
        }

        # endregion
    }
}
