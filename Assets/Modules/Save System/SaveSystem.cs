using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using DLS.SaveSystem.Serializable.SerializationHelper;
using UnityEngine;
using Newtonsoft.Json;

public static class SaveSystem
{
    public static string ActiveProjectName { get; set; } = "Untitled";
    public static string FileExtension { get; set; } = ".json";
    public static string ProjectSettingsFileName { get; private set; } = "CustomFolders";
    public static string ChipFolder { get; set; } = "Chips";

    private static string ProjectSettingsPath =>
        Path.Combine(ActiveProjectPath, ProjectSettingsFileName + FileExtension);

    static string ActiveProjectPath => Path.Combine(SaveDataDirectoryPath, ActiveProjectName);
    static string ChipPath => Path.Combine(ActiveProjectPath, ChipFolder);

    public static string SaveDataDirectoryPath => Path.Combine(Application.persistentDataPath, "SaveData");

    static string WireLayoutPath => Path.Combine(ChipPath, "WireLayout");

    static string EEPROMSaveFilePath => Path.Combine(ActiveProjectPath, "EEPROMContents.json");

    public static string GetPathToChip(string chipName, string ExtraPath = "") =>
        Path.Combine( ChipPath + ExtraPath, chipName + FileExtension);

    public static string GetPathToWireSaveFile(string saveFileName, string ExatraPath = "") =>
        Path.Combine(WireLayoutPath + ExatraPath, saveFileName + FileExtension);


    public static void Init()
    {
        // Create save directory (if doesn't exist already)
        Directory.CreateDirectory(ActiveProjectPath);
        Directory.CreateDirectory(ChipPath);
        Directory.CreateDirectory(WireLayoutPath);
        FolderLoader.CreateDefault(ProjectSettingsPath);
        UpdateSaveData();
    }

    public static FileInfo[] GetChipSavePaths()
    {
        DirectoryInfo directory = new DirectoryInfo(ChipPath);
        FileInfo[] files = directory.GetFiles("*" + FileExtension);
        var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        return filtered.ToArray();
    }

    public static void LoadAllChips(Manager manager)
    {
        ChipFolder = "Chips";
        FileExtension = ".json";
        // Load any saved chips
        ChipLoader.LoadAllChips(GetAllSavedChipsDic(), manager);
    }

    public static SavedChip[] GetAllSavedChips()
    {
        var chipPaths = GetChipSavePaths();
        var savedChips = new SavedChip[chipPaths.Length];

        // Read saved chips from file
        for (var i = 0; i < chipPaths.Length; i++)
        {
            var chipPath = chipPaths[i].FullName;
            var chipName = Path.GetFileNameWithoutExtension(chipPaths[i].Name);
            if (chipName.Equals(ProjectSettingsFileName)) continue;

            var chipSaveString = ReadFile(chipPath);
            savedChips[i] = DeserializeChip(chipSaveString);
        }

        foreach (var chip in savedChips)
            chip.ValidateDefaultData();

        return savedChips;
    }

    public static IDictionary<string, SavedChip> GetAllSavedChipsDic()
    {
        // Load any saved chips but is Dic
        return GetAllSavedChips()?.ToDictionary(chip => chip.Info.name);
    }


    public static byte[] LoadEEPROMContents()
    {
        if (!File.Exists(EEPROMSaveFilePath)) return new byte[] { };
        string jsonString = ReadFile(EEPROMSaveFilePath);
        return JsonConvert.DeserializeObject<byte[]>(
            jsonString);
    }


    public static void SaveEEPROMContents(byte[] contents)
    {
        string jsonStr = JsonConvert.SerializeObject(contents, Formatting.Indented);
        WriteFile(EEPROMSaveFilePath, jsonStr);
    }


    public static Dictionary<int, string> LoadCustomFolders()
    {
        return FolderLoader.LoadCustomFolders(ProjectSettingsPath);
    }

    public static void SaveCustomFolders(Dictionary<int, string> folders)
    {
        FolderLoader.SaveCustomFolders(ProjectSettingsPath, folders);
    }


    public static string ReadFile(string path)
    {
        using StreamReader reader = new StreamReader(path);
        return reader.ReadToEnd();
    }

