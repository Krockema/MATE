# MATE - Manufacturing on Actor Technology


<p align="center">

The Project build status [![Build Status](https://app.travis-ci.com/Krockema/MATE.svg?branch=main)](https://app.travis-ci.com/Krockema/MATE) and other details:
[![GitHub forks](https://img.shields.io/github/forks/Krockema/MATE)](https://github.com/Krockema/MATE/network)
[![GitHub stars](https://img.shields.io/github/stars/Krockema/MATE)](https://github.com/Krockema/MATE/stargazers)
![GitHub issues](https://img.shields.io/github/issues-raw/krockema/Mate)
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Ftwitter.com%2FKrockema)](https://twitter.com/intent/tweet?text=Wow:&url=https%3A%2F%2Fgithub.com%2FKrockema%2FMATE)

</p>
This project is part of a research project to evalutate self-organizing concepts in an industrial production planning and control application.

Short troduction into the topic of self-organizing production planning and controll and showcase of MATE. [Comunity Standup of Akka.Net on youtube.com from 13.04.2021](https://youtu.be/9pnVQHZYgNQ?t=1163)


#Project structure
<ul>
<li><p>Mate</p>
<blockquote>
  <p>Provides basic webinterface on ASP.Net for showcase purpose</p>
</blockquote></li>
<li><p>Mate.DataCore</p>
<blockquote>
  <p>Provides database and initializer for basic schema and primary data, as it is build as "code-first" with Entity Framework</p>
</blockquote></li>
<li><p>Mate.Production.CLI</p>
<blockquote>
  <p>Provides a small CLI that acts as headless runtime, used for remote deployment that register themself as worker for the Hangfire module.</p>
</blockquote></li>
<li><p>Mate.Production.Core</p>
<blockquote>
  <p>Actor simulation runtime that can be initalized as self-irganized, distributed or central organized system to validate production planning algorithms using tertiary objectives</p>
</blockquote></li>
<li><p>Mate.Production.Immutables</p>
<blockquote>
  <p>Immutable message definitions for the actor system.</p>
</blockquote></li>
<li><p>Mate.Test</p>
<blockquote>
  <p>Provides online and offline test for the project. Be aware that some of them are not used as intended ;).</p>
</blockquote></li>
</ul>


![Mate Interface](/Mate_Overview.gif)

## How to Install

Setup:
* Install Visual Studio (works with Visual Studio Community 2022 17.1) with default components and also make sure to have:   
  * .NET 6.0 Runtime
  * F#
  * SQL Server Express 2019 LocalDB
  * ASP Web-Development

1. Clone repository
2. If localDb shall be used (which is the default case for Windows user) - set it via command line with: setx UseLocalDb true
3. Open Visual Studio and go to Mate/libman.json. Left click and "Restore Client-Side Libraries"
4. Done - Run the project

To reset database: run the unit test: Mate.Test.SimulationEnvironment.AgentSystem.ResetAllDatabase (Delete and Create all DBs)

## Releated Papers

<ul>
<li>2018 - <a href="https://www.sne-journal.org/fileadmin/user_upload_sne/SNE_Issues_OA/SNE_30_1/articles/sne.30.1.10506.tn.OA.pdf" target="_blank">An Approach to a Self-organizing Production in Comparison to a Centrally Planned Production</a></li>
<li>2020 - <a href="https://www.semanticscholar.org/paper/SOBA%3A-A-Self-Organizing-Bucket-Architecture-to-in-Krockert-Matthes/8051f9cc34666a169d25f644063241b2d1603065" target="_blank">SOBA: A Self-Organizing Bucket Architecture to Reduce Setup Times in an Event-Driven Production</a></li>
<li>2021 - <a href="https://www.scitepress.org/Link.aspx?doi=10.5220/0010436204420451" target="_blank">Agent-based Decentral Production Planning and Control: A New Approach for Multi-resource Scheduling</a></li>
<li>2021 - <a href="https://www.sciencedirect.com/science/article/pii/S2351978921001542" target="_blank">Suitability of Self-Organization for Different Types of Production</a>
</li>
</ul>

## Releated Projects

<ul>
<li><a href="https://www.sne-journal.org/fileadmin/user_upload_sne/SNE_Issues_OA/SNE_30_1/articles/sne.30.1.10506.tn.OA.pdf" target="_blank">Akka.Hive</a> 
<p><blockquote>An Agent Simulation Framework based on Akka.Net</blockquote></p>
</li>
<li><a href="https://github.com/Krockema/Seed" target="_blank">SEED</a> - Sintesizer for Enterprise Experimentation Data
<p><blockquote>An Framework to populate Primary-Data, like Material, BOM, Operations. . . based on statistical indicators</blockquote></p></li>
</ul>

