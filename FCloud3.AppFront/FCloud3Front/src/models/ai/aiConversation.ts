export interface AiConversation {
    Id: number
    UserId: number
    AiInstanceConfigId: number
    Title: string | null
    CurrentWikiItemId: number
    ModelName: string | null
    MessageCount: number
    IsPinned: boolean
    IsArchived: boolean
    Created: string
    Updated: string
}

export interface AiMessage {
    Id: number
    ConversationId: number
    Role: AiMessageRole
    Content: string | null
    ToolCalls: string | null
    Order: number
    InputTokenCount: number
    OutputTokenCount: number
    ModelName: string | null
}

export enum AiMessageRole {
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3
}

export interface AiToolCallInfo {
    Name: string
    Arguments: string
}

export interface AiChatChunk {
    Text: string
    ToolCalls: AiToolCallInfo[] | null
}
