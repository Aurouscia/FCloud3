import { Api } from "./utils/httpClient"

interface configModel{
    api:{
        wikiItem:{
            create:Api,
            edit:Api,
            editExe:Api,
            loadSimple:Api,

            insertPara:Api,
            setParaOrders:Api
        }
        identities:{
            login:Api,
            identityTest:Api,
            edit:Api,
            editExe:Api
        }
    }
}

export const config:configModel = {
    api:{
        identities: {
            login: {
                reletiveUrl: "/api/Auth/Login",
                type: "postForm"
            },
            identityTest: {
                reletiveUrl: "/api/Auth/IdentityTest",
                type: "get"
            },
            edit: {
                reletiveUrl: "/api/User/Edit",
                type: "get"
            },
            editExe: {
                reletiveUrl: "/api/User/EditExe",
                type: "postRaw"
            }
        },
        wikiItem: {
            create: {
                reletiveUrl:"/api/WikiItem/Create",
                type:"postForm"
            },
            edit: {
                reletiveUrl:"/api/WikiItem/Edit",
                type:"postForm"
            },
            editExe: {
                reletiveUrl:"/api/WikiItem/EditExe",
                type:"postRaw"
            },
            loadSimple: {
                reletiveUrl:"/api/WikiItem/LoadSimple",
                type:"postForm"
            },
            insertPara: {
                reletiveUrl:"/api/WikiItem/InsertPara",
                type:"postForm"
            },
            setParaOrders:{
                reletiveUrl:"/api/WikiItem/SetParaOrders",
                type:"postRaw"
            }
        }
    }
}