export interface WikiRulesCommonsResult {
    Items: WikiRulesCommonsResultItem[];
}

export interface WikiRulesCommonsResultItem {
    RuleName?: string;
    Styles?: string;
    PreScripts?: string;
    PostScripts?: string;
}
