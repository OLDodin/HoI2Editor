using System.Collections.Generic;
using System.Linq;
using HoI2Editor.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Controllers
{
    /// <summary>
    ///     Data check process
    /// </summary>
    public static class DataChecker
    {
        #region Internal field

        /// <summary>
        ///     Check result output form
        /// </summary>
        private static DataCheckerForm _form;

        #endregion

        #region scenario

        /// <summary>
        ///     Check scenario data
        /// </summary>
        public static void CheckScenario()
        {
            // Do nothing before loading the scenario
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            WriteLine(Resources.CheckMessageScenario);

            List<Province> provinces = Provinces.Items.Where(province => province.IsLand && province.Id > 0).ToList();
            if (Scenarios.Data.Map != null)
            {
                provinces = Scenarios.Data.Map.All
                    ? provinces.Where(province => !Scenarios.Data.Map.No.Contains(province.Id)).ToList()
                    : provinces.Where(province => Scenarios.Data.Map.Yes.Contains(province.Id)).ToList();
            }

            // Owned Providence
            CheckOwnedProvinces(provinces);
            // Domination Providence
            CheckControlledProvinces(provinces);

            WriteLine(Resources.CheckMessageDone);
            WriteLine();
        }

        /// <summary>
        ///     Checking owned provisions
        /// </summary>
        /// <param name="provinces">Provincial list</param>
        private static void CheckOwnedProvinces(IEnumerable<Province> provinces)
        {
            foreach (Province province in provinces)
            {
                int id = province.Id;
                ProvinceSettings ps = Scenarios.GetProvinceSettings(id);
                IEnumerable<Country> countries = Scenarios.Data.Countries
                    .Where(settings => settings.OwnedProvinces.Contains(id))
                    .Select(settings => settings.Country).ToList();
                if (!countries.Any())
                {
                    string name = Scenarios.GetProvinceName(province, ps);
                    WriteLine("{0}: {1} [{2}]", Resources.CheckResultNoProvinceOwner, id, name);
                    Log.Error("[Scenario] no province owner: {0} [{1}]", id, name);
                }
                else if (countries.Count() > 1)
                {
                    string name = Scenarios.GetProvinceName(province, ps);
                    string tagList = Countries.GetTagList(countries);
                    WriteLine("{0}: {1} [{2}] - {3}", Resources.CheckResultDuplicatedProvinceOwner, id, name, tagList);
                    Log.Error("[Scenario] duplicated province owner: {0} [{1}] - {2}", id, name, tagList);
                }
            }
        }

        /// <summary>
        ///     Check for Domination Province
        /// </summary>
        /// <param name="provinces">Provincial list</param>
        private static void CheckControlledProvinces(IEnumerable<Province> provinces)
        {
            foreach (Province province in provinces)
            {
                int id = province.Id;
                ProvinceSettings ps = Scenarios.GetProvinceSettings(id);
                IEnumerable<Country> countries = Scenarios.Data.Countries
                    .Where(settings => settings.OwnedProvinces.Contains(id))
                    .Select(settings => settings.Country).ToList();
                if (!countries.Any())
                {
                    string name = Scenarios.GetProvinceName(province, ps);
                    WriteLine("{0}: {1} [{2}]", Resources.CheckResultNoProvinceController, id, name);
                    Log.Error("[Scenario] no province controller: {0} [{1}]", id, name);
                }
                else if (countries.Count() > 1)
                {
                    string name = Scenarios.GetProvinceName(province, ps);
                    string tagList = Countries.GetTagList(countries);
                    WriteLine("{0}: {1} [{2}] - {3}", Resources.CheckResultDuplicatedProvinceController, id, name,
                        tagList);
                    Log.Error("[Scenario] duplicated province controller: {0} [{1}] - {2}", id, name, tagList);
                }
            }
        }

        #endregion

        #region Check result output

        /// <summary>
        ///     Output check result
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public static void Write(string s, params object[] args)
        {
            if (_form == null)
            {
                _form = new DataCheckerForm();
            }
            if (!_form.Visible)
            {
                _form.Show();
            }
            _form.Write(s, args);
        }

        /// <summary>
        ///     Output check result
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public static void WriteLine(string s, params object[] args)
        {
            if (_form == null)
            {
                _form = new DataCheckerForm();
            }
            if (!_form.Visible)
            {
                _form.Show();
            }
            _form.WriteLine(s, args);
        }

        /// <summary>
        ///     Output check result
        /// </summary>
        public static void WriteLine()
        {
            if (_form == null)
            {
                _form = new DataCheckerForm();
            }
            if (!_form.Visible)
            {
                _form.Show();
            }
            _form.WriteLine();
        }

        #endregion
    }
}
