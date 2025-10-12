import { useMutation, useQueryClient } from "@tanstack/react-query";
import { PortfolioService } from "../services/PortfolioService";
import type { TradeRequest, TransactionDto } from "../types/transaction";
import { qk } from "./queryKeys";


export function useBuyMutation(usernameToInvalidate?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: TradeRequest) => PortfolioService.trades.buy(req),
    onSuccess: async (_tx: TransactionDto) => {
      // Invalidate data tied to holdings & transactions
      if (usernameToInvalidate) {
        await Promise.all([
          qc.invalidateQueries({ queryKey: qk.portfolio.summary(usernameToInvalidate) }),
          qc.invalidateQueries({ queryKey: qk.portfolio.transactions(usernameToInvalidate) }),
          qc.invalidateQueries({ queryKey: qk.portfolio.distribution(usernameToInvalidate) }),
        ]);
      }
    },
  });
}

export function useSellMutation(usernameToInvalidate?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: TradeRequest) => PortfolioService.trades.sell(req),
    onSuccess: async (_tx: TransactionDto) => {
      if (usernameToInvalidate) {
        await Promise.all([
          qc.invalidateQueries({ queryKey: qk.portfolio.summary(usernameToInvalidate) }),
          qc.invalidateQueries({ queryKey: qk.portfolio.transactions(usernameToInvalidate) }),
          qc.invalidateQueries({ queryKey: qk.portfolio.distribution(usernameToInvalidate) }),
        ]);
      }
    },
  });
}
