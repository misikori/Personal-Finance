
import { useQuery } from "@tanstack/react-query";
import { PortfolioService } from "../services/PortfolioService";
import type {
  PortfolioSummaryDto,
  PortfolioDistributionDto,
} from "../types/portfolio";
import type { RecommendationsDto } from "../types/recomendation";
import type { TransactionDto } from "../types/transaction";
import { qk } from "./queryKeys";

type CurrencyParams = { baseCurrency?: string };
type QueryOpts = { enabled?: boolean; staleTime?: number; gcTime?: number };

export function usePortfolioSummary(
  username: string,
  params?: CurrencyParams,
  opts?: QueryOpts
) {
  return useQuery<PortfolioSummaryDto>({
    queryKey: qk.portfolio.summary(username, params?.baseCurrency),
    queryFn: () => PortfolioService.portfolio.summary(username, params),
    enabled: !!username && (opts?.enabled ?? true),
    staleTime: opts?.staleTime ?? 30_000,
    gcTime: opts?.gcTime ?? 10 * 60_000,
  });
}

export function usePortfolioTransactions(
  username: string,
  opts?: QueryOpts
) {
  return useQuery<TransactionDto[]>({
    queryKey: qk.portfolio.transactions(username),
    queryFn: () => PortfolioService.portfolio.transactions(username),
    enabled: !!username && (opts?.enabled ?? true),
    staleTime: opts?.staleTime ?? 10_000,
    gcTime: opts?.gcTime ?? 5 * 60_000,
  });
}

export function usePortfolioDistribution(
  username: string,
  params?: CurrencyParams,
  opts?: QueryOpts
) {
  return useQuery<PortfolioDistributionDto>({
    queryKey: qk.portfolio.distribution(username, params?.baseCurrency),
    queryFn: () => PortfolioService.portfolio.distribution(username, params),
    enabled: !!username && (opts?.enabled ?? true),
    staleTime: opts?.staleTime ?? 60_000,
    gcTime: opts?.gcTime ?? 10 * 60_000,
  });
}

export function useRecommendations(opts?: QueryOpts) {
  return useQuery<RecommendationsDto>({
    queryKey: qk.portfolio.recommendations(),
    queryFn: () => PortfolioService.portfolio.recommendations(),
    enabled: opts?.enabled ?? true,
    staleTime: opts?.staleTime ?? 60_000,
    gcTime: opts?.gcTime ?? 10 * 60_000,
  });
}

