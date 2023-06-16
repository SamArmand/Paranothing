using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing;

sealed class ParanothingGame : Game
{
    const float OpacityPerSecond = 0.02f;

    const int ScreenWidth = 1280,
        ScreenHeight = 720;

    public static bool EndGame;

    internal static SpriteFont GameFont,
        MenuFont;

    static SpriteFont _titleFont;

    public bool GameInProgress;

    readonly GameController _gameController = GameController.Instance;
    readonly GraphicsDeviceManager _graphics;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheetManager _sheetManager = SpriteSheetManager.Instance;
    Effect _greyScale;
    float _fadeOpacity;
    GameTitle _title;
    SpriteBatch _spriteBatch;

    SpriteSheet _bruceSheet,
        _shadowSheet,
        _actionSheet,
        _floorSheet,
        _wallSheet,
        _wallpaperSheet,
        _wardrobeSheet,
        _portraitSheet,
        _rubbleSheet,
        _stairsSheet,
        _doorSheet,
        _oldPortraitSheet,
        _keySheet,
        _chairSheet,
        _bookcaseSheet,
        _buttonSheet;

    Stopwatch _stopwatch;

    Texture2D _controller,
        _white;

    Vector2 _startPosition;

    public ParanothingGame()
    {
        _graphics = new(this)
        {
            PreferredBackBufferWidth = ScreenWidth,
            PreferredBackBufferHeight = ScreenHeight
        };
        Content.RootDirectory = "Content";
    }

    internal GameState GameState
    {
        set => _gameController.GameState = value;
    }

    public void ResetGame()
    {
        GameInProgress = false;

        _gameController.SetPlayer(new() { Position = new(254, 240) });
        _gameController.SetCamera(new() { Position = new(0, 360), Width = 1280, Height = 720, Scale = 2.0f });
        _gameController.InitLevel(false);

        _fadeOpacity = 0;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new(20, 20, 20));

        switch (_gameController.GameState)
        {
            case GameState.MainMenu:
                _spriteBatch.Begin();
                _title.Draw(_spriteBatch);

                const string text = "Paranothing";

                _title.SetTopTextRectangle(_titleFont.MeasureString(text));

                var vectorText = new Vector2(_title.TopTextRectangle.X, _title.TopTextRectangle.Y);

                for (var layer = 0; layer < 3; ++layer)
                {
                    _spriteBatch.DrawString(_titleFont, text, vectorText, new(190, 190, 190));
                    ++vectorText.X;
                    ++vectorText.Y;
                }

                _spriteBatch.DrawString(_titleFont, text, vectorText, Color.WhiteSmoke);

                switch (_title.State)
                {
                    case GameTitle.TitleState.Title:
                        _spriteBatch.DrawString(GameFont, "Press Start", _startPosition, Color.White);
                        break;
                    case GameTitle.TitleState.Controls:
                        _spriteBatch.Draw(_controller, new Rectangle(200, 180, 500, 500), Color.White);
                        break;
                }

                break;
            case GameState.Game:

                _spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointWrap, null, null,
                    _gameController.TimePeriod != TimePeriod.Present ? _greyScale : null,
                    Matrix.Identity *
                    Matrix.CreateTranslation(-_gameController.Camera.Position.X, -_gameController.Camera.Position.Y,
                        0) *
                    Matrix.CreateScale(_gameController.Camera.Scale));

                var paperBounds = _wallpaperSheet.GetSprite(0);
                var paperColor = _gameController.Level.WallpaperColor;
                if (_gameController.TimePeriod == TimePeriod.Past)
                    paperColor.A = 16;
                var startX = -paperBounds.Width;
                var xCount = _gameController.Level.Width / paperBounds.Height + 2;
                var startY = (int)Math.Floor(-_gameController.Camera.Position.Y / paperBounds.Height) *
                             paperBounds.Height;
                var yCount = _gameController.Level.Height / paperBounds.Height + 1;

                for (var drawX = 0; drawX < xCount; ++drawX)
                for (var drawY = 0; drawY < yCount; ++drawY)
                {
                    var drawRect = new Rectangle(drawX * paperBounds.Width + startX,
                        drawY * paperBounds.Height + startY, paperBounds.Width,
                        paperBounds.Height);
                    var srcRect = new Rectangle(paperBounds.X, paperBounds.Y, paperBounds.Width, paperBounds.Height);
                    if ((drawY + 1) * paperBounds.Height + startY > _gameController.Level.Height)
                    {
                        drawRect.Height = _gameController.Level.Height - (drawY * paperBounds.Height + startY);
                        srcRect.Height = drawRect.Height;
                    }

                    _spriteBatch.Draw(_wallpaperSheet.Image, drawRect, srcRect, paperColor, 0f, new(),
                        SpriteEffects.None, DrawLayer.Wallpaper);
                }

