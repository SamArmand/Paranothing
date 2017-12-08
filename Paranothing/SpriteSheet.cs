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
    public class SpriteSheet
    {
        private Dictionary<String, List<int>> animList;
        private List<Rectangle> sprites;
        public Texture2D image;

        public SpriteSheet(Texture2D image)
        {
            this.image = image;
            animList = new Dictionary<string, List<int>>();
            sprites = new List<Rectangle>();
        }

        /// <summary>
        /// Define a sprite within the spritesheet.
        /// </summary>
        /// <param name="bounds">A Rectangle specifying the position and dimensions of the sprite on the sheet.</param>
        /// <returns>The index of the sprite added.</returns>
        public int addSprite(Rectangle bounds)
        {
            sprites.Add(bounds);
            return sprites.Count();
        }

        /// <summary>
        /// Define a sprite within the spritesheet.
        /// </summary>
        /// <param name="x">X position of the sprite</param>
        /// <param name="y">Y position of the sprite</param>
        /// <param name="width">Width of the sprite</param>
        /// <param name="height">Height of the sprite</param>
        /// <returns></returns>
        public int addSprite(int x, int y, int width, int height)
        {
            return addSprite(new Rectangle(x, y, width, height));
        }

        /// <summary>
        /// Divides the sprite sheet into a grid with cells of equal size. Good for uniform sheets.
        /// </summary>
        /// <param name="rows">The number of rows in the sprite sheet</param>
        /// <param name="columns">The number of columns in the sprite sheet</param>
        public void splitSheet(int rows, int columns)
        {
            splitSheet(rows, columns, 0, 0, 0);
        }

        /// <summary>
        /// Divides the sprite sheet into a grid with cells of equal size. Good for uniform sheets.
        /// </summary>
        /// <param name="rows">The number of rows in the sprite sheet</param>
        /// <param name="columns">The number of columns in the sprite sheet</param>
        /// <param name="padX">Amount of horizontal padding between sprites</param>
        /// <param name="padY">Amount of vertical padding between sprites</param>
        public void splitSheet(int rows, int columns, int padX, int padY)
        {
            splitSheet(rows, columns, padX, padY, 0); 
        }

        /// <summary>
        /// Divides the sprite sheet into a grid with cells of equal size. Good for uniform sheets.
        /// </summary>
        /// <param name="rows">The number of rows in the sprite sheet</param>
        /// <param name="columns">The number of columns in the sprite sheet</param>
        /// <param name="padX">Amount of horizontal padding between sprites</param>
        /// <param name="padY">Amount of vertical padding between sprites</param>
        /// <param name="limit">The maximum number of sprites in the sheet. 0 for no maximum.</param>
        public void splitSheet(int rows, int columns, int padX, int padY, int limit)
        {
            if (rows <= 0 || columns <= 0)
                return;
            int width = (int)Math.Floor((float)image.Width / columns);
            int height = (int)Math.Floor((float)image.Height / rows);
            int count = 0;
            for (int row = 0; row != rows; row++)
            {
                for (int col = 0; col != columns; col++)
                {
                    if (count < limit || limit <= 0)
                    {
                        sprites.Add(new Rectangle((width + padX) * col, (height + padY) * row, width, height));
                        count++;
                    }
                    else return;
                }
            }
        }

        /// <summary>
        /// Define an animation within the sprite sheet.
        /// </summary>
        /// <param name="name">The name of the animation (must be unique to the sprite sheet).</param>
        /// <param name="spriteIndices">A List of the indices of the sprites in the animation.</param>
        /// <returns>True if adding was successful, false otherwise.</returns>
        public bool addAnimation(String name, List<int> spriteIndices)
        {
            name = name.ToLower();
            if (animList.ContainsKey(name))
                return false;
            animList.Add(name, spriteIndices);
            return true;
        }

        /// <summary>
        /// Define an animation within the sprite sheet.
        /// </summary>
        /// <param name="name">The name of the animation (must be unique to the sprite sheet).</param>
        /// <param name="spriteIndices">An array of the indices of the sprites in the animation.</param>
        /// <returns>True if adding was successful, false otherwise.</returns>
        public bool addAnimation(String name, int[] spriteIndices)
        {
            name = name.ToLower();
            List<int> indexList = new List<int>();
            foreach (int i in spriteIndices)
            {
                indexList.Add(i);
            }
            return addAnimation(name, indexList);
        }

        /// <summary>
        /// Gets a list of sprite indices in an animation.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <returns>A list containing the indices of the sprites in the animation. If the animation does not exist, the list will be empty.</returns>
        public List<int> getAnimation(String name)
        {
            name = name.ToLower();
            List<int> animation = new List<int>();
            if (animList.ContainsKey(name))
                animList.TryGetValue(name, out animation);
            return animation;
        }

        /// <summary>
        /// Returns whether or not the spritesheet has an animation.
        /// </summary>
        /// <param name="name">The name of the animation to search for.</param>
        /// <returns>True if the animation exists, false otherwise.</returns>
        public bool hasAnimation(String name)
        {
            name = name.ToLower();
            return animList.ContainsKey(name);
        }

        /// <summary>
        /// Gets a list of the animations in the spritesheet.
        /// </summary>
        /// <returns>A List of the animations in the spritesheet.</returns>
        public List<String> getAnimList()
        {
            Dictionary<string, List<int>>.KeyCollection keys = animList.Keys;
            return keys.ToList();
        }

        /// <summary>
        /// Gets a rectangle representing a sprite in the spritesheet with the specified index.
        /// </summary>
        /// <param name="index">The index of the sprite to look for.</param>
        /// <returns>A Rectangle representing the position and dimension of the sprite within the sheet.</returns>
        public Rectangle getSprite(int index)
        {
            if (index < 0 || index >= sprites.Count)
                return new Rectangle();
            return sprites.ElementAt(index);
        }
    }

}
