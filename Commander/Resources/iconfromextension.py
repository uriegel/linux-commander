import gi
from gi.repository import Gio
gi.require_version('Gtk', '3.0')
from gi.repository import Gtk
import mimetypes
import sys
def get_icon_path(extension, size=16):
    type_, encoding = Gio.content_type_guess(extension, data=None)
    if type_:
        icon = Gio.content_type_get_icon(type_)
        theme = Gtk.IconTheme.get_default()
        info = theme.choose_icon(icon.get_names(), size, Gtk.IconLookupFlags.NO_SVG)
        return info.get_filename()
print(get_icon_path(sys.argv[1]))