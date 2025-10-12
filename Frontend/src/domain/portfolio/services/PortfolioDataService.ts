import { portfolioApi } from "../../../core/http/apiClients";
import { BaseService } from "../../../core/http/BaseService";
import { PortfolioSummaryDto, PortfolioDistributionDto } from "../types/portfolio";
import { RecommendationsDto } from "../types/recomendation";
import { TransactionDto } from "../types/transaction";



class PortfolioDataService extends BaseService{
    constructor() { super(portfolioApi, ""); }

        async summary(username: string, params?: { baseCurrency?: string }): Promise<PortfolioSummaryDto> {
            return await this.get<PortfolioSummaryDto>(
                `api/Portfolio/summary/${encodeURIComponent(username)}`,
                { params }
            );
        }


    async transactions(username: string): Promise<TransactionDto[]> {
    return await this.get<TransactionDto[]>(
    `api/Portfolio/transactions/${encodeURIComponent(username)}`
    );
    }


    async distribution(username: string, params?: { baseCurrency?: string }): Promise<PortfolioDistributionDto> {
    return await this.get<PortfolioDistributionDto>(
    `api/Portfolio/distribution/${encodeURIComponent(username)}`,
    { params }
    );
    }


    async recommendations(): Promise<RecommendationsDto> {
    return await this.get<RecommendationsDto>(`api/recommendations`);
    }
}

export const portfolioData = new PortfolioDataService();