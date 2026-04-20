using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem {
    public static PlayerData thisSave;
    public static string[] saves;
    public static int numSaves = 4;
    public static void Preload()
    {
        //call in main menu screen to open Load Saves
        saves = new string[numSaves];
        for (int i = 0; i < numSaves; i++)
        {
            string savepath = "save" + i.ToString() + ".json";
            string path = Path.Combine(Application.persistentDataPath, savepath);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                saves[i] = JsonUtility.FromJson<PlayerData>(json).ToString();
                //reference saves from another file to fill in scene-specific stuff
            }
        }
        
    }
    public static void Play(int saveID)
    {
        thisSave = SaveSystem.Load(saveID);
        Save();
        //LoadScene($"Chapter{thisSave.chapter}");
        //*************************************************** TO DO: MAKE ABOVE LINE HAPPEN***********
    }
    public static PlayerData Load(int saveID)
    {
        try
        {
            string savepath = "save" + saveID.ToString()+".json";
            string path = Path.Combine(Application.persistentDataPath, savepath);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Debug.Log("Returning player found");
                return JsonUtility.FromJson<PlayerData>(json);
            }
            else
            {
                Debug.LogWarning("No save file found, starting new save.");
                return new PlayerData(saveID);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Load failed: " + e.Message);
            return new PlayerData();
        }
    }
    public static void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(thisSave, true);
            string savepath = "save" + thisSave.saveID.ToString() + ".json";
            string path = Path.Combine(Application.persistentDataPath, savepath);
            string imagepath = $"/save{thisSave.saveID}images";
            string images = Path.Combine(Application.persistentDataPath, imagepath);
            if (!Directory.Exists(images))
            {
                Directory.CreateDirectory(images);
                //create a folder for images for this save if none exists
            }
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, json);
            Debug.Log("Game saved to: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Save failed: " + e.Message);
        }
    }
    public static void ClearSaveData(int saveID)
    {
        saves[saveID] = null;
        try
        {
            string savepath = "save" + saveID.ToString() + ".json";
            string path = Path.Combine(Application.persistentDataPath, savepath);
            string imagepath = $"/save{saveID}images";
            string images = Path.Combine(Application.persistentDataPath, imagepath);
            if (Directory.Exists(images))
            {
                Directory.Delete(images);
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Debug.Log("Game deleted from: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("New save failed: " + e.Message);
        }
    }
}

[System.Serializable]
public struct PlayerData
{
    public int saveID;
    public int chapter;
    //***************************************** TO DO: FILL WITH OTHER SAVE INFO ******************
    public PlayerData(int replaceID)
    {
        saveID = replaceID;
        chapter = 0;
    }
    public override string ToString()
    {
        return $"Chapter {chapter}";
    }
}
