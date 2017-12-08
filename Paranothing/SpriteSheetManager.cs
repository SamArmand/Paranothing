using System.Collections.Generic;

namespace Paranothing
{
    class SpriteSheetManager
    {
        private Dictionary<string, SpriteSheet> sheetDict;
        private static SpriteSheetManager instance;

        public static SpriteSheetManager getInstance()
        {
            if (instance == null)
                instance = new SpriteSheetManager();
            return instance;
        }

        private SpriteSheetManager()
        {
            sheetDict = new Dictionary<string, SpriteSheet>();
        }

        public SpriteSheet getSheet(string name)
        {
            SpriteSheet sheet;
            if (sheetDict.ContainsKey(name))
                sheetDict.TryGetValue(name, out sheet);
            else
                sheet = null;
            return sheet;
        }

        public void addSheet(string name, SpriteSheet sheet)
        {
            if (!sheetDict.ContainsKey(name))
                sheetDict.Add(name, sheet);
        }
    }
}
