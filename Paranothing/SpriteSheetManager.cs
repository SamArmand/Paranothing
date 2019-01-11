using System.Collections.Generic;

namespace Paranothing
{
    sealed class SpriteSheetManager
    {
        readonly Dictionary<string, SpriteSheet> _sheetDict;
        static SpriteSheetManager _instance;

        public static SpriteSheetManager GetInstance() => _instance ?? (_instance = new SpriteSheetManager());

        SpriteSheetManager() => _sheetDict = new Dictionary<string, SpriteSheet>();

        public SpriteSheet GetSheet(string name)
        {
            SpriteSheet sheet;
            if (_sheetDict.ContainsKey(name))
                _sheetDict.TryGetValue(name, out sheet);
            else
                sheet = null;
            return sheet;
        }

        public void AddSheet(string name, SpriteSheet sheet)
        {
            if (!_sheetDict.ContainsKey(name))
                _sheetDict.Add(name, sheet);
        }
    }
}
