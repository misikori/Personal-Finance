import './App.css'
import { AuthProvider } from './auth/AuthContext';
import { ConfigProvider } from './config/ConfigProvider';
import AppRoutes from './core/AppRoutes';


function App() {
  return (
    <ConfigProvider>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </ConfigProvider>
  );
}
export default App;

