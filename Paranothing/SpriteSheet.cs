using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
	/// <summary>
	/// A Spritesheet utility class
	/// </summary>
	sealed class SpriteSheet
	{
		readonly Dictionary<string, List<int>> _animations = new Dictionary<string, List<int>>();
		readonly List<Rectangle> _sprites = new List<Rectangle>();
		internal readonly Texture2D Image;

		internal SpriteSheet(Texture2D image) => Image = image;

		/// <summary>
		/// Define a sprite within the spritesheet.
		/// </summary>
		/// <param name="x">X position of the sprite</param>
		/// <param name="y">Y position of the sprite</param>
		/// <param name="width">Width of the sprite</param>
		/// <param name="height">Height of the sprite</param>
		/// <returns></returns>
		internal void AddSprite(int x, int y, int width, int height) =>
			_sprites.Add(new Rectangle(x, y, width, height));

		/// <summary>
		/// Divides the sprite sheet into a grid with cells of equal size. Good for uniform sheets.
		/// </summary>
		/// <param name="rows">The number of rows in the sprite sheet</param>
		/// <param name="columns">The number of columns in the sprite sheet</param>
		/// <param name="padX">Amount of horizontal padding between sprites</param>
		/// <param name="padY">Amount of vertical padding between sprites</param>
		/// <param name="limit">The maximum number of sprites in the sheet. 0 for no maximum.</param>
		internal void SplitSheet(int rows, int columns, int padX = 0, int padY = 0, int limit = 0)
		{
			if (rows <= 0 || columns <= 0)
				return;

			var width = (int) Math.Floor((float) Image.Width / columns);
			var height = (int) Math.Floor((float) Image.Height / rows);
			var count = 0;
			for (var row = 0; row != rows; ++row)
				for (var col = 0; col != columns; ++col)
					if (count++ < limit || limit <= 0)
						_sprites.Add(new Rectangle((width + padX) * col, (height + padY) * row, width, height));
					else
						return;
		}

		/// <summary>
		/// Define an animation within the sprite sheet.
		/// </summary>
		/// <param name="name">The name of the animation (must be unique to the sprite sheet).</param>
		/// <param name="spriteIndices">An array of the indices of the sprites in the animation.</param>
		/// <returns>True if adding was successful, false otherwise.</returns>
		internal void AddAnimation(string name, IEnumerable<int> spriteIndices)
		{
			name = name.ToLower();
			if (_animations.ContainsKey(name)) return;

			_animations.Add(name, spriteIndices.ToList());
		}

		/// <summary>
		/// Gets a list of sprite indices in an animation.
		/// </summary>
		/// <param name="name">The name of the animation.</param>
		/// <returns>A list containing the indices of the sprites in the animation. If the animation does not exist, the list will be empty.</returns>
		internal List<int> GetAnimation(string name)
		{
			name = name.ToLower();
			var animation = new List<int>();
			if (_animations.ContainsKey(name))
				_animations.TryGetValue(name, out animation);
			return animation;
		}

		/// <summary>
		/// Returns whether or not the spritesheet has an animation.
		/// </summary>
		/// <param name="name">The name of the animation to search for.</param>
		/// <returns>True if the animation exists, false otherwise.</returns>
		internal bool HasAnimation(string name) => _animations.ContainsKey(name.ToLower());

		/// <summary>
		/// Gets a rectangle representing a sprite in the spritesheet with the specified index.
		/// </summary>
		/// <param name="index">The index of the sprite to look for.</param>
		/// <returns>A Rectangle representing the position and dimension of the sprite within the sheet.</returns>
		internal Rectangle GetSprite(int index) =>
			index < 0 || index >= _sprites.Count ? new Rectangle() : _sprites.ElementAt(index);
	}
}