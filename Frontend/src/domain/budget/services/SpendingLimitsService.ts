import { BaseService } from "../../../core/http/BaseService";
import { budgetApi } from "../../../core/http/apiClients";
import { CreateSpendingLimitRequest, Guid, SpendingLimit } from "../types/budgetServiceTypes";


class SpendingLimitsService extends BaseService {
  constructor() { super(budgetApi, "/api/SpendingLimits"); }
  create(body: CreateSpendingLimitRequest) {
    return this.post<SpendingLimit>("", body);
  }
  getByWallet(walletId: Guid) {
    return this.get<SpendingLimit[]>(`/wallet/${walletId}`);
  }
}
export const spendingLimitsService = new SpendingLimitsService();