-- Should be correct.
-- select DueTime-FinishingTime from [SimulationOrders]


-- Machine Workload.
-- select Sum("End")-Sum("Start") as "Work", Max("End")-Min("Start") as "Machine WorkTime" 
-- , Cast((Sum("End")-Sum("Start")) as decimal) / CAST((Max("End")-Min("Start")) as decimal) as "WorkLoad %"
-- from SimulationWorkschedules
-- group by Machine

select * from Kpis
select 
Sum("End")-Sum("Start") as "Work", Max("End")-Min("Start") as "DLZ" , OrderId
--, Max("End"), Min("Start")
---, Cast((Sum("End")-Sum("Start")) as decimal) as "WorkLoad %"
from SimulationWorkschedules
group by OrderId
