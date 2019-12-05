2019-09-08

# New
- Introduce 2 types of desk_cop_5_lotsize_2 scenario: concurrent (where all 5 have the same dueTime) and sequentially(where all 5 have different dueTimes, but overlaps a bit)

# Fix
- Invalid times coming from job shop scheduling: start/end of job shop scheduling is now greater or equalt to startBackward/endBackward
- Default test config has other COPs than using the same config manually

# Enhancement
- Adapt desk model: no "packing the desk" anymore, instead "building the desk", remove manual & packaging

# Internal
- New tests:
  - Test for: tProductionOrderOperation.Start >= tProductionOrderOperation.StartBackward && tProductionOrderOperation.End >= tProductionOrderOperation.EndBackward
  - New additional test: TestPredecessorNodeTimeIsGreaterOrEqualInDemandToProviderGraph  
- Adapt namescapes to folder structure    


