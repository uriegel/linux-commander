import { useRef } from 'react'
import './App.css'
import Commander, { type CommanderHandle } from './Commander'
import './themes/adwaita.css'
import WithDialog from 'web-dialog-react'

const App = () => {

	const commander = useRef(null as CommanderHandle|null)

	return (
		<div className="App adwaitaTheme" >
			<WithDialog>
				<Commander ref={commander} ></Commander>
			</WithDialog>
		</div>
	)
}

export default App


