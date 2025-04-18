using Commander.Enums;
using CsTools.Extensions;

namespace Commander.Controllers;

class CopyFromRemoteProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
    : CopyProcessor(sourcePath, targetPath, selectedItemsType, selectedItems)
{
    protected override CopyItem? CreateCopyItem(Item source, string _, string targetPath)
    {
        var info = new FileInfo(targetPath.AppendPath(source.Name));
        var target = info.Exists ? new Item(info.Name, info.Length, info.LastWriteTime) : null;
        return new(source, target);
    }
}