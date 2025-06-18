import gi
from gi.repository import Gio
gi.require_version('Gtk', '3.0')
from gi.repository import Gtk
import mimetypes
import sys
def get_icon_name(name, size=16):
    if name:
        icon = Gio.content_type_get_icon(name)
        theme = Gtk.IconTheme.get_default()
        info = theme.choose_icon(icon.get_names(), size, 0)
        return info.get_filename()
print(get_icon_name(sys.argv[1]))