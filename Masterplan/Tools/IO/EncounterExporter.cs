using System;
using System.Xml;
using Masterplan.Data;

namespace Masterplan.Tools.IO
{
    internal class EncounterExporter
    {
        public static string ExportXml(Encounter enc)
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("Encounter"));

            XmlHelper.CreateChild(doc, doc.DocumentElement, "Source").InnerText = "Masterplan Adventure Design Studio";

            var creaturesNode = XmlHelper.CreateChild(doc, doc.DocumentElement, "Creatures");
            foreach (var slot in enc.Slots)
            {
                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

                foreach (var cd in slot.CombatData)
                {
                    var creatureNode = XmlHelper.CreateChild(doc, creaturesNode, "Creature");

                    var role = "";
                    if (creature.Role is Minion)
                        role += "Minion";
                    foreach (var type in slot.Card.Roles)
                    {
                        if (role != "")
                            role += ", ";

                        role += type;
                    }

                    if (slot.Card.Leader)
                        role += " (L)";

                    XmlHelper.CreateChild(doc, creatureNode, "Name").InnerText = cd.DisplayName;
                    XmlHelper.CreateChild(doc, creatureNode, "Level").InnerText = slot.Card.Level.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "Role").InnerText = role;
                    XmlHelper.CreateChild(doc, creatureNode, "Size").InnerText = creature.Size.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "Type").InnerText = creature.Type.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "Origin").InnerText = creature.Origin.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "Keywords").InnerText = creature.Keywords;
                    XmlHelper.CreateChild(doc, creatureNode, "Size").InnerText = creature.Size.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "HP").InnerText = slot.Card.Hp.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "InitBonus").InnerText = slot.Card.Initiative.ToString();
                    XmlHelper.CreateChild(doc, creatureNode, "Speed").InnerText = slot.Card.Movement;

                    var defencesNode = XmlHelper.CreateChild(doc, creatureNode, "Defenses");

                    XmlHelper.CreateChild(doc, defencesNode, "AC").InnerText = slot.Card.Ac.ToString();
                    XmlHelper.CreateChild(doc, defencesNode, "Fortitude").InnerText = slot.Card.Fortitude.ToString();
                    XmlHelper.CreateChild(doc, defencesNode, "Reflex").InnerText = slot.Card.Reflex.ToString();
                    XmlHelper.CreateChild(doc, defencesNode, "Will").InnerText = slot.Card.Will.ToString();

                    if (slot.Card.Regeneration != null)
                    {
                        var regenNode = XmlHelper.CreateChild(doc, creatureNode, "Regeneration");

                        XmlHelper.CreateChild(doc, regenNode, "Value").InnerText =
                            slot.Card.Regeneration.Value.ToString();
                        XmlHelper.CreateChild(doc, regenNode, "Details").InnerText = slot.Card.Regeneration.Details;
                    }

                    var dmgNode = XmlHelper.CreateChild(doc, creatureNode, "Damage");

                    foreach (var mod in slot.Card.DamageModifiers)
                        if (mod.Value < 0)
                        {
                            var modNode = XmlHelper.CreateChild(doc, dmgNode, "Resist");

                            XmlHelper.CreateChild(doc, modNode, "Type").InnerText = mod.Type.ToString();
                            XmlHelper.CreateChild(doc, modNode, "Details").InnerText = Math.Abs(mod.Value).ToString();
                        }
                        else if (mod.Value > 0)
                        {
                            var modNode = XmlHelper.CreateChild(doc, dmgNode, "Vulnerable");

                            XmlHelper.CreateChild(doc, modNode, "Type").InnerText = mod.Type.ToString();
                            XmlHelper.CreateChild(doc, modNode, "Details").InnerText = Math.Abs(mod.Value).ToString();
                        }
                        else
                        {
                            var modNode = XmlHelper.CreateChild(doc, dmgNode, "Immune");

                            XmlHelper.CreateChild(doc, modNode, "Type").InnerText = mod.Type.ToString();
                        }

                    if (slot.Card.Resist != "")
                        XmlHelper.CreateChild(doc, dmgNode, "Resist").InnerText = slot.Card.Resist;
                    if (slot.Card.Vulnerable != "")
                        XmlHelper.CreateChild(doc, dmgNode, "Vulnerable").InnerText = slot.Card.Vulnerable;
                    if (slot.Card.Immune != "")
                        XmlHelper.CreateChild(doc, dmgNode, "Immune").InnerText = slot.Card.Immune;

                    var abilitiesNode = XmlHelper.CreateChild(doc, creatureNode, "AbilityModifiers");
                    XmlHelper.CreateChild(doc, abilitiesNode, "Strength").InnerText =
                        creature.Strength.Modifier.ToString();
                    XmlHelper.CreateChild(doc, abilitiesNode, "Constitution").InnerText =
                        creature.Constitution.Modifier.ToString();
                    XmlHelper.CreateChild(doc, abilitiesNode, "Dexterity").InnerText =
                        creature.Dexterity.Modifier.ToString();
                    XmlHelper.CreateChild(doc, abilitiesNode, "Intelligence").InnerText =
                        creature.Intelligence.Modifier.ToString();
                    XmlHelper.CreateChild(doc, abilitiesNode, "Wisdom").InnerText = creature.Wisdom.Modifier.ToString();
                    XmlHelper.CreateChild(doc, abilitiesNode, "Charisma").InnerText =
                        creature.Charisma.Modifier.ToString();

                    XmlHelper.CreateChild(doc, creatureNode, "Senses").InnerText = slot.Card.Senses;
                    XmlHelper.CreateChild(doc, creatureNode, "Skills").InnerText = slot.Card.Skills;
                    XmlHelper.CreateChild(doc, creatureNode, "Equipment").InnerText = slot.Card.Equipment;
                    XmlHelper.CreateChild(doc, creatureNode, "Tactics").InnerText = slot.Card.Tactics;
                }
            }

            // PCs
            var heroesNode = XmlHelper.CreateChild(doc, doc.DocumentElement, "PCs");
            foreach (var hero in Session.Project.Heroes)
            {
                var heroNode = XmlHelper.CreateChild(doc, heroesNode, "PC");

                XmlHelper.CreateChild(doc, heroNode, "Name").InnerText = hero.Name;
                XmlHelper.CreateChild(doc, heroNode, "Description").InnerText = hero.Info;
                XmlHelper.CreateChild(doc, heroNode, "Size").InnerText = hero.Size.ToString();
                XmlHelper.CreateChild(doc, heroNode, "HP").InnerText = hero.Hp.ToString();
                XmlHelper.CreateChild(doc, heroNode, "InitBonus").InnerText = hero.InitBonus.ToString();

                var defencesNode = XmlHelper.CreateChild(doc, heroNode, "Defenses");

                XmlHelper.CreateChild(doc, defencesNode, "AC").InnerText = hero.Ac.ToString();
                XmlHelper.CreateChild(doc, defencesNode, "Fortitude").InnerText = hero.Fortitude.ToString();
                XmlHelper.CreateChild(doc, defencesNode, "Reflex").InnerText = hero.Reflex.ToString();
                XmlHelper.CreateChild(doc, defencesNode, "Will").InnerText = hero.Will.ToString();
            }

            return doc.OuterXml;
        }
    }
}
