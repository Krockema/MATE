-- Should be correct.
-- select DueTime-FinishingTime from [SimulationOrders]


-- Machine Workload.
select Sum("End")-Sum("Start") as "Work", Max("End")-Min("Start") as "Machine WorkTime" 
, Cast((Sum("End")-Sum("Start")) as decimal) / CAST((Max("End")-Min("Start")) as decimal) as "WorkLoad %"
from SimulationWorkschedules
group by Machine

select * from Kpis
select 
Sum("End")-Sum("Start") as "Work", Max("End")-Min("Start") as "DLZ" , OrderId
--, Max("End"), Min("Start")
---, Cast((Sum("End")-Sum("Start")) as decimal) as "WorkLoad %"
from SimulationWorkschedules
group by OrderId

drop table #Temp
drop table #Temp2
create table #Temp
(
    quantity int, 
    id int
)


create table #Temp2
(
    quantity int, 
    id int
)

Insert Into #Temp (quantity, id) (select Sum(Quantity), StockId
from StockExchanges 
where ExchangeType = 0
group by StockId)

Insert Into #Temp2 (quantity, id) (select Sum(Quantity), StockId
from StockExchanges 
where ExchangeType = 1
group by StockId)

select t2.quantity - t1.quantity, t1.id from #Temp t1
join #Temp2 t2 on t1.id = t2.id

select * from SimulationWorkschedules