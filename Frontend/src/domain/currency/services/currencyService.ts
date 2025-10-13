import { currencyApi } from "../../../core/http/apiClients";
import { Currency, CurrencyConvertRequest, CurrencyConvertResponse } from "../types/currencyTypes";


export const CurrencyService = {
  async getCurrencies(baseCurrencyCode: string): Promise<Currency[]> {
    const response = await currencyApi.get("/api/Currency", {
      params: { baseCurrencyCode },
    });
    return response.data.rates;
  },

  async convertCurrency(params: CurrencyConvertRequest): Promise<CurrencyConvertResponse> {
    const response = await currencyApi.get("/api/Currency/convert", {
      params,
    });
    return response.data;
  },
};
