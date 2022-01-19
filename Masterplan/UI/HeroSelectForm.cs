﻿using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class HeroSelectForm : Form
    {
        public Hero SelectedHero
        {
            get
            {
                if (YesBtn.Checked)
                    return HeroBox.SelectedItem as Hero;
                return null;
            }
        }

        public HeroSelectForm(Hero selected)
        {
            InitializeComponent();

            foreach (var hero in Session.Project.Heroes)
                HeroBox.Items.Add(hero);

            if (selected != null)
            {
                HeroBox.SelectedItem = selected;
                YesBtn.Checked = true;
            }
            else
            {
                HeroBox.SelectedIndex = 0;
                NoBtn.Checked = true;
            }
        }

        private void option_changed(object sender, EventArgs e)
        {
            HeroBox.Enabled = YesBtn.Checked;

            if (YesBtn.Checked)
                InfoLbl.Text = "The effect will be added to " + SelectedHero + "'s list.";
            else
                InfoLbl.Text = "The effect will be added to the list of predefined effects for this encounter only.";

            InfoLbl.Text += Environment.NewLine;
            InfoLbl.Text += "To apply the effect again, simply select it from the list.";
        }
    }
}
