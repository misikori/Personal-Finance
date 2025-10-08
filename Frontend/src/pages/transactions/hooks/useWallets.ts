import { useEffect, useState } from "react";
import { Wallet } from "../../../domain/budget/types/budgetServiceTypes";
import { BudgetService } from "../../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../../auth/store/authStore";


export function useWallets() {
  const [data, setData] = useState<Wallet[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let mounted = true;
    const run = async () => {
      setLoading(true);
      try {
        const userId = getCurrentUser()?.id;
        if (!userId) { setData([]); return; }
        const wallets = await BudgetService.wallets.getByUser(userId);
        if (mounted) setData(wallets);
      } finally {
        if (mounted) setLoading(false);
      }
    };
    run();
    return () => { mounted = false; };
  }, []);

  return { wallets: data, loading };
}