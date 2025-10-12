
import { useQuery } from "@tanstack/react-query";
import { PortfolioService } from "../services/PortfolioService";
import type { PriceQuoteDto, CandlestickResponseDto } from "../types/basic";
import type { PredictionDto } from "../types/transaction";
import { qk } from "./queryKeys";

type QueryOpts = {
  enabled?: boolean;
  staleTime?: number;
  gcTime?: number;
};

const DEFAULTS = {
  staleTime: 15_000,
  gcTime: 5 * 60_000,
};

export function usePrice(symbol: string, opts?: QueryOpts) {
  return useQuery<PriceQuoteDto>({
    queryKey: qk.market.price(symbol),                 // ["market","price",symbol]
    queryFn: () => PortfolioService.market.price(symbol),
    enabled: !!symbol && (opts?.enabled ?? true),
    staleTime: opts?.staleTime ?? DEFAULTS.staleTime,
    gcTime: opts?.gcTime ?? DEFAULTS.gcTime,
  });
}

export function useCandlesticks(symbol: string, opts?: QueryOpts) {
  return useQuery<CandlestickResponseDto>({
    queryKey: qk.market.candles(symbol),               // ["market","candles",symbol]
    queryFn: () => PortfolioService.market.candlesticks(symbol),
    enabled: !!symbol && (opts?.enabled ?? true),
    staleTime: opts?.staleTime ?? 60_000,
    gcTime: opts?.gcTime ?? 10 * 60_000,
  });
}

export function usePrediction(symbol: string, opts?: QueryOpts) {
  return useQuery<PredictionDto>({
    queryKey: qk.market.predict(symbol),               // ["market","predict",symbol]
    queryFn: () => PortfolioService.market.predict(symbol),
    enabled: !!symbol && (opts?.enabled ?? true),
    staleTime: opts?.staleTime ?? 60_000,
    gcTime: opts?.gcTime ?? 10 * 60_000,
  });
}
