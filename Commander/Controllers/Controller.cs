namespace Commander.Controllers;

abstract class Controller(string folderId)
{
    public string FolderId { get => folderId; }
    public abstract string Id { get; }
    public abstract Task<ChangePathResult> ChangePathAsync(string path, bool showHidden);
    public virtual Task<PrepareCopyResult> PrepareCopy(PrepareCopyRequest data) => throw new NotImplementedException();
    public virtual Task<CopyResult> Copy(CopyRequest copyRequest) => throw new NotImplementedException();
    public virtual Task<DeleteResult> Delete(DeleteRequest deleteRequest) => throw new NotImplementedException();
    public virtual Task<CreateFolderResult> CreateFolder(CreateFolderRequest createFolderRequest) => throw new NotImplementedException();
    public virtual Task<RenameResult> Rename(RenameRequest rename) => throw new NotImplementedException();
    public virtual Task<OnEnterResult> OnEnter(OnEnterRequest rename) => throw new NotImplementedException();

    protected bool CheckInitial()
    {
        if (initial)
        {
            initial = false;
            return true;
        }
        else
            return false;
    }

    bool initial = true;
}

