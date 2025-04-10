using GtkDotNet;

using Commander.Enums;
using Commander.UI;
using Commander.DataContexts;
using CsTools.Extensions;

namespace Commander.Controllers;

class CopyProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
{
    public async Task CopyItems()
    {
        // TODO 3. suggested button is the default button
        // TODO 4. make present async method returning the same respone like AdwAlertDialog
        // TODO 5. copy directories
        // TODO 6. Move
        if (targetPath?.StartsWith('/') != true)
            return;
        var text = selectedItemsType switch
        {
            SelectedItemsType.Both => "Möchtest Du die markierten Einträge kopieren?",
            SelectedItemsType.Files => "Möchtest Du die markierten Dateien kopieren?",
            SelectedItemsType.Folders => "Möchtest Du die markierten Verzeichnisse kopieren?",
            SelectedItemsType.File => "Möchtest Du die markierte Datei kopieren?",
            SelectedItemsType.Folder => "Möchtest Du das markierte Verzeichnis kopieren?",
            _ => ""
        };

        var copyItems = MakeCopyItems(selectedItems);
        var conflicts = copyItems.Where(n => n.Target != null).ToArray();
        if (conflicts.Length > 0)
        {
            var res = await ConflictDialog.PresentAsync(conflicts);
            throw new TaskCanceledException();
        }
        else
        {
            var response = await AlertDialog.PresentAsync("Kopieren?", text);
            if (response != "ok")
                throw new TaskCanceledException();
        }
        try
        {
            var index = 0;
            var cancellation = CopyProgressContext.Instance.Start("Fortschritt beim Kopieren", copyItems.Sum(n => n.Source.Size), copyItems.Length);
            var buffer = new byte[15000];
            foreach (var item in copyItems)
            {
                if (cancellation.IsCancellationRequested)
                    throw new TaskCanceledException();
                CopyProgressContext.Instance.SetNewFileProgress(item.Source.Name, item.Source.Size, ++index);
                var newFileName = targetPath.AppendPath(item.Source.Name);
                var tmpNewFileName = targetPath.AppendPath(TMP_PREFIX + item.Source.Name);
                await Task.Run(() =>
                {
                    using var source = File.OpenRead(sourcePath.AppendPath(item.Source.Name)).WithProgress(CopyProgressContext.Instance.SetProgress);
                    using var target = File.Create(tmpNewFileName);
                    while (true)
                    {
                        if (cancellation.IsCancellationRequested)
                        {
                            try
                            {
                                File.Delete(tmpNewFileName);
                            }
                            catch { }
                            throw new TaskCanceledException();
                        }

                        var read = source.Read(buffer, 0, buffer.Length);
                        if (read == 0)
                            break;
                        target.Write(buffer, 0, Math.Min(read, buffer.Length));
                    }
                });
                using var gsf = GFile.New(sourcePath.AppendPath(item.Source.Name));
                using var gtf = GFile.New(tmpNewFileName);
                gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
                File.Move(tmpNewFileName, newFileName, true);

                // TODO Move
                // using var file = GFile.New(CurrentPath.AppendPath(item.Name));
                // await file.CopyAsync(targetPath.AppendPath(item.Name), FileCopyFlags.Overwrite, false, (c, t) => CopyProgressContext.Instance.SetProgress(c));
            }
        }
        finally
        {
            CopyProgressContext.Instance.Stop();
        }
    }

    CopyItem[] MakeCopyItems(IEnumerable<DirectoryItem> fileNames)
        => fileNames
            .Where(n => !n.IsDirectory)
            .SelectFilterNull(CreateCopyItem)
            .ToArray();

    CopyItem? CreateCopyItem(DirectoryItem item)
    {
        var info = new FileInfo(sourcePath.AppendPath(item.Name));
        if (!info.Exists)
            return null;
        var source = new Item(info.Name, info.Length, info.LastWriteTime);
        info = new FileInfo(targetPath.AppendPath(item.Name));
        var target = info.Exists ? new Item(info.Name, info.Length, info.LastWriteTime) : null;
        return new(source, target);
    }

    const string TMP_PREFIX = "tmp-commander-";
}

record Item(string Name, long Size, DateTime DateTime);
record CopyItem(Item Source, Item? Target);