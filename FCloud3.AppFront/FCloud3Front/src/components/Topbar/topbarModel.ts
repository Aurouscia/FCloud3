export interface TopbarModel {
    Items: TopbarModelItem[]
}

export interface TopbarModelItem{
    Title: string
    Link?: string
    IsActive?: boolean
    SubItems?: TopbarModelSubItem[]
}

export interface TopbarModelSubItem{
    Title: string
    Link: string
}

export async function getTopbarModel(): Promise<TopbarModel> {
    const model: TopbarModel = {
        Items: [
            {
                Title: "主页",
                Link: "/HomePage",
            },
            {
                Title: "作品",
                SubItems: [
                    {
                        Title: "目录",
                        Link: "/d"
                    },
                    {
                        Title: "素材",
                        Link: "/materials"
                    }
                ]
            },
            {
                Title: "身份",
                SubItems:[
                    {
                        Title: "登录",
                        Link: "/Login"
                    },
                    {
                        Title: "用户组",
                        Link: "/UserGroup"
                    },
                    {
                        Title: "用户中心",
                        Link: "/u"
                    }
                ]
            }
        ]
    }
    return model;
}