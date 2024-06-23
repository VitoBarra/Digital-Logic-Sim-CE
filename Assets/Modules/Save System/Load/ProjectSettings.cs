using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public static class ProjectSettings
{

    public static Dictionary<int, string> LoadProjectSettings(string path)
    {
        if (!File.Exists(path)) return new Dictionary<int, string>();
        string FoldersJson = SaveSystem.ReadFile(path);
        SaveCompatibility.FixFolderCompatibility(ref FoldersJson);
        return JsonConvert.DeserializeObject<Dictionary<int, string>>(FoldersJson);
    }

    // TODO: implement the project Settings
    public static void SaveProjectSettings(string path, IDictionary<int, string> folders)
    {
        string jsonString = JsonConvert.SerializeObject(folders, Formatting.Indented);
        SaveSystem.WriteFile(path, jsonString);
    }

    public static void CreateDefault(string path)
    {
        if (File.Exists(path)) return;

        SaveProjectSettings(path, FolderSystem.DefaultFolder);

    }
}
