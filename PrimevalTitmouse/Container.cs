using System;

namespace PrimevalTitmouse
{

    public class Container
    {
        public float absorbency;
        public float containment;
        public string description;
        public bool drying;
        public float messiness;
        public string name;
        public bool plural;
        public string prefix;
        public int price;
        public int spriteIndex;
        public bool washable;
        public float wetness;

        public Container()
        {
            wetness = 0.0f;
            messiness = 0.0f;
            drying = false;
        }

        public Container(string type, float wetness = 0.0f, float messiness = 0.0f)
        {
            this.wetness = 0.0f;
            this.messiness = 0.0f;
            drying = false;
            Initialize(type, wetness, messiness);
        }

        public float AddPee(float amount)
        {
            wetness += amount;
            if (wetness > (double)absorbency)
                return Math.Max(amount, wetness - absorbency);
            return 0.0f;
        }

        public float AddPoop(float amount)
        {
            this.messiness += amount;
            if (messiness > (double)containment)
                return Math.Max(amount, messiness - containment);
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
