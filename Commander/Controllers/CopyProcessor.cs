using GtkDotNet;

using Commander.Enums;
using GtkDotNet.SafeHandles;
using Commander.UI;
using Commander.DataContexts;
using CsTools.Extensions;

namespace Commander.Controllers;

class CopyProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, Func<DirectoryItem[]> getSelectedItems)
{
    public async Task CopyItems()
    {


        // TODO Test
        // var dialog1 = new ConflictDialog();
        // dialog1.Show();

        // return;

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
        var builder = Builder.FromDotNetResource("alertdialog");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        dialog.Heading("Kopieren?");
        dialog.Body(text);
        var response = await dialog.PresentAsync(MainWindow.MainWindowHandle);
        if (response == "ok")
        {
            // TODO 4. ConflictItems file Dialog
            // TODO 5. copy directories
            var items = getSelectedItems();
            try
            {
                var index = 0;
                var cancellation = CopyProgressContext.Instance.Start("Fortschritt beim Kopieren", items.Sum(n => n.Size), items.Length);
                var buffer = new byte[15000];
                foreach (var item in items)
                {
                    if (cancellation.IsCancellationRequested)
                        throw new TaskCanceledException();
                    CopyProgressContext.Instance.SetNewFileProgress(item.Name, item.Size, ++index);
                    var newFileName = targetPath.AppendPath(item.Name);
                    var tmpNewFileName = targetPath.AppendPath(TMP_PREFIX + item.Name);
                    await Task.Run(() =>
                    {
                        using var source = File.OpenRead(sourcePath.AppendPath(item.Name)).WithProgress(CopyProgressContext.Instance.SetProgress);
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
                    using var gsf = GFile.New(sourcePath.AppendPath(item.Name));
                    using var gtf = GFile.New(targetPath.AppendPath(item.Name));
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
        else
            throw new TaskCanceledException();
    }

    const string TMP_PREFIX = "tmp-commander-";
}