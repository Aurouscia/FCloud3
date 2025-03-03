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
                        Title: "所有作品",
                        Link: "/d"
                    },
                    {
                        Title: "我的词条",
                        Link: "/MyWikis"
                    },
                    {
                        Title: "素材列表",
                        Link: "/materials"
                    },
                    {
                        Title: "正文搜索",
                        Link: "/WikiContentSearch"
                    },
                    {
                        Title: "语法指南",
                        Link: "/GrammarHelp"
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
                        Title: "用户列表",
                        Link: "/UserList"
                    },
                    {
                        Title: "关于",
                        Link: "/About"
                    }
                ]
            }
        ]
    }
    if(import.meta.env.VITE_ShowTopbarTools === 'true'){
        model.Items.push(            {
            Title: "工具",
            Link: "/Tools"
        })
    }
    return model;
}