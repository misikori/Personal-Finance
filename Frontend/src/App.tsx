import './App.css'
import { AuthProvider } from './auth/AuthContext';
import { ConfigProvider } from './config/ConfigProvider';
import AppRoutes from './core/AppRoutes';
import ColorModeProvider from './theme/ColorModeProvider';



function App() {
  return (
    <ColorModeProvider>
      <ConfigProvider>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </ConfigProvider>
    </ColorModeProvider>
  );
}
export default App;

