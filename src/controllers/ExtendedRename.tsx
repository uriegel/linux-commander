import { ResultType, type DialogHandle } from "web-dialog-react";
import { Directory, type ExtendedRenameProps } from "./directory";
import ExtendedRenamePart from "../components/dialogparts/ExtendedRenamePart";
import { formatDateTime, formatSize, IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller";
import type { TableColumns } from "virtual-table-react";
import type { FolderViewItem } from "../components/FolderView";
import IconName from "../components/IconName";
import { onExtendedRename } from "../requests/requests";


export const showExtendedRename = async (dialog: DialogHandle, currentController: IController) => {
    const res = await dialog.show({
        text: "Erweitertes Umbenennen",
        extension: ExtendedRenamePart,
        extensionProps: {
            prefix: localStorage.getItem("extendedRenamePrefix") ?? "Bild",
            digits: localStorage.getItem("extendedRenameDigits")?.parseInt() ?? 3,
            startNumber: localStorage.getItem("extendedRenameStartNumber")?.parseInt() ?? 1
        } as ExtendedRenameProps,
        btnOk: true,
        btnCancel: true,
        defBtnOk: true
    })
    return (res.result == ResultType.Ok && !(currentController instanceof ExtendedRename))
        ? new ExtendedRename()
        : (res.result != ResultType.Ok && (currentController instanceof ExtendedRename))
        ? new Directory()
        : null    
}


export class ExtendedRename extends Directory {
    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
                { name: "Name", isSortable: true, subColumn: "Erw." },
                { name: "Neuer Name" },
                { name: "Datum", isSortable: true },
                { name: "Größe", isSortable: true, isRightAligned: true }
            ],
            getRowClasses: super.getRowClasses,
            renderRow 
        }
    }

    onSelectionChanged(items: FolderViewItem[]) { 
        const prefix = localStorage.getItem("extendedRenamePrefix") ?? "Bild"
        const digits = localStorage.getItem("extendedRenameDigits")?.parseInt() ?? 3
        const startNumber = localStorage.getItem("extendedRenameStartNumber")?.parseInt() ?? 1
        items.reduce((p, n) => {
            n.newName = n.isSelected && !n.isDirectory
                ? `${prefix}${p.toString().padStart(digits, '0')}.${n.name.split('.').pop()}`
                : null
            return p + (n.isSelected && !n.isDirectory ? 1 : 0)
        }, startNumber)
    }

    sort(items: FolderViewItem[], sortIndex: number, sortDescending: boolean) {
        const sorted = super.sort(items, sortIndex == 0 ? 0 : sortIndex - 1, sortDescending)
        this.onSelectionChanged(sorted)
        return sorted
    }

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        return enterData.id && enterData.dialog && enterData.selectedItems?.find(n => n.newName)
        ? this.onRename(enterData.id, enterData.path, enterData.selectedItems, enterData.dialog)
        : super.onEnter(enterData)
    }

    async onRename(id: string, path: string, items: FolderViewItem[], dialog: DialogHandle) {
        const res = await dialog.show({
            text: "Umbenennungen starten?",
            btnOk: true,
            btnCancel: true
        })
        if (res.result == ResultType.Ok)
            await onExtendedRename({ id, path, items })
        return {
            processed: true
        }
    }

    constructor() {
        super()
        this.id = "RENAME"
    }
}

const renderRow = (item: FolderViewItem) => [
	(<IconName namePart={item.name} type={
			item.isParent
			? IconNameType.Parent
			: item.isDirectory
			? IconNameType.Folder
			: IconNameType.File}
        iconPath={item.name.getExtension()} />),
    item.newName ?? "",
	(<span className={item.exifData?.dateTime ? "exif" : "" } >{formatDateTime(item?.exifData?.dateTime ?? item?.time)}</span>),
	formatSize(item.size)
]
