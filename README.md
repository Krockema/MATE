# MATE - Manufacturing on Actor Technology


<p align="center">

The Project build status [![Build Status](https://travis-ci.com/Krockema/ng-erp-4.0.svg?branch=AkkaSim)](https://travis-ci.com/Krockema/Master-4.0) and other details:

[![Issues](https://img.shields.io/github/issues/Krockema/ng-erp-4.0.svg)](https://img.shields.io/github/issues/Krockema/ng-erp-4.0.svg) 
[![forks](https://img.shields.io/github/forks/Krockema/ng-erp-4.0.svg)](https://img.shields.io/github/forks/Krockema/ng-erp-4.0.svg) 
[![stars](https://img.shields.io/github/stars/Krockema/ng-erp-4.0.svg)](https://img.shields.io/github/stars/Krockema/ng-erp-4.0.svg) 
[![Issues](https://img.shields.io/github/license/Krockema/ng-erp-4.0.svg)](https://img.shields.io/github/license/Krockema/ng-erp-4.0.svg)

</p>


This project is part of a research project of evalutating self-organizing concepts in an industrial production planning and control application.


Code Structure
<ul>
<li>Mate</li>
<li>Mate.DataCore</li>
<li>Mate.Production.CLI</li>
<li>Mate.Production.Core</li>
<li>Mate.Production.Immutables</li>
<li>Mate.Test</li>
</ul>


![Mate Interface](/Mate_Overview.gif)


## How to install

Setup:
* Install Visual Studio (works with Visual Studio Community 2022 17.1) with default components and also make sure to have:   
  * .NET 5.0 / 6.0 Runtime
  * F#
  * SQL Server Express 2019 LocalDB
  * other components

1. Clone repository
2. If localDb shall be used (which is the default case) - set it via command line with: setx UseLocalDb true"
3. Run the unit test: Mate.Test.SimulationEnvironment.AgentSystem.ResetAllDatabase (Delete and Create all DBs)
4. Check your installation by running the unit test: Mate.Test.SimulationEnvironment.AgentSystem.AgentSystemTest (this can take a while)
