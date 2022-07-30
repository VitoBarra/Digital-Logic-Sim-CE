using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RenameFolderMenu : MonoBehaviour
{
    ChipBarUI chipBarUI;
    public TMP_InputField RenamingFolderField;
    public TMP_Text RenamingTextLabel;
    public Button OKRenameFolder;
    private string FolderName = "";

    private void Start()
    {
        chipBarUI = ChipBarUI.instance;
    }

    public void RenameFolder()
    {
        string newFolderName = RenamingFolderField.text;
        RenamingFolderField.SetTextWithoutNotify("");
        OKRenameFolder.interactable = false;

        FolderSystem.RenameFolder(FolderName, newFolderName);
        EditChipBar();
    }

    public void EditChipBar()
    {
        Manager.instance.spawnableChips.Clear();
        SaveSystem.LoadAll(Manager.instance);
        chipBarUI.NotifyFolderNameChanged();
    }


    public void InitMenu(string name) // call from editor
    {
        FolderName = name;
        RenamingTextLabel.text = $"{name}";
        RenamingFolderField.Select();
    }

    public void CheckFolderName(bool endEdit = false)
    {
        var validName = FolderNameValidator.ValidateFolderName(RenamingFolderField.text, endEdit);

        OKRenameFolder.interactable = validName.Length > 0 && FolderSystem.FolderNameAvailable(validName);
        RenamingFolderField.SetTextWithoutNotify(validName);
    }
}




