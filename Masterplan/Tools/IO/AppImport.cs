using System;
using System.Drawing;
using System.IO;
using System.Xml;
using Masterplan.Data;

namespace Masterplan.Tools.IO
{
    internal class AppImport
    {
        public static Creature ImportCreature(string xml)
        {
            Creature c = null;

            try
            {
                var doc = XmlHelper.LoadSource(xml);
                if (doc == null)
                    return null;

                XmlNode docElement = doc.DocumentElement;

                c = new Creature();

                var roleHasBeenSet = false;

                foreach (XmlNode node in docElement.ChildNodes)
                    if (node.Name == "ID")
                    {
                    }
                    else if (node.Name == "AbilityScores")
                    {
                        try
                        {
                            var valuesNode = node.FirstChild;
                            foreach (XmlNode valueNode in valuesNode.ChildNodes)
                            {
                                var name = XmlHelper.NodeText(valueNode, "Name");
                                var value = XmlHelper.GetIntAttribute(valueNode, "FinalValue");
                                value = Math.Max(value, 0);

                                if (name == "Strength")
                                    c.Strength.Score = value;

                                if (name == "Constitution")
                                    c.Constitution.Score = value;

                                if (name == "Dexterity")
                                    c.Dexterity.Score = value;

                                if (name == "Intelligence")
                                    c.Intelligence.Score = value;

                                if (name == "Wisdom")
                                    c.Wisdom.Score = value;

                                if (name == "Charisma")
                                    c.Charisma.Score = value;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Defenses")
                    {
                        try
                        {
                            var valuesNode = node.FirstChild;
                            foreach (XmlNode valueNode in valuesNode.ChildNodes)
                            {
                                var name = XmlHelper.NodeText(valueNode, "Name");
                                var value = XmlHelper.GetIntAttribute(valueNode, "FinalValue");

                                if (name == "AC")
                                    c.Ac = value;

                                if (name == "Fortitude")
                                    c.Fortitude = value;

                                if (name == "Reflex")
                                    c.Reflex = value;

                                if (name == "Will")
                                    c.Will = value;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "AttackBonuses")
                    {
                    }
                    else if (node.Name == "Skills")
                    {
                        try
                        {
                            var skills = "";

                            var valuesNode = node.FirstChild;
                            foreach (XmlNode valueNode in valuesNode.ChildNodes)
                            {
                                var name = XmlHelper.NodeText(valueNode, "Name");
                                var value = XmlHelper.GetIntAttribute(valueNode, "FinalValue");

                                var trained = false;
                                var trainedStr = XmlHelper.NodeText(valueNode, "Trained");
                                if (trainedStr != "")
                                    trained = bool.Parse(trainedStr);

                                if (!trained)
                                    continue;

                                if (skills != "")
                                    skills += ", ";

                                var sign = value >= 0 ? "+" : "";
                                skills += name + " " + sign + value;
                            }

                            c.Skills = skills;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Name")
                    {
                        try
                        {
                            c.Name = node.InnerText;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Level")
                    {
                        try
                        {
                            c.Level = int.Parse(node.InnerText);
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Size")
                    {
                        try
                        {
                            var dataNode = XmlHelper.FindChild(node, "ReferencedObject");
                            if (dataNode != null)
                            {
                                var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                if (nameNode != null)
                                {
                                    var str = nameNode.InnerText;
                                    c.Size = (CreatureSize)Enum.Parse(typeof(CreatureSize), str);
                                }
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Origin")
                    {
                        try
                        {
                            var dataNode = XmlHelper.FindChild(node, "ReferencedObject");
                            if (dataNode != null)
                            {
                                var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                if (nameNode != null)
                                {
                                    var str = nameNode.InnerText;
                                    c.Origin = (CreatureOrigin)Enum.Parse(typeof(CreatureOrigin), str);
                                }
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Type")
                    {
                        try
                        {
                            var dataNode = XmlHelper.FindChild(node, "ReferencedObject");
                            if (dataNode != null)
                            {
                                var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                if (nameNode != null)
                                {
                                    var str = nameNode.InnerText.Replace(" ", "");
                                    c.Type = (CreatureType)Enum.Parse(typeof(CreatureType), str);
                                }
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "GroupRole")
                    {
                        try
                        {
                            var dataNode = XmlHelper.FindChild(node, "ReferencedObject");
                            if (dataNode != null)
                            {
                                var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                if (nameNode != null)
                                {
                                    var str = nameNode.InnerText;

                                    if (str == "Minion")
                                    {
                                        var m = new Minion();
                                        if (roleHasBeenSet)
                                        {
                                            var cr = c.Role as ComplexRole;

                                            m.HasRole = true;
                                            m.Type = cr.Type;
                                        }

                                        c.Role = m;
                                    }
                                    else
                                    {
                                        var flag = (RoleFlag)Enum.Parse(typeof(RoleFlag), str);

                                        var cr = c.Role as ComplexRole;
                                        cr.Flag = flag;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Role")
                    {
                        try
                        {
                            var roleNode = XmlHelper.FindChild(node, "ReferencedObject");
                            if (roleNode != null)
                            {
                                var nameNode = XmlHelper.FindChild(roleNode, "Name");
                                if (nameNode != null)
                                {
                                    var str = nameNode.InnerText;
                                    var type = (RoleType)Enum.Parse(typeof(RoleType), str);

                                    if (c.Role is ComplexRole)
                                    {
                                        var cr = c.Role as ComplexRole;
                                        cr.Type = type;
                                    }

                                    if (c.Role is Minion)
                                    {
                                        var m = c.Role as Minion;
                                        m.HasRole = true;
                                        m.Type = type;
                                    }

                                    roleHasBeenSet = true;
                                }
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "IsLeader")
                    {
                        try
                        {
                            var str = node.InnerText;
                            var leader = bool.Parse(str);

                            if (c.Role is ComplexRole && leader)
                            {
                                var cr = c.Role as ComplexRole;
                                cr.Leader = true;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Items")
                    {
                        try
                        {
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var itemNode = XmlHelper.FindChild(child, "Item");
                                var nameNode = XmlHelper.FindChild(XmlHelper.FindChild(itemNode, "ReferencedObject"),
                                    "Name");
                                var name = nameNode.InnerText;
                                var quantity = int.Parse(XmlHelper.NodeText(child, "Quantity"));

                                if (c.Equipment != "")
                                    c.Equipment += ", ";
                                c.Equipment += name;
                                if (quantity != 1)
                                    c.Equipment += " x" + quantity;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Languages")
                    {
                        try
                        {
                            var langs = "";

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var dataNode = XmlHelper.FindChild(child, "ReferencedObject");
                                if (dataNode != null)
                                {
                                    var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                    if (nameNode != null)
                                    {
                                        var str = nameNode.InnerText;

                                        if (langs != "")
                                            langs += ", ";

                                        langs += str;
                                    }
                                }
                            }

                            c.Languages = langs;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Senses")
                    {
                        try
                        {
                            var senses = "";

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var roNode = XmlHelper.FindChild(child, "ReferencedObject");
                                if (roNode != null)
                                {
                                    var sense = XmlHelper.NodeText(roNode, "Name");

                                    var range = XmlHelper.NodeText(child, "Range");
                                    if (range != "" && range != "0")
                                        sense += " " + range;

                                    if (senses != "")
                                        senses += ", ";

                                    senses += sense;
                                }
                            }

                            if (senses != "")
                            {
                                if (c.Senses != "")
                                    c.Senses += "; ";

                                c.Senses += senses;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Regeneration")
                    {
                        try
                        {
                            var regen = new Regeneration();

                            regen.Value = XmlHelper.GetIntAttribute(node, "FinalValue");

                            var details = XmlHelper.NodeText(node, "Details");
                            if (details != "")
                                regen.Details = details;

                            if (regen.Value != 0 || regen.Details != "")
                                c.Regeneration = regen;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Keywords")
                    {
                        try
                        {
                            var keywords = "";

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var dataNode = XmlHelper.FindChild(child, "ReferencedObject");
                                if (dataNode != null)
                                {
                                    var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                    if (nameNode != null)
                                    {
                                        var str = nameNode.InnerText;

                                        if (keywords != "")
                                            keywords += ", ";

                                        keywords += str;
                                    }
                                }

                                c.Keywords = keywords;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Alignment")
                    {
                        try
                        {
                            var dataNode = XmlHelper.FindChild(node, "ReferencedObject");
                            if (dataNode != null)
                            {
                                var nameNode = XmlHelper.FindChild(dataNode, "Name");
                                if (nameNode != null)
                                {
                                    var str = nameNode.InnerText;
                                    c.Alignment = str;
                                }
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Powers")
                    {
                        try
                        {
                            foreach (XmlNode child in node.ChildNodes)
                                import_power(child, c);
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Initiative")
                    {
                        try
                        {
                            c.Initiative = XmlHelper.GetIntAttribute(node, "FinalValue");
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "HitPoints")
                    {
                        try
                        {
                            c.Hp = XmlHelper.GetIntAttribute(node, "FinalValue");
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "ActionPoints")
                    {
                    }
                    else if (node.Name == "Experience")
                    {
                    }
                    else if (node.Name == "Auras")
                    {
                        /*
						try
						{
							foreach (XmlNode aura_node in node.ChildNodes)
							{
								string name = XMLHelper.NodeText(aura_node, "Name");
								string details = XMLHelper.NodeText(aura_node, "Details");

								int radius = 0;
								XmlNode range_node = XMLHelper.FindChild(aura_node, "Range");
								if (range_node != null)
									radius = XMLHelper.GetIntAttribute(range_node, "FinalValue");

								Aura aura = new Aura();
								aura.Name = name;
								aura.Details = (radius > 0) ? radius + ": " + details : details;

								c.Auras.Add(aura);
							}
						}
						catch
						{
							LogSystem.Trace("Error parsing " + node.Name);
						}
						*/
                    }
                    else if (node.Name == "LandSpeed")
                    {
                        try
                        {
                            var speedNode = XmlHelper.FindChild(node, "Speed");
                            var value = XmlHelper.GetIntAttribute(speedNode, "FinalValue");

                            c.Movement = value.ToString();

                            var details = "";
                            var detailsNode = XmlHelper.FindChild(node, "Details");
                            if (detailsNode != null)
                                details = detailsNode.InnerText;
                            if (details != "")
                                c.Movement += " " + details;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Speeds")
                    {
                        try
                        {
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var nameNode = XmlHelper.FindChild(child, "ReferencedObject");
                                var speedNode = XmlHelper.FindChild(child, "Speed");
                                var detailsNode = XmlHelper.FindChild(child, "Details");

                                var name = nameNode.FirstChild.NextSibling.InnerText;
                                var value = XmlHelper.GetIntAttribute(speedNode, "FinalValue");
                                var details = detailsNode != null ? detailsNode.InnerText : "";

                                if (c.Movement != "")
                                    c.Movement += ", ";

                                var info = "";
                                if (name != "")
                                    info += name;
                                if (details != "")
                                {
                                    if (info != "")
                                        info += " ";
                                    info += details;
                                }

                                c.Movement += info + " " + value;
                            }
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "SavingThrows")
                    {
                    }
                    else if (node.Name == "Resistances")
                    {
                        try
                        {
                            var str = "";

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var nameNode = XmlHelper.FindChild(XmlHelper.FindChild(child, "ReferencedObject"),
                                    "Name");
                                var valueNode = XmlHelper.FindChild(child, "Amount");
                                var detailsNode = XmlHelper.FindChild(child, "Details");

                                var name = nameNode.InnerText;
                                var value = XmlHelper.GetIntAttribute(valueNode, "FinalValue");
                                var details = detailsNode != null ? detailsNode.InnerText : "";

                                if (details == "")
                                {
                                    var mod = DamageModifier.Parse(name, -value);
                                    if (mod != null)
                                    {
                                        c.DamageModifiers.Add(mod);
                                        continue;
                                    }
                                }

                                if (name == "" && details == "")
                                    continue;

                                if (str != "")
                                    str += ", ";

                                var info = "";
                                if (name != "0")
                                    info += name;

                                if (value > 0)
                                    str += info + " " + value;
                                else
                                    str += info;

                                if (details != "")
                                {
                                    if (str != "")
                                        str += " ";
                                    str += details;
                                }
                            }

                            c.Resist = str;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Weaknesses")
                    {
                        try
                        {
                            var str = "";

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var nameNode = XmlHelper.FindChild(XmlHelper.FindChild(child, "ReferencedObject"),
                                    "Name");
                                var valueNode = XmlHelper.FindChild(child, "Amount");
                                var detailsNode = XmlHelper.FindChild(child, "Details");

                                var name = nameNode.InnerText;
                                var value = XmlHelper.GetIntAttribute(valueNode, "FinalValue");
                                var details = detailsNode != null ? detailsNode.InnerText : "";

                                if (details == "")
                                {
                                    var mod = DamageModifier.Parse(name, value);
                                    if (mod != null)
                                    {
                                        c.DamageModifiers.Add(mod);
                                        continue;
                                    }
                                }

                                if (name == "" && details == "")
                                    continue;

                                if (str != "")
                                    str += ", ";

                                var info = "";
                                if (name != "0")
                                    info += name;

                                if (value > 0)
                                    str += info + " " + value;
                                else
                                    str += info;

                                if (details != "")
                                {
                                    if (str != "")
                                        str += " ";
                                    str += details;
                                }
                            }

                            c.Vulnerable = str;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Immunities")
                    {
                        try
                        {
                            var str = "";

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var nameNode = XmlHelper.FindChild(XmlHelper.FindChild(child, "ReferencedObject"),
                                    "Name");
                                var mod = DamageModifier.Parse(nameNode.InnerText, 0);
                                if (mod != null)
                                {
                                    c.DamageModifiers.Add(mod);
                                    continue;
                                }

                                if (str != "")
                                    str += ", ";

                                str += nameNode.InnerText;
                            }

                            c.Immune = str;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "Tactics")
                    {
                        try
                        {
                            c.Tactics = node.InnerText;
                        }
                        catch
                        {
                            LogSystem.Trace("Error parsing " + node.Name);
                        }
                    }
                    else if (node.Name == "SourceBook")
                    {
                        //
                    }
                    else if (node.Name == "Description")
                    {
                        //
                    }
                    else if (node.Name == "Race")
                    {
                        //
                    }
                    else if (node.Name == "TemplateApplications")
                    {
                        //
                    }
                    else if (node.Name == "EliteUpgradeID")
                    {
                        //
                    }
                    else if (node.Name == "FullPortrait")
                    {
                        //
                    }
                    else if (node.Name == "CompendiumUrl")
                    {
                        //
                    }
                    else if (node.Name == "Phasing")
                    {
                        //
                    }
                    else if (node.Name == "SourceBooks")
                    {
                        //
                    }
                    else
                    {
                        LogSystem.Trace("Unhandled XML node: " + node.Name);
                    }
            }
            catch
            {
            }

            return c;
        }

        private static void import_power(XmlNode powerNode, Creature c)
        {
            try
            {
                var power = new CreaturePower();

                power.Name = XmlHelper.NodeText(powerNode, "Name");

                var req = XmlHelper.NodeText(powerNode, "Requirements");
                if (req != "")
                    power.Condition = req;

                var type = XmlHelper.NodeText(powerNode, "Type");
                if (type != "Trait")
                {
                    var action = XmlHelper.NodeText(powerNode, "Action");
                    var isBasic = XmlHelper.NodeText(powerNode, "IsBasic");
                    var usage = XmlHelper.NodeText(powerNode, "Usage");

                    power.Action = new PowerAction();
                    if (isBasic == "true")
                    {
                        power.Action.Use = PowerUseType.Basic;
                    }
                    else
                    {
                        if (usage.StartsWith("At-Will"))
                        {
                            power.Action.Use = PowerUseType.AtWill;
                        }
                        else
                        {
                            power.Action.Use = PowerUseType.Encounter;
                            if (!usage.StartsWith("Encounter"))
                                if (usage.ToLower().StartsWith("recharge"))
                                {
                                    var details = XmlHelper.NodeText(powerNode, "UsageDetails");

                                    if (details == "")
                                        details = PowerAction.Recharge6;

                                    if (details == "2")
                                        details = PowerAction.Recharge2;

                                    if (details == "3")
                                        details = PowerAction.Recharge3;

                                    if (details == "4")
                                        details = PowerAction.Recharge4;

                                    if (details == "5")
                                        details = PowerAction.Recharge5;

                                    if (details == "6")
                                        details = PowerAction.Recharge6;

                                    power.Action.Recharge = details;
                                }
                        }
                    }

                    if (action.ToLower().StartsWith("standard"))
                        power.Action.Action = ActionType.Standard;
                    //power.Action.Trigger = action.Substring("standard".Length);
                    if (action.ToLower().StartsWith("move"))
                        power.Action.Action = ActionType.Move;
                    //power.Action.Trigger = action.Substring("move".Length);
                    if (action.ToLower().StartsWith("minor"))
                        power.Action.Action = ActionType.Minor;
                    //power.Action.Trigger = action.Substring("minor".Length);
                    if (action.ToLower().StartsWith("immediate interrupt"))
                        power.Action.Action = ActionType.Interrupt;
                    //power.Action.Trigger = action.Substring("immediate interrupt".Length);
                    if (action.ToLower().StartsWith("immediate reaction"))
                        power.Action.Action = ActionType.Reaction;
                    //power.Action.Trigger = action.Substring("immediate reaction".Length);
                    if (action.ToLower().StartsWith("opportunity"))
                        power.Action.Action = ActionType.Opportunity;
                    //power.Action.Trigger = action.Substring("opportunity".Length);
                    if (action.ToLower().StartsWith("free"))
                        power.Action.Action = ActionType.Free;
                    //power.Action.Trigger = action.Substring("free".Length);
                    if (action.ToLower().StartsWith("none"))
                        power.Action.Action = ActionType.None;
                    //power.Action.Trigger = action.Substring("none".Length);
                    if (action.ToLower().StartsWith("no action"))
                        power.Action.Action = ActionType.None;
                    //power.Action.Trigger = action.Substring("no action".Length);
                    if (action == "") power.Action.Action = ActionType.None;
                }
                else
                {
                    // Might be an aura
                    var rangeNode = XmlHelper.FindChild(powerNode, "Range");
                    if (rangeNode != null)
                    {
                        var range = XmlHelper.GetIntAttribute(rangeNode, "FinalValue");
                        var details = XmlHelper.NodeText(powerNode, "Details");

                        if (range == 0)
                        {
                            power.Action = null;
                            power.Details = details;
                        }
                        else
                        {
                            var aura = new Aura();
                            aura.Name = power.Name;
                            aura.Details = range + " " + details;

                            c.Auras.Add(aura);

                            return;
                        }
                    }
                }

                if (power.Action != null)
                    power.Action.Trigger = XmlHelper.NodeText(powerNode, "Trigger");

                if (power.Action != null && power.Action.Trigger != "")
                {
                    var trigger = power.Action.Trigger.Trim();

                    // Remove leading comma
                    if (trigger.StartsWith(", "))
                        trigger = trigger.Substring(2);

                    // Remove leading semicolon
                    if (trigger.StartsWith("; "))
                        trigger = trigger.Substring(2);

                    // Remove parentheses
                    if (trigger.StartsWith("("))
                        trigger = trigger.Substring(1);
                    if (trigger.EndsWith(")"))
                        trigger = trigger.Substring(0, trigger.Length - 1);

                    power.Action.Trigger = trigger;
                }

                var keywordsNode = XmlHelper.FindChild(powerNode, "Keywords");
                if (keywordsNode != null)
                    foreach (XmlNode keywordNode in keywordsNode.ChildNodes)
                    {
                        var keywordNameNode = XmlHelper.FindChild(keywordNode, "ReferencedObject");
                        var str = XmlHelper.NodeText(keywordNameNode, "Name");

                        if (str != "")
                        {
                            if (power.Keywords != "")
                                power.Keywords += ", ";

                            power.Keywords += str;
                        }
                    }

                var attacksNode = XmlHelper.FindChild(powerNode, "Attacks");
                if (attacksNode != null)
                {
                    var range = "";
                    var target = "";
                    var damage = "";

                    var desc = "";

                    foreach (XmlNode attackNode in attacksNode.ChildNodes)
                    {
                        var bonusNode = XmlHelper.FindChild(attackNode, "AttackBonuses");
                        var isAttack = bonusNode != null && bonusNode.ChildNodes.Count != 0;

                        foreach (XmlNode fieldNode in attackNode.ChildNodes)
                        {
                            if (fieldNode.Name == "Name")
                                continue;

                            if (fieldNode.Name == "Range")
                            {
                                range = fieldNode.InnerText.ToLower();
                                range = range.Replace("basic ", "");
                            }
                            else if (fieldNode.Name == "Targets")
                            {
                                target = fieldNode.InnerText;
                            }
                            else if (fieldNode.Name == "AttackBonuses")
                            {
                                if (fieldNode.FirstChild != null)
                                {
                                    var bonus = XmlHelper.GetIntAttribute(fieldNode.FirstChild, "FinalValue");
                                    var defenceNode = XmlHelper.FindChild(fieldNode.FirstChild, "Defense");
                                    var defenceNameNode =
                                        XmlHelper.FindChild(XmlHelper.FindChild(defenceNode, "ReferencedObject"),
                                            "DefenseName");
                                    var defence = defenceNameNode.InnerText;

                                    power.Attack = new PowerAttack();
                                    power.Attack.Bonus = bonus;
                                    power.Attack.Defence = (DefenceType)Enum.Parse(typeof(DefenceType), defence);
                                }
                            }
                            else if (fieldNode.Name == "Description")
                            {
                                power.Description = fieldNode.InnerText;
                            }
                            else if (fieldNode.Name == "Damage")
                            {
                                damage = Statistics.NormalDamage(c.Level);
                            }
                            else
                            {
                                var header = XmlHelper.NodeText(fieldNode, "Name");
                                if (header == "")
                                    header = "Hit";

                                if (!isAttack)
                                    if (header == "Hit" || header == "Miss")
                                        continue;

                                var dmgNode = XmlHelper.FindChild(fieldNode, "Damage");
                                if (dmgNode != null)
                                    damage = XmlHelper.NodeText(dmgNode, "Expression");

                                var details = XmlHelper.NodeText(fieldNode, "Description");
                                if (damage != "" && details == "")
                                    details = "damage";
                                if (details == "")
                                    continue;

                                var line = header + ": " + damage + " " + details;

                                var special = XmlHelper.NodeText(fieldNode, "Special");
                                if (special != "")
                                    line += Environment.NewLine + "Special: " + special;

                                // Look through Attacks node
                                var subAttacksNode = XmlHelper.FindChild(fieldNode, "Attacks");
                                if (subAttacksNode != null)
                                    foreach (XmlNode subAttackNode in subAttacksNode.ChildNodes)
                                    {
                                        var str = secondary_attack(subAttackNode);
                                        if (str != "")
                                        {
                                            line += Environment.NewLine;
                                            line += str;
                                        }
                                    }

                                if (desc != "")
                                    desc += "\n";

                                desc += line;
                            }
                        }
                    }

                    // TODO: Sustain
                    // TODO: Aftereffect
                    // TODO: FailedSavingThrows

                    var rngTgt = range;
                    if (target != "")
                        rngTgt += " (" + target + ")";
                    power.Range = rngTgt;

                    power.Details = desc;
                }
                else
                {
                    power.Details = XmlHelper.NodeText(powerNode, "Details");
                }

                c.CreaturePowers.Add(power);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private static string secondary_attack(XmlNode attackNode)
        {
            // Name & description
            var name = XmlHelper.NodeText(attackNode, "Name");
            var details = XmlHelper.NodeText(attackNode, "Description");

            var str = name + ": " + details;

            var hit = "";
            var miss = "";
            var effect = "";

            foreach (XmlNode node in attackNode.ChildNodes)
                switch (node.Name)
                {
                    case "Hit":
                        hit = secondary_attack_details(node);
                        break;
                    case "Miss":
                        miss = secondary_attack_details(node);
                        break;
                    case "Effect":
                        effect = secondary_attack_details(node);
                        break;
                }

            if (hit != "")
                str += Environment.NewLine + "Hit: " + hit;

            if (miss != "")
                str += Environment.NewLine + "Miss: " + miss;

            if (effect != "")
                str += Environment.NewLine + "Effect: " + effect;

            return str;
        }

        private static string secondary_attack_details(XmlNode detailsNode)
        {
            var dmgNode = XmlHelper.FindChild(detailsNode, "Damage");
            var dmg = XmlHelper.NodeText(dmgNode, "Expression");

            var desc = XmlHelper.NodeText(detailsNode, "Description");

            if (dmg != "" && desc != "")
                return dmg + " " + desc;
            if (dmg != "" && desc == "")
                return dmg + " damage";
            if (dmg == "" && desc != "")
                return desc;

            return "";
        }

        public static Hero ImportHero(string xml)
        {
            var hero = new Hero();

            try
            {
                xml = xml.Replace("RESISTANCE_+", "RESISTANCE_PLUS");
                xml = xml.Replace("CORMYR!", "CORMYR");
                xml = xml.Replace("SILVER_TONGUE,", "SILVER_TONGUE");

                var doc = XmlHelper.LoadSource(xml);
                if (doc == null)
                    return null;

                var csNode = XmlHelper.FindChild(doc.DocumentElement, "CharacterSheet");
                if (csNode != null)
                {
                    var detailsNode = XmlHelper.FindChild(csNode, "Details");
                    if (detailsNode != null)
                    {
                        hero.Name = XmlHelper.NodeText(detailsNode, "name").Trim();
                        hero.Player = XmlHelper.NodeText(detailsNode, "Player").Trim();
                        hero.Level = int.Parse(XmlHelper.NodeText(detailsNode, "Level"));

                        var portraitFile = XmlHelper.NodeText(detailsNode, "Portrait").Trim();
                        if (portraitFile != "")
                            try
                            {
                                var preamble = "file://";
                                if (portraitFile.StartsWith(preamble))
                                    portraitFile = portraitFile.Substring(preamble.Length);

                                if (File.Exists(portraitFile))
                                    hero.Portrait = Image.FromFile(portraitFile);
                            }
                            catch
                            {
                            }
                    }

                    var statsNode = XmlHelper.FindChild(csNode, "StatBlock");
                    if (statsNode != null)
                    {
                        // HP
                        var hpNode = get_stat_node(statsNode, "Hit Points");
                        if (hpNode != null)
                            hero.Hp = XmlHelper.GetIntAttribute(hpNode, "value");

                        // AC
                        var acNode = get_stat_node(statsNode, "AC");
                        if (acNode != null)
                            hero.Ac = XmlHelper.GetIntAttribute(acNode, "value");

                        // Fortitude
                        var fortNode = get_stat_node(statsNode, "Fortitude Defense");
                        if (fortNode != null)
                            hero.Fortitude = XmlHelper.GetIntAttribute(fortNode, "value");

                        // Reflex
                        var refNode = get_stat_node(statsNode, "Reflex Defense");
                        if (refNode != null)
                            hero.Reflex = XmlHelper.GetIntAttribute(refNode, "value");

                        // Will
                        var willNode = get_stat_node(statsNode, "Will Defense");
                        if (willNode != null)
                            hero.Will = XmlHelper.GetIntAttribute(willNode, "value");

                        // Initiative bonus
                        var initNode = get_stat_node(statsNode, "Initiative");
                        if (initNode != null)
                            hero.InitBonus = XmlHelper.GetIntAttribute(initNode, "value");

                        // Passive perception
                        var percNode = get_stat_node(statsNode, "Passive Perception");
                        if (percNode != null)
                            hero.PassivePerception = XmlHelper.GetIntAttribute(percNode, "value");

                        // Passive insight
                        var insNode = get_stat_node(statsNode, "Passive Insight");
                        if (insNode != null)
                            hero.PassiveInsight = XmlHelper.GetIntAttribute(insNode, "value");
                    }

                    var rulesNode = XmlHelper.FindChild(csNode, "RulesElementTally");
                    if (rulesNode != null)
                    {
                        // Race
                        var raceNode = XmlHelper.FindChildWithAttribute(rulesNode, "type", "Race");
                        if (raceNode != null)
                            hero.Race = XmlHelper.GetAttribute(raceNode, "name");

                        // Class
                        var classNode = XmlHelper.FindChildWithAttribute(rulesNode, "type", "Class");
                        if (classNode != null)
                            hero.Class = XmlHelper.GetAttribute(classNode, "name");

                        // Paragon Path
                        var ppNode = XmlHelper.FindChildWithAttribute(rulesNode, "type", "Paragon Path");
                        if (ppNode != null)
                            hero.ParagonPath = XmlHelper.GetAttribute(ppNode, "name");

                        // Epic Destiny
                        var edNode = XmlHelper.FindChildWithAttribute(rulesNode, "type", "Epic Destiny");
                        if (edNode != null)
                            hero.EpicDestiny = XmlHelper.GetAttribute(edNode, "name");

                        // Role
                        var roleNode = XmlHelper.FindChildWithAttribute(rulesNode, "type", "Role");
                        if (roleNode != null)
                            hero.Role = (HeroRoleType)Enum.Parse(typeof(HeroRoleType),
                                XmlHelper.GetAttribute(roleNode, "name"));

                        // Power source
                        var sourceNode = XmlHelper.FindChildWithAttribute(rulesNode, "type", "Power Source");
                        if (sourceNode != null)
                            hero.PowerSource = XmlHelper.GetAttribute(sourceNode, "name");

                        // Languages
                        var langNodes = XmlHelper.FindChildrenWithAttribute(rulesNode, "type", "Language");
                        foreach (var langNode in langNodes)
                        {
                            var lang = XmlHelper.GetAttribute(langNode, "name");
                            if (lang != "")
                            {
                                if (hero.Languages != "")
                                    hero.Languages += ", ";

                                hero.Languages += lang;
                            }
                        }
                    }
                }

                var levelNode = XmlHelper.FindChild(doc.DocumentElement, "Level");
                if (levelNode != null)
                {
                    var level1 = XmlHelper.FindChildWithAttribute(levelNode, "name", "1");
                    if (level1 != null)
                    {
                        if (hero.Race == "")
                        {
                            var raceNode = XmlHelper.FindChildWithAttribute(level1, "type", "Race");
                            if (raceNode != null)
                                hero.Race = XmlHelper.GetAttribute(raceNode, "name");
                        }

                        if (hero.Class == "")
                        {
                            var classNode = XmlHelper.FindChildWithAttribute(level1, "type", "Class");
                            if (classNode != null)
                                hero.Class = XmlHelper.GetAttribute(classNode, "name");
                        }
                    }

                    var level11 = XmlHelper.FindChildWithAttribute(levelNode, "name", "11");
                    if (level11 != null)
                        if (hero.ParagonPath == "")
                        {
                            var ppNode = XmlHelper.FindChildWithAttribute(level11, "type", "ParagonPath");
                            if (ppNode != null)
                                hero.ParagonPath = XmlHelper.GetAttribute(ppNode, "name");
                        }

                    var level21 = XmlHelper.FindChildWithAttribute(levelNode, "name", "21");
                    if (level21 != null)
                        if (hero.EpicDestiny == "")
                        {
                            var edNode = XmlHelper.FindChildWithAttribute(level21, "type", "EpicDestiny");
                            if (edNode != null)
                                hero.EpicDestiny = XmlHelper.GetAttribute(edNode, "name");
                        }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return hero;
        }

        private static XmlNode get_stat_node(XmlNode parent, string name)
        {
            var node = XmlHelper.FindChildWithAttribute(parent, "name", name);
            if (node != null)
                return node;

            foreach (XmlNode child in parent.ChildNodes)
            {
                node = XmlHelper.FindChildWithAttribute(child, "name", name);
                if (node != null)
                    return child;
            }

            return null;
        }
    }
}
