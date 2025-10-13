import { marketService } from "./MarketDataService";
import { portfolioData } from "./PortfolioDataService";
import { tradeService } from "./TradesService";


export const  PortfolioService ={
    trades: tradeService,
    portfolio: portfolioData,
    market: marketService,
}