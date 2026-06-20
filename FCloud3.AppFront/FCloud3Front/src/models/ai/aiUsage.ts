export interface AiUsageRecord {
    Id: number
    UserId: number
    AiInstanceConfigId: number
    InputTokens: number
    OutputTokens: number
    TotalTokens: number
    ModelName: string | null
    Success: boolean
    PromptSummary: string | null
    RelatedWikiItemId: number
    ConversationId: number | null
    DurationMs: number
    CachedInputTokens: number
    Created: string
}

export interface ConfigUsageSummary {
    UserId: number
    TotalTokens: number
    CallCount: number
}
