===============================================================================
==== R E A C T . N E T :  Readme for the 'examples' directory.             ====
===============================================================================

  $Id: README.txt 125 2005-12-04 15:39:08Z Eric Roe $

This directory contains several sub-directories each of which contains the
sources for one or more example simulations.  All of the examples are written
in C#.

To compile the example simulations, execute the 'BuildExamples' target of
the React.proj MSBuild file (located in the top-level directory):

   msbuild React.proj /t:BuildExamples

The following examples are provided.  In addition, pre-compiled versions are
available in the 'bin' directory.

   BarberShop
      A simple barber shop simulation.  Customers and barbers are modeled as
      processes; the shop is a child of the Simulation class.

   FuelDepot
      Demonstrates using a Consumable.  Includes acquiring from a Consumable
      and then re-supplying the Consumable.

   Greetings
      Three very simple, "Hello, World" style simulations.  Demonstrates a
      few ways in which simulations and processes can be created.

   Recess
      Demonstrates using anonymous and tracked resources.

