using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	sealed class Game1 : Game
	{
		readonly GraphicsDeviceManager _graphics;

		SpriteBatch _spriteBatch;

		# region Attributes

		Effect _greyScale;

		Texture2D _controller,
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

		//Fonts
		internal static SpriteFont GameFont,
								   MenuFont;

		static SpriteFont _titleFont;

		//Title
		GameTitle _title;
		Vector2 _startPosition;

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

			MediaPlayer.Play(Content.Load<Song>("Sounds/Soundtrack"));
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = 0.1f;

			_greyScale = Content.Load<Effect>("Effects/Greyscale");

			_wallpaperSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/Wallpaper"));
			_wallpaperSheet.SplitSheet(1, 2);

			_wardrobeSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/wardrobe"));
			_wardrobeSheet.SplitSheet(1, 5);
			_wardrobeSheet.AddAnimation("wardrobeclosed", new[] {0});
			_wardrobeSheet.AddAnimation("wardrobeopening", new[] {1, 2, 3});
			_wardrobeSheet.AddAnimation("wardrobeopen", new[] {4});

			_portraitSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/portrait"));
			_portraitSheet.SplitSheet(2, 1);

			_oldPortraitSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/PortraitWoman"));
			_oldPortraitSheet.SplitSheet(2, 1);

			_rubbleSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/rubble"));
			_rubbleSheet.AddSprite(0, 0, 37, 28);

			_actionSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/actions"));
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

			_boySheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/BruceSheet"));
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

			_shadowSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/Shadow"));
			_shadowSheet.SplitSheet(1, 4);
			_shadowSheet.AddAnimation("walk", new[] {0, 1, 2});
			_shadowSheet.AddAnimation("stopwalk", new[] {2, 1, 0});
			_shadowSheet.AddAnimation("stand", new[] {3});

			_floorSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/floor"));
			_floorSheet.SplitSheet(2, 1);

			_wallSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/wall"));
			_wallSheet.SplitSheet(1, 2);

			_stairSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/Staircase"));
			_stairSheet.SplitSheet(1, 2);

			_doorSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/door"));
			_doorSheet.SplitSheet(2, 3);
			_doorSheet.AddAnimation("doorclosedpast", new[] {0});
			_doorSheet.AddAnimation("dooropeningpast", new[] {1});
			_doorSheet.AddAnimation("dooropenpast", new[] {2});
			_doorSheet.AddAnimation("doorclosedpresent", new[] {3});
			_doorSheet.AddAnimation("dooropeningpresent", new[] {4});
			_doorSheet.AddAnimation("dooropenpresent", new[] {5});

			_keySheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/Key"));
			_keySheet.SplitSheet(2, 1);

			_chairSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/chair"));
			_chairSheet.SplitSheet(1, 2);

			_finalDoorSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/door_final"));
			_finalDoorSheet.SplitSheet(1, 7);
			_finalDoorSheet.AddAnimation("bookcaseclosed", new[] {0});
			_finalDoorSheet.AddAnimation("bookcaseopening", new[] {1, 2, 3, 4, 5});
			_finalDoorSheet.AddAnimation("bookcaseclosing", new[] {5, 4, 3, 2, 1});
			_finalDoorSheet.AddAnimation("bookcaseopen", new[] {6});

			_buttonSheet = new SpriteSheet(Content.Load<Texture2D>("Sprites/button"));
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

			_gameController.AddLevel(new Level("Content/Levels/tutorial.lvl"));
			_gameController.AddLevel(new Level("Content/Levels/level1.lvl"));
			_gameController.AddLevel(new Level("Content/Levels/level2.lvl"));
			_gameController.AddLevel(new Level("Content/Levels/level3.lvl"));
			_gameController.AddLevel(new Level("Content/Levels/level4.lvl"));
			_gameController.GoToLevel("Tutorial");

			_gameController.SetPlayer(new Boy(254, 240, new ActionBubble()));
			_gameController.SetCamera(new Camera(0, 360, 1280, 720, 2.0f));
			_gameController.InitLevel(false);

			_controller = Content.Load<Texture2D>("Sprites/controller");

			//Title Contents
			_titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
			GameFont = Content.Load<SpriteFont>("Fonts/GameFont");
			MenuFont = Content.Load<SpriteFont>("Fonts/MenuFont");
			_title = new GameTitle(Content.Load<Texture2D>("Sprites/screenshot_for_menu"),
								   new Rectangle(0, 0, ScreenWidth, ScreenHeight));
			_title.SetBottomTextRectangle(GameFont.MeasureString("Press Start"));
			_startPosition = new Vector2(_title.BottomTextRectangle.X, _title.BottomTextRectangle.Y);
		}

		public void ResetGame()
		{
			GameInProgress = false;

			_gameController.SetPlayer(new Boy(254, 240, new ActionBubble()));
			_gameController.SetCamera(new Camera(0, 360, 1280, 720, 2.0f));
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
				 GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start)) && _gameController.State == GameState.Game && !EndGame)
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

				else if (_stopwatch.ElapsedMilliseconds >= 10000)
				{
					EndGame = false;
					_stopwatch.Reset();
					_gameController.State = GameState.MainMenu;
					_title.State = GameTitle.TitleState.Credits;
					_title.MenuSize = 5;
					GameInProgress = false;
				}
			}

			//FADE OUT UPDATE
			else
				switch (_gameController.State)
				{
					case GameState.MainMenu:
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

					//Title Text
					_title.SetTopTextRectangle(_titleFont.MeasureString("Paranothing"));

					var text = "Paranothing";

					var vectorText = new Vector2(_title.TopTextRectangle.X, _title.TopTextRectangle.Y);

					//solid
					for (var layer = 0; layer < 3; layer++)
					{
						_spriteBatch.DrawString(_titleFont, text, vectorText, new Color(190, 190, 190));
						vectorText.X++;
						vectorText.Y++;
					}

					//top of character
					_spriteBatch.DrawString(_titleFont, text, vectorText, Color.WhiteSmoke);


					if (_title.State == GameTitle.TitleState.Title)
						_spriteBatch.DrawString(GameFont, "Press Start", _startPosition, Color.White);

					if (_title.State == GameTitle.TitleState.Controls)
						_spriteBatch.Draw(_controller, new Rectangle(200, 180, 500, 500), Color.White);
					break;
				case GameState.Game:

					_spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointWrap, null, null, _gameController.TimePeriod != TimePeriod.Present ? _greyScale : null,
									   Matrix.Identity * Matrix.CreateTranslation(-_gameController.Camera.X, -_gameController.Camera.Y, 0) * Matrix.CreateScale(_gameController.Camera.Scale));

					#region wallpaper

					var paperBounds = _wallpaperSheet.GetSprite(0);
					var paperColor = _gameController.Level.WallpaperColor;
					if (_gameController.TimePeriod == TimePeriod.Past)
						paperColor.A = 16;
					var startX = -paperBounds.Width;
					var xCount = _gameController.Level.Width / paperBounds.Height + 2;
					var startY = (int)Math.Floor((float)-_gameController.Camera.Y / paperBounds.Height) * paperBounds.Height;
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

							_spriteBatch.Draw(_wallpaperSheet.Image, drawRect, srcRect, paperColor, 0f, new Vector2(),
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
								var srcRect = new Rectangle(paperBounds.X, paperBounds.Y, paperBounds.Width, paperBounds.Height);
								if ((drawY + 1) * paperBounds.Height + startY > _gameController.Level.Height)
								{
									drawRect.Height = _gameController.Level.Height - (drawY * paperBounds.Height + startY);
									srcRect.Height = drawRect.Height;
								}

								_spriteBatch.Draw(_wallpaperSheet.Image, drawRect, srcRect, Color.White, 0f, new Vector2(),
												 SpriteEffects.None, DrawLayer.WallpaperTears);
							}
					}

					_gameController.DrawObjs(_spriteBatch);

					#endregion wallpaper

					if (EndGame)
					{
						_spriteBatch.End();
						_spriteBatch.Begin();

						_spriteBatch.Draw(_white, new Vector2(0, 0), null, Color.White * _fadeOpacity, 0f, Vector2.Zero,
										   new Vector2(ScreenWidth, ScreenHeight), SpriteEffects.None, 0f);

						if (_fadeOpacity >= 1)
							_spriteBatch.DrawString(MenuFont, "Bruce... you're safe now.", new Vector2(280, 300), Color.Black,
													 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}