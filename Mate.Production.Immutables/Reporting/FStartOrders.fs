module FStartOrders

open Mate.DataCore.DataModel

    type public FStartOrder = {
        customerOrderPart : T_CustomerOrderPart
        currentTime : int64
    }
