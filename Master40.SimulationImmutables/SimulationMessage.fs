namespace Master40.SimulationImmutables

open System
open Master40.DB.DataModel
open Akka.Actor
open System.Linq

type public ElementStatus = Created=0 | Ready=1 | InQueue=2 | Processed=3 | Finished=4
type public ResourceType = Machine=0 | Human=1 | Dispo=2 | Storage=3 | Production=4 | Hub=5
//module Message = 

    type public IKey = 
        abstract member Key : Guid with get
    
    type public FRequestItem =         
        {   Key : Guid
            Article : M_Article
            StockExchangeId : Guid
            StorageAgent: IActorRef
            Quantity : int
            DueTime : int64
            OriginRequester : IActorRef
            DispoRequester : IActorRef
            ProviderList : System.Collections.Generic.List<IActorRef> 
            CustomerOrderId : int
            Providable : int 
            Provided : bool
            IsHeadDemand : bool 
            FinishedAt : int64 } 
            interface IKey with 
                member this.Key with get() = this.Key
        
            member this.UpdateFinishedAt f = { this with FinishedAt = f }
            member this.UpdateOriginRequester r = { this with OriginRequester = r }
            member this.UpdateDispoRequester r = { this with DispoRequester = r }
            member this.UpdateCustomerOrderAndDue id due storage = { this with CustomerOrderId = id; DueTime = due; StorageAgent = storage }
            member this.UpdateArticle article = { this with Article = article }
            member this.UpdateStorageAgent s = { this with StorageAgent = s }
            member this.UpdateStockExchangeId i = { this with StockExchangeId  = i }
            member this.UpdateProviderList p = { this with ProviderList = p }
            member this.SetProvided = { this with Provided = true }


    type public FRequestRessource =
        {
            Discriminator : string
            ResourceType : ResourceType
            actorRef : IActorRef
        }

    type public FRequestBatch =
        {
                PrioRule :  FSharpFunc<int64, double> 
                mutable ItemPriority : double
                RequiredSetuo : string
        }


    type public FProposal =
        {
            PossibleSchedule : int64 
            Postponed : bool 
            PostponedFor : int64
            ResourceAgent : IActorRef
            WorkItemId : Guid
        }

    type public FStockReservation =
        {
            Quantity : int
            IsPurchsed : bool
            IsInStock : bool
            DueTime : int64
            TrackingId : Guid
        }
       
    type public FHubInformation = 
        {
            FromType : ResourceType
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }
        
    type public FWorkItem =
        { Key : Guid
          DueTime : int64 
          EstimatedStart : int64 
          EstimatedEnd : int64
          MaterialsProvided : bool 
          //public double Priority { get; set; }
          PrioRule :  FSharpFunc<int64, double> 
          mutable ItemPriority : double
          //Priority : double
          Status : ElementStatus 
          WasSetReady : bool
          ResourceAgent : IActorRef
          ProductionAgent : IActorRef
          HubAgent : IActorRef
          Operation : M_Operation
          Proposals : System.Collections.Generic.List<FProposal> 
          } interface IKey with 
                member this.Key  with get() = this.Key
        // Returns new Object with Updated Due
        member this.Priority time = this.ItemPriority <- this.PrioRule(time) // Recalculate ItemPriority
                                    this.ItemPriority                        // Return new Priority
        member this.UpdateStatus s = { this with Status = s }
        member this.UpdateMaterialsProvided m = { this with MaterialsProvided = m }
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }
        member this.UpdateResourceAgent r = { this with ResourceAgent = r }
        member this.UpdateHubAgent hub = { this with HubAgent = hub }
        member this.SetReady = { this with Status = ElementStatus.Ready; WasSetReady = true }
        member this.UpdateEstimations estimatedStart resourceAgent = { this with EstimatedEnd = estimatedStart +  (int64)this.Operation.Duration;
                                                                                    EstimatedStart = (int64)estimatedStart;
                                                                                    ResourceAgent = resourceAgent } 
        // with member this.UpdatePriority p = { this with Priority = (double)(p + this.WorkSchedule.Duration)}
               // member this.Priority(currentTime) = (double)(this.DueTime - this.WorkSchedule.Duration - currentTime)
               // member this.Priority(currentTime : int, setUp : StockReservation) = (double)(this.DueTime - this.WorkSchedule.Duration - currentTime - (int)setUp.DueTime)

    type public FItemStatus =
        {
            ItemId : Guid 
            Status : ElementStatus
            CurrentPriority : double 
        }

    type public FRessourceDefinition = {
        WorkTimeGenerator : obj
        Resource : obj
        Debug : bool
    }


    type public CreateSimulationWork = {
        WorkItem : FWorkItem
        CustomerOrderId : string
        IsHeadDemand : bool
        ArticleType : string
    }

    type public UpdateSimulationWork = {
        WorkScheduleId : string
        Duration : int64
        Start : int64
        Machine : string
    }
    
    type public UpdateSimulationWorkProvider = {
        ProductionAgents : System.Collections.Generic.List<IActorRef>
        RequestAgentId : string
        RequestAgentName : string
        IsHeadDemand : bool
        CustomerOrderId : int
        
    }

    type public UpdateStockValues = {
        StockName : string
        NewValue : double
        ArticleType : string
    }

    type public FBreakDown = {
        Machine : string
        MachineGroup : string
        IsBroken : bool
        Duration : int64
    } with member this.SetIsBroken s = { this with IsBroken = s }
    // let Priority (wi:WorkItem) currentTime = wi.PrioRule(currentTime)
    //
    //type WorkItem with member this.Prio = priorityCall this
    type public FSetEstimatedThroughputTime = {
        Time : int64
        ArticleName : string
    }