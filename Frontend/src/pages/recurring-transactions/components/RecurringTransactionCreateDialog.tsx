import * as React from "react";
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Stack, MenuItem
} from "@mui/material";
import { useWallets } from "../../transactions/hooks/useWallets";
import { BudgetService } from "../../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../../auth/store/authStore";
import { TransactionType } from "../../../domain/budget/types/transactionTypes";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

type Frequency = "Weekly" | "Monthly" | "Yearly";
const transactionTypes: TransactionType[] = ["Expense", "Income"];
const frequencies: Frequency[] = ["Weekly", "Monthly", "Yearly"];

const schema = z.object({
  walletId: z.string().min(1, "Wallet is required"),
  amount: z
    .string()
    .trim()
    .min(1, "Amount is required")
    .refine((v) => !Number.isNaN(Number(v)), "Amount must be a number")
    .refine((v) => Number(v) > 0, "Amount must be greater than 0"),
  currency: z
    .string()
    .trim()
    .min(1, "Currency is required")
    .regex(/^[A-Z]{3}$/, "Use a 3-letter code (e.g., EUR)"),
  description: z.string().trim().max(200, "Max 200 characters").optional().or(z.literal("")),
  transactionType: z.enum(["Expense", "Income"] as const),
  category: z.string().trim().max(80, "Max 80 characters").optional().or(z.literal("")),
  frequency: z.enum(["Weekly", "Monthly", "Yearly"] as const),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().optional().or(z.literal("")),
}).superRefine((val, ctx) => {
  // validate dates are valid ISO and end >= start if provided
  const startOk = !Number.isNaN(new Date(val.startDate).getTime());
  if (!startOk) ctx.addIssue({ code: z.ZodIssueCode.custom, message: "Invalid start date", path: ["startDate"] });

  if (val.endDate && val.endDate.length > 0) {
    const endOk = !Number.isNaN(new Date(val.endDate).getTime());
    if (!endOk) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, message: "Invalid end date", path: ["endDate"] });
    } else if (startOk && new Date(val.endDate) < new Date(val.startDate)) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, message: "End date must be on/after start date", path: ["endDate"] });
    }
  }
});

type FormValues = z.infer<typeof schema>;

export default function RecurringTransactionCreateDialog({
  open, onClose, onCreated,
}: {
  open: boolean;
  onClose: () => void;
  onCreated: () => void;
}) {
  const { wallets } = useWallets();
  const defaultWalletId = wallets[0]?.id ?? "";
  const defaultCurrency = wallets[0]?.currency ?? "";

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting, isValid },
    reset,
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    mode: "onChange",
    defaultValues: {
      walletId: defaultWalletId,
      amount: "",
      currency: defaultCurrency,
      description: "",
      transactionType: "Expense",
      category: "",
      frequency: "Monthly",
      startDate: "",
      endDate: "",
    },
  });

  // reset defaults when dialog opens or wallets change
  React.useEffect(() => {
    if (open) {
      reset((prev) => ({
        ...prev,
        walletId: wallets[0]?.id ?? "",
        currency: wallets[0]?.currency ?? "",
      }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, wallets]);

  // auto-sync currency when wallet changes
  const onWalletChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setValue("walletId", val, { shouldValidate: true });
    const w = wallets.find(w => String(w.id) === val);
    if (w?.currency) setValue("currency", w.currency, { shouldValidate: true });
  };

  const onSubmit = async (data: FormValues) => {
    const userId = getCurrentUser()?.id;
    if (!userId) throw new Error("User not found");

    await BudgetService.recurring.create({
      userId,
      walletId: data.walletId,
      amount: Number(data.amount),
      transactionType: data.transactionType,
      description: data.description || "",
      category: data.category || "",
      currency: data.currency.toUpperCase(),
      frequency: data.frequency,
      startDate: data.startDate,
      endDate: data.endDate || "",
      nextDueDate: data.startDate, // initial due date
    });

    onCreated();
    onClose();
    reset();
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Add Recurring Transaction</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            select
            label="Wallet"
            fullWidth
            required
            defaultValue={defaultWalletId}
            {...register("walletId")}
            onChange={onWalletChange}
            error={!!errors.walletId}
            helperText={errors.walletId?.message}
          >
            {wallets.map(w => (
              <MenuItem key={w.id} value={w.id}>{w.name} ({w.currency})</MenuItem>
            ))}
          </TextField>

          <TextField
            label="Amount"
            type="number"
            fullWidth
            required
            inputProps={{ step: "0.01", min: "0" }}
            {...register("amount")}
            error={!!errors.amount}
            helperText={errors.amount?.message}
          />

          <TextField
            label="Currency (e.g., EUR)"
            fullWidth
            required
            inputProps={{ maxLength: 3, style: { textTransform: "uppercase" } }}
            {...register("currency")}
            error={!!errors.currency}
            helperText={errors.currency?.message}
          />

          <TextField
            label="Description"
            fullWidth
            {...register("description")}
            error={!!errors.description}
            helperText={errors.description?.message}
          />

          <TextField
            select
            label="Type"
            fullWidth
            required
            defaultValue={"Expense"}
            {...register("transactionType")}
            error={!!errors.transactionType}
            helperText={errors.transactionType?.message}
          >
            {transactionTypes.map(type => (
              <MenuItem key={type} value={type}>{type}</MenuItem>
            ))}
          </TextField>

          <TextField
            label="Category"
            fullWidth
            {...register("category")}
            error={!!errors.category}
            helperText={errors.category?.message}
          />

          <TextField
            select
            label="Frequency"
            fullWidth
            required
            defaultValue={"Monthly"}
            {...register("frequency")}
            error={!!errors.frequency}
            helperText={errors.frequency?.message}
          >
            {frequencies.map(freq => (
              <MenuItem key={freq} value={freq}>{freq}</MenuItem>
            ))}
          </TextField>

          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <TextField
              label="Start Date"
              type="date"
              fullWidth
              required
              InputLabelProps={{ shrink: true }}
              {...register("startDate")}
              error={!!errors.startDate}
              helperText={errors.startDate?.message}
            />
            <TextField
              label="End Date"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              {...register("endDate")}
              error={!!errors.endDate}
              helperText={errors.endDate?.message}
            />
          </Stack>
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={isSubmitting}>Cancel</Button>
        <Button
          onClick={handleSubmit(onSubmit)}
          variant="contained"
          disabled={!isValid || isSubmitting}
        >
          Add
        </Button>
      </DialogActions>
    </Dialog>
  );
}
