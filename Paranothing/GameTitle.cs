using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing
{
    class GameTitle : GameBackground
    {
        # region Attribute

        private Rectangle topTextRect;
        private Rectangle bottomTextRect;

        private int menuIndex;
        public int menuSize = 5;
        private GamePadState prevPad;
        private KeyboardState prevKeys;
        public static bool toggleSound = true;
        private bool toggleMusic = true;
        private String soundText = "ON";
        private String musicText = "ON";

        private Color[] colors = new Color[6]{Color.Yellow, Color.White, Color.White, Color.White, Color.White, Color.White};

        private Vector2 choice1 = new Vector2(750, 300);
        private Vector2 choice2 = new Vector2(750, 360);
        private Vector2 choice3 = new Vector2(750, 420);
        private Vector2 choice4 = new Vector2(750, 480);
        private Vector2 choice5 = new Vector2(750, 540);
        private Vector2 choice6 = new Vector2(750, 600);

        private GameController control = GameController.getInstance();

        public static String levelName;

        public enum TitleState
        {
            Title,
            Menu,
            Options,
            Controls,
            Credits,
            Pause,
            Select
        }

        public TitleState titleState = TitleState.Title;

        # endregion

        # region Constructor

        public GameTitle(Texture2D inTexture, Rectangle inRect)
            : base(inTexture, inRect)
        {
            topTextRect = new Rectangle();
            bottomTextRect = new Rectangle();
        }

        # endregion

        # region Methods

        //Accessors
        public Rectangle TopTextRectangle
        {
            get
            {
                return topTextRect;
            }
        }
        public void setTopTextRectangle(Vector2 inVec)
        {
            topTextRect.X = BackgoundRectangle.Center.X - (int)(inVec.X / 2);
            topTextRect.Y = BackgoundRectangle.Top;
            topTextRect.Width = (int)inVec.X;
            topTextRect.Height = (int)inVec.Y;
        }
        public Rectangle BottomTextRectangle
        {
            get
            {
                return bottomTextRect;
            }
        }
        public void setBottomTextRectangle(Vector2 inVec)
        {
            bottomTextRect.X = BackgoundRectangle.Center.X - (int)(inVec.X / 2);
            bottomTextRect.Y = BackgoundRectangle.Bottom - (int)inVec.Y;
            bottomTextRect.Width = (int)inVec.X;
            bottomTextRect.Height = (int)inVec.Y;
        }

        //Update
        public override void Update(Game1 game, KeyboardState keys)
        {

            GamePadState padState = GamePad.GetState(PlayerIndex.One);

            //Press enter to start
            if ((keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.Start)) && titleState == TitleState.Title)
            {
                //game.GameState = GameState.Game;
                titleState = TitleState.Menu;

            }

            //Selecting menu options
            else if ((padState.IsButtonDown(Buttons.LeftThumbstickDown) || keys.IsKeyDown(Keys.Down)) && !(prevPad.IsButtonDown(Buttons.LeftThumbstickDown) || prevKeys.IsKeyDown(Keys.Down)) && menuIndex < menuSize - 1)
            {  

                colors[menuIndex] = Color.White;

                menuIndex++;

                colors[menuIndex] = Color.Yellow;

            }

            else if ((padState.IsButtonDown(Buttons.LeftThumbstickUp) || keys.IsKeyDown(Keys.Up)) && !(prevPad.IsButtonDown(Buttons.LeftThumbstickUp) || prevKeys.IsKeyDown(Keys.Up)) && menuIndex > 0)
            {

                colors[menuIndex] = Color.White;

                menuIndex--;

                colors[menuIndex] = Color.Yellow;

            }

            else if (titleState == TitleState.Menu && (keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) && !(prevKeys.IsKeyDown(Keys.Enter) || prevPad.IsButtonDown(Buttons.A)))
            {
                
                if (menuIndex == 0)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;

                    control.goToLevel("Tutorial");
                    control.initLevel(false);
                    levelName = "Tutorial";

                    game.GameState = GameState.Game;
                }
                else if (menuIndex == 1)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;
                    menuSize = 6;
                    titleState = TitleState.Select;
                }

                else if (menuIndex == 2) 
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;
                    menuSize = 3;
                    titleState = TitleState.Options;
                }
                else if (menuIndex == 3)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 1;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Controls;
                }
                else if (menuIndex == 4)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 1;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Credits;
                }

            }

            else if (titleState == TitleState.Select && (keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) && !(prevKeys.IsKeyDown(Keys.Enter) || prevPad.IsButtonDown(Buttons.A)))
            {

                if (menuIndex == 0)
                {
                    control.goToLevel("Tutorial");
                    control.initLevel(false);
                    game.GameState = GameState.Game;

                }

                else if (menuIndex == 1)
                {
                    control.goToLevel("Level1");
                    control.initLevel(false);
                    game.GameState = GameState.Game;

                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;

                }

                else if (menuIndex == 2)
                {
                    control.goToLevel("Level2");
                    control.initLevel(false);
                    game.GameState = GameState.Game;
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;
                }

                else if (menuIndex == 3)
                {
                    control.goToLevel("Level3");
                    control.initLevel(false);
                    game.GameState = GameState.Game;
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;
                }

                else if (menuIndex == 4)
                {
                    control.goToLevel("Level4");
                    control.initLevel(false);
                    game.GameState = GameState.Game;
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    colors[menuIndex] = Color.Yellow;
                }

                else if (menuIndex == 5)
                {
                    
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 5;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Menu;

                }

            }
            else if (titleState == TitleState.Pause && (keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) && !(prevKeys.IsKeyDown(Keys.Enter) || prevPad.IsButtonDown(Buttons.A)))
            {

                if (menuIndex == 0)
                    game.GameState = GameState.Game;

                else if (menuIndex == 1)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 5;
                    colors[menuIndex] = Color.Yellow;
                    game.ResetGame();
                    titleState = TitleState.Menu;
                }

                else if (menuIndex == 2)
                {

                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 3;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Options;

                }

                else if (menuIndex == 3)
                {

                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 1;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Controls;

                }


            }

            else if (titleState == TitleState.Options && (keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) && !(prevKeys.IsKeyDown(Keys.Enter) || prevPad.IsButtonDown(Buttons.A)))
            {

                if (menuIndex == 0 && toggleSound)
                {
                    soundText = "OFF";
                    toggleSound = false;
                }

                else if (menuIndex == 0 && !toggleSound)
                {
                    soundText = "ON";
                    toggleSound = true;
                }

                else if (menuIndex == 1 && toggleMusic)
                {
                    musicText = "OFF";
                    toggleMusic = false;
                    MediaPlayer.Pause();
                }

                else if (menuIndex == 1 && !toggleMusic)
                {
                    musicText = "ON";
                    toggleMusic = true;
                    MediaPlayer.Resume();
                }

                else if (menuIndex == 2 && !game.GameInProgress)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 5;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Menu;
                }

                else if (menuIndex == 2 && game.GameInProgress)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 4;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Pause;
                }

            }

            else if (titleState == TitleState.Controls && (keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) && !(prevKeys.IsKeyDown(Keys.Enter) || prevPad.IsButtonDown(Buttons.A)))
            {

                if (menuIndex == 0 && !game.GameInProgress)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 5;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Menu;
                }

                else if (menuIndex == 0 && game.GameInProgress)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 4;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Pause;
                }

            }

            else if (titleState == TitleState.Credits && (keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) && !(prevKeys.IsKeyDown(Keys.Enter) || prevPad.IsButtonDown(Buttons.A)))
            {

                if (menuIndex == 0 && !game.GameInProgress)
                {
                    colors[menuIndex] = Color.White;
                    menuIndex = 0;
                    menuSize = 5;
                    colors[menuIndex] = Color.Yellow;
                    titleState = TitleState.Menu;
                }

            }


            prevKeys = keys;
            prevPad = padState;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (titleState == TitleState.Menu)
            {
                spriteBatch.DrawString(Game1.MenuFont, "New Game", choice1, colors[0]);
                spriteBatch.DrawString(Game1.MenuFont, "Select Level", choice2, colors[1]);
                spriteBatch.DrawString(Game1.MenuFont, "Options", choice3, colors[2]);
                spriteBatch.DrawString(Game1.MenuFont, "Controls", choice4, colors[3]);
                spriteBatch.DrawString(Game1.MenuFont, "Credits", choice5, colors[4]);
            }

            if (titleState == TitleState.Select)
            {
                spriteBatch.DrawString(Game1.MenuFont, "Tutorial", choice1, colors[0]);
                spriteBatch.DrawString(Game1.MenuFont, "Level 1", choice2, colors[1]);
                spriteBatch.DrawString(Game1.MenuFont, "Level 2", choice3, colors[2]);
                spriteBatch.DrawString(Game1.MenuFont, "Level 3", choice4, colors[3]);
                spriteBatch.DrawString(Game1.MenuFont, "Level 4", choice5, colors[4]);
                spriteBatch.DrawString(Game1.MenuFont, "Back", choice6, colors[5]);
            }

            if (titleState == TitleState.Pause)
            {
                spriteBatch.DrawString(Game1.MenuFont, "Resume Game", choice1, colors[0]);
                spriteBatch.DrawString(Game1.MenuFont, "Main Menu", choice2, colors[1]);
                spriteBatch.DrawString(Game1.MenuFont, "Options", choice3, colors[2]);
                spriteBatch.DrawString(Game1.MenuFont, "Controls", choice4, colors[3]);

            }

            if (titleState == TitleState.Options)
            {
            
                //TODO: FIX OPTIONS
                
                spriteBatch.DrawString(Game1.MenuFont, "Toggle Sound: " + soundText, choice1, colors[0]);
                spriteBatch.DrawString(Game1.MenuFont, "Toggle Music: " + musicText, choice2, colors[1]);
                spriteBatch.DrawString(Game1.MenuFont, "Back", choice3, colors[2]);

            }

            if (titleState == TitleState.Controls)
            {

                spriteBatch.DrawString(Game1.MenuFont, "Back", choice6, colors[0]);
                //TODO: ADD CONTROLS

            }

            if (titleState == TitleState.Credits)
            {

                spriteBatch.DrawString(Game1.MenuFont, "Samuel Assaf", new Vector2(180, 200), Color.White);
                spriteBatch.DrawString(Game1.MenuFont, "Alex Attar", new Vector2(180, 260), Color.White);
                spriteBatch.DrawString(Game1.MenuFont, "David Campbell", new Vector2(180, 320), Color.White);
                spriteBatch.DrawString(Game1.MenuFont, "Ralph D'Almeida", new Vector2(180, 380), Color.White);

                spriteBatch.DrawString(Game1.MenuFont, "Back", choice6, colors[0]);
                //TODO: ADD CREDITS

            }


        }

        # endregion
    }
}
