using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Paranothing;

sealed class Level
{
    internal Color WallpaperColor;
    internal int PlayerX, PlayerY; // Bruce's starting position
    internal int Width, // Complete width of the level
        Height; // Complete height of the level;
    internal TimePeriod StartTime;

    internal Level(string filename)
    {
        Width = 640;
        Height = 360;
        PlayerX = 38;
        PlayerY = 58;

        var saveLines = new StreamReader(TitleContainer.OpenStream(filename)).ReadToEnd()
            .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("StartLevel", StringComparison.Ordinal) && lineNum < saveLines.Length)
            line = saveLines[lineNum++];
        var objData = new StringBuilder();
        while (!line.StartsWith("EndLevel", StringComparison.Ordinal) && lineNum < saveLines.Length)
        {
            line = saveLines[lineNum];

            if (line.StartsWith("levelName:", StringComparison.Ordinal))
                Name = line[10..].Trim();
            if (line.StartsWith("nextLevel:", StringComparison.Ordinal))
                NextLevel = line[10..].Trim();
            if (line.StartsWith("playerX:", StringComparison.Ordinal))
                PlayerX = int.Parse(line[8..]);
            if (line.StartsWith("playerY:", StringComparison.Ordinal))
                PlayerY = int.Parse(line[8..]);
            if (line.StartsWith("width:", StringComparison.Ordinal))
                Width = int.Parse(line[6..]);
            if (line.StartsWith("height:", StringComparison.Ordinal))
                Height = int.Parse(line[7..]);
            if (line.StartsWith("startTime:", StringComparison.Ordinal))
                StartTime = line[10..].Trim() switch
                {
                    "Past" => TimePeriod.Past,
                    "FarPast" => TimePeriod.FarPast,
                    _ => TimePeriod.Present
                };

            if (line.StartsWith("color:", StringComparison.Ordinal))
                WallpaperColor = ParseColor(line[6..]);

            if (line.StartsWith("StartDialogue", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndDialogue", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Dialogue(objData.ToString()));
            }

            if (line.StartsWith("StartShadow", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndShadow", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Shadow(objData.ToString()));
            }

            if (line.StartsWith("StartStairs", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndStairs", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Stairs(objData.ToString()));
            }

            if (line.StartsWith("StartRubble", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndRubble", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Rubble(objData.ToString()));
            }

            if (line.StartsWith("StartChair", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndChair", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Chair(objData.ToString()));
            }

            if (line.StartsWith("StartDoor", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndDoor", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Door(objData.ToString()));
            }

            if (line.StartsWith("StartWardrobe", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndWardrobe", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Wardrobe(objData.ToString()));
            }

            if (line.StartsWith("StartKey", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndKey", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new DoorKey(objData.ToString()));
            }

            if (line.StartsWith("StartPortrait", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Portrait(objData.ToString(), "EndPortrait"));
            }

            if (line.StartsWith("StartOldPortrait", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndOldPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Portrait(objData.ToString(), TimePeriod.FarPast));
            }

            if (line.StartsWith("StartMovedPortrait", StringComparison.Ordinal))
            {
                Portrait pPres = null, pPast = null;
                while (!line.StartsWith("EndMovedPortrait", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum];
                    if (line.StartsWith("StartPresentPortrait", StringComparison.Ordinal))
                    {
                        objData.Clear().Append(line);
                        while (!line.StartsWith("EndPresentPortrait", StringComparison.Ordinal) &&
                               lineNum < saveLines.Length)
                        {
                            line = saveLines[lineNum++];
                            objData.Append("\n" + line);
                        }

                        pPres = new(objData.ToString(), TimePeriod.Present);

                        LevelObjects.Add(pPres);
                    }

                    if (line.StartsWith("StartPastPortrait", StringComparison.Ordinal))
                    {
                        objData.Clear().Append(line);
                        while (!line.StartsWith("EndPastPortrait", StringComparison.Ordinal) &&
                               lineNum < saveLines.Length)
                        {
                            line = saveLines[lineNum++];
                            objData.Append("\n" + line);
                        }

                        pPast = new(objData.ToString(), TimePeriod.Past);
                        LevelObjects.Add(pPast);
                    }

                    ++lineNum;
                }

                if (pPres != null && pPast != null)
                {
                    pPres.MovedPosition = new(pPast.X, pPast.Y);
                    pPast.MovedPosition = new(pPres.X, pPres.Y);
                }
            }

            if (line.StartsWith("StartBookcase", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndBookcase", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Bookcase(objData.ToString()));
            }

            if (line.StartsWith("StartButton", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndButton", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Button(objData.ToString()));
            }

            if (line.StartsWith("StartWall", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndWall", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Wall(objData.ToString()));
            }

            if (line.StartsWith("StartFloor", StringComparison.Ordinal))
            {
                objData.Clear().Append(line);
                while (!line.StartsWith("EndFloor", StringComparison.Ordinal) && lineNum < saveLines.Length)
                {
                    line = saveLines[lineNum++];
                    objData.Append("\n" + line);
                }

                LevelObjects.Add(new Floor(objData.ToString()));
            }

            ++lineNum;
        }
    }

    internal List<object> LevelObjects { get; } = new();

    internal string Name { get; private set; }
    internal string NextLevel { get; private set; }

    static Color ParseColor(string color)
    {
        var rgb = color.Split(',');
        return new(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2]));
    }
}