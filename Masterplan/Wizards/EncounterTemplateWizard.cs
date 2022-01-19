using System.Collections.Generic;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Wizards
{
    internal class EncounterTemplateWizard : Wizard
    {
        private readonly Encounter _fEncounter;

        private AdviceData _fData = new AdviceData();

        public override object Data
        {
            get => _fData;
            set => _fData = value as AdviceData;
        }

        public EncounterTemplateWizard(List<Pair<EncounterTemplateGroup, EncounterTemplate>> templates, Encounter enc,
            int partyLevel)
            : base("Encounter Templates")
        {
            _fData.Templates = templates;
            _fData.PartyLevel = partyLevel;
            _fEncounter = enc;

            _fData.TabulaRasa = _fEncounter.Count == 0;
        }

        public override void AddPages()
        {
            Pages.Add(new EncounterTemplatePage());
            Pages.Add(new EncounterSelectionPage());
        }

        public override int NextPageIndex(int currentPage)
        {
            return base.NextPageIndex(currentPage);
        }

        public override int BackPageIndex(int currentPage)
        {
            return base.BackPageIndex(currentPage);
        }

        public override void OnFinish()
        {
            foreach (var templateSlot in _fData.SelectedTemplate.Slots)
                if (_fData.FilledSlots.ContainsKey(templateSlot))
                {
                    var slot = new EncounterSlot();
                    slot.Card = _fData.FilledSlots[templateSlot];

                    for (var n = 0; n != templateSlot.Count; ++n)
                        slot.CombatData.Add(new CombatData());

                    _fEncounter.Slots.Add(slot);
                }
        }

        public override void OnCancel()
        {
        }
    }

    internal class AdviceData
    {
        public Dictionary<EncounterTemplateSlot, EncounterCard> FilledSlots =
            new Dictionary<EncounterTemplateSlot, EncounterCard>();

        public int PartyLevel = Session.Project.Party.Level;
        public EncounterTemplate SelectedTemplate = null;
        public bool TabulaRasa = true;

        public List<Pair<EncounterTemplateGroup, EncounterTemplate>> Templates =
            new List<Pair<EncounterTemplateGroup, EncounterTemplate>>();
    }
}
