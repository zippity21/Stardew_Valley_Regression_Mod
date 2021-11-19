using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrimevalTitmouse
{
    //Lots of Regex functions to handle variability in our strings. 
  public static class Strings
  {
    private static Data t = Regression.t;
    private static Farmer who = Game1.player;

    public static string DescribeUnderwear(Container u, string baseDescription = null)
    {
      string newValue = baseDescription != null ? baseDescription : u.description;
      float num1 = u.wetness / u.absorbency;
      float num2 = u.messiness / u.containment;
      if ((double) num1 == 0.0 && (double) num2 == 0.0)
      {
        newValue = !u.IsDrying() ? Strings.t.Underwear_Clean.Replace("$UNDERWEAR_DESC$", newValue) : Strings.t.Underwear_Drying.Replace("$UNDERWEAR_DESC$", newValue);
      }
      else
      {
        if ((double) num2 > 0.0)
        {
          for (int index = 0; index < Strings.t.Underwear_Messy.Length; ++index)
          {
            float num3 = (float) (((double) index + 1.0) / ((double) Strings.t.Underwear_Messy.Length - 1.0));
            if (index == Strings.t.Underwear_Messy.Length - 1 || (double) num2 <= (double) num3)
            {
              newValue = Strings.ReplaceOptional(Strings.t.Underwear_Messy[index].Replace("$UNDERWEAR_DESC$", newValue), (double) num1 > 0.0);
              break;
            }
          }
        }
        if ((double) num1 > 0.0)
        {
          for (int index = 0; index < Strings.t.Underwear_Wet.Length; ++index)
          {
            float num3 = (float) (((double) index + 1.0) / ((double) Strings.t.Underwear_Wet.Length - 1.0));
            if (index == Strings.t.Underwear_Wet.Length - 1 || (double) num1 <= (double) num3)
            {
              string input = Strings.t.Underwear_Wet[index].Replace("$UNDERWEAR_DESC$", newValue);
              Regex regex = new Regex("<([^>]*)>");
              newValue = (double) num2 != 0.0 ? regex.Replace(input, "$1") : regex.Replace(input, "");
              break;
            }
          }
        }
      }
      return u.GetPrefix() + " " + newValue;
        }
       

        public static string InsertVariables(string msg, Body b, Container c = null)
    {
      string str = msg;
      if (b != null)
        c = b.underwear;
      if (c != null)
        str = Strings.ReplaceOr(str.Replace("$UNDERWEAR_NAME$", c.name).Replace("$UNDERWEAR_PREFIX$", c.GetPrefix()).Replace("$UNDERWEAR_DESC$", c.description).Replace("$INSPECT_UNDERWEAR_NAME$", Strings.DescribeUnderwear(c, c.name)).Replace("$INSPECT_UNDERWEAR_DESC$", Strings.DescribeUnderwear(c, c.description)), !c.plural, "#");
      if (b != null)
        str = str.Replace("$PANTS_NAME$", b.pants.name).Replace("$PANTS_PREFIX$", b.pants.GetPrefix()).Replace("$PANTS_DESC$", b.pants.description).Replace("$BEDDING_DRYTIME$", Game1.getTimeOfDayString(b.bed.timeWhenDoneDrying.time));
      return Strings.ReplaceOr(str, Strings.who.IsMale, "/").Replace("$FARMERNAME$", Strings.who.Name);
    }

    public static string InsertVariable(string inputString, string variableName, string variableValue)
    {
        string outputString = inputString;
            outputString = outputString.Replace(variableName, variableValue);
        return outputString;
    }

    public static string RandString(string[] msgs = null)
    {
      return msgs[Regression.rnd.Next(msgs.Length)];
    }

    public static string ReplaceAndOr(string str, bool first, bool second, string splitChar = "&")
    {
      Regex regex = new Regex("<([^>" + splitChar + "]*)" + splitChar + "([^>]*)>");
      if (first && !second)
        return regex.Replace(str, "$1");
      if (!first & second)
        return regex.Replace(str, "$2");
      if (first & second)
        return regex.Replace(str, "$1 and $2");
      return regex.Replace(str, "");
    }

    public static string ReplaceOptional(string str, bool keep)
    {
      return new Regex("<([^>]*)>").Replace(str, keep ? "$1" : "");
    }

    public static string ReplaceOr(string str, bool first, string splitChar = "/")
    {
      return new Regex("<([^>" + splitChar + "]*)" + splitChar + "([^>]*)>").Replace(str, first ? "$1" : "$2");
    }

    public static List<string> ValidUnderwearTypes()
    {
      List<string> list = Regression.t.Underwear_Options.Keys.ToList<string>();
      list.Remove("blue jeans");
      list.Remove("bed");
      return list;
    }
  }
}
