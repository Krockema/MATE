module FCentralResourceHubInformations
    type public FResourceHubInformation = {
        ResourceList : obj
        DbConnectionString : string
        MasterDbConnectionString : string
        PathToGANTTPLANOptRunner : string
        WorkTimeGenerator: obj
    }