using System.Text;

public static class ItemDescriptionFormatter
{
    public static string BuildDescription(ItemData item)
    {
        if (item == null) return string.Empty;

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(item.description))
        {
            sb.Append(item.description.Trim());
        }

        string skillText = BuildSkillText(item);
        if (!string.IsNullOrEmpty(skillText))
        {
            if (sb.Length > 0) sb.AppendLine().AppendLine();
            sb.Append(skillText);
        }

        return sb.ToString();
    }

    public static string BuildStatsText(ItemData item)
    {
        if (item == null || item.modifiers == null || item.modifiers.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();

        foreach (ItemData.StatMod modifier in item.modifiers)
        {
            if (!modifier.isMainStat) continue;

            string sign = modifier.value >= 0 ? "+" : string.Empty;
            string valueText = modifier.percent ? $"{sign}{modifier.value}%" : $"{sign}{modifier.value}";
            sb.AppendLine($"<color=#FFB300><b>{modifier.stat}: {valueText}</b></color>");
        }

        foreach (ItemData.StatMod modifier in item.modifiers)
        {
            if (modifier.isMainStat) continue;

            string sign = modifier.value >= 0 ? "+" : string.Empty;
            string valueText = modifier.percent ? $"{sign}{modifier.percentValue:0.##}%" : $"{sign}{modifier.value}";
            sb.AppendLine($"  <color=#DDDDDD>- {modifier.stat}: {valueText}</color>");
        }

        return sb.ToString().TrimEnd();
    }

    private static string BuildSkillText(ItemData item)
    {
        if (!item.hasSkill) return string.Empty;

        WeaponSkill skill = item.weaponSkill;
        if (string.IsNullOrWhiteSpace(skill.skillName) && string.IsNullOrWhiteSpace(skill.description))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();
        string skillName = string.IsNullOrWhiteSpace(skill.skillName) ? "Weapon Skill" : skill.skillName.Trim();
        sb.AppendLine($"<color=#66D9EF><b>Skill: {skillName}</b></color>");

        if (!string.IsNullOrWhiteSpace(skill.description))
        {
            sb.AppendLine(skill.description.Trim());
        }

        if (skill.cooldown > 0f)
        {
            sb.AppendLine($"Cooldown: {skill.cooldown:0.#}s");
        }

        if (skill.damageMultiplier > 0f)
        {
            sb.AppendLine($"Damage: x{skill.damageMultiplier:0.##}");
        }

        return sb.ToString().TrimEnd();
    }
}
