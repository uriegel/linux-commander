import { IconNameType } from "../controllers/controller"
import Android from "../svg/Android"
import Folder from "../svg/Folder"
import New from "../svg/New"
import Parent from "../svg/Parent"

interface IconNameProps {
    namePart: string
    iconPath?: string
    type: IconNameType
}

const IconName = ({ namePart, type, iconPath }: IconNameProps) => 
    (<span> { type == IconNameType.Folder
        ? (<Folder />)
        : type == IconNameType.File
        ? (<img className="iconImage" src={`http://localhost:20000/commander/geticon?path=${iconPath}`} alt="" />)
        : type == IconNameType.Root
        ? (<img className="image" src={`http://localhost:20000/iconfromname/drive-removable-media`} alt="" />)
        : type == IconNameType.RootEjectable
        ? (<img className="image" src={`http://localhost:20000/iconfromname/media-removable`} alt="" />)
        : type == IconNameType.Home
        ? (<img className="image" src={`http://localhost:20000/iconfromname/user-home`} alt="" />)
        : type == IconNameType.Android
        ? (<Android />)
        : type == IconNameType.Remote
        ? (<img className="image" src={`http://localhost:20000/iconfromname/network-server`} alt="" />)
        : type == IconNameType.New
        ? (<New />)
        : type == IconNameType.Favorite
        ? (<img className="image" src={`http://localhost:20000/iconfromname/starred`} alt="" />)
        : (<Parent />)
        }
        <span>{namePart}</span>
    </span>)

export default IconName