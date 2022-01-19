using System.Windows.Forms;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Class providing static methods for saving and restoring the visible items in a ListView control.
    /// </summary>
    public class ListState
    {
        private int _fSelectedIndex = -1;
        private int _fTopIndex = -1;

        /// <summary>
        ///     Saves the state of a ListView control.
        ///     The state can be re-applied to the listview by calling ListState.SetState.
        /// </summary>
        /// <param name="list">The ListView control.</param>
        /// <returns>Returns a ListState object containing the current state of the ListView control.</returns>
        public static ListState GetState(ListView list)
        {
            var state = new ListState();

            state._fTopIndex = list.Items.IndexOf(list.TopItem);

            state._fSelectedIndex = -1;
            if (list.SelectedIndices.Count != 0)
                state._fSelectedIndex = list.SelectedIndices[0];

            return state;
        }

        /// <summary>
        ///     Restores a saved view to a ListView control.
        /// </summary>
        /// <param name="list">The ListView control.</param>
        /// <param name="state">The ListState object containing the saved state to be reset.</param>
        public static void SetState(ListView list, ListState state)
        {
            if (state._fTopIndex != -1 && state._fTopIndex < list.Items.Count)
                list.TopItem = list.Items[state._fTopIndex];

            if (state._fSelectedIndex != -1 && state._fSelectedIndex < list.Items.Count)
                list.SelectedIndices.Add(state._fSelectedIndex);
        }
    }
}
