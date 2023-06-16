using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Paranothing;

sealed class GameTitle : GameBackground
{
    internal static bool ToggleSound = true;

    internal int MenuSize = 5;
    internal TitleState State = TitleState.Title;

    readonly Color[] _colors = { Color.Yellow, Color.White, Color.White, Color.White, Color.White, Color.White };
    readonly GameController _control = GameController.Instance;
    readonly Vector2 _choice1 = new(750, 300),
        _choice2 = new(750, 360),
        _choice3 = new(750, 420),
        _choice4 = new(750, 480),
        _choice5 = new(750, 540),
        _choice6 = new(750, 600);

    int _menuIndex;
    GamePadState _previousGamePadState;
    KeyboardState _previousKeyboardState;
    bool _toggleMusic = true;
    Rectangle _topTextRect,
        _bottomTextRect;

    string _soundText = "ON",
        _musicText = "ON";

    internal GameTitle(Texture2D texture, Rectangle rectangle)
        : base(texture, rectangle)
    {
    }

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

    internal Rectangle TopTextRectangle => _topTextRect;

    internal Rectangle BottomTextRectangle => _bottomTextRect;

    public new void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        switch (State)
        {
            case TitleState.Menu:
                spriteBatch.DrawString(ParanothingGame.MenuFont, "New Game", _choice1, _colors[0]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Select Level", _choice2, _colors[1]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Options", _choice3, _colors[2]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Controls", _choice4, _colors[3]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Credits", _choice5, _colors[4]);
                break;
            case TitleState.Select:
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Tutorial", _choice1, _colors[0]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Level 1", _choice2, _colors[1]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Level 2", _choice3, _colors[2]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Level 3", _choice4, _colors[3]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Level 4", _choice5, _colors[4]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Back", _choice6, _colors[5]);
                break;
            case TitleState.Pause:
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Resume Game", _choice1, _colors[0]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Main Menu", _choice2, _colors[1]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Options", _choice3, _colors[2]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Controls", _choice4, _colors[3]);
                break;
            case TitleState.Options:
                //TODO: FIX OPTIONS
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Toggle Sound: " + _soundText, _choice1, _colors[0]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Toggle Music: " + _musicText, _choice2, _colors[1]);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Back", _choice3, _colors[2]);
                break;
            case TitleState.Controls:
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Back", _choice6, _colors[0]);
                break;
            case TitleState.Credits:
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Sam Assaf", new(180, 200), Color.White);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Alex Attar", new(180, 260), Color.White);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "David Campbell", new(180, 320), Color.White);
                spriteBatch.DrawString(ParanothingGame.MenuFont, "Ralph D'Almeida", new(180, 380), Color.White);

                spriteBatch.DrawString(ParanothingGame.MenuFont, "Back", _choice6, _colors[0]);
                break;
        }
    }

    public void Update(ParanothingGame game)
    {
        var gamePadState = GamePad.GetState(PlayerIndex.One);
        var keyboardState = Keyboard.GetState();

        if ((keyboardState.IsKeyDown(Keys.Enter) || gamePadState.IsButtonDown(Buttons.Start)) &&
            State == TitleState.Title)
            State = TitleState.Menu;

        else if ((gamePadState.IsButtonDown(Buttons.LeftThumbstickDown) || keyboardState.IsKeyDown(Keys.Down)) &&
                 !(_previousGamePadState.IsButtonDown(Buttons.LeftThumbstickDown) ||
                   _previousKeyboardState.IsKeyDown(Keys.Down)) &&
                 _menuIndex < MenuSize - 1)
        {
            _colors[_menuIndex++] = Color.White;
            _colors[_menuIndex] = Color.Yellow;
        }

        else if ((gamePadState.IsButtonDown(Buttons.LeftThumbstickUp) || keyboardState.IsKeyDown(Keys.Up)) &&
                 !(_previousGamePadState.IsButtonDown(Buttons.LeftThumbstickUp) ||
                   _previousKeyboardState.IsKeyDown(Keys.Up)) &&
                 _menuIndex > 0)
        {
            _colors[_menuIndex--] = Color.White;
            _colors[_menuIndex] = Color.Yellow;
        }

        else if ((keyboardState.IsKeyDown(Keys.Enter) || gamePadState.IsButtonDown(Buttons.A)) &&
                 !(_previousKeyboardState.IsKeyDown(Keys.Enter) || _previousGamePadState.IsButtonDown(Buttons.A)))
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
                            game.ResetGame();
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
                            game.ResetGame();
                            break;
                        case 1:
                            _control.GoToLevel("Level1");
                            _control.InitLevel(false);
                            game.GameState = GameState.Game;
                            game.ResetGame();
                            break;
                        case 2:
                            _control.GoToLevel("Level2");
                            _control.InitLevel(false);
                            game.GameState = GameState.Game;
                            game.ResetGame();
                            break;
                        case 3:
                            _control.GoToLevel("Level3");
                            _control.InitLevel(false);
                            game.GameState = GameState.Game;
                            game.ResetGame();
                            break;
                        case 4:
                            _control.GoToLevel("Level4");
                            _control.InitLevel(false);
                            game.GameState = GameState.Game;
                            game.ResetGame();
                            break;
                        case 5:
                            MenuSize = 5;
                            State = TitleState.Menu;
                            break;
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
            }

        _previousKeyboardState = keyboardState;
        _previousGamePadState = gamePadState;
    }

    internal void SetBottomTextRectangle(Vector2 inVec)
    {
        var (x, y) = inVec;
        _bottomTextRect.X = BackgoundRectangle.Center.X - (int)(x / 2);
        _bottomTextRect.Y = BackgoundRectangle.Bottom - (int)y;
        _bottomTextRect.Width = (int)x;
        _bottomTextRect.Height = (int)y;
    }

    internal void SetTopTextRectangle(Vector2 inVec)
    {
        var (x, y) = inVec;

        _topTextRect.X = BackgoundRectangle.Center.X - (int)(x / 2);
        _topTextRect.Y = BackgoundRectangle.Top;
        _topTextRect.Width = (int)x;
        _topTextRect.Height = (int)y;
    }
}