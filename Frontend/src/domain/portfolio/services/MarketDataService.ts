import { portfolioApi } from "../../../core/http/apiClients";
import { BaseService } from "../../../core/http/BaseService";
import { PriceQuoteDto, CandlestickResponseDto } from "../types/basic";
import { PredictionDto } from "../types/transaction";


class MarketService extends BaseService{
    constructor() { super(portfolioApi, "Portfolio"); }

async price(symbol: string): Promise<PriceQuoteDto> {
return await this.get<PriceQuoteDto>(`price/${encodeURIComponent(symbol)}`);
}


async candlesticks(symbol: string): Promise<CandlestickResponseDto> {
return  await this.get<CandlestickResponseDto>(
`api/candlestick/${encodeURIComponent(symbol)}`
);

}


async predict(symbol: string): Promise<PredictionDto> {
return await this.get<PredictionDto>(`predict/${encodeURIComponent(symbol)}`);

}

}

export const marketService = new MarketService();