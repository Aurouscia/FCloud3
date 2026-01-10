import { LocalConfigModel } from "../localConfigModel";

export interface WikiCenteredHomePageLocalConfig extends LocalConfigModel {
    latestWikiCount: number
}

export function wikiCenteredHomePageLocalConfigDefault(): WikiCenteredHomePageLocalConfig {
    return {
        key: 'wikiCenteredHomePage',
        version: '20260111',
        latestWikiCount: 10
    }
}