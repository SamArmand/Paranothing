using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing
{
	enum GameState
	{
		MainMenu,
		Game
	}

	enum Direction
	{
		Left,
		Right,
		Up,
		Down
	}

	enum TimePeriod
	{
		FarPast,
		Past,
		Present
	}

	struct DrawLayer
	{
		internal const float Chairs = 0.015f,
							 Player = 0.02f,
							 Rubble = 0.03f,
							 Key = 0.035f,
							 Wardrobe = 0.04f,
							 Floor = 0.05f,
							 Stairs = 0.06f,
							 PlayerBehindStairs = 0.07f,
							 Background = 0.08f,
							 WallpaperTears = 0.09f,
							 Wallpaper = 0.10f,
							 ActionBubble = 0.01f;
	}

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	sealed class Game1 : Game
	{
		readonly GraphicsDeviceManager _graphics;

		SpriteBatch _spriteBatch,
					_spriteBatch2;

		# region Attributes

		Effect _greyScale;

		Song _bgMusic;

		Texture2D _boyTex,
				  _shadowTex,
				  _actionTex,
				  _floorTex,
				  _wallTex,
				  _wallpaperTex,
				  _wardrobeTex,
				  _portraitTex,
				  _rubbleTex,
				  _stairTex,
				  _doorTex,
				  _oldPortraitTex,
				  _keyTex,
				  _chairTex,
				  _finalDoorTex,
				  _buttonTex,
				  _controller,
				  _white;

		SpriteSheet _boySheet,
					_shadowSheet,
					_actionSheet,
					_floorSheet,
					_wallSheet,
					_wallpaperSheet,
					_wardrobeSheet,
					_portraitSheet,
					_rubbleSheet,
					_stairSheet,
					_doorSheet,
					_oldPortraitSheet,
					_keySheet,
					_chairSheet,
					_finalDoorSheet,
					_buttonSheet;

		//Portrait

		readonly GameController _gameController = GameController.GetInstance();
		readonly SpriteSheetManager _sheetManager = SpriteSheetManager.GetInstance();
		readonly SoundManager _soundManager = SoundManager.Instance();

		const int ScreenWidth = 1280,
				  ScreenHeight = 720;

		Boy _player;
		ActionBubble _actionBubble;

		//Fonts
		internal static SpriteFont GameFont,
								   MenuFont;

		static SpriteFont _titleFont;

		//Title
		GameTitle _title;
		Vector2 _startPosition;

		Level _tutorial,
			  _level1,
			  _level2,
			  _level3,
			  _level4;

		public bool GameInProgress;
		public static bool EndGame;
		float _fadeOpacity;
		const float OpacityPerSecond = 0.02f;
		Stopwatch _stopwatch;

		# endregion

		# region Methods

		internal GameState GameState
		{
			set => _gameController.State = value;
		}

		/// <summary>
		/// Draws text on the screen
		/// </summary>
		/// <param name="text">text to write</param>
		/// <param name="font">font of text</param>
		/// <param name="textColor">color of text</param>
		/// <param name="x">left hand edge of text</param>
		/// <param name="y">top of text</param>
		void DrawText(string text, SpriteFont font, Color textColor, float x, float y)
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
		void LoadTitleContents()
		{
			_titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
			GameFont = Content.Load<SpriteFont>("Fonts/GameFont");
			MenuFont = Content.Load<SpriteFont>("Fonts/MenuFont");
			_title = new GameTitle(Content.Load<Texture2D>("Sprites/screenshot_for_menu"),
								   new Rectangle(0, 0, ScreenWidth, ScreenHeight));
			_title.SetBottomTextRectangle(GameFont.MeasureString("Press Start"));
			_startPosition = new Vector2(_title.BottomTextRectangle.X, _title.BottomTextRectangle.Y);
		}

		void DrawTitleText()
		{
			_title.SetTopTextRectangle(_titleFont.MeasureString("Paranothing"));
			DrawText("Paranothing", _titleFont, Color.WhiteSmoke, _title.TopTextRectangle.X, _title.TopTextRectangle.Y);
			if (_title.State == GameTitle.TitleState.Title)
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
			_gameController.State = GameState.MainMenu;

			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_spriteBatch2 = new SpriteBatch(GraphicsDevice);

			//Stuff for fade
			_stopwatch = new Stopwatch();
			_white = Content.Load<Texture2D>("Sprites/white");

			_soundManager.SoundEffects.Add("Button Press", Content.Load<SoundEffect>("Sounds/Button Press"));
			_soundManager.SoundEffects.Add("Chair Drop", Content.Load<SoundEffect>("Sounds/Chair Drop"));
			_soundManager.SoundEffects.Add("Death", Content.Load<SoundEffect>("Sounds/Death"));
			_soundManager.SoundEffects.Add("Door Unlock", Content.Load<SoundEffect>("Sounds/Door Unlock"));
			_soundManager.SoundEffects.Add("Final Door Part 1", Content.Load<SoundEffect>("Sounds/Final Door Part 1"));
			_soundManager.SoundEffects.Add("Final Door Part 2", Content.Load<SoundEffect>("Sounds/Final Door Part 2"));
			_soundManager.SoundEffects.Add("Key", Content.Load<SoundEffect>("Sounds/Key"));
			_soundManager.SoundEffects.Add("Portrait Travel", Content.Load<SoundEffect>("Sounds/Portrait Travel"));
			_soundManager.SoundEffects.Add("Pushing Wardrobe", Content.Load<SoundEffect>("Sounds/Pushing Wardrobe"));
			_soundManager.SoundEffects.Add("Shadow", Content.Load<SoundEffect>("Sounds/Shadow"));
			_soundManager.SoundEffects.Add("Step 1", Content.Load<SoundEffect>("Sounds/Step 1"));
			_soundManager.SoundEffects.Add("Step 2", Content.Load<SoundEffect>("Sounds/Step 2"));
			_soundManager.SoundEffects.Add("Step 3", Content.Load<SoundEffect>("Sounds/Step 3"));
			_soundManager.SoundEffects.Add("Step 4", Content.Load<SoundEffect>("Sounds/Step 4"));
			_soundManager.SoundEffects.Add("Step 5", Content.Load<SoundEffect>("Sounds/Step 5"));
			_soundManager.SoundEffects.Add("Step 6", Content.Load<SoundEffect>("Sounds/Step 6"));
			_soundManager.SoundEffects.Add("Wardrobe Travel", Content.Load<SoundEffect>("Sounds/Wardrobe Travel"));
			_soundManager.SoundEffects.Add("Wardrobe Unlock", Content.Load<SoundEffect>("Sounds/Wardrobe Unlock"));

			_bgMusic = Content.Load<Song>("Sounds/Soundtrack");
			MediaPlayer.Play(_bgMusic);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = 0.1f;

			_greyScale = Content.Load<Effect>("Effects/Greyscale");

			_wallpaperTex = Content.Load<Texture2D>("Sprites/Wallpaper");
			_wallpaperSheet = new SpriteSheet(_wallpaperTex);
			_wallpaperSheet.SplitSheet(1, 2);

			_wardrobeTex = Content.Load<Texture2D>("Sprites/wardrobe");
			_wardrobeSheet = new SpriteSheet(_wardrobeTex);
			_wardrobeSheet.SplitSheet(1, 5);
			_wardrobeSheet.AddAnimation("wardrobeclosed", new[] {0});
			_wardrobeSheet.AddAnimation("wardrobeopening", new[] {1, 2, 3});
			_wardrobeSheet.AddAnimation("wardrobeopen", new[] {4});

			//Portrait
			_portraitTex = Content.Load<Texture2D>("Sprites/portrait");
			_portraitSheet = new SpriteSheet(_portraitTex);
			_portraitSheet.SplitSheet(2, 1);

			_rubbleTex = Content.Load<Texture2D>("Sprites/rubble");
			_rubbleSheet = new SpriteSheet(_rubbleTex);
			_rubbleSheet.AddSprite(0, 0, 37, 28);

			_actionTex = Content.Load<Texture2D>("Sprites/actions");
			_actionSheet = new SpriteSheet(_actionTex);
			_actionSheet.SplitSheet(3, 3);
			_actionSheet.AddAnimation("bubble", new[] {0});
			_actionSheet.AddAnimation("wardrobe", new[] {1});
			_actionSheet.AddAnimation("push", new[] {2});
			_actionSheet.AddAnimation("chair", new[] {3});
			_actionSheet.AddAnimation("stair", new[] {4});
			_actionSheet.AddAnimation("portrait", new[] {5});
			_actionSheet.AddAnimation("oldportrait", new[] {6});
			_actionSheet.AddAnimation("bookcase", new[] {7});
			_actionSheet.AddAnimation("negate", new[] {8});

			_boyTex = Content.Load<Texture2D>("Sprites/BruceSheet");
			_boySheet = new SpriteSheet(_boyTex);
			_boySheet.SplitSheet(7, 9, 0, 0, 58);
			_boySheet.AddAnimation("walk", new[] {0, 1, 2, 3, 4, 5, 6, 7});
			_boySheet.AddAnimation("stand", new[] {8});
			_boySheet.AddAnimation("leavewardrobe", new[] {9, 10, 11, 12, 13, 14, 15, 16});
			_boySheet.AddAnimation("enterwardrobe", new[] {18, 19, 20, 21, 22, 23, 24, 25});
			_boySheet.AddAnimation("enterportrait", new[] {27, 28, 29, 30, 31, 32, 33, 34});
			_boySheet.AddAnimation("leaveportrait", new[] {34, 33, 32, 31, 30, 29, 28, 27});
			_boySheet.AddAnimation("startpush", new[] {36, 37, 38, 39});
			_boySheet.AddAnimation("endpush", new[] {39, 38, 37, 36});
			_boySheet.AddAnimation("push", new[] {41, 42, 43, 44, 45, 46, 47, 48});
			_boySheet.AddAnimation("pushstill", new[] {49});
			_boySheet.AddAnimation("controlstart", new[] {50, 51, 52});
			_boySheet.AddAnimation("control", new[] {53});
			_boySheet.AddAnimation("controlend", new[] {52, 51, 50});
			_boySheet.AddAnimation("disappear", new[] {50, 51, 52, 53, 54, 55, 56, 57});

			_shadowTex = Content.Load<Texture2D>("Sprites/Shadow");
			_shadowSheet = new SpriteSheet(_shadowTex);
			_shadowSheet.SplitSheet(1, 4);
			_shadowSheet.AddAnimation("walk", new[] {0, 1, 2});
			_shadowSheet.AddAnimation("stopwalk", new[] {2, 1, 0});
			_shadowSheet.AddAnimation("stand", new[] {3});

			_floorTex = Content.Load<Texture2D>("Sprites/floor");
			_floorSheet = new SpriteSheet(_floorTex);
			_floorSheet.SplitSheet(2, 1);

			_wallTex = Content.Load<Texture2D>("Sprites/wall");
			_wallSheet = new SpriteSheet(_wallTex);
			_wallSheet.SplitSheet(1, 2);

			_stairTex = Content.Load<Texture2D>("Sprites/Staircase");
			_stairSheet = new SpriteSheet(_stairTex);
			_stairSheet.SplitSheet(1, 2);

			_doorTex = Content.Load<Texture2D>("Sprites/door");
			_doorSheet = new SpriteSheet(_doorTex);
			_doorSheet.SplitSheet(2, 3);
			_doorSheet.AddAnimation("doorclosedpast", new[] {0});
			_doorSheet.AddAnimation("dooropeningpast", new[] {1});
			_doorSheet.AddAnimation("dooropenpast", new[] {2});
			_doorSheet.AddAnimation("doorclosedpresent", new[] {3});
			_doorSheet.AddAnimation("dooropeningpresent", new[] {4});
			_doorSheet.AddAnimation("dooropenpresent", new[] {5});

			//Old Portrait
			_oldPortraitTex = Content.Load<Texture2D>("Sprites/PortraitWoman");
			_oldPortraitSheet = new SpriteSheet(_oldPortraitTex);
			_oldPortraitSheet.SplitSheet(2, 1);

			_keyTex = Content.Load<Texture2D>("Sprites/Key");
			_keySheet = new SpriteSheet(_keyTex);
			_keySheet.SplitSheet(2, 1);

			_chairTex = Content.Load<Texture2D>("Sprites/chair");
			_chairSheet = new SpriteSheet(_chairTex);
			_chairSheet.SplitSheet(1, 2);

			_finalDoorTex = Content.Load<Texture2D>("Sprites/door_final");
			_finalDoorSheet = new SpriteSheet(_finalDoorTex);
			_finalDoorSheet.SplitSheet(1, 7);
			_finalDoorSheet.AddAnimation("bookcaseclosed", new[] {0});
			_finalDoorSheet.AddAnimation("bookcaseopening", new[] {1, 2, 3, 4, 5});
			_finalDoorSheet.AddAnimation("bookcaseclosing", new[] {5, 4, 3, 2, 1});
			_finalDoorSheet.AddAnimation("bookcaseopen", new[] {6});

			_buttonTex = Content.Load<Texture2D>("Sprites/button");
			_buttonSheet = new SpriteSheet(_buttonTex);
			_buttonSheet.SplitSheet(1, 2);

			_sheetManager.AddSheet("wallpaper", _wallpaperSheet);
			_sheetManager.AddSheet("wardrobe", _wardrobeSheet);
			_sheetManager.AddSheet("portrait", _portraitSheet);
			_sheetManager.AddSheet("rubble", _rubbleSheet);
			_sheetManager.AddSheet("action", _actionSheet);
			_sheetManager.AddSheet("boy", _boySheet);
			_sheetManager.AddSheet("floor", _floorSheet);
			_sheetManager.AddSheet("wall", _wallSheet);
			_sheetManager.AddSheet("stair", _stairSheet);
			_sheetManager.AddSheet("door", _doorSheet);
			_sheetManager.AddSheet("oldportrait", _oldPortraitSheet);
			_sheetManager.AddSheet("key", _keySheet);
			_sheetManager.AddSheet("chair", _chairSheet);
			_sheetManager.AddSheet("bookcase", _finalDoorSheet);
			_sheetManager.AddSheet("button", _buttonSheet);
			_sheetManager.AddSheet("shadow", _shadowSheet);

			_actionBubble = new ActionBubble();
			_player = new Boy(254, 240, _actionBubble);
			var camera = new Camera(0, 360, 1280, 720, 2.0f);
			_tutorial = new Level("Content/Levels/tutorial.lvl");
			_level1 = new Level("Content/Levels/level1.lvl");
			_level2 = new Level("Content/Levels/level2.lvl");
			_level3 = new Level("Content/Levels/level3.lvl");
			_level4 = new Level("Content/Levels/level4.lvl");
			_gameController.AddLevel(_tutorial);
			_gameController.AddLevel(_level1);
			_gameController.AddLevel(_level2);
			_gameController.AddLevel(_level3);
			_gameController.AddLevel(_level4);
			_gameController.GoToLevel("Tutorial");

			_gameController.SetPlayer(_player);
			_gameController.SetCamera(camera);
			_gameController.InitLevel(false);

			_controller = Content.Load<Texture2D>("Sprites/controller");

			LoadTitleContents();
		}

		public void ResetGame()
		{
			GameInProgress = false;

			_actionBubble = new ActionBubble();
			_player = new Boy(254, 240, _actionBubble);

			var camera = new Camera(0, 360, 1280, 720, 2.0f);

			_gameController.SetPlayer(_player);
			_gameController.SetCamera(camera);
			_gameController.InitLevel(false);

			_fadeOpacity = 0;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() => _graphics.Dispose();

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

			if ((Keyboard.GetState().IsKeyDown(Keys.P) ||
				 GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start)) && _gameController.State == GameState.Game)
			{
				_gameController.State = GameState.MainMenu;
				_title.State = GameTitle.TitleState.Pause;
				_title.MenuSize = 4;
			}

			if (EndGame)
			{
				if (!_stopwatch.IsRunning)
					_stopwatch.Start();

				if (_fadeOpacity < 1)
					_fadeOpacity = _stopwatch.ElapsedMilliseconds / 100f * OpacityPerSecond;

				if (Math.Abs(_fadeOpacity - 1) <= 0 && _stopwatch.ElapsedMilliseconds >= 15000)
				{
					EndGame = false;
					_stopwatch.Reset();
					_gameController.State = GameState.MainMenu;
					_title.State = GameTitle.TitleState.Menu;
					_title.MenuSize = 5;
					GameInProgress = false;
				}
			}

			//FADE OUT UPDATE
			else
				switch (_gameController.State)
				{
					case GameState.MainMenu:
						EndGame = false;
						_stopwatch.Reset();
						_title.Update(this, Keyboard.GetState());
						break;
					case GameState.Game:

						GameInProgress = true;

						_gameController.UpdateObjs(gameTime);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(new Color(20, 20, 20));

			switch (_gameController.State)
			{
				case GameState.MainMenu:
					_spriteBatch.Begin();
					_title.Draw(_spriteBatch);
					DrawTitleText();

					if (_title.State == GameTitle.TitleState.Controls)
						_spriteBatch.Draw(_controller, new Rectangle(200, 180, 500, 500), Color.White);
					break;
				case GameState.Game:
					Effect pastEffect = null;
					if (_gameController.TimePeriod != TimePeriod.Present)
						pastEffect = _greyScale;
					var transform = Matrix.Identity;
					transform *= Matrix.CreateTranslation(-_gameController.Camera.X, -_gameController.Camera.Y, 0);
					transform *= Matrix.CreateScale(_gameController.Camera.Scale);
					_spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointWrap, null, null, pastEffect,
									   transform);
					DrawWallpaper(_spriteBatch, _wallpaperSheet);
					_gameController.DrawObjs(_spriteBatch);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_spriteBatch.End();

			_spriteBatch2.Begin();


			if (EndGame)
			{
				_spriteBatch2.Draw(_white, new Vector2(0, 0), null, Color.White * _fadeOpacity, 0f, Vector2.Zero,
								   new Vector2(ScreenWidth, ScreenHeight), SpriteEffects.None, 0f);

				if (_fadeOpacity >= 1)
					_spriteBatch2.DrawString(MenuFont, "Bruce... you're safe now.", new Vector2(280, 300), Color.Black,
											 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
			}

			_spriteBatch2.End();

			base.Draw(gameTime);
		}

		void DrawWallpaper(SpriteBatch spriteBatch, SpriteSheet wallpaper)
		{
			var paperBounds = wallpaper.GetSprite(0);
			var paperColor = _gameController.Level.WallpaperColor;
			if (_gameController.TimePeriod == TimePeriod.Past)
				paperColor.A = 16;
			var startX = -paperBounds.Width;
			var xCount = _gameController.Level.Width / paperBounds.Height + 2;
			var startY = (int) (Math.Floor((float) -_gameController.Camera.Y / paperBounds.Height)) * paperBounds.Height;
			var yCount = _gameController.Level.Height / paperBounds.Height + 1;

			for (var drawX = 0; drawX < xCount; drawX++)
				for (var drawY = 0; drawY < yCount; drawY++)
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

					spriteBatch.Draw(wallpaper.Image, drawRect, srcRect, paperColor, 0f, new Vector2(),
									 SpriteEffects.None, DrawLayer.Wallpaper);
				}

			if (_gameController.TimePeriod != TimePeriod.Present) return;

			paperBounds = wallpaper.GetSprite(1);

			for (var drawX = 0; drawX < xCount; drawX++)
				for (var drawY = 0; drawY < yCount; drawY++)
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

					spriteBatch.Draw(wallpaper.Image, drawRect, srcRect, Color.White, 0f, new Vector2(),
									 SpriteEffects.None, DrawLayer.WallpaperTears);
				}
		}
	}
}