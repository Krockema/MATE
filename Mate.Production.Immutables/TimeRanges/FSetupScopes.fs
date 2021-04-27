module FSetupSlots

open ITimeRanges

    type public FSetupSlot = 
        {   
            Start : int64
            End : int64 
        }
            interface ITimeRange with
                member this.Start with get() = this.Start
                member this.End with get() = this.End

           
    
                