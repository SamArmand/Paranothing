using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing
{
    internal enum GameState { MainMenu, Game }

    public enum Direction { Left, Right, Up, Down }
    public enum TimePeriod { FarPast, Past, Present }

    internal struct DrawLayer {
        internal const float ActionBubble = 0.01f;
        public const float Chairs = 0.015f;
        public const float Player = 0.02f;
        public const float Rubble = 0.03f;
        public const float Key = 0.035f;
        public const float Wardrobe = 0.04f;
        public const float Floor = 0.05f;
        public const float Stairs = 0.06f;
        public const float PlayerBehindStairs = 0.07f;
        public const float Background = 0.08f;
        public const float WallpaperTears = 0.09f;
        public const float Wallpaper = 0.10f;
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    internal sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteBatch _spriteBatch2;

        # region Attributes
        //Effect greyScale;

        private Song _bgMusic;

        private Texture2D _boyTex;
        private SpriteSheet _boySheet;

        private Texture2D _shadowTex;
        private SpriteSheet _shadowSheet;

        private Texture2D _actionTex;
        private SpriteSheet _actionSheet;

        private Texture2D _floorTex;
        private SpriteSheet _floorSheet;

        private Texture2D _wallTex;
        private SpriteSheet _wallSheet;

        private Texture2D _wallpaperTex;
        private SpriteSheet _wallpaperSheet;

        private Texture2D _wardrobeTex;
        private SpriteSheet _wardrobeSheet;

        //Portrait
        private Texture2D _portraitTex;
        private SpriteSheet _portraitSheet;

        private Texture2D _rubbleTex;
        private SpriteSheet _rubbleSheet;

        private Texture2D _stairTex;
        private SpriteSheet _stairSheet;

        private Texture2D _doorTex;
        private SpriteSheet _doorSheet;

        private Texture2D _oldPortraitTex;
        private SpriteSheet _oldPortraitSheet;

        private Texture2D _keyTex;
        private SpriteSheet _keySheet;

        private Texture2D _chairTex;
        private SpriteSheet _chairSheet;

        private Texture2D _finalDoorTex;
        private SpriteSheet _finalDoorSheet;

        private Texture2D _buttonTex;
        private SpriteSheet _buttonSheet;

        private Texture2D _controller;

        private readonly GameController _control = GameController.getInstance();
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.getInstance();
        private readonly SoundManager _soundMan = SoundManager.getInstance();

        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        private Boy _player;
        private ActionBubble _actionBubble;

        //Fonts
        public static SpriteFont GameFont;
        private static SpriteFont _titleFont;
        public static SpriteFont MenuFont;
        //Title
        private GameTitle _title;
        private Vector2 _startPosition;

        private Level _tutorial, _level1, _level2, _level3, _level4;

        public bool GameInProgress;
        public static bool EndGame;
        private float _fadeOpacity;
        private readonly float _opacityPerSecond = 0.02f;
        private Stopwatch _stopwatch;
        private Texture2D _white;

        # endregion

        # region Methods

        public GameState GameState
        {
            set => _control.state = value;
        }

        /// <summary>
        /// Draws text on the screen
        /// </summary>
        /// <param name="text">text to write</param>
        /// <param name="font">font of text</param>
        /// <param name="textColor">color of text</param>
        /// <param name="x">left hand edge of text</param>
        /// <param name="y">top of text</param>
        private void DrawText(string text, SpriteFont font, Color textColor, float x, float y)
        {
            int layer;
            var vectorText = new Vector2(x, y);

            //solid
            for (layer = 0; layer < 3; layer++)
            {
                _spriteBatch.DrawString(font, text, vectorText, new Color(190, 190, 190));
                vectorText.X++;
                vectorText.Y++;
            }

            //top of character
            _spriteBatch.DrawString(font, text, vectorText, textColor);
        }

        //Title
        private void LoadTitleContents()
        {
            _titleFont = Content.Load<SpriteFont>("TitleFont");
            GameFont = Content.Load<SpriteFont>("GameFont");
            MenuFont = Content.Load<SpriteFont>("GameFont");
            _title = new GameTitle(Content.Load<Texture2D>("screenshot_for_menu"), new Rectangle(0, 0, ScreenWidth, ScreenHeight));
            _title.setBottomTextRectangle(GameFont.MeasureString("Press Start"));
            _startPosition = new Vector2(_title.BottomTextRectangle.X, _title.BottomTextRectangle.Y);
        }
        private void DrawTitleText()
        {
            _title.setTopTextRectangle(_titleFont.MeasureString("Paranothing"));
            DrawText("Paranothing", _titleFont, Color.WhiteSmoke, _title.TopTextRectangle.X, _title.TopTextRectangle.Y);
            if (_title.titleState == GameTitle.TitleState.Title)
                _spriteBatch.DrawString(GameFont, "Press Start", _startPosition, Color.White);
        }

        # endregion

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = ScreenWidth,
                PreferredBackBufferHeight = ScreenHeight
            };
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _control.state = GameState.MainMenu;

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _spriteBatch2 = new SpriteBatch(GraphicsDevice);

            //Stuff for fade
            _stopwatch = new Stopwatch();
            _white = Content.Load<Texture2D>("white");

            _soundMan.SoundEffects.Add("Button Press", Content.Load<SoundEffect>("Sounds/Button Press"));
            _soundMan.SoundEffects.Add("Chair Drop", Content.Load<SoundEffect>("Sounds/Chair Drop"));
            _soundMan.SoundEffects.Add("Death", Content.Load<SoundEffect>("Sounds/Death"));
            _soundMan.SoundEffects.Add("Door Unlock", Content.Load<SoundEffect>("Sounds/Door Unlock"));
            _soundMan.SoundEffects.Add("Final Door Part 1", Content.Load<SoundEffect>("Sounds/Final Door Part 1"));
            _soundMan.SoundEffects.Add("Final Door Part 2", Content.Load<SoundEffect>("Sounds/Final Door Part 2"));
            _soundMan.SoundEffects.Add("Key", Content.Load<SoundEffect>("Sounds/Key"));
            _soundMan.SoundEffects.Add("Portrait Travel", Content.Load<SoundEffect>("Sounds/Portrait Travel"));
            _soundMan.SoundEffects.Add("Pushing Wardrobe", Content.Load<SoundEffect>("Sounds/Pushing Wardrobe"));
            _soundMan.SoundEffects.Add("Shadow", Content.Load<SoundEffect>("Sounds/Shadow"));
            _soundMan.SoundEffects.Add("Step 1", Content.Load<SoundEffect>("Sounds/Step 1"));
            _soundMan.SoundEffects.Add("Step 2", Content.Load<SoundEffect>("Sounds/Step 2"));
            _soundMan.SoundEffects.Add("Step 3", Content.Load<SoundEffect>("Sounds/Step 3"));
            _soundMan.SoundEffects.Add("Step 4", Content.Load<SoundEffect>("Sounds/Step 4"));
            _soundMan.SoundEffects.Add("Step 5", Content.Load<SoundEffect>("Sounds/Step 5"));
            _soundMan.SoundEffects.Add("Step 6", Content.Load<SoundEffect>("Sounds/Step 6"));
            _soundMan.SoundEffects.Add("Wardrobe Travel", Content.Load<SoundEffect>("Sounds/Wardrobe Travel"));
            _soundMan.SoundEffects.Add("Wardrobe Unlock", Content.Load<SoundEffect>("Sounds/Wardrobe Unlock"));

            _bgMusic = Content.Load<Song>("Sounds/Soundtrack");
            MediaPlayer.Play(_bgMusic);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.1f;

            //greyScale = Content.Load<Effect>("Greyscale");

            _wallpaperTex = Content.Load<Texture2D>("Sprites/Wallpaper");
            _wallpaperSheet = new SpriteSheet(_wallpaperTex);
            _wallpaperSheet.splitSheet(1, 2);

            _wardrobeTex = Content.Load<Texture2D>("Sprites/wardrobe");
            _wardrobeSheet = new SpriteSheet(_wardrobeTex);
            _wardrobeSheet.splitSheet(1, 5);
            _wardrobeSheet.addAnimation("wardrobeclosed", new[] { 0 });
            _wardrobeSheet.addAnimation("wardrobeopening", new[] { 1, 2, 3 });
            _wardrobeSheet.addAnimation("wardrobeopen", new[] { 4 });

            //Portrait
            _portraitTex = Content.Load<Texture2D>("Sprites/portrait");
            _portraitSheet = new SpriteSheet(_portraitTex);
            _portraitSheet.splitSheet(2, 1);

            _rubbleTex = Content.Load<Texture2D>("Sprites/rubble");
            _rubbleSheet = new SpriteSheet(_rubbleTex);
            _rubbleSheet.addSprite(0, 0, 37, 28);

            _actionTex = Content.Load<Texture2D>("Sprites/actions");
            _actionSheet = new SpriteSheet(_actionTex);
            _actionSheet.splitSheet(3, 3);
            _actionSheet.addAnimation("bubble", new[] { 0 });
            _actionSheet.addAnimation("wardrobe", new[] { 1 });
            _actionSheet.addAnimation("push", new[] { 2 });
            _actionSheet.addAnimation("chair", new[] { 3 });
            _actionSheet.addAnimation("stair", new[] { 4 });
            _actionSheet.addAnimation("portrait", new[] { 5 });
            _actionSheet.addAnimation("oldportrait", new[] { 6 });
            _actionSheet.addAnimation("bookcase", new[] { 7 });
            _actionSheet.addAnimation("negate", new[] { 8 });

            _boyTex = Content.Load<Texture2D>("Sprites/BruceSheet");
            _boySheet = new SpriteSheet(_boyTex);
            _boySheet.splitSheet(7, 9, 0, 0, 58);
            _boySheet.addAnimation("walk", new[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            _boySheet.addAnimation("stand", new[] { 8 });
            _boySheet.addAnimation("leavewardrobe", new[] { 9, 10, 11, 12, 13, 14, 15, 16 });
            _boySheet.addAnimation("enterwardrobe", new[] { 18, 19, 20, 21, 22, 23, 24, 25 });
            _boySheet.addAnimation("enterportrait", new[] { 27, 28, 29, 30, 31, 32, 33, 34 });
            _boySheet.addAnimation("leaveportrait", new[] { 34, 33, 32, 31, 30, 29, 28, 27 });
            _boySheet.addAnimation("startpush", new[] { 36, 37, 38, 39 });
            _boySheet.addAnimation("endpush", new[] { 39, 38, 37, 36 });
            _boySheet.addAnimation("push", new[] { 41, 42, 43, 44, 45, 46, 47, 48 });
            _boySheet.addAnimation("pushstill", new[] { 49 });
            _boySheet.addAnimation("controlstart", new[] { 50, 51, 52 });
            _boySheet.addAnimation("control", new[] { 53 });
            _boySheet.addAnimation("controlend", new[] { 52, 51, 50 });
            _boySheet.addAnimation("disappear", new[] { 50, 51, 52, 53, 54, 55, 56, 57 });

            _shadowTex = Content.Load<Texture2D>("Sprites/Shadow");
            _shadowSheet = new SpriteSheet(_shadowTex);
            _shadowSheet.splitSheet(1, 4);
            _shadowSheet.addAnimation("walk", new[] { 0, 1, 2 });
            _shadowSheet.addAnimation("stopwalk", new[] { 2, 1, 0 });
            _shadowSheet.addAnimation("stand", new[] { 3 });

            _floorTex = Content.Load<Texture2D>("Sprites/floor");
            _floorSheet = new SpriteSheet(_floorTex);
            _floorSheet.splitSheet(2, 1);

            _wallTex = Content.Load<Texture2D>("Sprites/wall");
            _wallSheet = new SpriteSheet(_wallTex);
            _wallSheet.splitSheet(1, 2);

            _stairTex = Content.Load<Texture2D>("Sprites/Staircase");
            _stairSheet = new SpriteSheet(_stairTex);
            _stairSheet.splitSheet(1, 2);

            _doorTex = Content.Load<Texture2D>("Sprites/door");
            _doorSheet = new SpriteSheet(_doorTex);
            _doorSheet.splitSheet(2, 3);
            _doorSheet.addAnimation("doorclosedpast", new[] { 0 });
            _doorSheet.addAnimation("dooropeningpast", new[] { 1 });
            _doorSheet.addAnimation("dooropenpast", new[] { 2 });
            _doorSheet.addAnimation("doorclosedpresent", new[] { 3 });
            _doorSheet.addAnimation("dooropeningpresent", new[] { 4 });
            _doorSheet.addAnimation("dooropenpresent", new[] { 5 });

            //Old Portrait
            _oldPortraitTex = Content.Load<Texture2D>("Sprites/PortraitWoman");
            _oldPortraitSheet = new SpriteSheet(_oldPortraitTex);
            _oldPortraitSheet.splitSheet(2, 1);

            _keyTex = Content.Load<Texture2D>("Sprites/Key");
            _keySheet = new SpriteSheet(_keyTex);
            _keySheet.splitSheet(2, 1);

            _chairTex = Content.Load<Texture2D>("Sprites/chair");
            _chairSheet = new SpriteSheet(_chairTex);
            _chairSheet.splitSheet(1, 2);

            _finalDoorTex = Content.Load<Texture2D>("Sprites/door_final");
            _finalDoorSheet = new SpriteSheet(_finalDoorTex);
            _finalDoorSheet.splitSheet(1, 7);
            _finalDoorSheet.addAnimation("bookcaseclosed", new[] { 0 });
            _finalDoorSheet.addAnimation("bookcaseopening", new[] { 1, 2, 3, 4, 5 });
            _finalDoorSheet.addAnimation("bookcaseclosing", new[] { 5, 4, 3, 2, 1 });
            _finalDoorSheet.addAnimation("bookcaseopen", new[] { 6 });

            _buttonTex = Content.Load<Texture2D>("Sprites/button");
            _buttonSheet = new SpriteSheet(_buttonTex);
            _buttonSheet.splitSheet(1, 2);

            _sheetMan.addSheet("wallpaper", _wallpaperSheet);
            _sheetMan.addSheet("wardrobe", _wardrobeSheet);
            _sheetMan.addSheet("portrait", _portraitSheet);
            _sheetMan.addSheet("rubble", _rubbleSheet);
            _sheetMan.addSheet("action", _actionSheet);
            _sheetMan.addSheet("boy", _boySheet);
            _sheetMan.addSheet("floor", _floorSheet);
            _sheetMan.addSheet("wall", _wallSheet);
            _sheetMan.addSheet("stair", _stairSheet);
            _sheetMan.addSheet("door", _doorSheet);
            _sheetMan.addSheet("oldportrait", _oldPortraitSheet);
            _sheetMan.addSheet("key", _keySheet);
            _sheetMan.addSheet("chair", _chairSheet);
            _sheetMan.addSheet("bookcase", _finalDoorSheet);
            _sheetMan.addSheet("button", _buttonSheet);
            _sheetMan.addSheet("shadow", _shadowSheet);

            _actionBubble = new ActionBubble();
            _player = new Boy(254, 240, _actionBubble);
            var camera = new Camera(0, 360, 1280, 720, 2.0f);
            _tutorial = new Level("levels/tutorial.lvl");
            _level1 = new Level("levels/level1.lvl");
            _level2 = new Level("levels/level2.lvl");
            _level3 = new Level("levels/level3.lvl");
            _level4 = new Level("levels/level4.lvl");
            _control.addLevel(_tutorial);
            _control.addLevel(_level1);
            _control.addLevel(_level2);
            _control.addLevel(_level3);
            _control.addLevel(_level4);
            _control.goToLevel("Tutorial");

            GameTitle.levelName = "Tutorial";

            _control.setPlayer(_player);
            _control.setCamera(camera);
            _control.initLevel(false);

            _controller = Content.Load<Texture2D>("controller");

            // TODO: use this.Content to load your game content here
            LoadTitleContents();
        }

        public void ResetGame()
        {
            GameInProgress = false;

            _actionBubble = new ActionBubble();
            _player = new Boy(254, 240, _actionBubble);

            var camera = new Camera(0, 360, 1280, 720, 2.0f);

            _control.setPlayer(_player);
            _control.setCamera(camera);
            _control.initLevel(false);

            _fadeOpacity = 0;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            _graphics.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((Keyboard.GetState().IsKeyDown(Keys.Pause) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start)) && _control.state == GameState.Game)
            {
                _control.state = GameState.MainMenu;
                _title.titleState = GameTitle.TitleState.Pause;
                _title.menuSize = 4;
            }

            if (EndGame)
            {

                if (!_stopwatch.IsRunning)
                    _stopwatch.Start();

                if (_fadeOpacity < 1)
                    _fadeOpacity = _stopwatch.ElapsedMilliseconds/100f * _opacityPerSecond;

                if (Math.Abs(_fadeOpacity - 1) <= 0 && _stopwatch.ElapsedMilliseconds >= 15000)
                {
                    EndGame = false;
                    _stopwatch.Reset();
                    _control.state = GameState.MainMenu;
                    _title.titleState = GameTitle.TitleState.Menu;
                    _title.menuSize = 5;
                    GameInProgress = false;
                }
            }

            //FADE OUT UPDATE
            else
            {
                switch (_control.state)
                {
                    case GameState.MainMenu:
                        EndGame = false;
                        _stopwatch.Reset();
                        _title.Update(this, Keyboard.GetState());
                        break;
                    case GameState.Game:

                        GameInProgress = true;

                        _control.updateObjs(gameTime);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20,20,20));

            switch (_control.state)
            {
                case GameState.MainMenu:
                    _spriteBatch.Begin();
                    _title.Draw(_spriteBatch);
                    DrawTitleText();

                    if (_title.titleState == GameTitle.TitleState.Controls)
                        _spriteBatch.Draw(_controller, new Rectangle(200, 180, 500, 500), Color.White);
                    break;
                case GameState.Game:
                    Effect pastEffect = null;
                    //if (control.timePeriod != TimePeriod.Present)
                    //    pastEffect = greyScale;
                    var transform = Matrix.Identity;
                    transform *= Matrix.CreateTranslation(-_control.camera.X, -_control.camera.Y, 0);
                    transform *= Matrix.CreateScale(_control.camera.scale);
                    _spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointWrap, null, null, pastEffect, transform);
                    DrawWallpaper(_spriteBatch, _wallpaperSheet);
                    _control.drawObjs(_spriteBatch);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _spriteBatch.End();

            _spriteBatch2.Begin();


            if (EndGame)
            {
                _spriteBatch2.Draw(_white, new Vector2(0, 0), null, Color.White * _fadeOpacity, 0f, Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight), SpriteEffects.None, 0f);

                if (_fadeOpacity >= 1)
                    _spriteBatch2.DrawString(MenuFont, "Bruce... you're safe now.", new Vector2(280, 300), Color.Black, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);

            }

            _spriteBatch2.End();

            base.Draw(gameTime);
        }

        private void DrawWallpaper(SpriteBatch spriteBatch, SpriteSheet wallpaper)
        {
            var paperBounds = wallpaper.getSprite(0);
            var paperColor = _control.level.wallpaperColor;
            if (_control.timePeriod == TimePeriod.Past)
                paperColor.A = 16;
            var startX = -paperBounds.Width;
            var xCount = _control.level.Width / paperBounds.Height + 2;
            int startY = (int)(Math.Floor((float)-_control.camera.Y / paperBounds.Height)) * paperBounds.Height;
            int yCount = _control.level.Height / paperBounds.Height + 1;
               // float minZ = (float)Math.Floor(ball.Z / 10) * 10.0f - 10;
            for (int drawX = 0; drawX < xCount; drawX++)
            {
                for (int drawY = 0; drawY < yCount; drawY++)
                {
                    Rectangle drawRect = new Rectangle(drawX * paperBounds.Width + startX, drawY * paperBounds.Height + startY, paperBounds.Width, paperBounds.Height);
                    Rectangle srcRect = new Rectangle(paperBounds.X, paperBounds.Y, paperBounds.Width, paperBounds.Height);
                    if ((drawY + 1) * paperBounds.Height + startY > _control.level.Height)
                    {
                        drawRect.Height = _control.level.Height - (drawY * paperBounds.Height + startY);
                        srcRect.Height = drawRect.Height;
                    }
                    spriteBatch.Draw(wallpaper.image, drawRect, srcRect, paperColor, 0f, new Vector2(),SpriteEffects.None, DrawLayer.Wallpaper);
                }
            }
            if (_control.timePeriod == TimePeriod.Present)
            {
                paperBounds = wallpaper.getSprite(1);
                //dest = new Rectangle(0, 0, ScreenWidth / 2, ScreenHeight / 2);
                for (int drawX = 0; drawX < xCount; drawX++)
                {
                    for (int drawY = 0; drawY < yCount; drawY++)
                    {
                        Rectangle drawRect = new Rectangle(drawX * paperBounds.Width + startX, drawY * paperBounds.Height + startY, paperBounds.Width, paperBounds.Height);
                        Rectangle srcRect = new Rectangle(paperBounds.X, paperBounds.Y, paperBounds.Width, paperBounds.Height);
                        if ((drawY + 1) * paperBounds.Height + startY > _control.level.Height)
                        {
                            drawRect.Height = _control.level.Height - (drawY * paperBounds.Height + startY);
                            srcRect.Height = drawRect.Height;
                        }
                        spriteBatch.Draw(wallpaper.image, drawRect, srcRect, Color.White, 0f, new Vector2(), SpriteEffects.None, DrawLayer.WallpaperTears);
                    }
                }
            }
        }
    }
}
