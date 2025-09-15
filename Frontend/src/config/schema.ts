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
})

export type AppConfig = z.infer<typeof AppConfigSchema>;