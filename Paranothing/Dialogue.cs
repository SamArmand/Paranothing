using System;
using Microsoft.Xna.Framework;

namespace Paranothing
{
    class Dialogue : Collideable, Updatable, Saveable
    {
        private GameController control = GameController.getInstance();
        public bool played { get; private set; }
        public string text;
        private Vector2 position;
        public int X { get { return (int)position.X; } set { position.X = value; } }
        public int Y { get { return (int)position.Y; } set { position.Y = value; } }
        private Rectangle bounds { get {return new Rectangle(X, Y, 20, 20);}}
        public Dialogue(string text, int X, int Y)
        {
            position = new Vector2(X, Y);
            this.text = text;
            played = false;
        }

        public Dialogue(string saveString)
        {
            X = 0;
            Y = 0;
            text = "...";
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndDialogue") && lineNum < lines.Length)
            {
                line = lines[lineNum];
                if (line.StartsWith("x:"))
                {
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("y:"))
                {
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("text:"))
                {
                    try { text = line.Substring(5).Trim(); }
                    catch (FormatException) { }
                }
                lineNum++;
            }
        }

        public Rectangle getBounds()
        {
            return bounds;
        }

        public bool isSolid()
        {
            return false;
        }

        public void Play()
        {
            if (!played)
            {
                control.showDialogue(text);
                played = true;
            }
        }

        public void update(GameTime time) {}

        public string saveData()
        {
            return "StartDialogue\nx:" + X + "\ny:" + Y + "text:" + text + "EndDialogue";
        }

        public void reset()
        {
            played = false;
        }
    }
}
