module FCentralResourceHubInformations
    type public FResourceHubInformation = {
        ResourceList : obj
        DbConnectionString : string
        MasterDbConnectionString : string
        WorkTimeGenerator: obj
    }