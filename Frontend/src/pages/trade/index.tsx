import { useEffect, useMemo, useState } from "react";
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, FormControl,
  FormControlLabel, InputLabel, MenuItem, Radio, RadioGroup, Select,
  Stack, TextField, Typography,
} from "@mui/material";
import { z } from "zod";
import { SubmitHandler, useForm } from "react-hook-form";
import Grid from "@mui/material/Grid";
import { zodResolver } from "@hookform/resolvers/zod";
import { tradeService } from "../../domain/portfolio/services/TradesService";
import { walletsService } from "../../domain/budget/services/WalletsService";
import type { TransactionDto, TradeRequest } from "../../domain/portfolio/types/transaction";
import type { Wallet, Guid } from "../../domain/budget/types/budgetServiceTypes";
import { getCurrentUser } from "../../auth/store/authStore";

const schema = z.object({
  side: z.enum(["Buy", "Sell"]),
  symbol: z.string().trim().min(1, "Symbol is required").transform(s => s.toUpperCase()),
  quantity: z.number().int("Whole shares only").positive("Quantity must be > 0"),
  walletId: z.string().min(1, "Select a wallet"),
});
type FormValues = z.infer<typeof schema>;
export default function TradePage() {
const user = getCurrentUser();
const username = user?.email ?? "";     
const userId = user?.id as Guid | undefined;
const {
  register,
  handleSubmit,
  formState: { errors, isSubmitting },
  reset,
  watch,
  setValue,
} = useForm<FormValues, any, FormValues>({
  resolver: zodResolver(schema),   // <-- no generic here
  defaultValues: { side: "Buy", symbol: "", quantity: 1, walletId: "" },
});
  const [wallets, setWallets] = useState<Wallet[]>([]);
  const [wLoading, setWLoading] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;
    if (!userId) return;

    (async () => {
      setWLoading(true);
      try {
        const res = await walletsService.getByUser(userId);
        if (alive) {
          setWallets(res);
          // auto-select first wallet if none chosen
          if (res.length && !watch("walletId")) setValue("walletId", String(res[0].id));
        }
      } catch (e: any) {
        if (alive) setApiError(e?.message ?? "Failed to load wallets");
      } finally {
        if (alive) setWLoading(false);
      }
    })();

    return () => {
      alive = false;
    };
  }, [userId]); // eslint-disable-line react-hooks/exhaustive-deps

  const onSubmit: SubmitHandler<FormValues> = async (values: FormValues) => {
    setApiError(null);
    setSuccess(null);

    if (!username) {
      setApiError("No username found. Please sign in again.");
      return;
    }

    try {
      const req: TradeRequest = {
        username,
        symbol: values.symbol,
        quantity: values.quantity,
        walletId: values.walletId,  // Pass walletId to Portfolio API
      };

      // Portfolio API handles both the trade AND budget deduction
      const trade: TransactionDto =
        values.side === "Buy" ? await tradeService.buy(req) : await tradeService.sell(req);

      setSuccess(`${values.side} ${trade.quantity} ${trade.symbol} completed. Budget updated.`);
      reset({ ...values, symbol: "", quantity: 1 }); // keep side/wallet
    } catch (e: any) {
      setApiError(e?.message ?? "Trade failed");
    }
  };

  const side = watch("side");

  const walletMenu = useMemo(
    () =>
      wallets.map((w) => (
        <MenuItem key={String(w.id)} value={String(w.id)}>
          {w.name} ({w.currency})
        </MenuItem>
      )),
    [wallets]
  );

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Trade
      </Typography>

      <Card variant="outlined">
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <Stack spacing={2}>
              {apiError && <Alert severity="error">{apiError}</Alert>}
              {success && <Alert severity="success">{success}</Alert>}

              {/* Side */}
              <FormControl>
                <RadioGroup
                  row
                  value={watch("side")}
                  onChange={(e) => setValue("side", e.target.value as "Buy" | "Sell")}
                >
                  <FormControlLabel value="Buy" control={<Radio />} label="Buy" />
                  <FormControlLabel value="Sell" control={<Radio />} label="Sell" />
                </RadioGroup>
              </FormControl>

              <Grid container spacing={2}>
                <Grid>
                  <TextField
                    label="Symbol"
                    placeholder="AAPL"
                    fullWidth
                    {...register("symbol")}
                    error={!!errors.symbol}
                    helperText={errors.symbol?.message}
                    inputProps={{ style: { textTransform: "uppercase" } }}
                  />
                </Grid>

                <Grid  >
                  <TextField
                    type="number"
                    label="Quantity"
                    fullWidth
                    // For zod + RHF, manually coerce to number:
                    {...register("quantity", { valueAsNumber: true })}
                    error={!!errors.quantity}
                    helperText={errors.quantity?.message}
                    inputProps={{ min: 1, step: 1 }}
                  />
                </Grid>

                <Grid >
                  <FormControl fullWidth error={!!errors.walletId}>
                    <InputLabel id="wallet-select-label">Wallet</InputLabel>
                    <Select
                      labelId="wallet-select-label"
                      label="Wallet"
                      value={watch("walletId")}
                      onChange={(e) => setValue("walletId", e.target.value)}
                    >
                      {wLoading ? (
                        <MenuItem value="" disabled>
                          <CircularProgress size={18} />
                        </MenuItem>
                      ) : (
                        walletMenu
                      )}
                    </Select>
                    {errors.walletId?.message && (
                      <Typography variant="caption" color="error">
                        {errors.walletId.message}
                      </Typography>
                    )}
                  </FormControl>
                </Grid>
              </Grid>

              <Stack direction="row" spacing={2}>
                <Button type="submit" variant="contained" disabled={isSubmitting || wLoading}>
                  {isSubmitting ? "Processing…" : `${side} & Update Budget`}
                </Button>
                <Button type="button" variant="outlined" onClick={() => setSuccess(null)}>
                  Clear Message
                </Button>
              </Stack>

              <Typography variant="body2" color="text.secondary">
                This will {side === "Buy" ? "create an Expense" : "create an Income"} in the selected wallet.
              </Typography>
            </Stack>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
