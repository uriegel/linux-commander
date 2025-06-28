# commander
A Norton Commander clone based on Web Components and C# (.NET 9)

## React dev tools
```
sudo npm install -g react-devtools
```
In terminal:
```
react-devtools
```

```
// TODO    static async Task<bool> Rename(IRequest request)
// TODO Extended rename
// TODO Favorites
// TODO Remotes


//TODO:
//TODO    static async Task<bool> Rename(IRequest request)
//TODO    {
//TODO        var data = await request.DeserializeAsync<RenameRequest>();
//TODO        if (data != null)
//TODO        {
//TODO            var response = await GetController(data.Id).Rename(data);
//TODO            await request.SendJsonAsync(response, response.GetType());
//TODO        }
//TODO        return true;
//TODO    }

// TODO tsc without errors

// TODO root detect new drives/removed drives
// TODO Initial scrollbar in virtual table view to large
// TODO TrackViewer better overlays like latest Gtk4 Commander?

// TODO Text viewer/editor
// TODO Track viewer some inconsistencies like max velocity too high, trackpoints not containing data any more...

// TODO When item is opened wait till process stops and refresh item
// TODO Rename remote
// TODO Android range
// TODO Dont copy directory from remote
```