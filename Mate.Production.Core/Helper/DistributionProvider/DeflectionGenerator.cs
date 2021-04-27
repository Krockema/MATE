using System;
using System.Collections.Generic;
using System.Numerics;
using Mate.Production.Core.Types;
using MidpointRounding = System.MidpointRounding;

namespace Mate.Production.Core.Helper.DistributionProvider
{
    public class DeflectionGenerator
    {

        private readonly Dictionary<int, ResourceUsage> _resourceUsages = new Dictionary<int, ResourceUsage>();
        Random _random = null;
        public DeflectionGenerator(int seed)
        {
            var random = new Random(Seed: seed);

        }

        public ResourceUsage AddUsage(int setupId)
        {
           var usage = GetUsageFor(setupId);
           if (usage.NumberOfUses >= 75) usage.NumberOfUses = 0; //machine fixed
           else usage.NumberOfUses++;
           return usage;
        }
        private ResourceUsage GetUsageFor(int resourceId)
        {
           if (!_resourceUsages.TryGetValue(resourceId, out ResourceUsage usage))
           {
               usage = new ResourceUsage { NumberOfUses = 0 };
               _resourceUsages.Add(resourceId, usage);
           }

           return usage;
        }
        public double GetOneDirectionalDeflection(ResourceUsage resourceUsage)
        {   
            var m = 1.0 / 50000000.0;
            var x = resourceUsage.NumberOfUses;
            var deflectionValue = m* Math.Pow(x, 4);
            return Math.Round(deflectionValue, 3, MidpointRounding.AwayFromZero); //actual deflection of estimated value
        }

        public Vector2 GetMultiDirectionalDeflection(int numberOfUses, double deflectionAngle) //TODO: always deflection?  how to apply number of uses? limited deflection?
        {
            var randomAngle = 0.0;
            if (deflectionAngle != 0.0) randomAngle = deflectionAngle; //TODO: random angle on init, not here?
            else randomAngle = _random.NextDouble() * 360;
            var radians = randomAngle * (Math.PI / 180);
            var deflectionY = Math.Tan(radians);
            var deflectionX = deflectionY / (Math.Sin(radians));
            return new Vector2(Convert.ToSingle(deflectionX), Convert.ToSingle(deflectionY)); //TODO: Convert.ToSingle() rational?
        }
    }
}