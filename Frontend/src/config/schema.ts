import {z} from "zod";

export const AppConfigSchema = z.object({

    API_BASE_URL:z.string().min(1),
    MOCK: z.boolean().optional().default(false),
    FEATURES: z.object({
        import: z.boolean().optional().default(false),
        portfolios: z.boolean().optional().default(false),
    }).partial().default({}),
    DEFAULT_CURRENCY: z.string().optional().default("EUR"),
    LOCALE:z.string().optional().default("en-US"),
    API_BUGET_URL:z.string().min(1),
    API_PORTFOLIO_URL:z.string().min(1),
    API_CURRENCY_URL:z.string().min(1)
})

export type AppConfig = z.infer<typeof AppConfigSchema>;