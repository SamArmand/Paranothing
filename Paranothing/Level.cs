using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Paranothing
{
    internal sealed class Level
    {
        internal int Width; // Complete width and height of the level
        internal int Height; // Complete width and height of the level
        internal int PlayerX, PlayerY; // Player's starting position
        internal Color WallpaperColor;
        internal string Name { get; private set; }
        internal string NextLevel { get; private set; }
        internal TimePeriod StartTime;
        private List<ISaveable> _savedObjs;

        public Level(string filename)
        {
            _savedObjs = new List<ISaveable>();
            LoadFromFile(filename);
        }

        private void AddObj(ISaveable obj)
        {
            _savedObjs.Add(obj);
        }

        public IEnumerable<ISaveable> GetObjs()
        {
            return _savedObjs;
        }

        private void LoadFromFile(string filename)
        {
            try
            {
                CreateFromString(new StreamReader(TitleContainer.OpenStream(filename)).ReadToEnd());
            }
            catch (FileNotFoundException)
            {
            }
        }

        private void CreateFromString(string saveString)
        {
            Width = 640;
            Height = 360;
            PlayerX = 38;
            PlayerY = 58;
            _savedObjs = new List<ISaveable>();

            var saveLines = saveString.Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("StartLevel", StringComparison.Ordinal) && lineNum < saveLines.Length) line = saveLines[lineNum++];
            var objData = new StringBuilder();
            while (!line.StartsWith("EndLevel", StringComparison.Ordinal) && lineNum < saveLines.Length)
            {
                line = saveLines[lineNum];
                // Level attributes
                if (line.StartsWith("levelName:", StringComparison.Ordinal))
                    Name = line.Substring(10).Trim();
                if (line.StartsWith("nextLevel:", StringComparison.Ordinal))
                    NextLevel = line.Substring(10).Trim();
                if (line.StartsWith("playerX:", StringComparison.Ordinal))
                    PlayerX = int.Parse(line.Substring(8));
                if (line.StartsWith("playerY:", StringComparison.Ordinal))
                    PlayerY = int.Parse(line.Substring(8));
                if (line.StartsWith("width:", StringComparison.Ordinal))
                    Width = int.Parse(line.Substring(6));
                if (line.StartsWith("height:", StringComparison.Ordinal))
                    Height = int.Parse(line.Substring(7));
                if (line.StartsWith("startTime:", StringComparison.Ordinal))
                {
                    StartTime = TimePeriod.Present;
                    var time = line.Substring(10).Trim();
                    switch (time)
                    {
                        case "Past":
                            StartTime = TimePeriod.Past;
                            break;
                        case "FarPast":
                            StartTime = TimePeriod.FarPast;
                            break;
                        default:
                            StartTime = TimePeriod.Present;
                            break;
                    }
                }
                if (line.StartsWith("color:", StringComparison.Ordinal))
                    WallpaperColor = ParseColor(line.Substring(6));
                // Dialogue trigger
                if (line.StartsWith("StartDialogue", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndDialogue", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Dialogue(objData.ToString()));
                }
                // Shadow
                if (line.StartsWith("StartShadow", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndShadow", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Shadows(objData.ToString()));
                }
                // Stairs
                if (line.StartsWith("StartStair", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndStair", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Stairs(objData.ToString()));
                }
                // Rubble
                if (line.StartsWith("StartRubble", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndRubble", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Rubble(objData.ToString()));
                }
                // Chair
                if (line.StartsWith("StartChairs", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndChairs", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Chair(objData.ToString()));
                }
                // Door
                if (line.StartsWith("StartDoor", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndDoor", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Door(objData.ToString()));
                }
                // Wardrobe
                if (line.StartsWith("StartWardrobe", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndWardrobe", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Wardrobe(objData.ToString()));
                }
                // Key
                if (line.StartsWith("StartKey", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndKey", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new DoorKey(objData.ToString()));
                }
                // Portrait
                if (line.StartsWith("StartPortrait", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Portrait(objData.ToString(), "EndPortrait"));
                }
                // Older Painting
                if (line.StartsWith("StartOldPortrait", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndOldPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Portrait(objData.ToString(), TimePeriod.FarPast));
                }
                // Moved Portrait
                if (line.StartsWith("StartMovedPortrait", StringComparison.Ordinal))
                {
                    Portrait pPres = null, pPast = null;
                    while (!line.StartsWith("EndMovedPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        if (line.StartsWith("StartPresentPortrait", StringComparison.Ordinal))
                        {
                            objData.Clear().Append(line);
                            while (!line.StartsWith("EndPresentPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                            {
                                line = saveLines[lineNum++];
                                objData.Append("\n" + line);
                            }
                            pPres = new Portrait(objData.ToString(), TimePeriod.Present);

                            AddObj(pPres);
                        }
                        if (line.StartsWith("StartPastPortrait", StringComparison.Ordinal))
                        {
                            objData.Clear().Append(line);
                            while (!line.StartsWith("EndPastPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                            {
                                line = saveLines[lineNum++];
                                objData.Append("\n" + line);
                            }
                            pPast = new Portrait(objData.ToString(), TimePeriod.Past);
                            AddObj(pPast);
                        }
                        lineNum++;
                    }
                    if (pPres != null && pPast  != null)
                    {
                        pPres.MovedPos = new Vector2(pPast.X, pPast.Y);
                        pPast.MovedPos = new Vector2(pPres.X, pPres.Y);
                    }
                }
                // Bookcase
                if (line.StartsWith("StartBookcase", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndBookcase", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Bookcase(objData.ToString()));
                }
                // Button
                if (line.StartsWith("StartButton", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndButton", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Button(objData.ToString()));
                }
                // Wall
                if (line.StartsWith("StartWall", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndWall", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Wall(objData.ToString()));
                }
                // Floor
                if (line.StartsWith("StartFloor", StringComparison.Ordinal))
                {
                    objData.Clear().Append(line);
                    while (!line.StartsWith("EndFloor", StringComparison.Ordinal) && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum++];
                        objData.Append("\n" + line);
                    }
                    AddObj(new Floor(objData.ToString()));
                }
                lineNum++;
            }
        }

        private static Color ParseColor(string color)
        {
            var rgb = color.Split(',');
            return new Color(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2]));
        }
    }
}
