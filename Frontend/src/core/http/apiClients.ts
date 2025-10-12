import { createApiClient } from "./apiClientFactory";
import { loadAppConfig } from "../../config/appConfig";

const cfg = loadAppConfig();

export const budgetApi    = createApiClient(cfg.API_BUGET_URL ?? cfg.API_BASE_URL);
export const authApi = createApiClient(cfg.API_BASE_URL)
export const portfolioApi = createApiClient(cfg.API_PORTFOLIO_URL ?? cfg.API_BASE_URL);
export const currencyApi  = createApiClient(cfg.API_CURRENCY_URL ?? cfg.API_BASE_URL);