module FProcessingSlots

open ITimeRanges

    type public FProcessingSlot = 
        {   Start : int64
            End : int64 }
            interface ITimeRange with
                member this.Start with get() = this.Start
                member this.End with get() = this.End
