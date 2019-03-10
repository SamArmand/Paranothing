using System.Collections.Generic;

namespace Paranothing
{
	sealed class SpriteSheetManager
	{
		readonly Dictionary<string, SpriteSheet> _spriteSheets;
		static SpriteSheetManager _instance;

		internal static SpriteSheetManager GetInstance() => _instance ?? (_instance = new SpriteSheetManager());

		SpriteSheetManager() => _spriteSheets = new Dictionary<string, SpriteSheet>();

		internal SpriteSheet GetSheet(string name)
		{
			SpriteSheet sheet;
			if (_spriteSheets.ContainsKey(name))
				_spriteSheets.TryGetValue(name, out sheet);
			else
				sheet = null;
			return sheet;
		}

		internal void AddSheet(string name, SpriteSheet sheet)
		{
			if (!_spriteSheets.ContainsKey(name))
				_spriteSheets.Add(name, sheet);
		}
	}
}