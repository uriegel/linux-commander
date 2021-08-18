# commander
A Norton Commander clone based on Web Components and C# (.NET 5)
## Prerequisites on Linux (Fedora)
* ```sudo dnf install gtk3-devel```
* ```sudo dnf install libsoup-devel```
* ```sudo dnf install webkit2gtk3-devel.x86_64```
* ```sudo dnf install libudev-devel```
## Prerequisites on KDE
* ```sudo apt install build-essential```
* ```sudo apt-get install libgtk-3-dev```
* ```sudo apt install libsoup2.4-dev```
* ```sudo apt-get install libwebkit2gtk-4.0-dev```

## Installation of GTK schema
```
    sudo install -D de.uriegel.commander.gschema.xml /usr/share/glib-2.0/schemas/
    $ sudo glib-compile-schemas /usr/share/glib-2.0/schemas/
```    
