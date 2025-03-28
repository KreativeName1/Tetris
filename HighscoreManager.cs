using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tetris;

public static class HighscoreManager
{
    private const int MaxHighscores = 10;
    private static readonly string FilePath = @"./game.dat";

    private static readonly string EncryptionKey = "MySecretKey123456";

    public static bool IsNewHighscore(int score)
    {
        var highscores = LoadHighscores();

        if (highscores.Count == 0 && score > 0) return true;

        for (var i = 0; i < highscores.Count; i++)
            if (score > highscores[i].Score)
                return true;

        return false;
    }

    public static int GetNewHighscoreIndex(int score)
    {
        var highscores = LoadHighscores();

        if (highscores.Count == 0 && score > 0) return 0;

        for (var i = 0; i < highscores.Count; i++)
            if (score > highscores[i].Score)
                return i;

        return -1;
    }

    public static void SaveHighscore(HighscoreData newScore)
    {
        var directoryPath = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        var highscores = LoadHighscores();

        highscores.Add(newScore);

        highscores = highscores.OrderByDescending(h => h.Score).ToList();

        if (highscores.Count > MaxHighscores) highscores.RemoveAt(highscores.Count - 1);
        var jsonData = JsonSerializer.Serialize(highscores);
        var encryptedData = Encrypt(jsonData, EncryptionKey);

        File.WriteAllBytes(FilePath, encryptedData);
    }

    public static List<HighscoreData> LoadHighscores()
    {
        if (!File.Exists(FilePath)) return new List<HighscoreData>();

        try
        {
            var encryptedData = File.ReadAllBytes(FilePath);

            if (encryptedData.Length == 0)
                return new List<HighscoreData>();

            var jsonData = Decrypt(encryptedData, EncryptionKey);

            return JsonSerializer.Deserialize<List<HighscoreData>>(jsonData) ?? new List<HighscoreData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading highscores: {ex.Message}");
            return new List<HighscoreData>();
        }
    }

    private static byte[] Encrypt(string plainText, string key)
    {
        using var aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(PadOrTrimKey(key, 16));
        aes.IV = new byte[16];

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }

            cs.Close();
        }

        return ms.ToArray();
    }


    private static string PadOrTrimKey(string key, int length)
    {
        if (key.Length < length) return key.PadRight(length, ' ');

        return key.Substring(0, length);
    }

    private static string Decrypt(byte[] cipherText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(PadOrTrimKey(key, 16));
        aes.IV = new byte[16];

        using MemoryStream ms = new(cipherText);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        return sr.ReadToEnd();
    }
}