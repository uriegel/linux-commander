import { IconNameType } from "../controllers/controller"
import Remote from "../svg/Android"
import Android from "../svg/Android"
import Drive from "../svg/Drive"
import Favorite from "../svg/Favorite"
import Folder from "../svg/Folder"
import Home from "../svg/Home"
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
        //? (<Drive />)
        ? (<img className="image" src={`http://localhost:20000/iconfromname/drive-removable-media`} alt="" />)
        : type == IconNameType.Home
        //? (<Home />)
        ? (<img className="image" src={`http://localhost:20000/iconfromname/user-home`} alt="" />)
        : type == IconNameType.Android
        ? (<Android />)
        : type == IconNameType.Remote
        ? (<Remote />)
        : type == IconNameType.New
        ? (<New />)
        : type == IconNameType.Favorite
        ? (<Favorite />)
        : (<Parent />)
        }
        <span>{namePart}</span>
    </span>)

export default IconName