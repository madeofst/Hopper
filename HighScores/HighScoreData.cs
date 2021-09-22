using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class HighScoreData
{
    private string highScoreFile = "HighScores/HighScores.json";
    private List<HighScore> _list;
    public List<HighScore> List
    {
        get
        {
            if (!File.Exists(highScoreFile)) Init();
            string jsonString = File.ReadAllText(highScoreFile);
            _list = JsonSerializer.Deserialize<List<HighScore>>(jsonString);           
            return _list.OrderByDescending(hs => hs.Score).ToList();
        }
        set
        {
            _list = value.OrderByDescending(hs => hs.Score).ToList();
            string jsonString = JsonSerializer.Serialize<List<HighScore>>(_list, options);
            File.WriteAllText(highScoreFile, jsonString);
        }
    }

    JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

    public void Init()
    {
        List<HighScore> highScores = new List<HighScore>();

        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });
        highScores.Add(new HighScore() { UserName = "...", Score = 0 });

        List = highScores;
    }

    public HighScore Add(HighScore highScore)
    {
        if (IsHigh(highScore))
        {
            List<HighScore> HighScores = List;

            HighScores.Add(highScore);
            HighScores = HighScores.OrderByDescending(hs => hs.Score).ToList();
            HighScores.Remove(HighScores.LastOrDefault());

            List = HighScores;
            return highScore;
        }
        return null;
    }

    public bool IsHigh(HighScore HighScore)
    {
        if (HighScore.Score > List.LastOrDefault().Score)
        {
            return true;
        }
        return false;
    }
}