import MockAdapter from "axios-mock-adapter";
import apiClient from "../shared/api/apiClient";
import type { ApiResponse } from "../shared/api/types";
import { getCurrentUser } from "../auth/services/authService";

export function setupApiMocks() {
  const mock = new MockAdapter(apiClient, { delayResponse: 300 });


  mock.onGet(/\/user\/me$/).reply(() => {
    const user = getCurrentUser();
    if (!user) {

      return [401, { message: "Unauthorized" }];
    }
    const payload: ApiResponse<typeof user> = {
      success: true,
      data: user,
      message: "ok",
    };
    return [200, payload];
  });

  mock.onGet(/\/ping$/).reply(200, { success: true, data: "pong" });

  return mock;
}
