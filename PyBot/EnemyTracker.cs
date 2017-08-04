using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.PyBot
{
	public class EnemyTracker
	{
		// our AI, with game state details per turn
		public BotMain bot;
		// the map
		public MapDetails map;
		// probabilities of enemy controlling various territories
		public Dictionary<TerritoryIDType, double> enemyProbs;
		// number of troops enemy deployed last turn
		public int enemyDeployed;
		// our guess as to enemy income
		public int enemyIncome;

		/// <summary>
		/// Initialize enemy tracker with null arguments, which will be updated once we know initial position of map.
		/// </summary> 
		public EnemyTracker()
		{
			this.bot = null;
			this.map = null;
			this.enemyProbs = null;
			this.enemyDeployed = 0;
			this.enemyIncome = 0;
		}

		/// <summary>
		/// Initialize the 
		/// </summary>
		/// <param name="bot">The name of the bot containing current game-state</param>
		public void init(BotMain Bot)
		{
			bot = Bot;
			map = bot.Map;
			enemyProbs = new Dictionary<TerritoryIDType, double>();
			foreach (KeyValuePair<TerritoryIDType, TerritoryDetails> territory in map.Territories)
			{
				enemyProbs[territory.Key] = 0.0;
			}
			enemyIncome = Bot.Settings.MinimumArmyBonus;
		}

		/// <summary>
		/// Updates enemy tracker with data from new turn.
		/// </summary>
		/// <param name="bot">Bot with current game state</param>
		public void update(BotMain bot)
		{
			// TODO: Weighted Updating here
			// TODO: Handle multiple enemies
			foreach(KeyValuePair<TerritoryIDType, TerritoryStanding> territory in bot.Standing.Territories)
			{
				// enemy doesn't control this territory
				if (territory.Value.IsNeutral || bot.IsTeammateOrUs(territory.Value.OwnerPlayerID) || territory.Value.OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
				{
					enemyProbs[territory.Key] = 0.0;
				}
				else if (territory.Value.OwnerPlayerID == TerritoryStanding.FogPlayerID)
				{
					// Weighted update based on neighbors
				}
				else
				{
					enemyProbs[territory.Key] = 1.0;
				}
			}
		}

		/// <summary>
		/// Returns the probability the enemy controls a given bonus. Calculated as Sqrt(minimum prob of enemy contrlling territory in bonus). The sqrt is to account for the fact that the enemy will not attack randomly but will instead concentrate forces in a bonus. Also assumes enemies don't split bonuses.
		/// </summary>
		/// <param name="bonus">The bonus we are seeing if the enemy controls.</param>
		/// <returns>Probability enemy controls bonus.</returns>
		public double probEnemyControls(BonusDetails bonus)
		{
			double minProb = 1.0;
			foreach (TerritoryIDType territory in bonus.Territories)
			{
				minProb = Math.Min(minProb, enemyProbs[territory]);
			}
			return Math.Sqrt(minProb);
		}

		/// <summary>
		/// Computes approximate enemy income given what it thinks about their bonus acquisitions. TODO: Does not account for overriden bonuses.
		/// </summary>
		/// <returns>Approximate enemy income</returns>
		public double computeEnemyIncome()
		{
			double enemyIncome = bot.Settings.MinimumArmyBonus;
			foreach (KeyValuePair<BonusIDType, BonusDetails> bonus in map.Bonuses)
			{
				enemyIncome += probEnemyControls(bonus.Value) * bonus.Value.Amount;
			}
			return enemyIncome;
		}

		/// <summary>
		/// Is this tracker initialized?
		/// </summary>
		/// <returns></returns>
		public bool isInit()
		{
			return (this.map != null);
		}
	}
}