                if (_gameController.TimePeriod == TimePeriod.Present)
                {
                    paperBounds = _wallpaperSheet.GetSprite(1);

                    for (var drawX = 0; drawX < xCount; ++drawX)
                    for (var drawY = 0; drawY < yCount; ++drawY)
                    {
                        var drawRect = new Rectangle(drawX * paperBounds.Width + startX,
                            drawY * paperBounds.Height + startY, paperBounds.Width,
                            paperBounds.Height);
                        var srcRect = new Rectangle(paperBounds.X, paperBounds.Y, paperBounds.Width,
                            paperBounds.Height);
                        if ((drawY + 1) * paperBounds.Height + startY > _gameController.Level.Height)
                        {
                            drawRect.Height = _gameController.Level.Height - (drawY * paperBounds.Height + startY);
                            srcRect.Height = drawRect.Height;
                        }

                        _spriteBatch.Draw(_wallpaperSheet.Image, drawRect, srcRect, Color.White, 0f, new(),
                            SpriteEffects.None, DrawLayer.WallpaperTears);
                    }
                }

                _gameController.DrawObjects(_spriteBatch);

                if (EndGame)
                {
                    _spriteBatch.End();
                    _spriteBatch.Begin();

                    _spriteBatch.Draw(_white, new(0, 0), null, Color.White * _fadeOpacity, 0f, Vector2.Zero,
                        new Vector2(ScreenWidth, ScreenHeight), SpriteEffects.None, 0f);

                    if (_fadeOpacity >= 1)
                        _spriteBatch.DrawString(MenuFont, "Bruce... you're safe now.", new(280, 300), Color.Black,
                            0f, new(0, 0), 1f, SpriteEffects.None, 1f);
                }

                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void LoadContent()
    {
        _gameController.GameState = GameState.MainMenu;

        _spriteBatch = new(GraphicsDevice);

        _stopwatch = new();

        _white = Content.Load<Texture2D>("Sprites/White");

        var soundEffects = _soundManager.SoundEffects;

        soundEffects.Add("Button Press", Content.Load<SoundEffect>("Sounds/Button Press"));
        soundEffects.Add("Chair Drop", Content.Load<SoundEffect>("Sounds/Chair Drop"));
        soundEffects.Add("Death", Content.Load<SoundEffect>("Sounds/Death"));
        soundEffects.Add("Door Unlock", Content.Load<SoundEffect>("Sounds/Door Unlock"));
        soundEffects.Add("Final Door Part 1", Content.Load<SoundEffect>("Sounds/Final Door Part 1"));
        soundEffects.Add("Final Door Part 2", Content.Load<SoundEffect>("Sounds/Final Door Part 2"));
        soundEffects.Add("Key", Content.Load<SoundEffect>("Sounds/Key"));
        soundEffects.Add("Portrait TimeTravel", Content.Load<SoundEffect>("Sounds/Portrait Travel"));
        soundEffects.Add("Pushing Wardrobe", Content.Load<SoundEffect>("Sounds/Pushing Wardrobe"));
        soundEffects.Add("Shadow", Content.Load<SoundEffect>("Sounds/Shadow"));
        soundEffects.Add("Wardrobe TimeTravel", Content.Load<SoundEffect>("Sounds/Wardrobe Travel"));
        soundEffects.Add("Wardrobe Unlock", Content.Load<SoundEffect>("Sounds/Wardrobe Unlock"));

        MediaPlayer.Play(Content.Load<Song>("Sounds/Soundtrack"));
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.1f;

        _greyScale = Content.Load<Effect>("Effects/Greyscale");

        _wallpaperSheet = new(Content.Load<Texture2D>("Sprites/Wallpaper"));
        _wallpaperSheet.SplitSheet(1, 2);

        _wardrobeSheet = new(Content.Load<Texture2D>("Sprites/Wardrobe"));
        _wardrobeSheet.SplitSheet(1, 5);
        _wardrobeSheet.AddAnimation("wardrobe_closed", new[] { 0 });
        _wardrobeSheet.AddAnimation("wardrobe_opening", new[] { 1, 2, 3 });
        _wardrobeSheet.AddAnimation("wardrobe_open", new[] { 4 });

        _portraitSheet = new(Content.Load<Texture2D>("Sprites/Portrait"));
        _portraitSheet.SplitSheet(2, 1);

        _oldPortraitSheet = new(Content.Load<Texture2D>("Sprites/PortraitWoman"));
        _oldPortraitSheet.SplitSheet(2, 1);

        _rubbleSheet = new(Content.Load<Texture2D>("Sprites/Rubble"));
        _rubbleSheet.AddSprite(0, 0, 37, 28);

        _actionSheet = new(Content.Load<Texture2D>("Sprites/Actions"));
        _actionSheet.SplitSheet(3, 3);
        _actionSheet.AddAnimation("bubble", new[] { 0 });
        _actionSheet.AddAnimation("wardrobe", new[] { 1 });
        _actionSheet.AddAnimation("push", new[] { 2 });
        _actionSheet.AddAnimation("chair", new[] { 3 });
        _actionSheet.AddAnimation("stairs", new[] { 4 });
        _actionSheet.AddAnimation("portrait", new[] { 5 });
        _actionSheet.AddAnimation("old_portrait", new[] { 6 });
        _actionSheet.AddAnimation("bookcase", new[] { 7 });
        _actionSheet.AddAnimation("negate", new[] { 8 });

        _bruceSheet = new(Content.Load<Texture2D>("Sprites/Bruce"));
        _bruceSheet.SplitSheet(7, 9, 0, 0, 58);
        _bruceSheet.AddAnimation("walk", new[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        _bruceSheet.AddAnimation("stand", new[] { 8 });
        _bruceSheet.AddAnimation("leave_wardrobe", new[] { 9, 10, 11, 12, 13, 14, 15, 16 });
        _bruceSheet.AddAnimation("enter_wardrobe", new[] { 18, 19, 20, 21, 22, 23, 24, 25 });
        _bruceSheet.AddAnimation("enter_portrait", new[] { 27, 28, 29, 30, 31, 32, 33, 34 });
        _bruceSheet.AddAnimation("leave_portrait", new[] { 34, 33, 32, 31, 30, 29, 28, 27 });
        _bruceSheet.AddAnimation("start_push", new[] { 36, 37, 38, 39 });
        _bruceSheet.AddAnimation("end_push", new[] { 39, 38, 37, 36 });
        _bruceSheet.AddAnimation("push", new[] { 41, 42, 43, 44, 45, 46, 47, 48 });
        _bruceSheet.AddAnimation("push_still", new[] { 49 });
        _bruceSheet.AddAnimation("control_start", new[] { 50, 51, 52 });
        _bruceSheet.AddAnimation("control", new[] { 53 });
        _bruceSheet.AddAnimation("control_end", new[] { 52, 51, 50 });
        _bruceSheet.AddAnimation("disappear", new[] { 50, 51, 52, 53, 54, 55, 56, 57 });

        _shadowSheet = new(Content.Load<Texture2D>("Sprites/Shadow"));
        _shadowSheet.SplitSheet(1, 4);
        _shadowSheet.AddAnimation("walk", new[] { 0, 1, 2 });
        _shadowSheet.AddAnimation("stop_walk", new[] { 2, 1, 0 });
        _shadowSheet.AddAnimation("stand", new[] { 3 });

        _floorSheet = new(Content.Load<Texture2D>("Sprites/Floor"));
        _floorSheet.SplitSheet(2, 1);

        _wallSheet = new(Content.Load<Texture2D>("Sprites/Wall"));
        _wallSheet.SplitSheet(1, 2);

        _stairsSheet = new(Content.Load<Texture2D>("Sprites/Staircase"));
        _stairsSheet.SplitSheet(1, 2);

        _doorSheet = new(Content.Load<Texture2D>("Sprites/Door"));
        _doorSheet.SplitSheet(2, 3);
        _doorSheet.AddAnimation("door_closed_past", new[] { 0 });
        _doorSheet.AddAnimation("door_opening_past", new[] { 1 });
        _doorSheet.AddAnimation("door_open_past", new[] { 2 });
        _doorSheet.AddAnimation("door_closed_present", new[] { 3 });
        _doorSheet.AddAnimation("door_opening_present", new[] { 4 });
        _doorSheet.AddAnimation("door_open_present", new[] { 5 });

        _keySheet = new(Content.Load<Texture2D>("Sprites/Key"));
        _keySheet.SplitSheet(2, 1);

        _chairSheet = new(Content.Load<Texture2D>("Sprites/Chair"));
        _chairSheet.SplitSheet(1, 2);

        _bookcaseSheet = new(Content.Load<Texture2D>("Sprites/Bookcase"));
        _bookcaseSheet.SplitSheet(1, 7);
        _bookcaseSheet.AddAnimation("bookcase_closed", new[] { 0 });
        _bookcaseSheet.AddAnimation("bookcase_opening", new[] { 1, 2, 3, 4, 5 });
        _bookcaseSheet.AddAnimation("bookcase_closing", new[] { 5, 4, 3, 2, 1 });
        _bookcaseSheet.AddAnimation("bookcase_open", new[] { 6 });

        _buttonSheet = new(Content.Load<Texture2D>("Sprites/Button"));
        _buttonSheet.SplitSheet(1, 2);

        _sheetManager.AddSheet("wallpaper", _wallpaperSheet);
        _sheetManager.AddSheet("wardrobe", _wardrobeSheet);
        _sheetManager.AddSheet("portrait", _portraitSheet);
        _sheetManager.AddSheet("rubble", _rubbleSheet);
        _sheetManager.AddSheet("action", _actionSheet);
        _sheetManager.AddSheet("bruce", _bruceSheet);
        _sheetManager.AddSheet("floor", _floorSheet);
        _sheetManager.AddSheet("wall", _wallSheet);
        _sheetManager.AddSheet("stairs", _stairsSheet);
        _sheetManager.AddSheet("door", _doorSheet);
        _sheetManager.AddSheet("old_portrait", _oldPortraitSheet);
        _sheetManager.AddSheet("key", _keySheet);
        _sheetManager.AddSheet("chair", _chairSheet);
        _sheetManager.AddSheet("bookcase", _bookcaseSheet);
        _sheetManager.AddSheet("button", _buttonSheet);
        _sheetManager.AddSheet("shadow", _shadowSheet);

        _gameController.AddLevel(new("Content/Levels/tutorial.lvl"));
        _gameController.AddLevel(new("Content/Levels/level1.lvl"));
        _gameController.AddLevel(new("Content/Levels/level2.lvl"));
        _gameController.AddLevel(new("Content/Levels/level3.lvl"));
        _gameController.AddLevel(new("Content/Levels/level4.lvl"));
        _gameController.GoToLevel("Tutorial");

        _gameController.SetPlayer(new() { Position = new(254, 240) });
        _gameController.SetCamera(new() { Position = new(0, 360), Width = 1280, Height = 720, Scale = 2.0f });
        _gameController.InitLevel(false);

        _controller = Content.Load<Texture2D>("Sprites/Controller");

        _titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
        GameFont = Content.Load<SpriteFont>("Fonts/GameFont");
        MenuFont = Content.Load<SpriteFont>("Fonts/MenuFont");
        _title = new(Content.Load<Texture2D>("Sprites/Title Background"),
            new(0, 0, ScreenWidth, ScreenHeight));
        _title.SetBottomTextRectangle(GameFont.MeasureString("Press Start"));
        _startPosition = new(_title.BottomTextRectangle.X, _title.BottomTextRectangle.Y);
    }

    protected override void UnloadContent() => _graphics.Dispose();

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        else if ((keyboardState.IsKeyDown(Keys.P) ||
                  GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start)) &&
                 _gameController.GameState == GameState.Game && !EndGame)
        {
            _gameController.GameState = GameState.MainMenu;
            _title.State = GameTitle.TitleState.Pause;
            _title.MenuSize = 4;
        }

        if (EndGame)
        {
            if (!_stopwatch.IsRunning)
                _stopwatch.Start();

            if (_fadeOpacity < 1)
                _fadeOpacity = _stopwatch.ElapsedMilliseconds / 100f * OpacityPerSecond;

            else if (_stopwatch.ElapsedMilliseconds >= 10000)
            {
                EndGame = false;
                _stopwatch.Reset();
                _gameController.GameState = GameState.MainMenu;
                _title.State = GameTitle.TitleState.Credits;
                _title.MenuSize = 5;
                GameInProgress = false;
            }
        }

        else
            switch (_gameController.GameState)
            {
                case GameState.MainMenu:
                    _title.Update(this);
                    break;
                case GameState.Game:
                    GameInProgress = true;

                    _gameController.UpdateObjects(gameTime);
                    break;
            }

        base.Update(gameTime);
    }
}