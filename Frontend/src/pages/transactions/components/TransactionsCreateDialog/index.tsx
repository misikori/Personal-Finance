import { useEffect, useMemo, useState } from "react";
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, MenuItem, Stack
} from "@mui/material";
import { transactionsService } from "../../../../domain/budget/services/TransactionsService";
import { walletsService } from "../../../../domain/budget/services/WalletsService";
import { Wallet } from "../../../../domain/budget/types/budgetServiceTypes";
import type { TransactionType } from "../../../../domain/budget/types/transactionTypes";
import { useCurrencies } from "../../../../domain/currency/hooks/useCurrency";

import { useForm, SubmitHandler, Resolver } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { getCurrentUser } from "../../../../auth/store/authStore";

type Props = { open: boolean; onClose: () => void; onCreated?: () => void; };

const typeOptions: TransactionType[] = ["Income", "Expense"];

const makeSchema = (allowedCurrencies: string[]) =>
  z.object({
    walletId: z.string().min(1, "Wallet is required"),
    type: z.enum(["Income", "Expense"] as const), // <-- no required_error here
    amount: z.coerce.number().positive("Amount must be greater than 0"),
    currency: z
      .string()
      .min(1, "Currency is required")
      .refine(v => allowedCurrencies.includes(v), "Invalid currency"),
    categoryName: z.string().trim().max(80, "Max 80 characters").optional().or(z.literal("")),
    description: z.string().trim().max(200, "Max 200 characters").optional().or(z.literal("")),
    dateLocal: z
      .string()
      .min(1, "Date & Time is required")
      .refine(v => !Number.isNaN(new Date(v).getTime()), "Invalid date/time"),
  });

type FormValues = z.infer<ReturnType<typeof makeSchema>>;

export default function TransactionsCreateDialog({ open, onClose, onCreated }: Props) {
  const [wallets, setWallets] = useState<Wallet[]>([]);
  const [loading, setLoading] = useState(false);

  const currencies = useCurrencies("USD").currencies as { code: string }[];
  const currencyCodes = currencies.map(c => c.code);
  const schema = useMemo(() => makeSchema(currencyCodes), [currencyCodes]);

  const nowLocal = useMemo(() => new Date().toISOString().slice(0, 16), []);
  const resolver = zodResolver(schema) as Resolver<FormValues>;
  const {
    register,
    handleSubmit,
    setValue,
    reset,
    watch,
    formState: { errors, isValid, isSubmitting },
  } = useForm<FormValues>({
    resolver,           // <-- typed resolver
    mode: "onChange",
    defaultValues: {
      walletId: "",
      type: "Expense",
      amount: 0,
      currency: "",
      categoryName: "",
      description: "",
      dateLocal: nowLocal,
    },
  });

  const selectedWalletId = watch("walletId");

  useEffect(() => {
    if (!open) return;
    let mounted = true;
    (async () => {
      try {
        const user = getCurrentUser();
        if (!user) return;
        const ws = await walletsService.getByUser(user?.id); // supply userId if your service requires
        if (!mounted) return;
        setWallets(ws);
        if (ws.length) {
          setValue("walletId", ws[0].id, { shouldValidate: true });
          setValue("currency", ws[0].currency, { shouldValidate: true });
        }
      } catch {
        /* ignore */
      }
    })();
    return () => {
      mounted = false;
    };
  }, [open, setValue]);

  // sync currency when wallet changes
  useEffect(() => {
    if (!selectedWalletId) return;
    const w = wallets.find(x => x.id === selectedWalletId);
    if (w?.currency) setValue("currency", w.currency, { shouldValidate: true });
  }, [selectedWalletId, wallets, setValue]);

  const walletOptions = useMemo(
    () => wallets.map(w => <MenuItem key={w.id} value={w.id}>{w.name} ({w.currency})</MenuItem>),
    [wallets]
  );

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    setLoading(true);
    try {
      await transactionsService.create({
        userId: getCurrentUser()?.id,
        walletId: data.walletId,
        amount: Math.abs(data.amount),
        type: data.type as TransactionType,
        description: data.description || undefined,
        date: new Date(data.dateLocal).toISOString(), // UTC instant
        currency: data.currency,
        categoryName: data.categoryName || undefined,
      });
      onCreated?.();
      onClose();
      reset();
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>New Transaction</DialogTitle>
      <DialogContent dividers sx={{ pt: 2 }}>
        <Stack spacing={2}>
          <TextField
            select
            label="Wallet"
            {...register("walletId")}
            error={!!errors.walletId}
            helperText={errors.walletId?.message}
          >
            {walletOptions}
          </TextField>

          <TextField
            select
            label="Type"
            {...register("type")}
            error={!!errors.type}
            helperText={errors.type?.message}
          >
            {typeOptions.map(t => <MenuItem key={t} value={t}>{t}</MenuItem>)}
          </TextField>

          <TextField
            label="Amount"
            type="number"
            inputProps={{ step: "0.01", min: "0" }}
            {...register("amount")}
            error={!!errors.amount}
            helperText={errors.amount?.message}
          />

          <TextField
            select
            label="Currency"
            {...register("currency")}
            error={!!errors.currency}
            helperText={errors.currency?.message}
          >
            {currencyCodes.map(ccy => <MenuItem key={ccy} value={ccy}>{ccy}</MenuItem>)}
          </TextField>

          <TextField
            label="Category"
            {...register("categoryName")}
            error={!!errors.categoryName}
            helperText={errors.categoryName?.message}
          />

          <TextField
            label="Description"
            {...register("description")}
            error={!!errors.description}
            helperText={errors.description?.message}
          />

          <TextField
            label="Date & Time"
            type="datetime-local"
            InputLabelProps={{ shrink: true }}
            {...register("dateLocal")}
            error={!!errors.dateLocal}
            helperText={errors.dateLocal?.message}
          />
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={loading || isSubmitting}>Cancel</Button>
        <Button
          onClick={handleSubmit(onSubmit)}
          variant="contained"
          disabled={loading || isSubmitting || !isValid}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
}
