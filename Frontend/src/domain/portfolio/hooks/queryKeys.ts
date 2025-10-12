export const qk = {
  market: {
    root: ["market"] as const,
    price: (symbol: string) => [...qk.market.root, "price", symbol] as const,
    candles: (symbol: string) => [...qk.market.root, "candles", symbol] as const,
    predict: (symbol: string) => [...qk.market.root, "predict", symbol] as const,
  },
  portfolio: {
    root: ["portfolio"] as const,
    summary: (username: string, baseCurrency?: string) =>
      [...qk.portfolio.root, "summary", username, baseCurrency ?? ""] as const,
    transactions: (username: string) =>
      [...qk.portfolio.root, "transactions", username] as const,
    distribution: (username: string, baseCurrency?: string) =>
      [...qk.portfolio.root, "distribution", username, baseCurrency ?? ""] as const,
    recommendations: () => [...qk.portfolio.root, "recommendations"] as const,
  },
  trades: {
    root: ["trades"] as const,
  },
};