    public static void WriteFile(string path, string content)
    {
        FileInfo FilePath = new FileInfo(path);
        Directory.CreateDirectory(FilePath.Directory.ToString());
        File.WriteAllText(path, content);
    }


    public static SavedChip DeserializeChip(string ChipSave) =>
        JsonConvert.DeserializeObject<SavedChip>(ChipSave, ColorConverterHEX.GenerateSerializerSettings());

    public static string SerializeChip(SavedChip chipSave) =>
        JsonConvert.SerializeObject(chipSave, Formatting.Indented, ColorConverterHEX.GenerateSerializerSettings());
    private static string SerializeWireLayout(SavedWireLayout wireLayout) =>
        JsonConvert.SerializeObject(wireLayout, Formatting.Indented, ColorConverterHEX.GenerateSerializerSettings());



    public static SavedChip ReadChip(string chipName) =>
        DeserializeChip((ReadFile(GetPathToChip(chipName))));

    public static SavedWireLayout ReadWireLayout(string wireFile) =>
        JsonUtility.FromJson<SavedWireLayout>(ReadFile(GetPathToWireSaveFile(wireFile)));


    public static void SaveChip(string chipName, SavedChip saveString, string ExatraPath = "") =>
        WriteFile(GetPathToChip(chipName, ExatraPath), SerializeChip(saveString) );

    //Legacy function hera to read old save format
    public static void SaveWireLayout(string chipName, SavedWireLayout wireLayout, string ExatraPath = "") =>
        WriteFile(GetPathToWireSaveFile(chipName, ExatraPath), SerializeWireLayout(wireLayout));



    public static void WriteFoldersFile(string FolderFileStr) => WriteFile(ProjectSettingsPath, FolderFileStr);


    public static string[] GetProjectNames()
    {
        string[] savedProjectPaths = Array.Empty<string>();
        if (Directory.Exists(SaveDataDirectoryPath))
        {
            savedProjectPaths = Directory.GetDirectories(SaveDataDirectoryPath);
        }

        for (int i = 0; i < savedProjectPaths.Length; i++)
        {
            string[] pathSections =
                savedProjectPaths[i].Split(Path.DirectorySeparatorChar);
            savedProjectPaths[i] = pathSections[^1];
        }

        return savedProjectPaths;
    }

    public static void MigrateSaves()
    {
        //old appdata path is at ../../Sebastian Lague/Digital Logic Sim

        string oldAppDataPath = Path.Combine(new string[]
        {
            Directory.GetParent(Application.persistentDataPath).Parent.FullName, "Sebastian Lague", "Digital Logic Sim"
        });
        if (!Directory.Exists(oldAppDataPath)) return;

        string oldSaveDataPath = Path.Combine(oldAppDataPath, "SaveData");
        string[] savedProjectPaths = Directory.GetDirectories(oldSaveDataPath);
        foreach (string path in savedProjectPaths)
        {
            string folderName = Path.Combine(SaveDataDirectoryPath, Path.GetFileName(path));
            if (Directory.Exists(folderName))
                folderName = Path.Combine(SaveDataDirectoryPath, Path.GetFileName(path) + " - Copy");
            Directory.Move(path, folderName);
        }

        Directory.Delete(
            Path.Combine(Directory.GetParent(Application.persistentDataPath)?.Parent?.FullName ?? string.Empty,
                "Sebastian Lague"), true);
    }


    public static void UpdateSaveData()
    {
        FileExtension = ".txt";
        ChipFolder = "";
        FileInfo[] chipPaths = GetChipSavePaths();

        // Read saved chips from file
        foreach (var chipPat in chipPaths)
        {
            var chipPath = chipPat.FullName;
            var chipName = Path.GetFileNameWithoutExtension(chipPat.Name);
            if (chipName.Equals(ProjectSettingsFileName)) continue;

            var chipSaveString = ReadFile(chipPath);
            var updateSaveFile = SaveCompatibility.FixSaveCompatibility(chipSaveString, chipName);
            ChipFolder = "Chips";
            FileExtension = ".json";
            if (SaveCompatibility.CanWriteFile)
                SaveChip(chipName, updateSaveFile);
        }

        foreach (var oldChipFile in chipPaths)
            File.Delete(oldChipFile.FullName);


    }
}