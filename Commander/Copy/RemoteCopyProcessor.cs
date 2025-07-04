using Commander.Controllers;

namespace Commander.Copy;

class RemoteCopyProcessor(string sourcePath, string targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems, bool move)
    : CopyProcessor(sourcePath, targetPath, selectedItemsType, selectedItems, move)
{
    protected override DirectoryItem? ValidateFile(DirectoryItem item, string subPath, string path) => item;
}
