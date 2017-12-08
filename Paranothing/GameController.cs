using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing
{
    class GameController
    {
        private SoundManager soundMan = SoundManager.getInstance();
        public KeyboardState keyState;
        public GamePadState padState;

        private List<Updatable> updatableObjs;
        private List<Drawable> drawableObjs;
        private List<Collideable> collideableObjs;
        public Boy player;
        public GameState state;
        public TimePeriod timePeriod;
        public Level level;
        public Camera camera;

        private bool soundTriggered = false;
        private Vector2 soundPos;
        private bool showingDialogue = false;
        private string dialogue = "";
        private int dialogueTimer = 0;

        private Dictionary<string, Level> levels;

        private static GameController instance;

        public static GameController getInstance()
        {
            if (null == instance)
            {
                instance = new GameController();
            }
            return instance;
        }

        protected GameController()
        {
            updatableObjs = new List<Updatable>();
            drawableObjs = new List<Drawable>();
            collideableObjs = new List<Collideable>();
            levels = new Dictionary<string, Level>();
            state = GameState.Game;
            timePeriod = TimePeriod.Present;
        }

        public void addLevel(Level level)
        {
            levels.Add(level.name, level);
        }

        public void goToLevel(string levelName)
        {
            if (levels.ContainsKey(levelName))
            {
                levels.TryGetValue(levelName, out level);
            }
        }

        public bool nextLevel()
        {
            string nextLevel = level.nextLevel;
            if (levels.ContainsKey(nextLevel))
            {
                levels.TryGetValue(nextLevel, out level);
                return true;
            }
            return false;
        }

        public string currLevel()
        {
            return level.name;
        }

        public void setPlayer(Boy player)
        {
            this.player = player;
            addObject(player);
            addObject(player.actionBubble);
        }

        public void setCamera(Camera camera)
        {
            this.camera = camera;
            addObject(camera);
        }

        public void initLevel(bool preserveTime)
        {
            showingDialogue = false;
            updatableObjs = new List<Updatable>();
            drawableObjs = new List<Drawable>();
            collideableObjs = new List<Collideable>();
            player.reset();
            player.X = level.playerX;
            player.Y = level.playerY;
            addObject(player);
            addObject(player.actionBubble);
            addObject(camera);
            foreach (Saveable obj in level.getObjs())
            {
                obj.reset();
                addObject(obj);
            }
            if (!preserveTime)
                timePeriod = level.startTime;
        }

        public void resetLevel()
        {
            showingDialogue = false;
            player.X = level.playerX;
            player.Y = level.playerY;
            foreach (Saveable obj in level.getObjs())
            {
                obj.reset();
            }
            timePeriod = level.startTime;
        }

        public void updateObjs(GameTime time)
        {
            keyState = Keyboard.GetState();
            padState = GamePad.GetState(PlayerIndex.One);
            if (showingDialogue)
            {
                dialogueTimer += time.ElapsedGameTime.Milliseconds;
            }
            else
            {
                dialogueTimer = 0;
            }
            foreach (Updatable obj in updatableObjs)
            {
                obj.update(time);
            }
            player.actionBubble.hide();
            if (soundPos != null && soundPos.X != 0 && soundPos.Y != 0)
                soundTriggered = true;
            else soundTriggered = false;
            foreach (Collideable obj in collideableObjs)
            {
                Boy.BoyState currState = player.state;
                bool colliding = collides(((Collideable)obj).getBounds(), player.getBounds());
                if (obj is Shadows)
                {
                    Shadows shadow = (Shadows)obj;
                    if (soundTriggered && timePeriod == TimePeriod.Present)
                    {
                        if (soundPos.Y >= shadow.Y && soundPos.Y <= shadow.Y + 81)
                            shadow.stalkNoise((int)soundPos.X, (int)soundPos.Y);
                    }
                    if (colliding && timePeriod == TimePeriod.Present && player.state != Boy.BoyState.StairsLeft && player.state != Boy.BoyState.StairsRight)
                    {
                        if (shadow.X > player.X)
                            player.direction = Direction.Right;
                        else
                            player.direction = Direction.Left;
                        player.state = Boy.BoyState.Die;

                        if (GameTitle.toggleSound)
                        {
                            soundMan.playSound("Death");
                        }
                        shadow.state = Shadows.ShadowState.Idle;
                    }
                }
                else if (obj is Dialogue)
                {
                    if (colliding)
                    {
                        Dialogue d = (Dialogue)obj;
                        d.Play();
                    }
                }
                else if (obj is DoorKeys)
                {
                    if (colliding)
                    {
                        DoorKeys key = DoorKeys.getKey(((DoorKeys)obj).name);
                        if (!key.restrictTime || timePeriod == key.inTime)
                            key.pickedUp = true;
                    }
                }
                else if (obj is Button)
                {
                    Button button = (Button)obj;
                    bool pressed = false;
                    foreach (Collideable c in collideableObjs)
                    {
                        if ((c is Boy || (timePeriod == TimePeriod.Present && c is Shadows)) && collides(button.getBounds(), c.getBounds()))
                        {
                            pressed = true;
                            break;
                        }
                    }
                    if (!button.stepOn && pressed && GameTitle.toggleSound)
                        soundMan.playSound("Button Press");
                    button.stepOn = pressed;
                }
                else if (obj is Stairs)
                {
                    Stairs stair = (Stairs)obj;
                    if (stair.isSolid())
                    {
                        if (colliding)
                        {
                            if (player.X + 30 >= stair.X && player.X + 8 <= stair.X)
                            {
                                if (((stair.direction == Direction.Left && player.Y + 58 == stair.getSmallBounds().Y)
                                    || (stair.direction == Direction.Right && player.Y + 58 == stair.Y + stair.getBounds().Height))
                                    && (player.state == Boy.BoyState.Idle || player.state == Boy.BoyState.Walk))
                                {
                                    player.actionBubble.setAction(ActionBubble.BubbleAction.Stair, false);
                                    player.interactor = (Interactive)obj;
                                    player.actionBubble.show();
                                }
                            }
                            else if (player.X + 30 >= stair.X + stair.getBounds().Width && player.X + 8 <= stair.X + stair.getBounds().Width)
                            {
                                if (((stair.direction == Direction.Right && player.Y + 58 == stair.getSmallBounds().Y)
                                    || (stair.direction == Direction.Left && player.Y + 58 == stair.Y + stair.getBounds().Height))
                                    && (player.state == Boy.BoyState.Idle || player.state == Boy.BoyState.Walk))
                                {
                                    player.actionBubble.setAction(ActionBubble.BubbleAction.Stair, false);
                                    player.interactor = (Interactive)obj;
                                    player.actionBubble.show();
                                }
                            }
                            if (player.state == Boy.BoyState.StairsLeft || player.state == Boy.BoyState.StairsRight)
                            {
                                if (stair.direction == Direction.Left)
                                {
                                    if ((player.direction == Direction.Left && (int)player.Y + 58 == stair.getSmallBounds().Y)
                                        || (player.direction == Direction.Right && (int)player.Y + 58 == stair.Y + stair.getBounds().Height))
                                    {
                                        player.state = Boy.BoyState.Walk;
                                    }
                                }
                                if (stair.direction == Direction.Right)
                                {
                                    if ((player.direction == Direction.Right && (int)player.Y + 58 == stair.getSmallBounds().Y)
                                        || (player.direction == Direction.Left && (int)player.Y + 58 == stair.Y + stair.getBounds().Height))
                                    {
                                        player.state = Boy.BoyState.Walk;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (obj is Chairs)
                {
                    Chairs chair = (Chairs)obj;
                    if (chair.state == Chairs.ChairsState.Falling)
                    {
                        foreach (Collideable c in collideableObjs)
                        {
                            if (c is Floor)
                            {
                                if (collides(c.getBounds(), chair.getBounds()))
                                {
                                    while (collides(c.getBounds(), chair.getBounds()))
                                        chair.Y--;
                                    chair.state = Chairs.ChairsState.Idle;
                                    soundPos = new Vector2(chair.X, chair.Y);

                                    if (GameTitle.toggleSound)
                                    {
                                        soundMan.playSound("Chair Drop");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (obj is Wardrobe)
                {
                    Wardrobe wardrobe = (Wardrobe)obj;
                    if (wardrobe.state == Wardrobe.WardrobeState.Opening)
                        soundPos = new Vector2(wardrobe.X + wardrobe.getBounds().Width / 2, wardrobe.Y + wardrobe.getBounds().Height / 2);
                    if (colliding && player.X + (player.direction == Direction.Left ? 8 : 32) > wardrobe.X)
                    {
                        bool negated = false;
                        if (collides(wardrobe.enterBox, player.getBounds()))
                        {
                            Wardrobe linkedWR = wardrobe.getLinkedWR();
                            if (wardrobe.isLocked() || linkedWR == null
                                || linkedWR.isLocked() || collidingWithSolid(linkedWR.enterBox))
                                negated = true;
                            if (player.state == Boy.BoyState.Idle || player.state == Boy.BoyState.Walk)
                            {
                                player.actionBubble.setAction(ActionBubble.BubbleAction.Wardrobe, negated);
                                player.actionBubble.show();
                                if (!negated)
                                    player.interactor = (Interactive)obj;
                            }
                        }
                        else
                        {
                            negated = collidingWithSolid(wardrobe.getBounds(), false);
                            if (player.state == Boy.BoyState.Idle || player.state == Boy.BoyState.Walk)
                            {
                                player.actionBubble.setAction(ActionBubble.BubbleAction.Push, negated);
                                player.actionBubble.show();
                                if (!negated)
                                    player.interactor = (Interactive)obj;
                            }
                        }
                    }
                }
                else if (obj is Bookcases)
                {
                    Bookcases bookcase = (Bookcases)obj;
                    if (colliding && bookcase.state == Bookcases.BookcasesState.Open)
                    {
                        player.actionBubble.setAction(ActionBubble.BubbleAction.Bookcase, false);
                        player.actionBubble.show();
                        player.interactor = (Interactive)obj;
                    }
                }
                else if (obj is Portrait)
                {
                    Portrait painting = (Portrait)obj;
                    if (!painting.wasMoved || painting.inTime == timePeriod)
                    {
                        if (colliding && player.X + player.Width - 10 > painting.X && (player.state == Boy.BoyState.Idle || player.state == Boy.BoyState.Walk))
                        {
                            bool negated = false;
                            if (painting.sendTime == TimePeriod.FarPast)
                                player.actionBubble.setAction(ActionBubble.BubbleAction.OldPortrait, negated);
                            else
                                player.actionBubble.setAction(ActionBubble.BubbleAction.Portrait, negated);
                            player.actionBubble.show();
                            player.interactor = (Interactive)obj;
                        }
                    }
                }
                else if (obj is Floor)
                {
                    if (player.state != Boy.BoyState.StairsLeft && player.state != Boy.BoyState.StairsRight)
                    {
                        Floor floor = (Floor)obj;
                        while (collides(player.getBounds(), floor.getBounds()))
                        {
                            player.Y--;
                        }
                    }
                }
                else if (obj is Doors)
                {
                    Doors door = (Doors)obj;
                    if (door.isLocked())
                    {
                        if (colliding && player.state == Boy.BoyState.Walk)
                        {
                            if ((player.direction == Direction.Left && player.X > door.getBounds().X)
                                || (player.direction == Direction.Right && player.X < door.getBounds().X))
                                player.state = Boy.BoyState.PushingStill;
                        }
                    }
                }
                else
                {
                    if (!player.actionBubble.isVisible() && !(player.interactor is Wardrobe && (player.state == Boy.BoyState.PushingStill || player.state == Boy.BoyState.PushWalk)))
                        player.interactor = null;
                    Collideable collider = (Collideable)obj;
                    if (colliding && player.state == Boy.BoyState.Walk && collider.isSolid())
                    {
                        if ((player.direction == Direction.Left && player.X > collider.getBounds().X)
                            || (player.direction == Direction.Right && player.X < collider.getBounds().X))
                            player.state = Boy.BoyState.PushingStill;
                    }
                }
            }
            if (soundTriggered)
                soundPos = new Vector2();
            if (!collides(player.getBounds(), new Rectangle(0, 0, level.Width, level.Height)))
            {
                if (nextLevel())
                {
                    GameTitle.levelName = level.name;
                    initLevel(true);
                }
                else
                {
                    //TODO: CHANGE THIS STUFF
                    goToLevel("Level1");
                    initLevel(false);
                    state = GameState.MainMenu;
                }
            }
        }

        public void drawObjs(SpriteBatch renderer)
        {

            if (showingDialogue)
            {
                Vector2 textDim = Game1.GameFont.MeasureString(dialogue);
                Vector2 drawPos = new Vector2();
                drawPos.X = camera.X + (camera.Width / 2) / camera.scale - textDim.X / 2;
                drawPos.Y = camera.Y + camera.Height / camera.scale - textDim.Y - 10;
                renderer.DrawString(Game1.GameFont, dialogue, drawPos, Color.White);
            }
            Color tint = Color.White;
            if (timePeriod == TimePeriod.Past)
            {
                tint = Color.White;
                tint.A = 32;
            }
            else if (timePeriod == TimePeriod.FarPast)
            {
                tint = Color.White;
                tint.A = 4;
            }
            foreach (Drawable obj in drawableObjs)
            {
                obj.draw(renderer, tint);
            }

            player.draw(renderer, tint);

        }

        public bool collides(Rectangle box1, Rectangle box2)
        {
            Rectangle i = Rectangle.Intersect(box1, box2);
                return i.Width != 0;
        }

        public void addObject(Object obj)
        {
            if (obj is Drawable)
            {
                drawableObjs.Add((Drawable)obj);
            }
            if (obj is Updatable)
            {
                updatableObjs.Add((Updatable)obj);
            }
            if (obj is Collideable)
            {
                collideableObjs.Add((Collideable)obj);
            }

        }

        public bool collidingWithSolid(Rectangle box)
        {
            return collidingWithSolid(box, true);
        }

        public bool collidingWithSolid(Rectangle box, bool includePlayer)
        {
            foreach (Collideable col in collideableObjs)
            {
                if (!includePlayer && col is Boy)
                    continue;
                if (col is Stairs)
                    continue;
                if (col.isSolid() && collides(box, col.getBounds()))
                {
                    return true;
                }
            }
            return false;
        }

        public void showDialogue(string text)
        {
            showingDialogue = true;
            dialogue = text;
        }

        public void hideDialogue()
        {
            if (dialogueTimer >= 3000)
                showingDialogue = false;
        }
    }
}