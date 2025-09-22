import { SxProps, Theme } from "@mui/material/styles";

export const paperSx: SxProps<Theme> = (theme) => ({
  overflow: "hidden",
  backgroundImage: "none",
  boxShadow: theme.palette.mode === "dark" ? "0 0 0 1px rgba(255,255,255,.06)" : "0 0 0 1px rgba(0,0,0,.04)",
});

export const containerSx: SxProps<Theme> = {
  // fixed container: no internal scroll
  maxHeight: "none",
};

export const footerRowSx: SxProps<Theme> = (theme) => ({
  "& td": { fontWeight: 600, borderTop: `1px solid ${theme.palette.divider}` },
});

export const plPosSx: SxProps<Theme> = { color: (t) => t.palette.success.main };
export const plNegSx: SxProps<Theme> = { color: (t) => t.palette.error.main };

export const headerCellSx: SxProps<Theme> = (theme) => ({
  backgroundColor:
    theme.palette.mode === "dark" ? "rgba(255,255,255,0.04)" : "rgba(0,0,0,0.02)",
  fontWeight: 600,
});
