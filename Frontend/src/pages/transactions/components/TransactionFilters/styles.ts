import { SxProps, Theme } from "@mui/material/styles";

export const formRootSx: SxProps<Theme> = { mb: 2 };

export const rowStackSx: SxProps<Theme> = {
  "& .MuiTextField-root": { minWidth: 160 },
  "& .date-from, & .date-to": { minWidth: 180 },
  "& .types-select": { minWidth: 220 },
  "& .account-select": { minWidth: 180 },
};
