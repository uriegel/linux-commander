import { useCallback, useEffect, useRef, useState } from 'react'
import VirtualTable, { type VirtualTableHandle } from 'virtual-table-react'
import './CopyConflicts.css'
import IconName from '../IconName'
import type { ExtensionProps } from 'web-dialog-react'
import { formatDateTime, formatSize, IconNameType } from '../../controllers/controller'

export interface ConflictItem {
	name: string
	subPath: string
	iconPath: string
    size: number
    time: string
    targetSize: number
    targetTime: string
}


const CopyConflicts = ({ props }: ExtensionProps) => {

    const virtualTable = useRef<VirtualTableHandle<ConflictItem>>(null)

    const [items, setItems] = useState([] as ConflictItem[])

	const getColumns = () => [
		{ name: "Name"  },
		{ name: "Datum" },
		{ name: "Größe", isRightAligned: true }
	]

	const renderRowItem = ({ name, subPath, iconPath, time, targetTime, size, targetSize }: ConflictItem) => [
		(<div>
			<IconName namePart={name} type={IconNameType.File} iconPath={iconPath} />
			<div className={subPath ? 'subPath' : 'subPath empty'}>{subPath ?? "___"}</div>
		</div>),
		(<div className=
			{
				time > targetTime
				? "overwrite"
				: time < targetTime
				? "notOverwrite"
				: "equal"
			}>
			<div>{formatDateTime(time)}</div>
			<div>{formatDateTime(targetTime)}</div>
		</div>),
		(<div className={targetSize == size ? "equal" : ""}>
			<div>{formatSize(size)}</div>
			<div>{formatSize(targetSize)}</div>
		</div>)
	]

	const renderRow = useCallback((item: ConflictItem) => 
        renderRowItem(item),
    [])

    useEffect(() => {
		virtualTable.current?.setColumns({
			columns: getColumns(), 
			renderRow
		})

		const items = props as ConflictItem[]
		setItems(items)
    }, [setItems, props, renderRow])
    
    return (
        <div className="tableContainer">
			<VirtualTable className='wdr-focusable' ref={virtualTable} items={items} />
        </div>
    )
}

export default CopyConflicts
