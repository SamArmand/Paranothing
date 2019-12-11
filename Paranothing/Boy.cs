using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing
{
	sealed class Boy : IDrawable, IUpdatable, ICollideable
	{
		readonly GameController _gameController = GameController.GetInstance();
		readonly SoundManager _soundManager = SoundManager.Instance();
		readonly SpriteSheet _sheet = SpriteSheetManager.GetInstance().GetSheet("boy");
		int _frame;
		int _frameLength = 60;
		int _frameTime;
		string _animName;
		List<int> _animFrames;

		string Animation
		{
			get => _animName;
			set
			{
				if (!_sheet.HasAnimation(value) || _animName == value) return;

				_animName = value;
				_animFrames = _sheet.GetAnimation(_animName);
				_frame = 0;
				_frameTime = 0;
			}
		}

		float _drawLayer = DrawLayer.Player;
		float _moveSpeedX, _moveSpeedY; // Pixels per animation frame
		Vector2 _position;
		internal int Width = 38;
		int _height = 58;

		internal float X
		{
			get => _position.X;
			set => _position.X = value;
		}

		internal float Y
		{
			get => _position.Y;
			set => _position.Y = value;
		}

		internal enum BoyState
		{
			Idle,
			Walk,
			StairsLeft,
			StairsRight,
			PushWalk,
			PushingStill,
			Teleport,
			TimeTravel,
			ControllingChair,
			Die
		}

		internal BoyState State = BoyState.Idle;
		internal Direction Direction { get; set; } = Direction.Right;
		internal ActionBubble ActionBubble { get; }
		Vector2 _teleportTo;
		TimePeriod _timeTravelTo;
		internal Chair NearestChair { get; set; }
		internal IInteractable Interactor;

		internal Boy(float x, float y, ActionBubble actionBubble)
		{
			_position = new Vector2(x, y);
			Animation = "stand";
			ActionBubble = actionBubble;
			ActionBubble.Boy = this;
			ActionBubble.Show();
		}

		internal void Reset()
		{
			_frame = 0;
			_frameTime = 0;
			_frameLength = 60;
			_position = new Vector2(X, Y);
			Width = 38;
			_height = 58;
			State = BoyState.Idle;
			Animation = "stand";
			Direction = Direction.Right;
			ActionBubble.Boy = this;
			ActionBubble.Show();
			_teleportTo = new Vector2();
			NearestChair = null;
		}

		public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, _position, _sheet.GetSprite(_animFrames.ElementAt(_frame)), tint, 0f, new Vector2(), 1f, Direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, _drawLayer);

		void CheckInput(GameController control)
		{
			if (State == BoyState.Die) return;

			if (control.KeyState.IsKeyDown(Keys.Space) || control.PadState.IsButtonDown(Buttons.A))
			{
				if ((State == BoyState.Walk || State == BoyState.Idle) && control.KeyState.IsKeyUp(Keys.Right) &&
					control.KeyState.IsKeyUp(Keys.Left)
				 && control.PadState.IsButtonUp(Buttons.LeftThumbstickRight) &&
					control.PadState.IsButtonUp(Buttons.LeftThumbstickLeft) || State == BoyState.PushWalk)
					Interactor?.Interact();
			}
			else if ((State == BoyState.PushingStill || State == BoyState.PushWalk) && Interactor != null) State = BoyState.Idle;

			if (control.KeyState.IsKeyDown(Keys.LeftShift) || control.PadState.IsButtonDown(Buttons.RightTrigger))
			{
				if (NearestChair != null)
				{
					State = BoyState.ControllingChair;
					NearestChair.State = Chair.ChairsState.Moving;
				}
			}
			else
			{
				if (NearestChair != null && NearestChair.State == Chair.ChairsState.Moving)
				{
					NearestChair.State = Chair.ChairsState.Falling;
					State = BoyState.Idle;
				}
			}

			if (control.KeyState.IsKeyUp(Keys.Left) && control.KeyState.IsKeyUp(Keys.Right)
													&& control.PadState.IsButtonUp(Buttons.LeftThumbstickLeft) &&
													   control.PadState.IsButtonUp(Buttons.LeftThumbstickRight)
													&& State != BoyState.Teleport && State != BoyState.TimeTravel)
			{
				if (State != BoyState.ControllingChair)
				{
					if (Direction == Direction.Right)
					{
						if (State != BoyState.StairsRight && State != BoyState.StairsLeft)
						{
							if ((State == BoyState.PushWalk || State == BoyState.PushingStill) &&
								Interactor != null)
								State = BoyState.PushingStill;
							else
								State = BoyState.Idle;
						}
					}
					else
					{
						if (State != BoyState.StairsRight && State != BoyState.StairsLeft)
						{
							if ((State == BoyState.PushWalk || State == BoyState.PushingStill) &&
								Interactor != null)
								State = BoyState.PushingStill;
							else
								State = BoyState.Idle;
						}
					}
				}
			}
			else
			{
				if (State != BoyState.ControllingChair)
				{
					if (control.KeyState.IsKeyDown(Keys.Right) ||
						control.PadState.IsButtonDown(Buttons.LeftThumbstickRight))
					{
						if (State != BoyState.PushWalk && State != BoyState.PushingStill ||
							Interactor != null && ((Wardrobe)Interactor).X > X)
							Direction = Direction.Right;
						if (State == BoyState.Idle)
							State = BoyState.Walk;
						if (State == BoyState.PushingStill && Direction == Direction.Right && Interactor != null)
							State = BoyState.PushWalk;
					}
					else if (control.KeyState.IsKeyDown(Keys.Left) ||
							 control.PadState.IsButtonDown(Buttons.LeftThumbstickLeft))
					{
						if (State != BoyState.PushWalk && State != BoyState.PushingStill ||
							Interactor != null && ((Wardrobe)Interactor).X < X)
							Direction = Direction.Left;
						if (State == BoyState.Idle)
							State = BoyState.Walk;
						if (State == BoyState.PushingStill && Direction == Direction.Left && Interactor != null)
							State = BoyState.PushWalk;
					}
				}
			}

			if (State != BoyState.ControllingChair) return;

			if (NearestChair != null && NearestChair.State == Chair.ChairsState.Moving)
			{
				if (control.KeyState.IsKeyDown(Keys.Right) ||
					control.PadState.IsButtonDown(Buttons.LeftThumbstickRight))
					NearestChair.Move(Direction.Right);
				else if (control.KeyState.IsKeyDown(Keys.Left) ||
						 control.PadState.IsButtonDown(Buttons.LeftThumbstickLeft))
					NearestChair.Move(Direction.Left);

				if (control.KeyState.IsKeyDown(Keys.Up) ||
					control.PadState.IsButtonDown(Buttons.LeftThumbstickUp))
					NearestChair.Move(Direction.Up);
				else if (control.KeyState.IsKeyDown(Keys.Down) ||
						 control.PadState.IsButtonDown(Buttons.LeftThumbstickDown))
					NearestChair.Move(Direction.Down);
			}
			else
				State = BoyState.Idle;
		}

		public void Update(GameTime time)
		{
			var elapsed = time.ElapsedGameTime.Milliseconds;
			_frameTime += elapsed;
			CheckInput(_gameController);
			_drawLayer = DrawLayer.Player;

			switch (State)
			{
				case BoyState.Idle:
					if (Animation == "pushstill" || Animation == "startpush" || Animation == "push")
					{
						Animation = "endpush";

						_soundManager.StopSound("Pushing Wardrobe");
					}

					if (Animation == "control" || Animation == "controlstart") Animation = "controlend";

					if (Animation == "endpush" || (Animation == "controlend" && _frame == 2) || Animation == "walk")
					{
						Animation = "stand";

						_soundManager.StopSound("Pushing Wardrobe");
					}

					_moveSpeedX = 0;
					_moveSpeedY = 0;
					break;
				case BoyState.Walk:
					Animation = "walk";
					_moveSpeedX = 3;
					_moveSpeedY = 0;
					break;
				case BoyState.StairsLeft:
					Animation = "walk";
					_moveSpeedX = 3;
					_moveSpeedY = 2;
					_drawLayer = DrawLayer.PlayerBehindStairs;
					break;
				case BoyState.StairsRight:
					Animation = "walk";
					_moveSpeedX = 3;
					_moveSpeedY = -2;
					_drawLayer = DrawLayer.PlayerBehindStairs;
					break;
				case BoyState.PushingStill:
					_soundManager.StopSound("Pushing Wardrobe");

					_moveSpeedX = 0;
					_moveSpeedY = 0;
					if (Animation == "walk" || Animation == "stand")
						Animation = "startpush";
					if (Animation == "startpush" && _frame == 3 || Animation == "push")
						Animation = "pushstill";
					break;
				case BoyState.PushWalk:
					_moveSpeedY = 0;
					if (Animation == "walk" || Animation == "stand")
					{
						_moveSpeedX = 0;
						Animation = "startpush";
					}

					if (Animation == "startpush" && _frame == 3 || Animation == "pushstill") Animation = "push";

					if (Animation == "push")
					{
						_moveSpeedX = 3;
						_soundManager.PlaySound("Pushing Wardrobe", true);
					}

					break;

				case BoyState.Teleport:
					_moveSpeedX = 0;
					_moveSpeedY = 0;
					if (Animation == "walk" || Animation == "stand")
					{
						Animation = "enterwardrobe";
						var targetWr = ((Wardrobe)Interactor).GetLinkedWr();
						if (targetWr != null)
						{
							var (x, y, _, _) = targetWr.GetBounds();
							_teleportTo = new Vector2(x + 16, y + 24);
							Interactor = null;
						}
					}

					if (Animation == "enterwardrobe" && _frame == 6)
					{
						_position = new Vector2(_teleportTo.X, _teleportTo.Y);
						Animation = "leavewardrobe";
					}

					if (Animation == "leavewardrobe" && _frame == 7)
					{
						Animation = "stand";
						State = BoyState.Idle;
					}

					break;
				case BoyState.TimeTravel:
					_moveSpeedX = 0;
					_moveSpeedY = 0;
					if (Animation == "walk" || Animation == "stand")
					{
						var p = (Portrait)Interactor;
						_teleportTo = p.WasMoved ? p.MovedPos : new Vector2();
						Animation = "enterportrait";
						if (_gameController.TimePeriod == TimePeriod.Present)
							_timeTravelTo = ((Portrait)Interactor).SendTime;
						Interactor = null;
					}

					if (Animation == "enterportrait" && _frame == 7)
					{
						_gameController.TimePeriod = _gameController.TimePeriod != TimePeriod.Present ? TimePeriod.Present : _timeTravelTo;
						Animation = "leaveportrait";
						if (Math.Abs(_teleportTo.X) > 0 && Math.Abs(_teleportTo.Y) > 0)
						{
							X = _teleportTo.X;
							Y = _teleportTo.Y;
						}
					}

					if (Animation == "leaveportrait" && _frame == 7)
					{
						Animation = "stand";
						State = BoyState.Idle;
					}

					break;
				case BoyState.ControllingChair:
					_moveSpeedX = 0;
					_moveSpeedY = 0;
					if (Animation != "control") Animation = "controlstart";

					if (Animation == "controlstart" && _frame == 2)
						Animation = "control";
					break;
				case BoyState.Die:
					_moveSpeedX = 0;
					_moveSpeedY = 0;
					Animation = "disappear";
					if (_frame == 7)
					{
						Reset();
						_gameController.ResetLevel();
					}

					break;
			}

			if (_frameTime < _frameLength) return;

			var flip = Direction == Direction.Left ? -1 : 1;
			X += _moveSpeedX * flip;
			if (State == BoyState.PushWalk && Animation == "push" && Interactor != null)
			{
				var w = (Wardrobe)Interactor;
				if (!_gameController.CollidingWithSolid(w.PushBox, false))
					w.X += (int)(_moveSpeedX * flip);
				else
					X -= _moveSpeedX * flip;
			}

			if (Math.Abs(_moveSpeedY) <= 0)
			{
				_moveSpeedY = 1;
				flip = 1;
			}

			Y += _moveSpeedY * flip;
			_frameTime = 0;
			_frame = (_frame + 1) % _animFrames.Count;
		}

		public Rectangle GetBounds() => new Rectangle((int)_position.X, (int)_position.Y, Width, _height);

		public bool IsSolid() => true;
	}
}