export interface RecommendationsItemDto {
symbol: string;
currentPrice: number;
predictedPrice: number;
expectedChange: number;
expectedChangePercent: number;
action: "Buy" | "Sell" | "Hold" | string;
confidence: number; // 0..1 or 0..100 (keep number, UI can format)
strength: string; // e.g., "Strong", "Moderate"
reason: string;
trend: string;
}


export interface RecommendationsDto {
analysisDate: string; // ISO
timeframe: string;
stocksAnalyzed: number;
buyRecommendations: RecommendationsItemDto[];
sellRecommendations: RecommendationsItemDto[];
holdRecommendations: RecommendationsItemDto[];
}