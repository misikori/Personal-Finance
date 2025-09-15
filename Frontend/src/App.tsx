import './App.css'
import { ConfigProvider } from './config/ConfigProvider';
import Dashboard from './pages/Dashboard';


function App() {

  return (
    <ConfigProvider>
      <Dashboard/>
    </ConfigProvider>
  );
}
export default App;

