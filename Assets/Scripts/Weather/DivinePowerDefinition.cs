using UnityEngine;
using TheGodAbove.UI;

namespace TheGodAbove.Weather
{
    public enum PowerCategory
    {
        Blessing,
        Disaster
    }

    [System.Serializable]
    public class DivinePowerDefinition
    {
        public DivinePowerType powerType;
        public string displayName;
        public PowerCategory category;
        public float cooldownDuration;
        public float aoeRadius;
        public Color indicatorColor;

        [Header("--- Environmental Impact ---")]
        public float temperatureDelta;
        public float moistureDelta;

        [Header("--- Population & Gameplay Impact ---")]
        public float happinessDelta;
        public int faithCost;

        public static DivinePowerDefinition GetDefault(DivinePowerType type)
        {
            return type switch
            {
                DivinePowerType.Sunny => new DivinePowerDefinition
                {
                    powerType = DivinePowerType.Sunny,
                    displayName = "Sunny",
                    category = PowerCategory.Blessing,
                    cooldownDuration = 3f,
                    aoeRadius = 14f,
                    indicatorColor = new Color(1f, 0.85f, 0.25f, 0.85f),
                    temperatureDelta = 3f,
                    moistureDelta = -3f,
                    happinessDelta = 4f
                },
                DivinePowerType.Rainy => new DivinePowerDefinition
                {
                    powerType = DivinePowerType.Rainy,
                    displayName = "Rain",
                    category = PowerCategory.Blessing,
                    cooldownDuration = 4f,
                    aoeRadius = 13f,
                    indicatorColor = new Color(0.25f, 0.75f, 1f, 0.85f),
                    temperatureDelta = -2f,
                    moistureDelta = 14f,
                    happinessDelta = 5f
                },
                DivinePowerType.Hot => new DivinePowerDefinition
                {
                    powerType = DivinePowerType.Hot,
                    displayName = "Heat",
                    category = PowerCategory.Disaster,
                    cooldownDuration = 5f,
                    aoeRadius = 10f,
                    indicatorColor = new Color(1f, 0.4f, 0.1f, 0.85f),
                    temperatureDelta = 8f,
                    moistureDelta = -10f,
                    happinessDelta = -5f
                },
                DivinePowerType.Lightning => new DivinePowerDefinition
                {
                    powerType = DivinePowerType.Lightning,
                    displayName = "Lightning",
                    category = PowerCategory.Disaster,
                    cooldownDuration = 6f,
                    aoeRadius = 4f,
                    indicatorColor = new Color(1f, 0.95f, 0.35f, 0.95f),
                    temperatureDelta = 1.5f,
                    moistureDelta = -1f,
                    happinessDelta = -8f
                },
                DivinePowerType.Wind => new DivinePowerDefinition
                {
                    powerType = DivinePowerType.Wind,
                    displayName = "Wind",
                    category = PowerCategory.Disaster,
                    cooldownDuration = 4f,
                    aoeRadius = 11f,
                    indicatorColor = new Color(0.35f, 0.95f, 0.85f, 0.85f),
                    temperatureDelta = -1f,
                    moistureDelta = -2f,
                    happinessDelta = -3f
                },
                DivinePowerType.Cold => new DivinePowerDefinition
                {
                    powerType = DivinePowerType.Cold,
                    displayName = "Cold",
                    category = PowerCategory.Disaster,
                    cooldownDuration = 5f,
                    aoeRadius = 11f,
                    indicatorColor = new Color(0.65f, 0.9f, 1f, 0.85f),
                    temperatureDelta = -7f,
                    moistureDelta = -3f,
                    happinessDelta = -6f
                },
                _ => new DivinePowerDefinition()
            };
        }
    }
}
