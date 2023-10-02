import { Api } from "./utils/httpClient"

interface configModel{
    api:{
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
        identities:{
            login:{
                reletiveUrl:"/api/Auth/Login",
                type:"postForm"
            },
            identityTest:{
                reletiveUrl:"/api/Auth/IdentityTest",
                type:"get"
            },
            edit:{
                reletiveUrl:"/api/User/Edit",
                type:"get"
            },
            editExe:{
                reletiveUrl:"/api/User/EditExe",
                type:"postRaw"
            }
        }
    }
}