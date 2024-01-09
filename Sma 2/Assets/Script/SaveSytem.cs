using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSytem 
{
    public static void SaveGame(GameManager gameManager)
    {
        string path = Application.persistentDataPath + "/Save.sma2gd";
        BinaryFormatter formatter = new BinaryFormatter();
        Debug.Log("Saving at Location: '" + path + "'");
        FileStream stream = new FileStream(path, FileMode.Create);

        GameData data = new GameData(gameManager);

        formatter.Serialize(stream, data);
        stream.Close();
    }  
    public static GameData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/Save.sma2gd";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameData data = formatter.Deserialize(stream) as GameData;

            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("Save file not found in : '" + path + "'");
            return null;
        }
    }
}
