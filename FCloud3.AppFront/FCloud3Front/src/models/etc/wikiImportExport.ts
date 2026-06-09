import { WikiParaType } from "@/models/wiki/wikiParaType";

export interface ExportedWikiInfo {
    Title: string;
    UrlPathName: string;
    Description?: string;
    LastActive: string;
}

export interface ExportedWikiPara {
    ObjName?: string;
    ParaName?: string;
    ParaType: WikiParaType;
    Data?: string;
}

export interface ExportedWiki {
    Info: ExportedWikiInfo;
    Paras: ExportedWikiPara[];
}

export interface AttachmentsSummary {
    UrlBase: string;
    Materials: string[];
    FileItems: string[];
}

export interface WikiPreviewItem {
    Title: string;
    OriginalUrlPathName: string;
    ResolvedUrlPathName: string;
    HasConflict: boolean;
    ParaTypes: number[];
}

export interface FilePreviewItem {
    DisplayName: string;
    StorePathName: string;
    FullUrl: string;
    Size?: number;
    Accessible?: boolean;
}

export interface ImportPreview {
    Wikis: WikiPreviewItem[];
    Files: FilePreviewItem[];
    UrlBase: string;
}

export interface FileStatusResult {
    Url: string;
    Accessible: boolean;
    Size?: number;
}
