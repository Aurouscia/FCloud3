export interface AiInstanceConfig {
    Id: number
    GroupId: number
    ApiBaseUrl: string | null
    ApiKey: string | null
    ModelName: string | null
    SystemPrompt: string | null
    Enabled: boolean
    DefaultDirId: number
    MaxContextMessages: number
    DailyTokenLimit: number
    MonthlyTokenLimit: number
}

export interface AiInstanceConfigEditModel {
    Id: number
    GroupId: number
    ApiBaseUrl: string | null
    ApiKey: string | null
    ModelName: string | null
    SystemPrompt: string | null
    Enabled: boolean
    DefaultDirId: number
    MaxContextMessages: number
    DailyTokenLimit: number
    MonthlyTokenLimit: number
}

export interface AiInstanceConfigSummary {
    Id: number
    GroupId: number
    GroupName: string | null
    ModelName: string | null
    SystemPrompt: string | null
}
