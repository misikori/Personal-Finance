import { BaseService } from "../../../core/http/BaseService";
import { budgetApi } from "../../../core/http/apiClients";
import { CreateWalletRequest, Wallet, Guid } from "../types/budgetServiceTypes";


class WalletsService extends BaseService {
  constructor() { super(budgetApi, "/api/Wallets"); }
  create(body: CreateWalletRequest) { return this.post<Wallet>("", body); }
  getById(walletId: Guid) { return this.get<Wallet>(`/${walletId}`); }
  getByUser(userId: Guid) { return this.get<Wallet[]>(`/user/${userId}`); }
}
export const walletsService = new WalletsService();