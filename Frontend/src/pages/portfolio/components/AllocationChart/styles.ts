import { SxProps, Theme } from "@mui/material/styles";

export const rootPaperSx: SxProps<Theme> = (theme) => ({
  p: 2,
  backgroundImage: "none",
  boxShadow: theme.palette.mode === "dark" ? "0 0 0 1px rgba(255,255,255,.06)" : "0 0 0 1px rgba(0,0,0,.04)",
});

export const titleSx: SxProps<Theme> = { mb: 1 };

export const containerStackSx: SxProps<Theme> = {
  // spacing controlled by parent
  alignItems: "center",
};

export const svgBoxSx: SxProps<Theme> = { flexShrink: 0 };

export const legendSx: SxProps<Theme> = { minWidth: 240 };

export const legendItemDotSx: SxProps<Theme> = (theme) => ({
  width: 10,
  height: 10,
  borderRadius: "50%",
  display: "inline-block",
  marginRight: theme.spacing(1),
});
