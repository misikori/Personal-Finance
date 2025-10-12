import { portfolioApi } from "../../../core/http/apiClients";
import { BaseService } from "../../../core/http/BaseService";
import { PortfolioSummaryDto, PortfolioDistributionDto } from "../types/portfolio";
import { RecommendationsDto } from "../types/recomendation";
import { TransactionDto } from "../types/transaction";



class PortfolioDataService extends BaseService {
    constructor() { super(portfolioApi, "/api/Portfolio"); }

    async summary(username: string, baseCurrency: string = "USD"): Promise<PortfolioSummaryDto> {
        return await this.get<PortfolioSummaryDto>(
            `/summary/${encodeURIComponent(username)}?baseCurrency=${encodeURIComponent(baseCurrency)}`
        );
    }

    async transactions(username: string): Promise<TransactionDto[]> {
        return await this.get<TransactionDto[]>(
            `/transactions/${encodeURIComponent(username)}`
        );
    }

    async distribution(username: string, baseCurrency: string = "USD"): Promise<PortfolioDistributionDto> {
        return await this.get<PortfolioDistributionDto>(
            `/distribution/${encodeURIComponent(username)}?baseCurrency=${encodeURIComponent(baseCurrency)}`
        );
    }

    async recommendations(): Promise<RecommendationsDto> {
        return await this.get<RecommendationsDto>(`/recommendations`);
    }
}

export const portfolioData = new PortfolioDataService();