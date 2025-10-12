import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { PortfolioService } from "../services/PortfolioService";
import type {
  PortfolioSummaryDto,
  PortfolioDistributionDto
} from "../types/portfolio";
import type { RecommendationsDto } from "../types/recomendation";
import type { CandlestickResponseDto, PriceQuoteDto } from "../types/basic";
import type { PredictionDto, TradeRequest, TransactionDto } from "../types/transaction";

const keys = {
  summary: (u: string, c: string) => ["portfolio","summary",u,c] as const,
  distribution: (u: string, c: string) => ["portfolio","distribution",u,c] as const,
  recommendations: () => ["portfolio","recommendations"] as const,
  transactions: (u: string) => ["portfolio","transactions",u] as const,
  price: (s: string) => ["market","price",s] as const,
  candles: (s: string, days: number) => ["market","candles",s,days] as const,
  prediction: (s: string) => ["market","prediction",s] as const,
};

export function usePortfolioSummary(username: string, opts: { baseCurrency: string }) {
  return useQuery<PortfolioSummaryDto>({
    queryKey: keys.summary(username, opts.baseCurrency),
    queryFn: () => PortfolioService.portfolio.summary(username, opts.baseCurrency),
    enabled: !!username
  });
}

export function usePortfolioDistribution(username: string, opts: { baseCurrency: string }) {
  return useQuery<PortfolioDistributionDto>({
    queryKey: keys.distribution(username, opts.baseCurrency),
    queryFn: () => PortfolioService.portfolio.distribution(username, opts.baseCurrency),
    enabled: !!username
  });
}

export function useRecommendations() {
  return useQuery<RecommendationsDto>({
    queryKey: keys.recommendations(),
    queryFn: () => PortfolioService.portfolio.recommendations()
  });
}

export function useTransactions(username: string) {
  return useQuery<TransactionDto[]>({
    queryKey: keys.transactions(username),
    queryFn: () => PortfolioService.portfolio.transactions(username),
    enabled: !!username
  });
}

export function usePrice(symbol: string) {
  return useQuery<PriceQuoteDto>({
    queryKey: keys.price(symbol),
    queryFn: () => PortfolioService.market.price(symbol),
    enabled: !!symbol
  });
}

export function useCandlesticks(symbol: string, days = 30) {
  return useQuery<CandlestickResponseDto>({
    queryKey: keys.candles(symbol, days),
    queryFn: () => PortfolioService.market.candlesticks(symbol, days),
    enabled: !!symbol
  });
}

export function usePrediction(symbol: string) {
  return useQuery<PredictionDto>({
    queryKey: keys.prediction(symbol),
    queryFn: () => PortfolioService.market.predict(symbol),
    enabled: !!symbol
  });
}

export function useBuyTrade() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: TradeRequest) => PortfolioService.trades.buy(req),
    onSuccess: (_res, req) => {
      // optimistic refetch: summary, distribution, transactions
      if (req.username) {
        qc.invalidateQueries({ queryKey: ["portfolio","summary", req.username] });
        qc.invalidateQueries({ queryKey: ["portfolio","distribution", req.username] });
        qc.invalidateQueries({ queryKey: ["portfolio","transactions", req.username] });
      }
    }
  });
}

export function useSellTrade() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: TradeRequest) => PortfolioService.trades.sell(req),
    onSuccess: (_res, req) => {
      if (req.username) {
        qc.invalidateQueries({ queryKey: ["portfolio","summary", req.username] });
        qc.invalidateQueries({ queryKey: ["portfolio","distribution", req.username] });
        qc.invalidateQueries({ queryKey: ["portfolio","transactions", req.username] });
      }
    }
  });
}
