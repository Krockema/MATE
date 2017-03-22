//=============================================================================
//=  $Id: Shop.cs 128 2005-12-04 20:12:00Z Eric Roe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2005, Eric K. Roe.  All rights reserved.
//=
//=  React.NET is free software; you can redistribute it and/or modify it
//=  under the terms of the GNU General Public License as published by the
//=  Free Software Foundation; either version 2 of the License, or (at your
//=  option) any later version.
//=
//=  React.NET is distributed in the hope that it will be useful, but WITHOUT
//=  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
//=  FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
//=  more details.
//=
//=  You should have received a copy of the GNU General Public License along
//=  with React.NET; if not, write to the Free Software Foundation, Inc.,
//=  51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//=============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using React;
using React.Distribution;

namespace BarberShop
{
    /// <summary>
    /// The Barber Shop demonstration simulation.
    /// </summary>
    /// <remarks>
    /// The simulation demonstrates using <see cref="TrackedResources"/> as well
    /// as having one <see cref="Process"/> block on another.
    /// <para>
    /// The simulation is kicked off via the <see cref="Generator"/> method,
    /// which serves as a <see cref="ProcessSteps"/> delegate for the generator
    /// <see cref="Process"/>.  <see cref="Generator"/> begins by creating a
    /// <see cref="TrackedResource"/> containing four <see cref="Barber"/>
    /// processes.  It then creates a new <see cref="Customer"/> about once
    /// every five minutes and passes the resource (i.e. the barbers) to the
    /// client as <em>activation data</em>.  After eight hours (8 * 60min),
    /// the barber shop closes for the day.  Of course, the barbers finish
    /// with those customers who have been waiting.
    /// </para>
    /// </remarks>
    public class Shop : Simulation
    {
        private const long ClosingTime = 8 * 60;

        private Shop()
        {
        }

        private IEnumerator<Task> Generator(Process p, object data)
        {
            Console.WriteLine("The barber shop is opening for business...");
            Resource barbers = CreateBarbers();

            Normal n = new Normal(5.0, 1.0);

            do
            {
                long d;
                do
                {
                    d = (long)n.NextDouble();
                } while (d <= 0L);

                yield return p.Delay(d);

                Customer c = new Customer(this);
                c.Activate(null, 0L, barbers);

            } while (Now < ClosingTime);

            Console.WriteLine("The barber shop is closed for the day.");

            if (barbers.BlockCount > 0)
            {
                Console.WriteLine("The barbers have to work late today.");
            }

            yield break;
        }

        private Resource CreateBarbers()
        {
            Barber[] barbers = new Barber[4];
            barbers[0] = new Barber(this, "Frank");
            barbers[1] = new Barber(this, "Tom");
            barbers[2] = new Barber(this, "Bill");
            barbers[3] = new Barber(this, "Joe");

            return Resource.Create(barbers);
        }

        static void Main(string[] args)
        {
            Shop shop = new Shop();
            Task generator = new Process(shop, shop.Generator);
            shop.Run(generator);
        }
    }
}
