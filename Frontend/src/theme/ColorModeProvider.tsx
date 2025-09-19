import React from "react";
import { ThemeProvider, createTheme, CssBaseline } from "@mui/material";

type ColorModeContextType = {
  toggle: () => void;
};

export const ColorModeContext = React.createContext<ColorModeContextType | undefined>(undefined);

export function useColorMode() {
  const ctx = React.useContext(ColorModeContext);
  if (!ctx) {
    throw new Error("useColorMode must be used within a ColorModeProvider");
  }
  return ctx;
}
export default function ColorModeProvider({ children }: { children: React.ReactNode }) {
  const [mode, setMode] = React.useState<"light" | "dark">("dark");

  const theme = React.useMemo(
    () => createTheme({ palette: { mode } }),
    [mode]
  );

  const value = React.useMemo(
    () => ({ toggle: () => setMode(m => (m === "light" ? "dark" : "light")) }),
    []
  );

  return (
    <ColorModeContext.Provider value={value}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ColorModeContext.Provider>
  );
}