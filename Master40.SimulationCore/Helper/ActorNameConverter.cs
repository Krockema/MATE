﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Master40.SimulationCore.Helper
{
    public static class ActorNameConverter
    {
        public static string ToActorName(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') 
                    || (c >= 'A' && c <= 'Z') 
                    || (c >= 'a' && c <= 'z') 
                    || c == '.' || c == '_'
                    || c == '-' || c == '='
                    || c == '(' || c == ')')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}