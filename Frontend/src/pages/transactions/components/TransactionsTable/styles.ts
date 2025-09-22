import { SxProps, Theme } from "@mui/material/styles";

export const tablePaperSx: SxProps<Theme> = {
  overflow: "hidden",
};

export const tableContainerSx: SxProps<Theme> = {
  maxHeight: 560,
};

export const focusableRowSx: SxProps<Theme> = {
  "&:focus": (theme) => ({
    outline: `2px solid ${theme.palette.primary.main}`,
    outlineOffset: -2,
  }),
};

export const paginationBarSx: SxProps<Theme> = {
  p: 1.5,
};

export const amountPositiveSx: SxProps<Theme> = {
  color: (t) => t.palette.success.main,
};

export const amountNegativeSx: SxProps<Theme> = {
  color: (t) => t.palette.error.main,
};
