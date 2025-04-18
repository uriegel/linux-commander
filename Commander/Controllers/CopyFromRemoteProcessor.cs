using Commander.Enums;

namespace Commander.Controllers;

class CopyFromRemoteProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
    : CopyProcessor(sourcePath, targetPath, selectedItemsType, selectedItems)
{

}