import { useEffect, useState } from "react";
import { CurrencyService } from "../services/currencyService";
import { Currency, CurrencyConvertRequest, CurrencyConvertResponse } from "../types/currencyTypes";

export function useCurrencies(baseCurrencyCode: string) {
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    CurrencyService.getCurrencies(baseCurrencyCode)
      .then(setCurrencies)
      .catch(e => setError(e.message || "Failed to fetch currencies"))
      .finally(() => setLoading(false));
  }, [baseCurrencyCode]);

  return { currencies, loading, error };
}

export function useCurrencyConvert() {
  const [result, setResult] = useState<CurrencyConvertResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const convert = async (params: CurrencyConvertRequest) => {
    setLoading(true);
    setError(null);
    try {
      const res = await CurrencyService.convertCurrency(params);
      setResult(res);
    } catch (e: any) {
      setError(e.message || "Conversion failed");
    } finally {
      setLoading(false);
    }
  };

  return { result, loading, error, convert };
}
