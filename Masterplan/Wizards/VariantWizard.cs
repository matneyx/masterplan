using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Wizards
{
    internal class VariantWizard : Wizard
    {
        private VariantData _fData = new VariantData();

        public override object Data
        {
            get => _fData;
            set => _fData = value as VariantData;
        }

        public VariantWizard()
            : base("Select Creature")
        {
        }

        public override void AddPages()
        {
            Pages.Add(new VariantBasePage());
            Pages.Add(new VariantTemplatesPage());
            Pages.Add(new VariantRolePage());
            Pages.Add(new VariantFinishPage());
        }

        public override int NextPageIndex(int currentPage)
        {
            if (currentPage == 0)
                return _fData.BaseCreature.Role is Minion ? 3 : 1;

            if (currentPage == 1)
                return _fData.Roles.Count == 1 ? 3 : 2;

            return base.NextPageIndex(currentPage);
        }

        public override int BackPageIndex(int currentPage)
        {
            if (currentPage == 3)
            {
                if (_fData.BaseCreature.Role is Minion)
                    return 0;

                return _fData.Roles.Count == 1 ? 1 : 2;
            }

            return base.BackPageIndex(currentPage);
        }

        public override void OnFinish()
        {
        }

        public override void OnCancel()
        {
        }
    }

    internal class VariantData
    {
        public ICreature BaseCreature = null;

        public int SelectedRoleIndex = 0;
        public List<CreatureTemplate> Templates = new List<CreatureTemplate>();

        public List<RoleType> Roles
        {
            get
            {
                var roles = new List<RoleType>();

                if (BaseCreature?.Role is ComplexRole)
                {
                    var cr = BaseCreature.Role as ComplexRole;
                    roles.Add(cr.Type);
                }

                foreach (var ct in Templates)
                    if (!roles.Contains(ct.Role))
                        roles.Add(ct.Role);

                roles.Sort();

                return roles;
            }
        }
    }
}
