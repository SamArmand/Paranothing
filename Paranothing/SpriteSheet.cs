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
        readonly Dictionary<string, List<int>> _animList;
        readonly List<Rectangle> _sprites;
        internal readonly Texture2D Image;

        internal SpriteSheet(Texture2D image)
        {
            Image = image;
            _animList = new Dictionary<string, List<int>>();
            _sprites = new List<Rectangle>();
        }

        /// <summary>
        /// Define a sprite within the spritesheet.
        /// </summary>
        /// <param name="x">X position of the sprite</param>
        /// <param name="y">Y position of the sprite</param>
        /// <param name="width">Width of the sprite</param>
        /// <param name="height">Height of the sprite</param>
        /// <returns></returns>
        internal void AddSprite(int x, int y, int width, int height) => _sprites.Add(new Rectangle(x, y, width, height));

        /// <summary>
        /// Divides the sprite sheet into a grid with cells of equal size. Good for uniform sheets.
        /// </summary>
        /// <param name="rows">The number of rows in the sprite sheet</param>
        /// <param name="columns">The number of columns in the sprite sheet</param>
        /// <param name="padX">Amount of horizontal padding between sprites</param>
        /// <param name="padY">Amount of vertical padding between sprites</param>
        /// <param name="limit">The maximum number of sprites in the sheet. 0 for no maximum.</param>
        public void SplitSheet(int rows, int columns, int padX = 0, int padY = 0, int limit = 0)
        {
            if (rows <= 0 || columns <= 0)
                return;

            var width = (int)Math.Floor((float)Image.Width / columns);
            var height = (int)Math.Floor((float)Image.Height / rows);
            var count = 0;
            for (var row = 0; row != rows; row++)
                for (var col = 0; col != columns; col++)
                    if (count++ < limit || limit <= 0)
                        _sprites.Add(new Rectangle((width + padX) * col, (height + padY) * row, width, height));
                    else
                        return;
        }

        /// <summary>
        /// Define an animation within the sprite sheet.
        /// </summary>
        /// <param name="name">The name of the animation (must be unique to the sprite sheet).</param>
        /// <param name="spriteIndices">A List of the indices of the sprites in the animation.</param>
        /// <returns>True if adding was successful, false otherwise.</returns>
        void AddAnimation(string name, List<int> spriteIndices)
        {
            name = name.ToLower();
            if (_animList.ContainsKey(name)) return;

            _animList.Add(name, spriteIndices);
        }

        /// <summary>
        /// Define an animation within the sprite sheet.
        /// </summary>
        /// <param name="name">The name of the animation (must be unique to the sprite sheet).</param>
        /// <param name="spriteIndices">An array of the indices of the sprites in the animation.</param>
        /// <returns>True if adding was successful, false otherwise.</returns>
        public void AddAnimation(string name, IEnumerable<int> spriteIndices) => AddAnimation(name.ToLower(), spriteIndices.ToList());

        /// <summary>
        /// Gets a list of sprite indices in an animation.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <returns>A list containing the indices of the sprites in the animation. If the animation does not exist, the list will be empty.</returns>
        public List<int> GetAnimation(string name)
        {
            name = name.ToLower();
            var animation = new List<int>();
            if (_animList.ContainsKey(name))
                _animList.TryGetValue(name, out animation);
            return animation;
        }

        /// <summary>
        /// Returns whether or not the spritesheet has an animation.
        /// </summary>
        /// <param name="name">The name of the animation to search for.</param>
        /// <returns>True if the animation exists, false otherwise.</returns>
        public bool HasAnimation(string name)
        {
            name = name.ToLower();
            return _animList.ContainsKey(name);
        }

        /// <summary>
        /// Gets a rectangle representing a sprite in the spritesheet with the specified index.
        /// </summary>
        /// <param name="index">The index of the sprite to look for.</param>
        /// <returns>A Rectangle representing the position and dimension of the sprite within the sheet.</returns>
        public Rectangle GetSprite(int index)
        {
            if (index < 0 || index >= _sprites.Count)
                return new Rectangle();

            return _sprites.ElementAt(index);
        }
    }

}
