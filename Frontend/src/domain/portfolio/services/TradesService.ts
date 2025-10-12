import { portfolioApi } from "../../../core/http/apiClients";
import { BaseService } from "../../../core/http/BaseService";
import { TradeRequest, TransactionDto } from "../types/transaction";


class TradesService extends BaseService{
    constructor() { super(portfolioApi, ""); }

    async buy(req: TradeRequest): Promise<TransactionDto> {
    return await this.post<TransactionDto>(`api/buy`, req);
}


async sell(req: TradeRequest): Promise<TransactionDto> {
return await this.post<TransactionDto>(`/api/sell`, req);
}

}

export const tradeService = new TradesService();