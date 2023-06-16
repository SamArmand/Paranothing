using System.Collections.Generic;

namespace Paranothing;

/// <summary>
///     A singleton class to manage sprite sheets.
/// </summary>
sealed class SpriteSheetManager
{
    /// <summary>
    ///     The instance of the singleton.
    /// </summary>
    static SpriteSheetManager _instance;

    /// <summary>
    ///     A dictionary containing the game's sprite sheets.
    /// </summary>
    readonly Dictionary<string, SpriteSheet> _spriteSheets = new();

    /// <summary>
    ///     Creates and/or returns the instance of the singleton.
    /// </summary>
    /// <value>The instance of the singleton.</value>
    internal static SpriteSheetManager Instance => _instance ??= new();

    /// <summary>
    ///     Adds a sprite sheet to the dictionary of sprite sheets.
    /// </summary>
    /// <param name="name">The name of the sprite sheet.</param>
    /// <param name="sheet">The sprite sheet that is being added.</param>
    internal void AddSheet(string name, SpriteSheet sheet) => _spriteSheets.TryAdd(name, sheet);

    /// <summary>
    ///     Gets a sprite sheet by its name.
    /// </summary>
    /// <param name="name">The name of the sprite sheet to retrieve.</param>
    /// <returns>The sprite sheet with that name.</returns>
    internal SpriteSheet GetSheet(string name) => _spriteSheets.TryGetValue(name, out var sheet) ? sheet : null;
}