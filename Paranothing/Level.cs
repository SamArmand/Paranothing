using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Paranothing
{
    public class Level
    {
        public int Width, Height; // Complete width and height of the level
        public int playerX, playerY; // Player's starting position
        public Color wallpaperColor;
        public string name { get; private set; }
        public string nextLevel { get; private set; }
        public TimePeriod startTime;
        private List<Saveable> savedObjs;
        public Level()
        {
            Width = 640;
            Height = 360;
            playerX = 38;
            playerY = 58;
            savedObjs = new List<Saveable>();
            startTime = TimePeriod.Present;
            wallpaperColor = Color.White;
        }

        public Level(int Width, int Height, int playerX, int playerY, Color wallpaperColor, TimePeriod startTime)
        {
            this.Width = Width;
            this.Height = Height;
            this.playerX = playerX;
            this.playerY = playerY;
            this.startTime = startTime;
            this.wallpaperColor = wallpaperColor;
            savedObjs = new List<Saveable>();
        }

        public Level(string filename)
        {
            savedObjs = new List<Saveable>();
            loadFromFile(filename);
        }

        public void addObj(Saveable obj)
        {
            savedObjs.Add(obj);
        }

        public void removeObj(Saveable obj)
        {
            savedObjs.Remove(obj);
        }

        public List<Saveable> getObjs()
        {
            return savedObjs;
        }

        public string getSaveString()
        {
            string saveString = "StartLevel";
            saveString += "\nlevelName:" + name;
            saveString += "\nplayerX:" + playerX;
            saveString += "\nplayerY:" + playerY;
            saveString += "\nwidth:" + Width;
            saveString += "\nheight:" + Height;
            saveString += "\nstartTime:" + startTime;
            saveString += "\ncolor:" + wallpaperColor.R+","+wallpaperColor.G+","+wallpaperColor.B;
            foreach (Saveable obj in savedObjs)
            {
                saveString += "\n" + obj.saveData();
            }
            saveString += "\nEndLevel";
            return saveString;
        }

        public void loadFromFile(string filename)
        {
            try
            {
                Stream stream = TitleContainer.OpenStream(filename);
                StreamReader reader = new StreamReader(stream);
                createFromString(reader.ReadToEnd());
            }
            catch (System.IO.FileNotFoundException)
            {
            }
        }

        public void createFromString(string saveString)
        {
            Width = 640;
            Height = 360;
            playerX = 38;
            playerY = 58;
            savedObjs = new List<Saveable>();

            string[] saveLines = saveString.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("StartLevel") && lineNum < saveLines.Length)
            {
                line = saveLines[lineNum];
                lineNum++;
            }
            while (!line.StartsWith("EndLevel") && lineNum < saveLines.Length)
            {
                string objData = "";
                line = saveLines[lineNum];
                // Level attributes
                if (line.StartsWith("levelName:"))
                    name = line.Substring(10).Trim();
                if (line.StartsWith("nextLevel:"))
                    nextLevel = line.Substring(10).Trim();
                if (line.StartsWith("playerX:"))
                    playerX = int.Parse(line.Substring(8));
                if (line.StartsWith("playerY:"))
                    playerY = int.Parse(line.Substring(8));
                if (line.StartsWith("width:"))
                    Width = int.Parse(line.Substring(6));
                if (line.StartsWith("height:"))
                    Height = int.Parse(line.Substring(7));
                if (line.StartsWith("startTime:"))
                {
                    startTime = TimePeriod.Present;
                    string time = line.Substring(10).Trim();
                    if (time == "Present")
                    {
                        startTime = TimePeriod.Present;
                    }
                    if (time == "Past")
                    {
                        startTime = TimePeriod.Past;
                    }
                    if (time == "FarPast")
                    {
                        startTime = TimePeriod.FarPast;
                    }
                }
                if (line.StartsWith("color:"))
                    wallpaperColor = parseColor(line.Substring(6));
                // Dialogue trigger
                if (line.StartsWith("StartDialogue"))
                {
                    objData = line;
                    while (!line.StartsWith("EndDialogue") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Dialogue(objData));
                }
                // Shadow
                if (line.StartsWith("StartShadow"))
                {
                    objData = line;
                    while (!line.StartsWith("EndShadow") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Shadows(objData));
                }
                // Stairs
                if (line.StartsWith("StartStair"))
                {
                    objData = line;
                    while (!line.StartsWith("EndStair") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Stairs(objData));
                }
                // Rubble
                if (line.StartsWith("StartRubble"))
                {
                    objData = line;
                    while (!line.StartsWith("EndRubble") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Rubble(objData));
                }
                // Chair
                if (line.StartsWith("StartChairs"))
                {
                    objData = line;
                    while (!line.StartsWith("EndChairs") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Chairs(objData));
                }
                // Door
                if (line.StartsWith("StartDoor"))
                {
                    objData = line;
                    while (!line.StartsWith("EndDoor") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Doors(objData));
                }
                // Wardrobe
                if (line.StartsWith("StartWardrobe"))
                {
                    objData = line;
                    while (!line.StartsWith("EndWardrobe") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Wardrobe(objData));
                }
                // Key
                if (line.StartsWith("StartKey"))
                {
                    objData = line;
                    while (!line.StartsWith("EndKey") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new DoorKeys(objData));
                }
                // Portrait
                if (line.StartsWith("StartPortrait"))
                {
                    objData = line;
                    while (!line.StartsWith("EndPortrait") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Portrait(objData, "EndPortrait"));
                }
                // Older Painting
                if (line.StartsWith("StartOldPortrait"))
                {
                    objData = line;
                    while (!line.StartsWith("EndOldPortrait") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Portrait(objData, TimePeriod.FarPast));
                }
                // Moved Portrait
                if (line.StartsWith("StartMovedPortrait"))
                {
                    Portrait pPres = null, pPast = null;
                    while (!line.StartsWith("EndMovedPortrait") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        if (line.StartsWith("StartPresentPortrait"))
                        {
                            objData = line;
                            while (!line.StartsWith("EndPresentPortrait") && lineNum < saveLines.Length)
                            {
                                line = saveLines[lineNum];
                                objData += "\n" + line;
                                lineNum++;
                            }
                            pPres = new Portrait(objData, TimePeriod.Present);

                            addObj(pPres);
                        }
                        if (line.StartsWith("StartPastPortrait"))
                        {
                            objData = line;
                            while (!line.StartsWith("EndPastPortrait") && lineNum < saveLines.Length)
                            {
                                line = saveLines[lineNum];
                                objData += "\n" + line;
                                lineNum++;
                            }
                            pPast = new Portrait(objData, TimePeriod.Past);
                            addObj(pPast);
                        }
                        lineNum++;
                    }
                    if (pPres != null && pPast  != null)
                    {
                        pPres.movedPos = new Vector2(pPast.X, pPast.Y);
                        pPast.movedPos = new Vector2(pPres.X, pPres.Y);
                    }
                }
                // Bookcase
                if (line.StartsWith("StartBookcase"))
                {
                    objData = line;
                    while (!line.StartsWith("EndBookcase") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Bookcases(objData));
                }
                // Button
                if (line.StartsWith("StartButton"))
                {
                    objData = line;
                    while (!line.StartsWith("EndButton") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Button(objData));
                }
                // Wall
                if (line.StartsWith("StartWall"))
                {
                    objData = line;
                    while (!line.StartsWith("EndWall") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Wall(objData)); 
                }
                // Floor
                if (line.StartsWith("StartFloor"))
                {
                    objData = line;
                    while (!line.StartsWith("EndFloor") && lineNum < saveLines.Length)
                    {
                        line = saveLines[lineNum];
                        objData += "\n" + line;
                        lineNum++;
                    }
                    addObj(new Floor(objData));
                }
                lineNum++;
            }
        }

        private Color parseColor(string color)
        {
            string[] rgb = color.Split(new char[] { ',' });
            int r = int.Parse(rgb[0]);
            int g = int.Parse(rgb[1]);
            int b = int.Parse(rgb[2]);

            return new Color(r, g, b);
        }
    }
}
