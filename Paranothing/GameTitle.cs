using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing
{
    internal sealed class GameTitle : GameBackground
    {
        # region Attribute

        private Rectangle _topTextRect;
        private Rectangle _bottomTextRect;

        private int _menuIndex;
        public int MenuSize = 5;
        private GamePadState _prevPad;
        private KeyboardState _prevKeys;
        public static bool ToggleSound = true;
        private bool _toggleMusic = true;
        private string _soundText = "ON";
        private string _musicText = "ON";

        private readonly Color[] _colors = {Color.Yellow, Color.White, Color.White, Color.White, Color.White, Color.White};

        private readonly Vector2 _choice1 = new Vector2(750, 300);
        private readonly Vector2 _choice2 = new Vector2(750, 360);
        private readonly Vector2 _choice3 = new Vector2(750, 420);
        private readonly Vector2 _choice4 = new Vector2(750, 480);
        private readonly Vector2 _choice5 = new Vector2(750, 540);
        private readonly Vector2 _choice6 = new Vector2(750, 600);

        private readonly GameController _control = GameController.GetInstance();

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

        public TitleState State = TitleState.Title;

        # endregion

        # region Constructor

        public GameTitle(Texture2D inTexture, Rectangle inRect)
            : base(inTexture, inRect)
        {
            _topTextRect = new Rectangle();
            _bottomTextRect = new Rectangle();
        }

        # endregion

        # region Methods

        //Accessors
        public Rectangle TopTextRectangle => _topTextRect;

        public void SetTopTextRectangle(Vector2 inVec)
        {
            _topTextRect.X = BackgoundRectangle.Center.X - (int)(inVec.X / 2);
            _topTextRect.Y = BackgoundRectangle.Top;
            _topTextRect.Width = (int)inVec.X;
            _topTextRect.Height = (int)inVec.Y;
        }
        public Rectangle BottomTextRectangle => _bottomTextRect;

        public void SetBottomTextRectangle(Vector2 inVec)
        {
            _bottomTextRect.X = BackgoundRectangle.Center.X - (int)(inVec.X / 2);
            _bottomTextRect.Y = BackgoundRectangle.Bottom - (int)inVec.Y;
            _bottomTextRect.Width = (int)inVec.X;
            _bottomTextRect.Height = (int)inVec.Y;
        }

        //Update
        public void Update(Game1 game, KeyboardState keys)
        {

            var padState = GamePad.GetState(PlayerIndex.One);

            //Press enter to start
            if ((keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.Start)) && State == TitleState.Title)
            {
                State = TitleState.Menu;
            }

            //Selecting menu options
            else if ((padState.IsButtonDown(Buttons.LeftThumbstickDown) || keys.IsKeyDown(Keys.Down)) && !(_prevPad.IsButtonDown(Buttons.LeftThumbstickDown) || _prevKeys.IsKeyDown(Keys.Down)) && _menuIndex < MenuSize - 1)
            {
                _colors[_menuIndex++] = Color.White;
                _colors[_menuIndex] = Color.Yellow;
            }

            else if ((padState.IsButtonDown(Buttons.LeftThumbstickUp) || keys.IsKeyDown(Keys.Up)) && !(_prevPad.IsButtonDown(Buttons.LeftThumbstickUp) || _prevKeys.IsKeyDown(Keys.Up)) && _menuIndex > 0)
            {
                _colors[_menuIndex--] = Color.White;
                _colors[_menuIndex] = Color.Yellow;
            }

            else if ((keys.IsKeyDown(Keys.Enter) || padState.IsButtonDown(Buttons.A)) &&
                !(_prevKeys.IsKeyDown(Keys.Enter) || _prevPad.IsButtonDown(Buttons.A)))
            {
                switch (State)
                {
                    case TitleState.Menu:
                        _colors[_menuIndex] = Color.White;
                        switch (_menuIndex)
                        {
                            case 0:
                                _control.GoToLevel("Tutorial");
                                _control.InitLevel(false);

                                game.GameState = GameState.Game;
                                break;
                            case 1:
                                MenuSize = 6;
                                State = TitleState.Select;
                                break;
                            case 2:
                                MenuSize = 3;
                                State = TitleState.Options;
                                break;
                            case 3:
                                MenuSize = 1;
                                State = TitleState.Controls;
                                break;
                            case 4:
                                MenuSize = 1;
                                State = TitleState.Credits;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        _menuIndex = 0;
                        _colors[0] = Color.Yellow;
                        break;
                    case TitleState.Select:
                        _colors[_menuIndex] = Color.White;
                        switch (_menuIndex)
                        {
                            case 0:
                                _control.GoToLevel("Tutorial");
                                _control.InitLevel(false);
                                game.GameState = GameState.Game;
                                break;
                            case 1:
                                _control.GoToLevel("Level1");
                                _control.InitLevel(false);
                                game.GameState = GameState.Game;
                                break;
                            case 2:
                                _control.GoToLevel("Level2");
                                _control.InitLevel(false);
                                game.GameState = GameState.Game;
                                break;
                            case 3:
                                _control.GoToLevel("Level3");
                                _control.InitLevel(false);
                                game.GameState = GameState.Game;
                                break;
                            case 4:
                                _control.GoToLevel("Level4");
                                _control.InitLevel(false);
                                game.GameState = GameState.Game;
                                break;
                            case 5:
                                MenuSize = 5;
                                State = TitleState.Menu;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        _menuIndex = 0;
                        _colors[0] = Color.Yellow;
                        break;
                    case TitleState.Pause:
                        _colors[_menuIndex] = Color.White;
                        switch (_menuIndex)
                        {
                            case 0:
                                game.GameState = GameState.Game;
                                break;
                            case 1:
                                MenuSize = 5;
                                game.ResetGame();
                                State = TitleState.Menu;
                                break;
                            case 2:
                                MenuSize = 3;
                                State = TitleState.Options;
                                break;
                            case 3:
                                MenuSize = 1;
                                State = TitleState.Controls;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        _menuIndex = 0;
                        _colors[0] = Color.Yellow;
                        break;
                    case TitleState.Options:
                        switch (_menuIndex)
                        {
                            case 0:
                                _soundText = ToggleSound ? "OFF" : "ON";
                                ToggleSound = !ToggleSound;
                                break;
                            case 1:
                                if (_toggleMusic)
                                {
                                    _musicText = "OFF";
                                    MediaPlayer.Pause();
                                }
                                else
                                {
                                    _musicText = "ON";
                                    MediaPlayer.Resume();
                                }
                                _toggleMusic = !_toggleMusic;
                                break;
                            case 2:
                                _colors[_menuIndex] = Color.White;
                                _menuIndex = 0;
                                _colors[0] = Color.Yellow;
                                if (game.GameInProgress)
                                {
                                    MenuSize = 4;
                                    State = TitleState.Pause;
                                }
                                else
                                {
                                    MenuSize = 5;
                                    State = TitleState.Menu;
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case TitleState.Controls:
                        _colors[_menuIndex] = Color.White;
                        _menuIndex = 0;
                        _colors[_menuIndex] = Color.Yellow;
                        if (game.GameInProgress)
                        {
                            MenuSize = 4;
                            State = TitleState.Pause;
                        }
                        else
                        {
                            MenuSize = 5;
                            State = TitleState.Menu;
                        }
                        break;
                    case TitleState.Credits when !game.GameInProgress:
                        _colors[_menuIndex] = Color.White;
                        _menuIndex = 0;
                        MenuSize = 5;
                        _colors[_menuIndex] = Color.Yellow;
                        State = TitleState.Menu;
                        break;
                    case TitleState.Title:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _prevKeys = keys;
            _prevPad = padState;

        }

        public new void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            switch (State)
            {
                case TitleState.Menu:
                    spriteBatch.DrawString(Game1.MenuFont, "New Game", _choice1, _colors[0]);
                    spriteBatch.DrawString(Game1.MenuFont, "Select Level", _choice2, _colors[1]);
                    spriteBatch.DrawString(Game1.MenuFont, "Options", _choice3, _colors[2]);
                    spriteBatch.DrawString(Game1.MenuFont, "Controls", _choice4, _colors[3]);
                    spriteBatch.DrawString(Game1.MenuFont, "Credits", _choice5, _colors[4]);
                    break;
                case TitleState.Select:
                    spriteBatch.DrawString(Game1.MenuFont, "Tutorial", _choice1, _colors[0]);
                    spriteBatch.DrawString(Game1.MenuFont, "Level 1", _choice2, _colors[1]);
                    spriteBatch.DrawString(Game1.MenuFont, "Level 2", _choice3, _colors[2]);
                    spriteBatch.DrawString(Game1.MenuFont, "Level 3", _choice4, _colors[3]);
                    spriteBatch.DrawString(Game1.MenuFont, "Level 4", _choice5, _colors[4]);
                    spriteBatch.DrawString(Game1.MenuFont, "Back", _choice6, _colors[5]);
                    break;
                case TitleState.Pause:
                    spriteBatch.DrawString(Game1.MenuFont, "Resume Game", _choice1, _colors[0]);
                    spriteBatch.DrawString(Game1.MenuFont, "Main Menu", _choice2, _colors[1]);
                    spriteBatch.DrawString(Game1.MenuFont, "Options", _choice3, _colors[2]);
                    spriteBatch.DrawString(Game1.MenuFont, "Controls", _choice4, _colors[3]);
                    break;
                case TitleState.Options:
                    //TODO: FIX OPTIONS
                    spriteBatch.DrawString(Game1.MenuFont, "Toggle Sound: " + _soundText, _choice1, _colors[0]);
                    spriteBatch.DrawString(Game1.MenuFont, "Toggle Music: " + _musicText, _choice2, _colors[1]);
                    spriteBatch.DrawString(Game1.MenuFont, "Back", _choice3, _colors[2]);
                    break;
                case TitleState.Controls:
                    spriteBatch.DrawString(Game1.MenuFont, "Back", _choice6, _colors[0]);
                    break;
                case TitleState.Credits:
                    spriteBatch.DrawString(Game1.MenuFont, "Sam Assaf", new Vector2(180, 200), Color.White);
                    spriteBatch.DrawString(Game1.MenuFont, "Alex Attar", new Vector2(180, 260), Color.White);
                    spriteBatch.DrawString(Game1.MenuFont, "David Campbell", new Vector2(180, 320), Color.White);
                    spriteBatch.DrawString(Game1.MenuFont, "Ralph D'Almeida", new Vector2(180, 380), Color.White);

                    spriteBatch.DrawString(Game1.MenuFont, "Back", _choice6, _colors[0]);
                    break;
                case TitleState.Title:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        # endregion
    }
}
