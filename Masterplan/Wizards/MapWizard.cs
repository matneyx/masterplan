using Masterplan.Tools.Generators;

namespace Masterplan.Wizards
{
    internal class MapWizard : Wizard
    {
        private static MapBuilderData _fData = new MapBuilderData();

        public override object Data
        {
            get => _fData;
            set => _fData = value as MapBuilderData;
        }

        public MapWizard(bool delveOnly)
            : base("AutoBuild Map")
        {
            _fData.DelveOnly = delveOnly;
        }

        public override void AddPages()
        {
            if (!_fData.DelveOnly)
                Pages.Add(new MapTypePage());

            Pages.Add(new MapLibrariesPage());
            Pages.Add(new MapAreasPage());
            Pages.Add(new MapSizePage());
        }

        public override int NextPageIndex(int currentPage)
        {
            if (currentPage == 1)
                switch (_fData.Type)
                {
                    case MapAutoBuildType.Warren:
                        return 2;
                    case MapAutoBuildType.FilledArea:
                    case MapAutoBuildType.Freeform:
                        return 3;
                }

            return base.NextPageIndex(currentPage);
        }

        public override int BackPageIndex(int currentPage)
        {
            if (currentPage == 2 || currentPage == 3)
                return 1;

            return base.BackPageIndex(currentPage);
        }

        public override void OnFinish()
        {
        }

        public override void OnCancel()
        {
        }
    }
}
