import { portfolioApi } from "../../../core/http/apiClients";
import { BaseService } from "../../../core/http/BaseService";
import { TradeRequest, TransactionDto } from "../types/transaction";


class TradesService extends BaseService{
    constructor() { super(portfolioApi, "/api/Portfolio"); }

    async buy(req: TradeRequest): Promise<TransactionDto> {
    return await this.post<TransactionDto>(`buy`, req);
}


async sell(req: TradeRequest): Promise<TransactionDto> {
return await this.post<TransactionDto>(`sell`, req);
}

}

export const tradeService = new TradesService();