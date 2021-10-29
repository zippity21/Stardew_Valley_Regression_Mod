using System;
using StardewValley;

namespace PrimevalTitmouse
{

    public class Container
    {
        public float absorbency;
        public float containment;
        public string description;
        public float messiness;
        public string name;
        public bool plural;
        public string prefix;
        public int price;
        public int spriteIndex;
        public bool washable;
        public float wetness;
        public int dryingTime;

        public struct Date
        {
            public int time;
            public int day;
            public int season;
            public int year;
        }
        public Date timeWhenDoneDrying;
        private bool drying = false;

        //This class describes anything that we could wet/mess in. Usually underwear, but it could also be something like the bed.
        //These functions are pretty self-explainatory
        public Container()
        {
            wetness = 0.0f;
            messiness = 0.0f;
            drying = false;
        }

        public Container(Container c)
        {
            Initialize(c, c.wetness, c.messiness);
        }
    

        public Container(string type, float wetness = 0.0f, float messiness = 0.0f)
        {
            this.wetness = 0.0f;
            this.messiness = 0.0f;
            drying = false;
            Initialize(type, wetness, messiness);
        }

        public void Wash()
        {
            if (washable)
            {
                drying = true;
                timeWhenDoneDrying.time = Game1.timeOfDay + dryingTime;
                timeWhenDoneDrying.day = Game1.dayOfMonth;
                timeWhenDoneDrying.season = Utility.getSeasonNumber(Game1.currentSeason);
                timeWhenDoneDrying.year = Game1.year;

                if(timeWhenDoneDrying.time >= 2400)
                {
                    timeWhenDoneDrying.time -= 2400;
                    timeWhenDoneDrying.day += 1;
                }
                if(timeWhenDoneDrying.day > 28)
                {
                    timeWhenDoneDrying.day -= 28;
                    timeWhenDoneDrying.season += 1;
                }
                if(timeWhenDoneDrying.season > 4)
                {
                    timeWhenDoneDrying.season -= 4;
                    timeWhenDoneDrying.year += 1;
                }
            }
        }

        public bool IsDrying()
        {
            if (!drying) return false;
            Date currentDate;
            currentDate.time = Game1.timeOfDay;
            currentDate.day = Game1.dayOfMonth;
            currentDate.season = Utility.getSeasonNumber(Game1.currentSeason);
            currentDate.year = Game1.year;

            bool yearEq   = currentDate.year == timeWhenDoneDrying.year;
            bool seasonEq = currentDate.season == timeWhenDoneDrying.season;
            bool dayEq    = currentDate.day == timeWhenDoneDrying.day;
            bool timeEq   = currentDate.time == timeWhenDoneDrying.time;
            bool yearGt   = currentDate.year > timeWhenDoneDrying.year;
            bool seasonGt = currentDate.season > timeWhenDoneDrying.season;
            bool dayGt    = currentDate.day > timeWhenDoneDrying.day;
            bool timeGt   = currentDate.time > timeWhenDoneDrying.time;
            if ((yearGt) || (yearEq && seasonGt) || (yearEq && seasonEq && dayGt) || (yearEq && seasonEq && dayEq && (timeGt || timeEq)))
            {
                drying = false;
                wetness = 0;
                messiness = 0;
            }
            return drying;
        }

        public float AddPee(float amount)
        {
            wetness += amount;
            float difference = wetness - absorbency;
            if (difference > 0)
            {
                wetness = absorbency;
                return difference;
            }
            return 0.0f;
        }

        public float AddPoop(float amount)
        {
            messiness += amount;
            float difference = messiness - containment;
            if (difference > 0)
            {
                messiness = containment;
                return difference;
            }
            return 0.0f;
        }

        private void Initialize(Container c, float wetness = 0.0f, float messiness = 0.0f)
        {
            name = c.name;
            prefix = c.prefix;
            description = c.description;
            absorbency = c.absorbency;
            containment = c.containment;
            spriteIndex = c.spriteIndex;
            price = c.price;
            washable = c.washable;
            plural = c.plural;
            dryingTime = c.dryingTime;
            drying = c.drying;
            timeWhenDoneDrying = c.timeWhenDoneDrying;
            this.wetness = wetness;
            this.messiness = messiness;
        }

        public void Initialize(string type, float wetness = 0.0f, float messiness = 0.0f)
        {
            Container c;

            if (!Regression.t.Underwear_Options.TryGetValue(type, out c))
                throw new Exception(string.Format("Invalid underwear choice: {0}", type));

            Initialize(c, wetness, messiness);
        }
    }
}
