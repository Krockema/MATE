namespace Master40.SimulationImmutables

open System
open Master40.DB.Models
open Akka.Actor

type public ElementStatus = Created=0 | Ready=1 | InQueue=2 | Processed=3 | Finished=4
type public ResourceType = Machine=0 | Human=1 | Dispo=2 | Storage=3 | Production=4

//module Message = 

    type public IKey = 
        abstract member Key : Guid with get
    
    type public RequestItem =         
        {   Key : Guid
            Article : Article
            StockExchangeId : Guid
            StorageAgent: IActorRef
            Quantity : int
            DueTime : int64
            Requester : IActorRef
            ProviderList : System.Collections.Generic.List<IActorRef> 
            OrderId : int
            Providable : int 
            Provided : bool
            IsHeadDemand : bool } 
            interface IKey with 
                member this.Key with get() = this.Key
        
            member this.UpdateRequester r = { this with Requester = r }
            member this.UpdateOrderAndDue id due storage = { this with OrderId = id; DueTime = due; StorageAgent = storage }
            member this.UpdateArticle article = { this with Article = article }
            member this.UpdateStorageAgent s = { this with StorageAgent = s }
            member this.UpdateStockExchangeId i = { this with StockExchangeId  = i }
            member this.UpdateProviderList p = { this with ProviderList = p }
            member this.SetProvided = { this with Provided = true }


    type public RequestRessource =
        {
            Discriminator : string
            ResourceType : ResourceType
            actorRef : IActorRef
        }

    type public RequestBatch =
        {
                PrioRule :  FSharpFunc<int64, double> 
                mutable ItemPriority : double
                RequiredSetuo : string
        }


    type public Proposal =
        {
            PossibleSchedule : int64 
            Postponed : bool 
            PostponedFor : int64
            ResourceAgent : IActorRef
            WorkItemId : Guid
        }

    type public StockReservation =
        {
            Quantity : int
            IsPurchsed : bool
            IsInStock : bool
            DueTime : int64
        }
       
    type public HubInformation = 
        {
            FromType : ResourceType
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }
        
    type public WorkItem =
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
          WorkSchedule : WorkSchedule
          Proposals : System.Collections.Generic.List<Proposal> 
          } interface IKey with 
                member this.Key  with get() = this.Key
        // Returns new Object with Updated Due
        member this.Priority time = this.ItemPriority <- this.PrioRule(time) // Recalculate ItemPriority
                                    this.ItemPriority                        // Return new Priority
        member this.UpdateStatus s = { this with Status = s }
        member this.UpdateMaterialsProvided m = { this with MaterialsProvided = m }
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }
        member this.UpdateResourceAgent r = { this with ProductionAgent = r }
        member this.UpdateHubAgent hub = { this with HubAgent = hub }
        member this.SetReady = { this with Status = ElementStatus.Ready; WasSetReady = true }
        member this.UpdateEstimations estimatedStart resourceAgent = { this with EstimatedEnd = estimatedStart +  (int64)this.WorkSchedule.Duration;
                                                                                    EstimatedStart = (int64)estimatedStart;
                                                                                    ResourceAgent = resourceAgent } 
        // with member this.UpdatePriority p = { this with Priority = (double)(p + this.WorkSchedule.Duration)}
               // member this.Priority(currentTime) = (double)(this.DueTime - this.WorkSchedule.Duration - currentTime)
               // member this.Priority(currentTime : int, setUp : StockReservation) = (double)(this.DueTime - this.WorkSchedule.Duration - currentTime - (int)setUp.DueTime)





    type public ItemStatus =
        {
            ItemId : Guid 
            Status : ElementStatus
            CurrentPriority : double 
        }

    type public RessourceDefinition = {
        WorkTimeGenerator : obj
        Resource : obj
        Debug : bool
    }
    
    // let Priority (wi:WorkItem) currentTime = wi.PrioRule(currentTime)
    //
    //type WorkItem with member this.Prio = priorityCall this