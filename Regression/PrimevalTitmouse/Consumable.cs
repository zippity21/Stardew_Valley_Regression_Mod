using System;
using System.Collections.Generic;

namespace PrimevalTitmouse
{
	public class Consumable
	{
		public string name;
		public float waterContent;
		public float calorieContent;

		public Consumable(string n, float w, float c)
        {
			name = n;
			waterContent = w;
			calorieContent = c;
        }
	}
}
