using CsTools.Extensions;
using GtkDotNet.SafeHandles;

namespace GtkDotNet;

public static class DragContext
{
    public static DragContextHandle DragAndDropFinished(this DragContextHandle dragContext, Action<bool> action)
        => dragContext.SideEffect(dc => dc.SignalConnect<PointerBoolDelegate>("dnd-finished", (_, s) => action(s)));
    public static DragContextHandle DragAndDropCancelled(this DragContextHandle dragContext, Action<DragCancelReason> action)
        => dragContext.SideEffect(dc => dc.SignalConnect<PointerIntDelegate>("cancel", (_, i) => action((DragCancelReason)i)));
}