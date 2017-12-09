using System.Collections.Generic;

namespace Paranothing
{
    internal sealed class SpriteSheetManager
    {
        private readonly Dictionary<string, SpriteSheet> _sheetDict;
        private static SpriteSheetManager _instance;

        public static SpriteSheetManager GetInstance()
        {
            return _instance ?? (_instance = new SpriteSheetManager());
        }

        private SpriteSheetManager()
        {
            _sheetDict = new Dictionary<string, SpriteSheet>();
        }

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
