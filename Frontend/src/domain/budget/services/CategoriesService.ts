import { BaseService } from "../../../core/http/BaseService";
import { budgetApi } from "../../../core/http/apiClients";
import { Category, CreateCategoryRequest, Guid } from "../types/budgetServiceTypes";

class CategoriesService extends BaseService {
  constructor() { super(budgetApi, "/api/Categories"); }
  listByUser(userId: Guid) { return this.get<Category[]>("", { userId }); }
  create(body: CreateCategoryRequest) { return this.post<Category>("", body); }
}
export const categoriesService = new CategoriesService();