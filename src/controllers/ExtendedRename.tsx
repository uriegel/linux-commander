import { ResultType, type DialogHandle } from "web-dialog-react";
import { Directory, type ExtendedRenameProps } from "./directory";
import ExtendedRenamePart from "../components/dialogparts/ExtendedRenamePart";
import { formatDateTime, formatSize, IconNameType, type IController } from "./controller";
import type { TableColumns } from "virtual-table-react";
import type { FolderViewItem } from "../components/FolderView";
import IconName from "../components/IconName";


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
