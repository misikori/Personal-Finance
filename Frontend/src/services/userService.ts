import BaseService from "../shared/api/BaseService";

export type MeDto = { id: string; email: string; roles: string[] };

class UserService extends BaseService {
  constructor() {
    super("/user"); 
  }
  getMe() {
    
    return this.get<MeDto>("/me");
  }
}

export const userService = new UserService();