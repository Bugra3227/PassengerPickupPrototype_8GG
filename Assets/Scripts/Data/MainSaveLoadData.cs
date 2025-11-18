using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class MainSaveLoad
{
    public static MainSaveLoad instance;
    
    
    public int levelCount = 0;
   
  
    
    public static void Save()
    {
        string path = Application.persistentDataPath + "/MainSaveLoad.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        bf.Serialize(file, instance);
        file.Close();
    }

    public static void Load()
    {
        
        string path = Application.persistentDataPath + "/MainSaveLoad.dat";
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            MainSaveLoad data = (MainSaveLoad)bf.Deserialize(file);
            file.Close();
            instance = data;
        }
        else
        {
            instance = new MainSaveLoad();
            Save();
        }
    }

  

#if UNITY_EDITOR

    [MenuItem("Level Editor/Clear Save File")]
    private static void ClearSave()
    {
        string path = Application.persistentDataPath + "/MainSaveLoad.dat";
        if (File.Exists(path))
        {
            File.Delete(path);
           
            
        }
    }
#endif   
}