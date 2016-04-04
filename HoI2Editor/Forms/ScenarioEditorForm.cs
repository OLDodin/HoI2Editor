﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Controllers;
using HoI2Editor.Models;
using HoI2Editor.Pages;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     シナリオエディタフォーム
    /// </summary>
    internal partial class ScenarioEditorForm : Form
    {
        #region 内部フィールド

        /// <summary>
        ///     シナリオエディタのコントローラ
        /// </summary>
        private readonly ScenarioEditorController _controller;

        /// <summary>
        ///     タブページ番号
        /// </summary>
        private TabPageNo _tabPageNo;

        /// <summary>
        ///     タブページの初期化フラグ
        /// </summary>
        private readonly bool[] _tabPageInitialized = new bool[Enum.GetValues(typeof (TabPageNo)).Length];

        /// <summary>
        ///     メインタブ
        /// </summary>
        private ScenarioEditorMainPage _mainPage;

        /// <summary>
        ///     同盟タブ
        /// </summary>
        private ScenarioEditorAlliancePage _alliancePage;

        /// <summary>
        ///     国家タブ
        /// </summary>
        private ScenarioEditorCountryPage _countryPage;

        /// <summary>
        ///     政府タブ
        /// </summary>
        private ScenarioEditorGovernmentPage _governmentPage;

        /// <summary>
        ///     初期部隊タブ
        /// </summary>
        private ScenarioEditorOobPage _oobPage;

        /// <summary>
        ///     編集項目IDとコントロールの関連付け
        /// </summary>
        internal readonly Dictionary<ScenarioEditorItemId, Control> _itemControls =
            new Dictionary<ScenarioEditorItemId, Control>();

        /// <summary>
        ///     技術項目リスト
        /// </summary>
        private List<TechItem> _techs;

        /// <summary>
        ///     発明イベントリスト
        /// </summary>
        private List<TechEvent> _inventions;

        /// <summary>
        ///     技術ツリーパネルのコントローラ
        /// </summary>
        private TechTreePanelController _techTreePanelController;

        /// <summary>
        ///     マップパネルのコントローラ
        /// </summary>
        private MapPanelController _mapPanelController;

        /// <summary>
        ///     マップパネルの初期化フラグ
        /// </summary>
        private bool _mapPanelInitialized;

        #endregion

        #region 内部定数

        /// <summary>
        ///     タブページ番号
        /// </summary>
        private enum TabPageNo
        {
            Main, // メイン
            Alliance, // 同盟
            Relation, // 関係
            Trade, // 貿易
            Country, // 国家
            Government, // 政府
            Technology, // 技術
            Province, // プロヴィンス
            Oob // 初期部隊
        }

        #endregion

        #region 初期化

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="controller">シナリオエディタコントローラ</param>
        internal ScenarioEditorForm(ScenarioEditorController controller)
        {
            InitializeComponent();

            _controller = controller;

            // フォームの初期化
            InitForm();
        }

        #endregion

        #region データ処理

        /// <summary>
        ///     データ読み込み後の処理
        /// </summary>
        internal void OnFileLoaded()
        {
            // 読み込み前ならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // シナリオ関連情報を初期化する
            Scenarios.Init();

            // 各タブページの初期化済み状態をクリアする
            foreach (TabPageNo page in Enum.GetValues(typeof (TabPageNo)))
            {
                _tabPageInitialized[(int) page] = false;
            }

            // 編集項目を更新する
            OnMainTabPageFileLoad();
            OnAllianceTabPageFileLoad();
            OnRelationTabPageFileLoad();
            OnTradeTabPageFileLoad();
            OnCountryTabPageFileLoad();
            OnGovernmentTabPageFileLoad();
            OnTechTabPageFileLoad();
            OnProvinceTabPageFileLoad();
            OnOobTabPageFileLoad();
        }

        /// <summary>
        ///     データ保存後の処理
        /// </summary>
        internal void OnFileSaved()
        {
            // 各タブページの初期化済み状態をクリアする
            foreach (TabPageNo page in Enum.GetValues(typeof (TabPageNo)))
            {
                _tabPageInitialized[(int) page] = false;
            }

            // 強制的に選択タブの表示を更新する
            OnScenarioTabControlSelectedIndexChanged(null, null);
        }

        /// <summary>
        ///     編集項目更新時の処理
        /// </summary>
        /// <param name="id">編集項目ID</param>
        internal void OnItemChanged(EditorItemId id)
        {
            // 何もしない
        }

        /// <summary>
        ///     マップ読み込み完了時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapFileLoad(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            if (e.Cancelled)
            {
                return;
            }

            // プロヴィンスタブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Province)
            {
                return;
            }

            // マップパネルを更新する
            UpdateMapPanel();
        }

        #endregion

        #region フォーム

        /// <summary>
        ///     フォームの初期化
        /// </summary>
        private void InitForm()
        {
            // ウィンドウの位置
            Location = HoI2EditorController.Settings.ScenarioEditor.Location;
            Size = HoI2EditorController.Settings.ScenarioEditor.Size;

            // 技術ツリーパネル
            _techTreePanelController = new TechTreePanelController(techTreePictureBox) { ApplyItemStatus = true };
            _techTreePanelController.ItemMouseClick += OnTechTreeItemMouseClick;
            _techTreePanelController.QueryItemStatus += OnQueryTechTreeItemStatus;

            // マップパネル
            _mapPanelController = new MapPanelController(provinceMapPanel, provinceMapPictureBox);
            _controller.AttachMapPanel(_mapPanelController);
        }

        /// <summary>
        ///     フォーム読み込み時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // 国家データを初期化する
            Countries.Init();

            // 閣僚特性を初期化する
            Ministers.InitPersonality();

            // ユニットデータを初期化する
            Units.Init();

            // プロヴィンスデータを初期化する
            Provinces.Init();

            // ゲーム設定ファイルを読み込む
            Misc.Load();

            // 文字列定義ファイルを読み込む
            Config.Load();

            // マップを遅延読み込みする
            Maps.LoadAsync(MapLevel.Level2, OnMapFileLoad);

            // 指揮官データを遅延読み込みする
            Leaders.LoadAsync(null);

            // 閣僚データを遅延読み込みする
            Ministers.LoadAsync(null);

            // 技術データを遅延読み込みする
            Techs.LoadAsync(null);

            // プロヴィンスデータを遅延読み込みする
            Provinces.LoadAsync(null);

            // ユニットデータを遅延読み込みする
            Units.LoadAsync(null);

            // メインタブを初期化する
            OnMainTabPageSelected();

            // 表示項目を初期化する
            OnRelationTabPageFormLoad();
            OnTradeTabPageFormLoad();
            OnTechTabPageFormLoad();
            OnProvinceTabPageFormLoad();

            // シナリオファイル読み込み済みなら編集項目を更新する
            if (Scenarios.IsLoaded())
            {
                OnFileLoaded();
            }
        }

        /// <summary>
        ///     フォームクローズ時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = _controller.OnFormClosing();
        }

        /// <summary>
        ///     フォームクローズ後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            _controller.OnFormClosed();
        }

        /// <summary>
        ///     フォーム移動時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.ScenarioEditor.Location = Location;
            }
        }

        /// <summary>
        ///     フォームリサイズ時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.ScenarioEditor.Size = Size;
            }
        }

        /// <summary>
        ///     チェックボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckButtonClick(object sender, EventArgs e)
        {
            // プロヴィンスデータ読み込み完了まで待つ
            Provinces.WaitLoading();

            DataChecker.CheckScenario();
        }

        /// <summary>
        ///     再読み込みボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReloadButtonClick(object sender, EventArgs e)
        {
            _controller.QueryReload();
        }

        /// <summary>
        ///     保存ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            _controller.Save();
        }

        /// <summary>
        ///     閉じるボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     選択タブ変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            _tabPageNo = (TabPageNo) scenarioTabControl.SelectedIndex;

            switch (_tabPageNo)
            {
                case TabPageNo.Main:
                    OnMainTabPageSelected();
                    break;

                case TabPageNo.Alliance:
                    OnAllianceTabPageSelected();
                    break;

                case TabPageNo.Relation:
                    OnRelationTabPageSelected();
                    break;

                case TabPageNo.Trade:
                    OnTradeTabPageSelected();
                    break;

                case TabPageNo.Country:
                    OnCountryTabPageSelected();
                    break;

                case TabPageNo.Government:
                    OnGovernmentTabPageSelected();
                    break;

                case TabPageNo.Technology:
                    OnTechTabPageSelected();
                    break;

                case TabPageNo.Province:
                    OnProvinceTabPageSelected();
                    break;

                case TabPageNo.Oob:
                    OnOobTabPageSelected();
                    break;
            }
        }

        #endregion

        #region メインタブ

        /// <summary>
        ///     メインタブのファイル読み込み時の処理
        /// </summary>
        private void OnMainTabPageFileLoad()
        {
            // メインタブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Main)
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Main])
            {
                return;
            }

            // 編集項目を初期化する
            _mainPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Main] = true;
        }

        /// <summary>
        ///     メインタブ選択時の処理
        /// </summary>
        private void OnMainTabPageSelected()
        {
            // タブページを作成する
            if (_mainPage == null)
            {
                _mainPage = new ScenarioEditorMainPage(_controller, this);
                scenarioTabControl.TabPages[(int) TabPageNo.Main].Controls.Add(_mainPage);
            }

            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Main])
            {
                return;
            }

            // 編集項目を初期化する
            _mainPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Main] = true;
        }

        /// <summary>
        ///     パネル画像を更新する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        internal void UpdatePanelImage(string fileName)
        {
            _mainPage.UpdatePanelImage(fileName);
        }

        /// <summary>
        ///     プロパガンダ画像を更新する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <param name="fileName">プロパガンダ画像名</param>
        internal void UpdatePropagandaImage(Country country, string fileName)
        {
            _mainPage.UpdatePropagandaImage(country, fileName);
        }

        #endregion

        #region 同盟タブ

        /// <summary>
        ///     同盟タブのファイル読み込み時の処理
        /// </summary>
        private void OnAllianceTabPageFileLoad()
        {
            // 同盟タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Alliance)
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Alliance])
            {
                return;
            }

            // 編集項目を初期化する
            _alliancePage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Alliance] = true;
        }

        /// <summary>
        ///     同盟タブ選択時の処理
        /// </summary>
        private void OnAllianceTabPageSelected()
        {
            // タブページを作成する
            if (_alliancePage == null)
            {
                _alliancePage = new ScenarioEditorAlliancePage(_controller, this);
                scenarioTabControl.TabPages[(int) TabPageNo.Alliance].Controls.Add(_alliancePage);
            }

            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Alliance])
            {
                return;
            }

            // 編集項目を初期化する
            _alliancePage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Alliance] = true;
        }

        /// <summary>
        ///     同盟リストビューの項目文字列を設定する
        /// </summary>
        /// <param name="no">項目番号</param>
        /// <param name="s">文字列</param>
        internal void SetAllianceListItemText(int no, string s)
        {
            _alliancePage.SetAllianceListItemText(no, s);
        }

        #endregion

        #region 関係タブ

        #region 関係タブ - 共通

        /// <summary>
        ///     関係タブを初期化する
        /// </summary>
        private void InitRelationTab()
        {
            InitRelationItems();
            InitGuaranteedItems();
            InitNonAggressionItems();
            InitPeaceItems();
            InitIntelligenceItems();
        }

        /// <summary>
        ///     関係タブを更新する
        /// </summary>
        private void UpdateRelationTab()
        {
            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Relation])
            {
                return;
            }

            // 選択国リストを更新する
            UpdateCountryListBox(relationCountryListBox);

            // 選択国リストを有効化する
            EnableRelationCountryList();

            // 国家関係リストをクリアする
            ClearRelationList();

            // 編集項目を無効化する
            DisableRelationItems();
            DisableGuaranteedItems();
            DisableNonAggressionItems();
            DisablePeaceItems();
            DisableIntelligenceItems();

            // 編集項目をクリアする
            ClearRelationItems();
            ClearGuaranteedItems();
            ClearNonAggressionItems();
            ClearPeaceItems();
            ClearIntelligenceItems();

            // 国家関係リストを有効化する
            EnableRelationList();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Relation] = true;
        }

        /// <summary>
        ///     関係タブのフォーム読み込み時の処理
        /// </summary>
        private void OnRelationTabPageFormLoad()
        {
            // 関係タブを初期化する
            InitRelationTab();
        }

        /// <summary>
        ///     関係タブのファイル読み込み時の処理
        /// </summary>
        private void OnRelationTabPageFileLoad()
        {
            // 関係タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Relation)
            {
                return;
            }

            // 初回遷移時には表示を更新する
            UpdateRelationTab();
        }

        /// <summary>
        ///     関係タブ選択時の処理
        /// </summary>
        private void OnRelationTabPageSelected()
        {
            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 初回遷移時には表示を更新する
            UpdateRelationTab();
        }

        #endregion

        #region 国家リスト

        /// <summary>
        ///     国家リストボックスを有効化する
        /// </summary>
        private void EnableRelationCountryList()
        {
            relationCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     国家リストボックスの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ国家関係リストをクリアする
            if (relationCountryListBox.SelectedIndex < 0)
            {
                // 国家関係リストをクリアする
                ClearRelationList();
                return;
            }

            // 国家関係リストを更新する
            UpdateRelationList();
        }

        /// <summary>
        ///     国家関係の選択国を取得する
        /// </summary>
        /// <returns>国家関係の選択国</returns>
        private Country GetSelectedRelationCountry()
        {
            return relationCountryListBox.SelectedIndex >= 0
                ? Countries.Tags[relationCountryListBox.SelectedIndex]
                : Country.None;
        }

        #endregion

        #region 関係タブ - 国家関係リスト

        /// <summary>
        ///     国家関係リストを更新する
        /// </summary>
        private void UpdateRelationList()
        {
            Country selected = GetSelectedRelationCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(selected);

            relationListView.BeginUpdate();
            relationListView.Items.Clear();
            foreach (Country target in Countries.Tags)
            {
                relationListView.Items.Add(CreateRelationListItem(selected, target, settings));
            }
            relationListView.EndUpdate();
        }

        /// <summary>
        ///     国家関係リストをクリアする
        /// </summary>
        private void ClearRelationList()
        {
            relationListView.BeginUpdate();
            relationListView.Items.Clear();
            relationListView.EndUpdate();
        }

        /// <summary>
        ///     国家関係リストを有効化する
        /// </summary>
        private void EnableRelationList()
        {
            relationListView.Enabled = true;
        }

        /// <summary>
        ///     国家関係リストビューの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ編集項目を無効化する
            if (relationListView.SelectedIndices.Count == 0)
            {
                // 編集項目を無効化する
                DisableRelationItems();
                DisableGuaranteedItems();
                DisableNonAggressionItems();
                DisablePeaceItems();
                DisableIntelligenceItems();

                // 編集項目をクリアする
                ClearRelationItems();
                ClearGuaranteedItems();
                ClearNonAggressionItems();
                ClearPeaceItems();
                ClearIntelligenceItems();
                return;
            }

            Country selected = GetSelectedRelationCountry();
            Country target = GetTargetRelationCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);
            Treaty nonAggression = Scenarios.GetNonAggression(selected, target);
            Treaty peace = Scenarios.GetPeace(selected, target);
            SpySettings spy = Scenarios.GetCountryIntelligence(selected, target);

            // 編集項目を更新する
            UpdateRelationItems(relation, target, settings);
            UpdateGuaranteedItems(relation);
            UpdateNonAggressionItems(nonAggression);
            UpdatePeaceItems(peace);
            UpdateIntelligenceItems(spy);

            // 編集項目を有効化する
            EnableRelationItems();
            EnableGuaranteedItems();
            EnableNonAggressionItems();
            EnablePeaceItems();
            EnableIntelligenceItems();
        }

        /// <summary>
        ///     貿易リストビューの項目文字列を設定する
        /// </summary>
        /// <param name="index">項目のインデックス</param>
        /// <param name="no">項目番号</param>
        /// <param name="s">文字列</param>
        internal void SetRelationListItemText(int index, int no, string s)
        {
            relationListView.Items[index].SubItems[no].Text = s;
        }

        /// <summary>
        ///     貿易リストビューの選択項目の文字列を設定する
        /// </summary>
        /// <param name="no">項目番号</param>
        /// <param name="s">文字列</param>
        internal void SetRelationListItemText(int no, string s)
        {
            relationListView.SelectedItems[0].SubItems[no].Text = s;
        }

        /// <summary>
        ///     国家関係リストビューの項目を作成する
        /// </summary>
        /// <param name="selected">選択国</param>
        /// <param name="target">対象国</param>
        /// <param name="settings">国家設定</param>
        /// <returns>国家関係リストビューの項目</returns>
        private ListViewItem CreateRelationListItem(Country selected, Country target, CountrySettings settings)
        {
            ListViewItem item = new ListViewItem(Countries.GetTagName(target));
            Relation relation = Scenarios.GetCountryRelation(selected, target);
            Treaty nonAggression = Scenarios.GetNonAggression(selected, target);
            Treaty peace = Scenarios.GetPeace(selected, target);
            SpySettings spy = Scenarios.GetCountryIntelligence(selected, target);

            item.SubItems.Add(
                ObjectHelper.ToString(_controller.GetItemValue(ScenarioEditorItemId.DiplomacyRelationValue, relation)));
            item.SubItems.Add(
                (Country) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyMaster, settings) == target
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (Country) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyMilitaryControl, settings) == target
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyMilitaryAccess, relation)
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyGuaranteed, relation) ? Resources.Yes : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyNonAggression, nonAggression)
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyPeace, peace) ? Resources.Yes : "");
            item.SubItems.Add(
                ObjectHelper.ToString(_controller.GetItemValue(ScenarioEditorItemId.IntelligenceSpies, spy)));

            return item;
        }

        /// <summary>
        ///     国家関係の対象国を取得する
        /// </summary>
        /// <returns>国家関係の対象国</returns>
        private Country GetTargetRelationCountry()
        {
            return relationListView.SelectedItems.Count > 0
                ? Countries.Tags[relationListView.SelectedIndices[0]]
                : Country.None;
        }

        #endregion

        #region 関係タブ - 国家関係

        /// <summary>
        ///     国家関係の編集項目を初期化する
        /// </summary>
        private void InitRelationItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyRelationValue, relationValueTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyMaster, masterCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyMilitaryControl, controlCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyMilitaryAccess, accessCheckBox);

            relationValueTextBox.Tag = ScenarioEditorItemId.DiplomacyRelationValue;
            masterCheckBox.Tag = ScenarioEditorItemId.DiplomacyMaster;
            controlCheckBox.Tag = ScenarioEditorItemId.DiplomacyMilitaryControl;
            accessCheckBox.Tag = ScenarioEditorItemId.DiplomacyMilitaryAccess;
        }

        /// <summary>
        ///     国家関係の編集項目を更新する
        /// </summary>
        /// <param name="relation">国家関係</param>
        /// <param name="target">相手国</param>
        /// <param name="settings">国家設定</param>
        private void UpdateRelationItems(Relation relation, Country target, CountrySettings settings)
        {
            _controller.UpdateItemValue(relationValueTextBox, relation);
            _controller.UpdateItemValue(masterCheckBox, target, settings);
            _controller.UpdateItemValue(controlCheckBox, target, settings);
            _controller.UpdateItemValue(accessCheckBox, relation);

            _controller.UpdateItemColor(relationValueTextBox, relation);
            _controller.UpdateItemColor(masterCheckBox, settings);
            _controller.UpdateItemColor(controlCheckBox, settings);
            _controller.UpdateItemColor(accessCheckBox, relation);
        }

        /// <summary>
        ///     国家関係の編集項目をクリアする
        /// </summary>
        private void ClearRelationItems()
        {
            relationValueTextBox.Text = "";
            masterCheckBox.Checked = false;
            controlCheckBox.Checked = false;
            accessCheckBox.Checked = false;
        }

        /// <summary>
        ///     国家関係の編集項目を有効化する
        /// </summary>
        private void EnableRelationItems()
        {
            relationGroupBox.Enabled = true;
        }

        /// <summary>
        ///     国家関係の編集項目を無効化する
        /// </summary>
        private void DisableRelationItems()
        {
            relationGroupBox.Enabled = false;
        }

        #endregion

        #region 関係タブ - 独立保障

        /// <summary>
        ///     独立保障の編集項目を初期化する
        /// </summary>
        private void InitGuaranteedItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteed, guaranteedCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteedEndYear, guaranteedYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteedEndMonth, guaranteedMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteedEndDay, guaranteedDayTextBox);

            guaranteedCheckBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteed;
            guaranteedYearTextBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteedEndYear;
            guaranteedMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteedEndMonth;
            guaranteedDayTextBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteedEndDay;
        }

        /// <summary>
        ///     独立保障の編集項目を更新する
        /// </summary>
        /// <param name="relation">国家関係</param>
        private void UpdateGuaranteedItems(Relation relation)
        {
            _controller.UpdateItemValue(guaranteedCheckBox, relation);
            _controller.UpdateItemValue(guaranteedYearTextBox, relation);
            _controller.UpdateItemValue(guaranteedMonthTextBox, relation);
            _controller.UpdateItemValue(guaranteedDayTextBox, relation);

            _controller.UpdateItemColor(guaranteedCheckBox, relation);
            _controller.UpdateItemColor(guaranteedYearTextBox, relation);
            _controller.UpdateItemColor(guaranteedMonthTextBox, relation);
            _controller.UpdateItemColor(guaranteedDayTextBox, relation);

            bool flag = (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyGuaranteed, relation);
            guaranteedYearTextBox.Enabled = flag;
            guaranteedMonthTextBox.Enabled = flag;
            guaranteedDayTextBox.Enabled = flag;
        }

        /// <summary>
        ///     独立保障の編集項目をクリアする
        /// </summary>
        private void ClearGuaranteedItems()
        {
            guaranteedCheckBox.Checked = false;
            guaranteedYearTextBox.Text = "";
            guaranteedMonthTextBox.Text = "";
            guaranteedDayTextBox.Text = "";

            guaranteedYearTextBox.Enabled = false;
            guaranteedMonthTextBox.Enabled = false;
            guaranteedDayTextBox.Enabled = false;
        }

        /// <summary>
        ///     独立保障の編集項目を有効化する
        /// </summary>
        private void EnableGuaranteedItems()
        {
            guaranteedGroupBox.Enabled = true;
        }

        /// <summary>
        ///     独立保障の編集項目を無効化する
        /// </summary>
        private void DisableGuaranteedItems()
        {
            guaranteedGroupBox.Enabled = false;
        }

        #endregion

        #region 関係タブ - 不可侵条約

        /// <summary>
        ///     不可侵条約の編集項目を初期化する
        /// </summary>
        private void InitNonAggressionItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggression, nonAggressionCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionStartYear, nonAggressionStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionStartMonth, nonAggressionStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionStartDay, nonAggressionStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionEndYear, nonAggressionEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionEndMonth, nonAggressionEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionEndDay, nonAggressionEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionType, nonAggressionTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionId, nonAggressionIdTextBox);

            nonAggressionCheckBox.Tag = ScenarioEditorItemId.DiplomacyNonAggression;
            nonAggressionStartYearTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionStartYear;
            nonAggressionStartMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionStartMonth;
            nonAggressionStartDayTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionStartDay;
            nonAggressionEndYearTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionEndYear;
            nonAggressionEndMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionEndMonth;
            nonAggressionEndDayTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionEndDay;
            nonAggressionTypeTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionType;
            nonAggressionIdTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionId;
        }

        /// <summary>
        ///     不可侵条約の編集項目を更新する
        /// </summary>
        /// <param name="treaty">協定</param>
        private void UpdateNonAggressionItems(Treaty treaty)
        {
            _controller.UpdateItemValue(nonAggressionCheckBox, treaty);
            _controller.UpdateItemValue(nonAggressionStartYearTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionStartMonthTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionStartDayTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionEndYearTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionEndMonthTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionEndDayTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionTypeTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionIdTextBox, treaty);

            _controller.UpdateItemColor(nonAggressionCheckBox, treaty);
            _controller.UpdateItemColor(nonAggressionStartYearTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionStartMonthTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionStartDayTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionEndYearTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionEndMonthTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionEndDayTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionTypeTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionIdTextBox, treaty);

            bool flag = (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyNonAggression, treaty);
            nonAggressionStartLabel.Enabled = flag;
            nonAggressionStartYearTextBox.Enabled = flag;
            nonAggressionStartMonthTextBox.Enabled = flag;
            nonAggressionStartDayTextBox.Enabled = flag;
            nonAggressionEndLabel.Enabled = flag;
            nonAggressionEndYearTextBox.Enabled = flag;
            nonAggressionEndMonthTextBox.Enabled = flag;
            nonAggressionEndDayTextBox.Enabled = flag;
            nonAggressionIdLabel.Enabled = flag;
            nonAggressionTypeTextBox.Enabled = flag;
            nonAggressionIdTextBox.Enabled = flag;
        }

        /// <summary>
        ///     不可侵条約の編集項目をクリアする
        /// </summary>
        private void ClearNonAggressionItems()
        {
            nonAggressionCheckBox.Checked = false;
            nonAggressionStartYearTextBox.Text = "";
            nonAggressionStartMonthTextBox.Text = "";
            nonAggressionStartDayTextBox.Text = "";
            nonAggressionEndYearTextBox.Text = "";
            nonAggressionEndMonthTextBox.Text = "";
            nonAggressionEndDayTextBox.Text = "";
            nonAggressionTypeTextBox.Text = "";
            nonAggressionIdTextBox.Text = "";

            nonAggressionStartLabel.Enabled = false;
            nonAggressionStartYearTextBox.Enabled = false;
            nonAggressionStartMonthTextBox.Enabled = false;
            nonAggressionStartDayTextBox.Enabled = false;
            nonAggressionEndLabel.Enabled = false;
            nonAggressionEndYearTextBox.Enabled = false;
            nonAggressionEndMonthTextBox.Enabled = false;
            nonAggressionEndDayTextBox.Enabled = false;
            nonAggressionIdLabel.Enabled = false;
            nonAggressionTypeTextBox.Enabled = false;
            nonAggressionIdTextBox.Enabled = false;
        }

        /// <summary>
        ///     不可侵条約の編集項目を有効化する
        /// </summary>
        private void EnableNonAggressionItems()
        {
            nonAggressionGroupBox.Enabled = true;
        }

        /// <summary>
        ///     不可侵条約の編集項目を無効化する
        /// </summary>
        private void DisableNonAggressionItems()
        {
            nonAggressionGroupBox.Enabled = false;
        }

        #endregion

        #region 関係タブ - 講和条約

        /// <summary>
        ///     講和条約の編集項目を初期化する
        /// </summary>
        private void InitPeaceItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeace, peaceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceStartYear, peaceStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceStartMonth, peaceStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceStartDay, peaceStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceEndYear, peaceEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceEndMonth, peaceEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceEndDay, peaceEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceType, peaceTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceId, peaceIdTextBox);

            peaceCheckBox.Tag = ScenarioEditorItemId.DiplomacyPeace;
            peaceStartYearTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceStartYear;
            peaceStartMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceStartMonth;
            peaceStartDayTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceStartDay;
            peaceEndYearTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceEndYear;
            peaceEndMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceEndMonth;
            peaceEndDayTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceEndDay;
            peaceTypeTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceType;
            peaceIdTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceId;
        }

        /// <summary>
        ///     講和条約の編集項目を更新する
        /// </summary>
        /// <param name="treaty">協定</param>
        private void UpdatePeaceItems(Treaty treaty)
        {
            _controller.UpdateItemValue(peaceCheckBox, treaty);
            _controller.UpdateItemValue(peaceStartYearTextBox, treaty);
            _controller.UpdateItemValue(peaceStartMonthTextBox, treaty);
            _controller.UpdateItemValue(peaceStartDayTextBox, treaty);
            _controller.UpdateItemValue(peaceEndYearTextBox, treaty);
            _controller.UpdateItemValue(peaceEndMonthTextBox, treaty);
            _controller.UpdateItemValue(peaceEndDayTextBox, treaty);
            _controller.UpdateItemValue(peaceTypeTextBox, treaty);
            _controller.UpdateItemValue(peaceIdTextBox, treaty);

            _controller.UpdateItemColor(peaceCheckBox, treaty);
            _controller.UpdateItemColor(peaceStartYearTextBox, treaty);
            _controller.UpdateItemColor(peaceStartMonthTextBox, treaty);
            _controller.UpdateItemColor(peaceStartDayTextBox, treaty);
            _controller.UpdateItemColor(peaceEndYearTextBox, treaty);
            _controller.UpdateItemColor(peaceEndMonthTextBox, treaty);
            _controller.UpdateItemColor(peaceEndDayTextBox, treaty);
            _controller.UpdateItemColor(peaceTypeTextBox, treaty);
            _controller.UpdateItemColor(peaceIdTextBox, treaty);

            bool flag = (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyPeace, treaty);
            peaceStartLabel.Enabled = flag;
            peaceStartYearTextBox.Enabled = flag;
            peaceStartMonthTextBox.Enabled = flag;
            peaceStartDayTextBox.Enabled = flag;
            peaceEndLabel.Enabled = flag;
            peaceEndYearTextBox.Enabled = flag;
            peaceEndMonthTextBox.Enabled = flag;
            peaceEndDayTextBox.Enabled = flag;
            peaceIdLabel.Enabled = flag;
            peaceTypeTextBox.Enabled = flag;
            peaceIdTextBox.Enabled = flag;
        }

        /// <summary>
        ///     講和条約の編集項目をクリアする
        /// </summary>
        private void ClearPeaceItems()
        {
            peaceCheckBox.Checked = false;
            peaceStartYearTextBox.Text = "";
            peaceStartMonthTextBox.Text = "";
            peaceStartDayTextBox.Text = "";
            peaceEndYearTextBox.Text = "";
            peaceEndMonthTextBox.Text = "";
            peaceEndDayTextBox.Text = "";
            peaceTypeTextBox.Text = "";
            peaceIdTextBox.Text = "";

            peaceStartLabel.Enabled = false;
            peaceStartYearTextBox.Enabled = false;
            peaceStartMonthTextBox.Enabled = false;
            peaceStartDayTextBox.Enabled = false;
            peaceEndLabel.Enabled = false;
            peaceEndYearTextBox.Enabled = false;
            peaceEndMonthTextBox.Enabled = false;
            peaceEndDayTextBox.Enabled = false;
            peaceIdLabel.Enabled = false;
            peaceTypeTextBox.Enabled = false;
            peaceIdTextBox.Enabled = false;
        }

        /// <summary>
        ///     講和条約の編集項目を有効化する
        /// </summary>
        private void EnablePeaceItems()
        {
            peaceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     講和条約の編集項目を無効化する
        /// </summary>
        private void DisablePeaceItems()
        {
            peaceGroupBox.Enabled = false;
        }

        #endregion

        #region 関係タブ - 諜報

        /// <summary>
        ///     諜報情報の編集項目を更新する
        /// </summary>
        private void InitIntelligenceItems()
        {
            _itemControls.Add(ScenarioEditorItemId.IntelligenceSpies, spyNumNumericUpDown);

            spyNumNumericUpDown.Tag = ScenarioEditorItemId.IntelligenceSpies;
        }

        /// <summary>
        ///     諜報情報の編集項目を更新する
        /// </summary>
        /// <param name="spy">諜報設定</param>
        private void UpdateIntelligenceItems(SpySettings spy)
        {
            _controller.UpdateItemValue(spyNumNumericUpDown, spy);

            _controller.UpdateItemColor(spyNumNumericUpDown, spy);
        }

        /// <summary>
        ///     諜報情報の編集項目をクリアする
        /// </summary>
        private void ClearIntelligenceItems()
        {
            spyNumNumericUpDown.Value = 0;
        }

        /// <summary>
        ///     諜報情報の編集項目を有効化する
        /// </summary>
        private void EnableIntelligenceItems()
        {
            intelligenceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     諜報情報の編集項目を無効化する
        /// </summary>
        private void DisableIntelligenceItems()
        {
            intelligenceGroupBox.Enabled = false;
        }

        #endregion

        #region 関係タブ - 編集項目

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);

            // 文字列を数値に変換できなければ値を戻す
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, relation);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (relation == null)
            {
                relation = new Relation { Country = target };
                settings.Relations.Add(relation);
                Scenarios.SetCountryRelation(selected, relation);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, relation);

            // 値を更新する
            _controller.SetItemValue(itemId, val, relation);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, relation, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, relation);
        }

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);

            // 文字列を数値に変換できなければ値を戻す
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, relation);
            if ((prev == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (relation == null)
            {
                relation = new Relation { Country = target };
                settings.Relations.Add(relation);
                Scenarios.SetCountryRelation(selected, relation);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, relation);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, relation);

            // 値を更新する
            _controller.SetItemValue(itemId, val, relation);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, relation, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, relation);
        }

        /// <summary>
        ///     チェックボックスのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);

            // 初期値から変更されていなければ何もしない
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, relation);
            if ((prev == null) && !val)
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (relation == null)
            {
                relation = new Relation { Country = target };
                settings.Relations.Add(relation);
                Scenarios.SetCountryRelation(selected, relation);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, relation);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, relation);

            // 値を更新する
            _controller.SetItemValue(itemId, val, relation);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, relation, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, relation, settings);
        }

        /// <summary>
        ///     チェックボックスのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationCountryItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);

            // 初期値から変更されていなければ何もしない
            Country val = control.Checked ? target : Country.None;
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev == null) && (val == Country.None))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (Country) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, settings);

            // 値を更新する
            _controller.SetItemValue(itemId, val, settings);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationNonAggressionItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetNonAggression(selected, target);

            // 文字列を数値に変換できなければ値を戻す
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            _controller.SetItemValue(itemId, val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     チェックボックスのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationNonAggressionItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetNonAggression(selected, target);

            // 初期値から変更されていなければ何もしない
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && !val)
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            if (val)
            {
                treaty = new Treaty
                {
                    Type = TreatyType.NonAggression,
                    Country1 = selected,
                    Country2 = target,
                    StartDate = new GameDate(),
                    EndDate = new GameDate(),
                    Id = Scenarios.GetNewTypeId(Scenarios.DefaultTreatyType, 1)
                };
                Scenarios.Data.GlobalData.NonAggressions.Add(treaty);
                Scenarios.SetNonAggression(treaty);
            }
            else
            {
                Scenarios.Data.GlobalData.NonAggressions.Remove(treaty);
                Scenarios.RemoveNonAggression(treaty);
            }

            _controller.OutputItemValueChangedLog(itemId, val, !val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, val ? treaty : null);
        }

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationPeaceItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetPeace(selected, target);

            // 文字列を数値に変換できなければ値を戻す
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            _controller.SetItemValue(itemId, val, treaty);

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     チェックボックスのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationPeaceItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetPeace(selected, target);

            // 初期値から変更されていなければ何もしない
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && !val)
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            if (val)
            {
                treaty = new Treaty
                {
                    Type = TreatyType.Peace,
                    Country1 = selected,
                    Country2 = target,
                    StartDate = new GameDate(),
                    EndDate = new GameDate(),
                    Id = Scenarios.GetNewTypeId(Scenarios.DefaultTreatyType, 1)
                };
                Scenarios.Data.GlobalData.Peaces.Add(treaty);
                Scenarios.SetPeace(treaty);
            }
            else
            {
                Scenarios.Data.GlobalData.Peaces.Remove(treaty);
                Scenarios.RemovePeace(treaty);
            }

            _controller.OutputItemValueChangedLog(itemId, val, !val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, val ? treaty : null);
        }

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationIntelligenceItemNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            NumericUpDown control = sender as NumericUpDown;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            SpySettings spy = Scenarios.GetCountryIntelligence(selected, target);

            // 初期値から変更されていなければ何もしない
            int val = (int) control.Value;
            object prev = _controller.GetItemValue(itemId, spy);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, spy);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (spy == null)
            {
                spy = new SpySettings { Country = target };
                Scenarios.SetCountryIntelligence(selected, spy);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, spy);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, spy);

            // 値を更新する
            _controller.SetItemValue(itemId, val, spy);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, spy, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, spy);
        }

        #endregion

        #endregion

        #region 貿易タブ

        #region 貿易タブ - 共通

        /// <summary>
        ///     貿易タブの編集項目を初期化する
        /// </summary>
        private void InitTradeTab()
        {
            InitTradeInfoItems();
            InitTradeDealsItems();
        }

        /// <summary>
        ///     貿易タブの編集項目を更新する
        /// </summary>
        private void UpdateTradeTab()
        {
            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Trade])
            {
                return;
            }

            // 貿易国コンボボックスを更新する
            UpdateCountryComboBox(tradeCountryComboBox1, false);
            UpdateCountryComboBox(tradeCountryComboBox2, false);

            // 貿易リストを更新する
            UpdateTradeList();

            // 貿易リストを有効化する
            EnableTradeList();

            // 新規ボタンを有効化する
            EnableTradeNewButton();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Trade] = true;
        }

        /// <summary>
        ///     貿易タブのフォーム読み込み時の処理
        /// </summary>
        private void OnTradeTabPageFormLoad()
        {
            // 貿易タブを初期化する
            InitTradeTab();
        }

        /// <summary>
        ///     貿易タブのファイル読み込み時の処理
        /// </summary>
        private void OnTradeTabPageFileLoad()
        {
            // 貿易タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Trade)
            {
                return;
            }

            // 初回遷移時には表示を更新する
            UpdateTradeTab();
        }

        /// <summary>
        ///     貿易タブ選択時の処理
        /// </summary>
        private void OnTradeTabPageSelected()
        {
            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 初回遷移時には表示を更新する
            UpdateTradeTab();
        }

        #endregion

        #region 貿易タブ - 貿易リスト

        /// <summary>
        ///     貿易リストの表示を更新する
        /// </summary>
        private void UpdateTradeList()
        {
            List<Treaty> trades = Scenarios.Data.GlobalData.Trades;
            tradeListView.BeginUpdate();
            tradeListView.Items.Clear();
            foreach (Treaty treaty in trades)
            {
                tradeListView.Items.Add(CreateTradeListViewItem(treaty));
            }
            tradeListView.EndUpdate();

            // 編集項目を無効化する
            DisableTradeInfoItems();
            DisableTradeDealsItems();
            DisableTradeButtons();

            // 編集項目をクリアする
            ClearTradeInfoItems();
            ClearTradeDealsItems();
        }

        /// <summary>
        ///     貿易リストを有効化する
        /// </summary>
        private void EnableTradeList()
        {
            tradeListView.Enabled = true;
        }

        /// <summary>
        ///     新規ボタンを有効化する
        /// </summary>
        private void EnableTradeNewButton()
        {
            tradeNewButton.Enabled = true;
        }

        /// <summary>
        ///     削除/上へ/下へボタンを有効化する
        /// </summary>
        private void EnableTradeButtons()
        {
            int index = tradeListView.SelectedIndices[0];
            int count = tradeListView.Items.Count;
            tradeUpButton.Enabled = index > 0;
            tradeDownButton.Enabled = index < count - 1;
            tradeRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     削除/上へ/下へボタンを無効化する
        /// </summary>
        private void DisableTradeButtons()
        {
            tradeUpButton.Enabled = false;
            tradeDownButton.Enabled = false;
            tradeRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     貿易リストビューの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ編集項目を無効化する
            if (tradeListView.SelectedIndices.Count == 0)
            {
                // 編集項目を無効化する
                DisableTradeInfoItems();
                DisableTradeDealsItems();
                DisableTradeButtons();

                // 編集項目をクリアする
                ClearTradeInfoItems();
                ClearTradeDealsItems();
                return;
            }

            Treaty treaty = GetSelectedTrade();

            // 編集項目を更新する
            UpdateTradeInfoItems(treaty);
            UpdateTradeDealsItems(treaty);

            // 編集項目を有効化する
            EnableTradeInfoItems();
            EnableTradeDealsItems();
            EnableTradeButtons();
        }

        /// <summary>
        ///     貿易の上へボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeUpButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            // 貿易リストビューの項目を移動する
            int index = tradeListView.SelectedIndices[0];
            ListViewItem item = tradeListView.Items[index];
            tradeListView.Items.RemoveAt(index);
            tradeListView.Items.Insert(index - 1, item);
            tradeListView.Items[index - 1].Focused = true;
            tradeListView.Items[index - 1].Selected = true;
            tradeListView.EnsureVisible(index - 1);

            // 貿易リストの項目を移動する
            Treaty trade = trades[index];
            trades.RemoveAt(index);
            trades.Insert(index - 1, trade);

            // 編集済みフラグを設定する
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     貿易の下へボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeDownButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            // 貿易リストビューの項目を移動する
            int index = tradeListView.SelectedIndices[0];
            ListViewItem item = tradeListView.Items[index];
            tradeListView.Items.RemoveAt(index);
            tradeListView.Items.Insert(index + 1, item);
            tradeListView.Items[index + 1].Focused = true;
            tradeListView.Items[index + 1].Selected = true;
            tradeListView.EnsureVisible(index + 1);

            // 貿易リストの項目を移動する
            Treaty trade = trades[index];
            trades.RemoveAt(index);
            trades.Insert(index + 1, trade);

            // 編集済みフラグを設定する
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     貿易の新規ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeNewButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            // 貿易リストに項目を追加する
            Treaty trade = new Treaty
            {
                StartDate = new GameDate(),
                EndDate = new GameDate(),
                Id = Scenarios.GetNewTypeId(Scenarios.DefaultTreatyType, 1)
            };
            trades.Add(trade);

            // 貿易リストビューに項目を追加する
            ListViewItem item = new ListViewItem { Tag = trade };
            item.SubItems.Add("");
            item.SubItems.Add("");
            tradeListView.Items.Add(item);

            // 編集済みフラグを設定する
            trade.SetDirty(Treaty.ItemId.StartYear);
            trade.SetDirty(Treaty.ItemId.StartMonth);
            trade.SetDirty(Treaty.ItemId.StartDay);
            trade.SetDirty(Treaty.ItemId.EndYear);
            trade.SetDirty(Treaty.ItemId.EndMonth);
            trade.SetDirty(Treaty.ItemId.EndDay);
            trade.SetDirty(Treaty.ItemId.Type);
            trade.SetDirty(Treaty.ItemId.Id);
            trade.SetDirty(Treaty.ItemId.Cancel);
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();

            // 追加した項目を選択する
            if (tradeListView.SelectedIndices.Count > 0)
            {
                ListViewItem prev = tradeListView.SelectedItems[0];
                prev.Focused = false;
                prev.Selected = false;
            }
            item.Focused = true;
            item.Selected = true;
        }

        /// <summary>
        ///     貿易の削除ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeRemoveButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            int index = tradeListView.SelectedIndices[0];
            Treaty trade = trades[index];

            // typeとidの組を削除する
            Scenarios.RemoveTypeId(trade.Id);

            // 貿易リストから項目を削除する
            trades.RemoveAt(index);

            // 貿易リストビューから項目を削除する
            tradeListView.Items.RemoveAt(index);

            // 編集済みフラグを設定する
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();

            // 削除した項目の次を選択する
            if (index == trades.Count)
            {
                index--;
            }
            if (index >= 0)
            {
                tradeListView.Items[index].Focused = true;
                tradeListView.Items[index].Selected = true;
            }
        }

        /// <summary>
        ///     貿易リストビューの選択項目の文字列を設定する
        /// </summary>
        /// <param name="no">項目番号</param>
        /// <param name="s">文字列</param>
        internal void SetTradeListItemText(int no, string s)
        {
            tradeListView.SelectedItems[0].SubItems[no].Text = s;
        }

        /// <summary>
        ///     貿易リストの項目を作成する
        /// </summary>
        /// <param name="treaty">貿易情報</param>
        /// <returns>貿易リストの項目</returns>
        private static ListViewItem CreateTradeListViewItem(Treaty treaty)
        {
            ListViewItem item = new ListViewItem
            {
                Text = Countries.GetName(treaty.Country1),
                Tag = treaty
            };
            item.SubItems.Add(Countries.GetName(treaty.Country2));
            item.SubItems.Add(treaty.GetTradeString());

            return item;
        }

        /// <summary>
        ///     選択中の貿易情報を取得する
        /// </summary>
        /// <returns>選択中の貿易情報</returns>
        private Treaty GetSelectedTrade()
        {
            return tradeListView.SelectedItems.Count > 0 ? tradeListView.SelectedItems[0].Tag as Treaty : null;
        }

        #endregion

        #region 貿易タブ - 貿易情報

        /// <summary>
        ///     貿易情報の編集項目を初期化する
        /// </summary>
        private void InitTradeInfoItems()
        {
            _itemControls.Add(ScenarioEditorItemId.TradeStartYear, tradeStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeStartMonth, tradeStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeStartDay, tradeStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeEndYear, tradeEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeEndMonth, tradeEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeEndDay, tradeEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeType, tradeTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeId, tradeIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeCancel, tradeCancelCheckBox);

            tradeStartYearTextBox.Tag = ScenarioEditorItemId.TradeStartYear;
            tradeStartMonthTextBox.Tag = ScenarioEditorItemId.TradeStartMonth;
            tradeStartDayTextBox.Tag = ScenarioEditorItemId.TradeStartDay;
            tradeEndYearTextBox.Tag = ScenarioEditorItemId.TradeEndYear;
            tradeEndMonthTextBox.Tag = ScenarioEditorItemId.TradeEndMonth;
            tradeEndDayTextBox.Tag = ScenarioEditorItemId.TradeEndDay;
            tradeTypeTextBox.Tag = ScenarioEditorItemId.TradeType;
            tradeIdTextBox.Tag = ScenarioEditorItemId.TradeId;
            tradeCancelCheckBox.Tag = ScenarioEditorItemId.TradeCancel;
        }

        /// <summary>
        ///     貿易情報の編集項目を更新する
        /// </summary>
        /// <param name="treaty">協定</param>
        private void UpdateTradeInfoItems(Treaty treaty)
        {
            // 編集項目の表示を更新する
            _controller.UpdateItemValue(tradeStartYearTextBox, treaty);
            _controller.UpdateItemValue(tradeStartMonthTextBox, treaty);
            _controller.UpdateItemValue(tradeStartDayTextBox, treaty);
            _controller.UpdateItemValue(tradeEndYearTextBox, treaty);
            _controller.UpdateItemValue(tradeEndMonthTextBox, treaty);
            _controller.UpdateItemValue(tradeEndDayTextBox, treaty);
            _controller.UpdateItemValue(tradeTypeTextBox, treaty);
            _controller.UpdateItemValue(tradeIdTextBox, treaty);
            _controller.UpdateItemValue(tradeCancelCheckBox, treaty);

            // 編集項目の色を更新する
            _controller.UpdateItemColor(tradeStartYearTextBox, treaty);
            _controller.UpdateItemColor(tradeStartMonthTextBox, treaty);
            _controller.UpdateItemColor(tradeStartDayTextBox, treaty);
            _controller.UpdateItemColor(tradeEndYearTextBox, treaty);
            _controller.UpdateItemColor(tradeEndMonthTextBox, treaty);
            _controller.UpdateItemColor(tradeEndDayTextBox, treaty);
            _controller.UpdateItemColor(tradeTypeTextBox, treaty);
            _controller.UpdateItemColor(tradeIdTextBox, treaty);
            _controller.UpdateItemColor(tradeCancelCheckBox, treaty);
        }

        /// <summary>
        ///     貿易情報の編集項目の表示をクリアする
        /// </summary>
        private void ClearTradeInfoItems()
        {
            tradeStartYearTextBox.Text = "";
            tradeStartMonthTextBox.Text = "";
            tradeStartDayTextBox.Text = "";
            tradeEndYearTextBox.Text = "";
            tradeEndMonthTextBox.Text = "";
            tradeEndDayTextBox.Text = "";
            tradeTypeTextBox.Text = "";
            tradeIdTextBox.Text = "";
            tradeCancelCheckBox.Checked = false;
        }

        /// <summary>
        ///     貿易情報の編集項目を有効化する
        /// </summary>
        private void EnableTradeInfoItems()
        {
            tradeInfoGroupBox.Enabled = true;
        }

        /// <summary>
        ///     貿易情報の編集項目を無効化する
        /// </summary>
        private void DisableTradeInfoItems()
        {
            tradeInfoGroupBox.Enabled = false;
        }

        #endregion

        #region 貿易タブ - 貿易内容

        /// <summary>
        ///     貿易内容の編集項目を初期化する
        /// </summary>
        private void InitTradeDealsItems()
        {
            _itemControls.Add(ScenarioEditorItemId.TradeCountry1, tradeCountryComboBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeCountry2, tradeCountryComboBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeEnergy1, tradeEnergyTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeEnergy2, tradeEnergyTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeMetal1, tradeMetalTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeMetal2, tradeMetalTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeRareMaterials1, tradeRareMaterialsTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeRareMaterials2, tradeRareMaterialsTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeOil1, tradeOilTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeOil2, tradeOilTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeSupplies1, tradeSuppliesTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeSupplies2, tradeSuppliesTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeMoney1, tradeMoneyTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeMoney2, tradeMoneyTextBox2);

            tradeCountryComboBox1.Tag = ScenarioEditorItemId.TradeCountry1;
            tradeCountryComboBox2.Tag = ScenarioEditorItemId.TradeCountry2;
            tradeEnergyTextBox1.Tag = ScenarioEditorItemId.TradeEnergy1;
            tradeEnergyTextBox2.Tag = ScenarioEditorItemId.TradeEnergy2;
            tradeMetalTextBox1.Tag = ScenarioEditorItemId.TradeMetal1;
            tradeMetalTextBox2.Tag = ScenarioEditorItemId.TradeMetal2;
            tradeRareMaterialsTextBox1.Tag = ScenarioEditorItemId.TradeRareMaterials1;
            tradeRareMaterialsTextBox2.Tag = ScenarioEditorItemId.TradeRareMaterials2;
            tradeOilTextBox1.Tag = ScenarioEditorItemId.TradeOil1;
            tradeOilTextBox2.Tag = ScenarioEditorItemId.TradeOil2;
            tradeSuppliesTextBox1.Tag = ScenarioEditorItemId.TradeSupplies1;
            tradeSuppliesTextBox2.Tag = ScenarioEditorItemId.TradeSupplies2;
            tradeMoneyTextBox1.Tag = ScenarioEditorItemId.TradeMoney1;
            tradeMoneyTextBox2.Tag = ScenarioEditorItemId.TradeMoney2;

            // 貿易資源ラベル
            tradeEnergyLabel.Text = Config.GetText(TextId.ResourceEnergy);
            tradeMetalLabel.Text = Config.GetText(TextId.ResourceMetal);
            tradeRareMaterialsLabel.Text = Config.GetText(TextId.ResourceRareMaterials);
            tradeOilLabel.Text = Config.GetText(TextId.ResourceOil);
            tradeSuppliesLabel.Text = Config.GetText(TextId.ResourceSupplies);
            tradeMoneyLabel.Text = Config.GetText(TextId.ResourceMoney);
        }

        /// <summary>
        ///     貿易内容の編集項目を更新する
        /// </summary>
        /// <param name="treaty">協定</param>
        private void UpdateTradeDealsItems(Treaty treaty)
        {
            // 編集項目の表示を更新する
            _controller.UpdateItemValue(tradeCountryComboBox1, treaty);
            _controller.UpdateItemValue(tradeCountryComboBox2, treaty);
            _controller.UpdateItemValue(tradeEnergyTextBox1, treaty);
            _controller.UpdateItemValue(tradeEnergyTextBox2, treaty);
            _controller.UpdateItemValue(tradeMetalTextBox1, treaty);
            _controller.UpdateItemValue(tradeMetalTextBox2, treaty);
            _controller.UpdateItemValue(tradeRareMaterialsTextBox1, treaty);
            _controller.UpdateItemValue(tradeRareMaterialsTextBox2, treaty);
            _controller.UpdateItemValue(tradeOilTextBox1, treaty);
            _controller.UpdateItemValue(tradeOilTextBox2, treaty);
            _controller.UpdateItemValue(tradeSuppliesTextBox1, treaty);
            _controller.UpdateItemValue(tradeSuppliesTextBox2, treaty);
            _controller.UpdateItemValue(tradeMoneyTextBox1, treaty);
            _controller.UpdateItemValue(tradeMoneyTextBox2, treaty);

            // 編集項目の色を更新する
            _controller.UpdateItemColor(tradeEnergyTextBox1, treaty);
            _controller.UpdateItemColor(tradeEnergyTextBox2, treaty);
            _controller.UpdateItemColor(tradeMetalTextBox1, treaty);
            _controller.UpdateItemColor(tradeMetalTextBox2, treaty);
            _controller.UpdateItemColor(tradeRareMaterialsTextBox1, treaty);
            _controller.UpdateItemColor(tradeRareMaterialsTextBox2, treaty);
            _controller.UpdateItemColor(tradeOilTextBox1, treaty);
            _controller.UpdateItemColor(tradeOilTextBox2, treaty);
            _controller.UpdateItemColor(tradeSuppliesTextBox1, treaty);
            _controller.UpdateItemColor(tradeSuppliesTextBox2, treaty);
            _controller.UpdateItemColor(tradeMoneyTextBox1, treaty);
            _controller.UpdateItemColor(tradeMoneyTextBox2, treaty);
        }

        /// <summary>
        ///     貿易内容の編集項目の表示をクリアする
        /// </summary>
        private void ClearTradeDealsItems()
        {
            tradeCountryComboBox1.SelectedIndex = -1;
            tradeCountryComboBox2.SelectedIndex = -1;
            tradeEnergyTextBox1.Text = "";
            tradeEnergyTextBox2.Text = "";
            tradeMetalTextBox1.Text = "";
            tradeMetalTextBox2.Text = "";
            tradeRareMaterialsTextBox1.Text = "";
            tradeRareMaterialsTextBox2.Text = "";
            tradeOilTextBox1.Text = "";
            tradeOilTextBox2.Text = "";
            tradeSuppliesTextBox1.Text = "";
            tradeSuppliesTextBox2.Text = "";
            tradeMoneyTextBox1.Text = "";
            tradeMoneyTextBox2.Text = "";
        }

        /// <summary>
        ///     貿易内容の編集項目を有効化する
        /// </summary>
        private void EnableTradeDealsItems()
        {
            tradeDealsGroupBox.Enabled = true;
        }

        /// <summary>
        ///     貿易内容の編集項目を無効化する
        /// </summary>
        private void DisableTradeDealsItems()
        {
            tradeDealsGroupBox.Enabled = false;
        }

        /// <summary>
        ///     貿易国入れ替えボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeSwapButtonClick(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            // 値を入れ替える
            Country country = treaty.Country1;
            treaty.Country1 = treaty.Country2;
            treaty.Country2 = country;

            treaty.Energy = -treaty.Energy;
            treaty.Metal = -treaty.Metal;
            treaty.RareMaterials = -treaty.RareMaterials;
            treaty.Oil = -treaty.Oil;
            treaty.Supplies = -treaty.Supplies;
            treaty.Money = -treaty.Money;

            // 編集済みフラグを設定する
            treaty.SetDirty(Treaty.ItemId.Country1);
            treaty.SetDirty(Treaty.ItemId.Country2);
            treaty.SetDirty(Treaty.ItemId.Energy);
            treaty.SetDirty(Treaty.ItemId.Metal);
            treaty.SetDirty(Treaty.ItemId.RareMaterials);
            treaty.SetDirty(Treaty.ItemId.Oil);
            treaty.SetDirty(Treaty.ItemId.Supplies);
            treaty.SetDirty(Treaty.ItemId.Money);
            Scenarios.SetDirty();

            // 貿易リストビューの項目を更新する
            ListViewItem item = tradeListView.SelectedItems[0];
            item.Text = Countries.GetName(treaty.Country1);
            item.SubItems[1].Text = Countries.GetName(treaty.Country2);
            item.SubItems[2].Text = treaty.GetTradeString();

            // 編集項目を更新する
            UpdateTradeDealsItems(treaty);
        }

        #endregion

        #region 貿易タブ - 編集項目

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // 文字列を数値に変換できなければ値を戻す
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val, treaty))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            _controller.SetItemValue(itemId, val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // 文字列を数値に変換できなければ値を戻す
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            // 無効な値ならば値を戻す
            if (!_controller.IsItemValueValid(itemId, val, treaty))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            _controller.SetItemValue(itemId, val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     チェックボックスのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // 値に変化がなければ何もしない
            bool val = control.Checked;
            if (val == (bool) _controller.GetItemValue(itemId, treaty))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            _controller.SetItemValue(itemId, val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     コンボボックスの項目描画処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeCountryItemComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // 項目がなければ何もしない
            if (e.Index < 0)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            // 背景を描画する
            e.DrawBackground();

            // 項目の文字列を描画する
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            Country val = (Country) _controller.GetItemValue(itemId, treaty);
            Country sel = (Country) _controller.GetListItemValue(itemId, e.Index);
            Brush brush = (val == sel) && _controller.IsItemDirty(itemId, treaty)
                ? new SolidBrush(Color.Red)
                : new SolidBrush(SystemColors.WindowText);
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // フォーカスを描画する
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     コンボボックスの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeCountryItemComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // 選択項目がなければ何もしない
            if (control.SelectedIndex < 0)
            {
                return;
            }

            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // 値に変化がなければ何もしない
            Country val = Countries.Tags[control.SelectedIndex];
            if (val == (Country) _controller.GetItemValue(itemId, treaty))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, treaty);

            // 値を更新する
            _controller.SetItemValue(itemId, val, treaty);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, treaty);

            // 項目色を変更するため描画更新する
            control.Refresh();

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, treaty);
        }

        #endregion

        #endregion

        #region 国家タブ

        /// <summary>
        ///     国家タブのファイル読み込み時の処理
        /// </summary>
        private void OnCountryTabPageFileLoad()
        {
            // 国家タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Country)
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Country])
            {
                return;
            }

            // 編集項目を初期化する
            _countryPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Country] = true;
        }

        /// <summary>
        ///     国家タブ選択時の処理
        /// </summary>
        private void OnCountryTabPageSelected()
        {
            // タブページを作成する
            if (_countryPage == null)
            {
                _countryPage = new ScenarioEditorCountryPage(_controller, this);
                scenarioTabControl.TabPages[(int) TabPageNo.Country].Controls.Add(_countryPage);
            }

            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Country])
            {
                return;
            }

            // 編集項目を初期化する
            _countryPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Country] = true;
        }

        #endregion

        #region 政府タブ

        /// <summary>
        ///     政府タブのファイル読み込み時の処理
        /// </summary>
        private void OnGovernmentTabPageFileLoad()
        {
            // 政府タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Government)
            {
                return;
            }

            // 閣僚データの読み込み完了まで待機する
            Ministers.WaitLoading();

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Government])
            {
                return;
            }

            // 編集項目を初期化する
            _governmentPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Government] = true;
        }

        /// <summary>
        ///     政府タブ選択時の処理
        /// </summary>
        private void OnGovernmentTabPageSelected()
        {
            // タブページを作成する
            if (_governmentPage == null)
            {
                _governmentPage = new ScenarioEditorGovernmentPage(_controller, this);
                scenarioTabControl.TabPages[(int) TabPageNo.Government].Controls.Add(_governmentPage);
            }

            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 閣僚データの読み込み完了まで待機する
            Ministers.WaitLoading();

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Government])
            {
                return;
            }

            // 編集項目を初期化する
            _governmentPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Government] = true;
        }

        #endregion

        #region 技術タブ

        #region 技術タブ - 共通

        /// <summary>
        ///     技術タブを初期化する
        /// </summary>
        private static void InitTechTab()
        {
            // 何もしない
        }

        /// <summary>
        ///     技術タブを更新する
        /// </summary>
        private void UpdateTechTab()
        {
            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Technology])
            {
                return;
            }

            // 技術カテゴリリストボックスを初期化する
            InitTechCategoryListBox();

            // 技術カテゴリリストボックスを有効化する
            EnableTechCategoryListBox();

            // 国家リストボックスを更新する
            UpdateCountryListBox(techCountryListBox);

            // 国家リストボックスを有効化する
            EnableTechCountryListBox();

            // 編集項目を無効化する
            DisableTechItems();

            // 編集項目をクリアする
            ClearTechItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Technology] = true;
        }

        /// <summary>
        ///     技術タブのフォーム読み込み時の処理
        /// </summary>
        private static void OnTechTabPageFormLoad()
        {
            // 技術タブを初期化する
            InitTechTab();
        }

        /// <summary>
        ///     技術タブのファイル読み込み時の処理
        /// </summary>
        private void OnTechTabPageFileLoad()
        {
            // 政府タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Technology)
            {
                return;
            }

            // 技術データの読み込み完了まで待機する
            Techs.WaitLoading();

            // 初回遷移時には表示を更新する
            UpdateTechTab();
        }

        /// <summary>
        ///     技術タブ選択時の処理
        /// </summary>
        private void OnTechTabPageSelected()
        {
            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 技術データの読み込み完了まで待機する
            Techs.WaitLoading();

            // 初回遷移時には表示を更新する
            UpdateTechTab();
        }

        #endregion

        #region 技術タブ - 技術カテゴリ

        /// <summary>
        ///     技術カテゴリリストボックスを初期化する
        /// </summary>
        private void InitTechCategoryListBox()
        {
            techCategoryListBox.BeginUpdate();
            techCategoryListBox.Items.Clear();
            foreach (TechGroup grp in Techs.Groups)
            {
                techCategoryListBox.Items.Add(grp);
            }
            techCategoryListBox.SelectedIndex = 0;
            techCategoryListBox.EndUpdate();
        }

        /// <summary>
        ///     技術カテゴリリストボックスを有効化する
        /// </summary>
        private void EnableTechCategoryListBox()
        {
            techCategoryListBox.Enabled = true;
        }

        /// <summary>
        ///     技術カテゴリリストボックスの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechCategoryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択中の国家がなければ編集項目を無効化する
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                // 編集項目を無効化する
                DisableTechItems();

                // 編集項目をクリアする
                ClearTechItems();
                return;
            }

            // 編集項目を更新する
            UpdateTechItems();

            // 編集項目を有効化する
            EnableTechItems();
        }

        /// <summary>
        ///     選択中の技術グループを取得する
        /// </summary>
        /// <returns>選択中の技術グループ</returns>
        private TechGroup GetSelectedTechGroup()
        {
            if (techCategoryListBox.SelectedIndex < 0)
            {
                return null;
            }
            return Techs.Groups[techCategoryListBox.SelectedIndex];
        }

        #endregion

        #region 技術タブ - 国家

        /// <summary>
        ///     国家リストボックスを有効化する
        /// </summary>
        private void EnableTechCountryListBox()
        {
            techCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     国家リストボックスの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ編集項目を無効化する
            if (techCountryListBox.SelectedIndex < 0)
            {
                // 編集項目を無効化する
                DisableTechItems();

                // 編集項目をクリアする
                ClearTechItems();
                return;
            }

            // 編集項目を更新する
            UpdateTechItems();

            // 編集項目を有効化する
            EnableTechItems();
        }

        /// <summary>
        ///     選択中の国家を取得する
        /// </summary>
        /// <returns>選択中の国家</returns>
        private Country GetSelectedTechCountry()
        {
            if (techCountryListBox.SelectedIndex < 0)
            {
                return Country.None;
            }
            return Countries.Tags[techCountryListBox.SelectedIndex];
        }

        #endregion

        #region 技術タブ - 編集項目

        /// <summary>
        ///     技術の編集項目を更新する
        /// </summary>
        private void UpdateTechItems()
        {
            Country country = GetSelectedTechCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            TechGroup grp = GetSelectedTechGroup();

            // 保有技術リスト
            _techs = grp.Items.OfType<TechItem>().ToList();
            UpdateOwnedTechList(settings);

            // 青写真リスト
            UpdateBlueprintList(settings);

            // 発明イベントリスト
            _inventions = Techs.Groups.SelectMany(g => g.Items.OfType<TechEvent>()).ToList();
            UpdateInventionList(settings);

            // 技術ツリーを更新する
            _techTreePanelController.Category = grp.Category;
            _techTreePanelController.Update();
        }

        /// <summary>
        ///     技術の編集項目の表示をクリアする
        /// </summary>
        private void ClearTechItems()
        {
            // 技術ツリーをクリアする
            _techTreePanelController.Clear();

            // 編集項目をクリアする
            ownedTechsListView.Items.Clear();
            blueprintsListView.Items.Clear();
            inventionsListView.Items.Clear();
        }

        /// <summary>
        ///     技術の編集項目を有効化する
        /// </summary>
        private void EnableTechItems()
        {
            ownedTechsLabel.Enabled = true;
            ownedTechsListView.Enabled = true;
            blueprintsLabel.Enabled = true;
            blueprintsListView.Enabled = true;
            inventionsLabel.Enabled = true;
            inventionsListView.Enabled = true;
        }

        /// <summary>
        ///     技術の編集項目を無効化する
        /// </summary>
        private void DisableTechItems()
        {
            ownedTechsLabel.Enabled = false;
            ownedTechsListView.Enabled = false;
            blueprintsLabel.Enabled = false;
            blueprintsListView.Enabled = false;
            inventionsLabel.Enabled = false;
            inventionsListView.Enabled = false;
        }

        /// <summary>
        ///     保有技術リストの表示を更新する
        /// </summary>
        /// <param name="settings">国家設定</param>
        private void UpdateOwnedTechList(CountrySettings settings)
        {
            ownedTechsListView.ItemChecked -= OnOwnedTechsListViewItemChecked;
            ownedTechsListView.BeginUpdate();
            ownedTechsListView.Items.Clear();
            if (settings != null)
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    ownedTechsListView.Items.Add(new ListViewItem
                    {
                        Text = name,
                        Checked = settings.TechApps.Contains(item.Id),
                        ForeColor = settings.IsDirtyOwnedTech(item.Id) ? Color.Red : ownedTechsListView.ForeColor,
                        Tag = item
                    });
                }
            }
            else
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    ownedTechsListView.Items.Add(new ListViewItem { Text = name, Tag = item });
                }
            }
            ownedTechsListView.EndUpdate();
            ownedTechsListView.ItemChecked += OnOwnedTechsListViewItemChecked;
        }

        /// <summary>
        ///     青写真リストの表示を更新する
        /// </summary>
        /// <param name="settings">国家設定</param>
        private void UpdateBlueprintList(CountrySettings settings)
        {
            blueprintsListView.ItemChecked -= OnBlueprintsListViewItemChecked;
            blueprintsListView.BeginUpdate();
            blueprintsListView.Items.Clear();
            if (settings != null)
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    blueprintsListView.Items.Add(new ListViewItem
                    {
                        Text = name,
                        Checked = settings.BluePrints.Contains(item.Id),
                        ForeColor = settings.IsDirtyBlueprint(item.Id) ? Color.Red : ownedTechsListView.ForeColor,
                        Tag = item
                    });
                }
            }
            else
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    blueprintsListView.Items.Add(new ListViewItem { Text = name, Tag = item });
                }
            }
            blueprintsListView.EndUpdate();
            blueprintsListView.ItemChecked += OnBlueprintsListViewItemChecked;
        }

        /// <summary>
        ///     発明イベントリストの表示を更新する
        /// </summary>
        /// <param name="settings">国家設定</param>
        private void UpdateInventionList(CountrySettings settings)
        {
            inventionsListView.ItemChecked -= OnInveitionsListViewItemChecked;
            inventionsListView.BeginUpdate();
            inventionsListView.Items.Clear();
            if (settings != null)
            {
                foreach (TechEvent ev in _inventions)
                {
                    inventionsListView.Items.Add(new ListViewItem
                    {
                        Text = ev.ToString(),
                        Checked = settings.Inventions.Contains(ev.Id),
                        ForeColor = settings.IsDirtyInvention(ev.Id) ? Color.Red : inventionsListView.ForeColor,
                        Tag = ev
                    });
                }
            }
            else
            {
                foreach (TechEvent ev in _inventions)
                {
                    inventionsListView.Items.Add(new ListViewItem { Text = ev.ToString(), Tag = ev });
                }
            }
            inventionsListView.EndUpdate();
            inventionsListView.ItemChecked += OnInveitionsListViewItemChecked;
        }

        /// <summary>
        ///     保有技術リストビューのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOwnedTechsListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // 選択中の国家がなければ何もしない
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechItem item = e.Item.Tag as TechItem;
            if (item == null)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // 値に変化がなければ何もしない
            bool val = e.Item.Checked;
            if ((settings != null) && (val == settings.TechApps.Contains(item.Id)))
            {
                return;
            }

            Log.Info("[Scenario] owned techs: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // 値を更新する
            if (val)
            {
                settings.TechApps.Add(item.Id);
            }
            else
            {
                settings.TechApps.Remove(item.Id);
            }

            // 編集済みフラグを設定する
            settings.SetDirtyOwnedTech(item.Id);
            Scenarios.SetDirty();

            // 文字色を変更する
            e.Item.ForeColor = Color.Red;

            // 技術ツリーの項目ラベルを更新する
            _techTreePanelController.UpdateItem(item);
        }

        /// <summary>
        ///     青写真リストビューのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBlueprintsListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // 選択中の国家がなければ何もしない
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechItem item = e.Item.Tag as TechItem;
            if (item == null)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // 値に変化がなければ何もしない
            bool val = e.Item.Checked;
            if ((settings != null) && (val == settings.BluePrints.Contains(item.Id)))
            {
                return;
            }

            Log.Info("[Scenario] blurprints: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // 値を更新する
            if (val)
            {
                settings.BluePrints.Add(item.Id);
            }
            else
            {
                settings.BluePrints.Remove(item.Id);
            }

            // 編集済みフラグを設定する
            settings.SetDirtyBlueprint(item.Id);
            Scenarios.SetDirty();

            // 文字色を変更する
            e.Item.ForeColor = Color.Red;

            // 技術ツリーの項目ラベルを更新する
            _techTreePanelController.UpdateItem(item);
        }

        /// <summary>
        ///     発明イベントリストビューのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInveitionsListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // 選択中の国家がなければ何もしない
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechEvent ev = e.Item.Tag as TechEvent;
            if (ev == null)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // 値に変化がなければ何もしない
            bool val = e.Item.Checked;
            if ((settings != null) && (val == settings.Inventions.Contains(ev.Id)))
            {
                return;
            }

            Log.Info("[Scenario] inventions: {0}{1} ({2})", val ? '+' : '-', ev.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // 値を更新する
            if (val)
            {
                settings.Inventions.Add(ev.Id);
            }
            else
            {
                settings.Inventions.Remove(ev.Id);
            }

            // 編集済みフラグを設定する
            settings.SetDirtyInvention(ev.Id);
            Scenarios.SetDirty();

            // 文字色を変更する
            e.Item.ForeColor = Color.Red;

            // 技術ツリーの項目ラベルを更新する
            _techTreePanelController.UpdateItem(ev);
        }

        #endregion

        #region 技術タブ - 技術ツリー

        /// <summary>
        ///     項目ラベルマウスクリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechTreeItemMouseClick(object sender, TechTreePanelController.ItemMouseEventArgs e)
        {
            // 選択中の国家がなければ何もしない
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechItem tech = e.Item as TechItem;
            if (tech != null)
            {
                // 左クリックで保有技術の有無を切り替える
                if (e.Button == MouseButtons.Left)
                {
                    ToggleOwnedTech(tech, country);
                }
                // 右クリックで青写真の有無を切り替える
                else if (e.Button == MouseButtons.Right)
                {
                    ToggleBlueprint(tech, country);
                }
                return;
            }

            TechEvent ev = e.Item as TechEvent;
            if (ev != null)
            {
                // 左クリックで保有技術の有無を切り替える
                if (e.Button == MouseButtons.Left)
                {
                    ToggleInvention(ev, country);
                }
            }
        }

        /// <summary>
        ///     保有技術の有無を切り替える
        /// </summary>
        /// <param name="item">対象技術</param>
        /// <param name="country">対象国</param>
        private void ToggleOwnedTech(TechItem item, Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            bool val = (settings == null) || !settings.TechApps.Contains(item.Id);

            Log.Info("[Scenario] owned techs: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // 値を更新する
            if (val)
            {
                settings.TechApps.Add(item.Id);
            }
            else
            {
                settings.TechApps.Remove(item.Id);
            }

            // 編集済みフラグを設定する
            settings.SetDirtyOwnedTech(item.Id);
            Scenarios.SetDirty();

            // 技術ツリーの項目ラベルを更新する
            _techTreePanelController.UpdateItem(item);

            // 保有技術リストビューの表示を更新する
            int index = _techs.IndexOf(item);
            if (index >= 0)
            {
                ListViewItem li = ownedTechsListView.Items[index];
                li.Checked = val;
                li.ForeColor = Color.Red;
                li.EnsureVisible();
            }
        }

        /// <summary>
        ///     保有技術の有無を切り替える
        /// </summary>
        /// <param name="item">対象技術</param>
        /// <param name="country">対象国</param>
        private void ToggleBlueprint(TechItem item, Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            bool val = (settings == null) || !settings.BluePrints.Contains(item.Id);

            Log.Info("[Scenario] blueprints: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            if (val)
            {
                settings.BluePrints.Add(item.Id);
            }
            else
            {
                settings.BluePrints.Remove(item.Id);
            }

            // 編集済みフラグを設定する
            settings.SetDirtyBlueprint(item.Id);
            Scenarios.SetDirty();

            // 技術ツリーの項目ラベルを更新する
            _techTreePanelController.UpdateItem(item);

            // 保有技術リストビューの表示を更新する
            int index = _techs.IndexOf(item);
            if (index >= 0)
            {
                ListViewItem li = blueprintsListView.Items[index];
                li.Checked = val;
                li.ForeColor = Color.Red;
                li.EnsureVisible();
            }
        }

        /// <summary>
        ///     発明イベントの有無を切り替える
        /// </summary>
        /// <param name="item">対象発明イベント</param>
        /// <param name="country">対象国</param>
        private void ToggleInvention(TechEvent item, Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            bool val = (settings == null) || !settings.Inventions.Contains(item.Id);

            Log.Info("[Scenario] inventions: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            if (val)
            {
                settings.Inventions.Add(item.Id);
            }
            else
            {
                settings.Inventions.Remove(item.Id);
            }

            // 編集済みフラグを設定する
            settings.SetDirtyInvention(item.Id);
            Scenarios.SetDirty();

            // 技術ツリーの項目ラベルを更新する
            _techTreePanelController.UpdateItem(item);

            // 保有技術リストビューの表示を更新する
            int index = _inventions.IndexOf(item);
            if (index >= 0)
            {
                ListViewItem li = inventionsListView.Items[index];
                li.Checked = val;
                li.ForeColor = Color.Red;
                li.EnsureVisible();
            }
        }

        /// <summary>
        ///     技術項目の状態を返す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnQueryTechTreeItemStatus(object sender, TechTreePanelController.QueryItemStatusEventArgs e)
        {
            // 選択中の国家がなければ何もしない
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            CountrySettings settings = Scenarios.GetCountrySettings(country);
            if (settings == null)
            {
                return;
            }

            TechItem tech = e.Item as TechItem;
            if (tech != null)
            {
                e.Done = settings.TechApps.Contains(tech.Id);
                e.Blueprint = settings.BluePrints.Contains(tech.Id);
                return;
            }

            TechEvent ev = e.Item as TechEvent;
            if (ev != null)
            {
                e.Done = settings.Inventions.Contains(ev.Id);
            }
        }

        #endregion

        #endregion

        #region プロヴィンスタブ

        #region プロヴィンスタブ - 共通

        /// <summary>
        ///     プロヴィンスタブを初期化する
        /// </summary>
        private void InitProvinceTab()
        {
            InitMapFilter();
            InitProvinceIdTextBox();
            InitProvinceCountryItems();
            InitProvinceInfoItems();
            InitProvinceResourceItems();
            InitProvinceBuildingItems();
        }

        /// <summary>
        ///     プロヴィンスタブの表示を更新する
        /// </summary>
        private void UpdateProvinceTab()
        {
            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Province])
            {
                return;
            }

            // 陸地プロヴィンスリストを初期化する
            _controller.InitProvinceList();

            // プロヴィンスリストを初期化する
            InitProvinceList();

            // 国家フィルターを更新する
            UpdateProvinceCountryFilter();

            // プロヴィンスリストを有効化する
            EnableProvinceList();

            // 国家フィルターを有効化する
            EnableProvinceCountryFilter();

            // IDテキストボックスを有効化する
            EnableProvinceIdTextBox();

            // 編集項目を無効化する
            DisableProvinceCountryItems();
            DisableProvinceInfoItems();
            DisableProvinceResourceItems();
            DisableProvinceBuildingItems();

            // 編集項目の表示をクリアする
            ClearProvinceCountryItems();
            ClearProvinceInfoItems();
            ClearProvinceResourceItems();
            ClearProvinceBuildingItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Province] = true;
        }

        /// <summary>
        ///     プロヴィンスタブのフォーム読み込み時の処理
        /// </summary>
        private void OnProvinceTabPageFormLoad()
        {
            // プロヴィンスタブを初期化する
            InitProvinceTab();
        }

        /// <summary>
        ///     プロヴィンスタブのファイル読み込み時の処理
        /// </summary>
        private void OnProvinceTabPageFileLoad()
        {
            // プロヴィンスタブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Province)
            {
                return;
            }

            // プロヴィンスデータの読み込み完了まで待機する
            Provinces.WaitLoading();

            // 初回遷移時には表示を更新する
            UpdateProvinceTab();
        }

        /// <summary>
        ///     プロヴィンスタブ選択時の処理
        /// </summary>
        private void OnProvinceTabPageSelected()
        {
            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // プロヴィンスデータの読み込み完了まで待機する
            Provinces.WaitLoading();

            // 初回遷移時には表示を更新する
            UpdateProvinceTab();

            // 読み込み済みで未初期化ならばマップパネルを更新する
            UpdateMapPanel();
        }

        #endregion

        #region プロヴィンスタブ - マップ

        /// <summary>
        ///     マップパネルを更新する
        /// </summary>
        private void UpdateMapPanel()
        {
            // マップ読み込み前ならば何もしない
            if (!Maps.IsLoaded[(int) MapLevel.Level2])
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_mapPanelInitialized)
            {
                return;
            }

            // 初期化済みフラグを設定する
            _mapPanelInitialized = true;

            // マップパネルを有効化する
            _mapPanelController.ProvinceMouseClick += OnMapPanelMouseClick;
            _mapPanelController.Show();

            // マップフィルターを有効化する
            EnableMapFilter();

            // 選択プロヴィンスが表示されるようにスクロールする
            Province province = GetSelectedProvince();
            if (province != null)
            {
                _mapPanelController.ScrollToProvince(province.Id);
            }
        }

        /// <summary>
        ///     マップパネルのマウスクリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapPanelMouseClick(object sender, MapPanelController.ProvinceEventArgs e)
        {
            // 左クリック以外では何もしない
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 陸地プロヴィンスでなければ何もしない
            if (_controller.GetLandProvinceIndex(e.Id) < 0)
            {
                return;
            }

            // 選択中のプロヴィンスIDを更新する
            provinceIdTextBox.Text = IntHelper.ToString(e.Id);

            // プロヴィンスを選択する
            SelectProvince(e.Id);

            Country country = GetSelectedProvinceCountry();
            switch (_mapPanelController.FilterMode)
            {
                case MapPanelController.MapFilterMode.None:
                    Country target = (from settings in Scenarios.Data.Countries
                        where settings.ControlledProvinces.Contains(e.Id)
                        select settings.Country).FirstOrDefault();
                    provinceCountryFilterComboBox.SelectedIndex = Array.IndexOf(Countries.Tags, target) + 1;
                    break;

                case MapPanelController.MapFilterMode.Core:
                    if (country != Country.None)
                    {
                        coreProvinceCheckBox.Checked = !coreProvinceCheckBox.Checked;
                    }
                    break;

                case MapPanelController.MapFilterMode.Owned:
                    if (country != Country.None && ownedProvinceCheckBox.Enabled)
                    {
                        ownedProvinceCheckBox.Checked = !ownedProvinceCheckBox.Checked;
                    }
                    break;

                case MapPanelController.MapFilterMode.Controlled:
                    if (country != Country.None && controlledProvinceCheckBox.Enabled)
                    {
                        controlledProvinceCheckBox.Checked = !controlledProvinceCheckBox.Checked;
                    }
                    break;

                case MapPanelController.MapFilterMode.Claimed:
                    if (country != Country.None)
                    {
                        claimedProvinceCheckBox.Checked = !claimedProvinceCheckBox.Checked;
                    }
                    break;
            }
        }

        #endregion

        #region プロヴィンスタブ - マップフィルター

        /// <summary>
        ///     マップフィルターを初期化する
        /// </summary>
        private void InitMapFilter()
        {
            mapFilterNoneRadioButton.Tag = MapPanelController.MapFilterMode.None;
            mapFilterCoreRadioButton.Tag = MapPanelController.MapFilterMode.Core;
            mapFilterOwnedRadioButton.Tag = MapPanelController.MapFilterMode.Owned;
            mapFilterControlledRadioButton.Tag = MapPanelController.MapFilterMode.Controlled;
            mapFilterClaimedRadioButton.Tag = MapPanelController.MapFilterMode.Claimed;
        }

        /// <summary>
        ///     マップフィルターを有効化する
        /// </summary>
        private void EnableMapFilter()
        {
            mapFilterGroupBox.Enabled = true;
        }

        /// <summary>
        ///     マップフィルターラジオボタンのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapFilterRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
            {
                return;
            }

            // チェックなしの時には他の項目にチェックがついているので処理しない
            if (!radioButton.Checked)
            {
                return;
            }

            // フィルターモードを更新する
            _mapPanelController.FilterMode = (MapPanelController.MapFilterMode) radioButton.Tag;
        }

        #endregion

        #region プロヴィンスタブ - 国家フィルター

        /// <summary>
        ///     国家フィルターを更新する
        /// </summary>
        private void UpdateProvinceCountryFilter()
        {
            provinceCountryFilterComboBox.BeginUpdate();
            provinceCountryFilterComboBox.Items.Clear();
            provinceCountryFilterComboBox.Items.Add("");
            foreach (Country country in Countries.Tags)
            {
                provinceCountryFilterComboBox.Items.Add(Scenarios.GetCountryTagName(country));
            }
            provinceCountryFilterComboBox.EndUpdate();
        }

        /// <summary>
        ///     国家フィルターを有効化する
        /// </summary>
        private void EnableProvinceCountryFilter()
        {
            provinceCountryFilterLabel.Enabled = true;
            provinceCountryFilterComboBox.Enabled = true;
        }

        /// <summary>
        ///     国家フィルターの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceCountryFilterComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            Country country = GetSelectedProvinceCountry();

            // プロヴィンスリストを更新する
            UpdateProvinceList(country);

            // マップフィルターを更新する
            _mapPanelController.SelectedCountry = country;

            // プロヴィンス国家グループボックスの編集項目を更新する
            Province province = GetSelectedProvince();
            if ((country != Country.None) && (province != null))
            {
                CountrySettings settings = Scenarios.GetCountrySettings(country);
                UpdateProvinceCountryItems(province, settings);
                EnableProvinceCountryItems();
            }
            else
            {
                DisableProvinceCountryItems();
                ClearProvinceCountryItems();
            }
        }

        /// <summary>
        ///     選択国を取得する
        /// </summary>
        /// <returns>選択国</returns>
        private Country GetSelectedProvinceCountry()
        {
            if (provinceCountryFilterComboBox.SelectedIndex <= 0)
            {
                return Country.None;
            }
            return Countries.Tags[provinceCountryFilterComboBox.SelectedIndex - 1];
        }

        #endregion

        #region プロヴィンスタブ - プロヴィンスID

        /// <summary>
        ///     プロヴィンスIDテキストボックスを初期化する
        /// </summary>
        private void InitProvinceIdTextBox()
        {
            _itemControls.Add(ScenarioEditorItemId.ProvinceId, provinceIdTextBox);

            provinceIdTextBox.Tag = ScenarioEditorItemId.ProvinceId;
        }

        /// <summary>
        ///     プロヴィンスIDテキストボックスを有効化する
        /// </summary>
        private void EnableProvinceIdTextBox()
        {
            provinceIdLabel.Enabled = true;
            provinceIdTextBox.Enabled = true;
        }

        /// <summary>
        ///     プロヴィンスIDテキストボックスのID変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceIdTextBoxValidated(object sender, EventArgs e)
        {
            Province province = GetSelectedProvince();

            // 文字列を数値に変換できなければ値を戻す
            int val;
            if (!IntHelper.TryParse(provinceIdTextBox.Text, out val))
            {
                if (province != null)
                {
                    provinceIdTextBox.Text = IntHelper.ToString(province.Id);
                }
                return;
            }

            // 初期値から変更されていなければ何もしない
            if ((province == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if ((province != null) && (val == province.Id))
            {
                return;
            }

            // プロヴィンスを選択する
            SelectProvince(val);
        }

        /// <summary>
        ///     プロヴィンスを選択する
        /// </summary>
        /// <param name="id">プロヴィンスID</param>
        private void SelectProvince(int id)
        {
            // プロヴィンスリストビューの選択項目を変更する
            int index = _controller.GetLandProvinceIndex(id);
            if (index >= 0)
            {
                ListViewItem item = provinceListView.Items[index];
                item.Focused = true;
                item.Selected = true;
                item.EnsureVisible();
            }
        }

        #endregion

        #region プロヴィンスタブ - プロヴィンスリスト

        /// <summary>
        ///     陸地プロヴィンスリストを初期化する
        /// </summary>
        private void InitProvinceList()
        {
            provinceListView.BeginUpdate();
            provinceListView.Items.Clear();
            foreach (Province province in _controller.GetLandProvinces())
            {
                ListViewItem item = CreateProvinceListItem(province);
                provinceListView.Items.Add(item);
            }
            provinceListView.EndUpdate();
        }

        /// <summary>
        ///     プロヴィンスリストを更新する
        /// </summary>
        /// <param name="country">選択国</param>
        private void UpdateProvinceList(Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            provinceListView.BeginUpdate();
            if (settings != null)
            {
                foreach (ListViewItem item in provinceListView.Items)
                {
                    Province province = (Province) item.Tag;
                    item.SubItems[2].Text = province.Id == settings.Capital ? Resources.Yes : "";
                    item.SubItems[3].Text = settings.NationalProvinces.Contains(province.Id) ? Resources.Yes : "";
                    item.SubItems[4].Text = settings.OwnedProvinces.Contains(province.Id) ? Resources.Yes : "";
                    item.SubItems[5].Text = settings.ControlledProvinces.Contains(province.Id) ? Resources.Yes : "";
                    item.SubItems[6].Text = settings.ClaimedProvinces.Contains(province.Id) ? Resources.Yes : "";
                }
            }
            else
            {
                foreach (ListViewItem item in provinceListView.Items)
                {
                    item.SubItems[2].Text = "";
                    item.SubItems[3].Text = "";
                    item.SubItems[4].Text = "";
                    item.SubItems[5].Text = "";
                    item.SubItems[6].Text = "";
                }
            }
            provinceListView.EndUpdate();
        }

        /// <summary>
        ///     プロヴィンスリストを有効化する
        /// </summary>
        private void EnableProvinceList()
        {
            provinceListView.Enabled = true;
        }

        /// <summary>
        ///     プロヴィンスリストビューの項目文字列を設定する
        /// </summary>
        /// <param name="index">プロヴィンスリストビューのインデックス</param>
        /// <param name="no">項目番号</param>
        /// <param name="s">文字列</param>
        internal void SetProvinceListItemText(int index, int no, string s)
        {
            provinceListView.Items[index].SubItems[no].Text = s;
        }

        /// <summary>
        ///     プロヴィンスリストビューの項目を作成する
        /// </summary>
        /// <param name="province">プロヴィンスデータ</param>
        /// <returns>プロヴィンスリストビューの項目</returns>
        private static ListViewItem CreateProvinceListItem(Province province)
        {
            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            ListViewItem item = new ListViewItem { Text = IntHelper.ToString(province.Id), Tag = province };
            item.SubItems.Add(Scenarios.GetProvinceName(province, settings));
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");

            return item;
        }

        /// <summary>
        ///     選択中のプロヴィンスを取得する
        /// </summary>
        /// <returns></returns>
        private Province GetSelectedProvince()
        {
            if (provinceListView.SelectedIndices.Count == 0)
            {
                return null;
            }
            return provinceListView.SelectedItems[0].Tag as Province;
        }

        /// <summary>
        ///     プロヴィンスリストビューの選択項目変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ編集項目を無効化する
            Province province = GetSelectedProvince();
            if (province == null)
            {
                // 編集項目を無効化する
                DisableProvinceCountryItems();
                DisableProvinceInfoItems();
                DisableProvinceResourceItems();
                DisableProvinceBuildingItems();

                // 編集項目の表示をクリアする
                ClearProvinceCountryItems();
                ClearProvinceInfoItems();
                ClearProvinceResourceItems();
                ClearProvinceBuildingItems();
                return;
            }

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);
            Country country = GetSelectedProvinceCountry();
            CountrySettings countrySettings = Scenarios.GetCountrySettings(country);

            // 編集項目の表示を更新する
            UpdateProvinceCountryItems(province, countrySettings);
            UpdateProvinceInfoItems(province, settings);
            UpdateProvinceResourceItems(settings);
            UpdateProvinceBuildingItems(settings);

            // 編集項目を有効化する
            if (country != Country.None)
            {
                EnableProvinceCountryItems();
            }
            EnableProvinceInfoItems();
            EnableProvinceResourceItems();
            EnableProvinceBuildingItems();

            // マップをスクロールさせる
            _mapPanelController.ScrollToProvince(province.Id);
        }

        #endregion

        #region プロヴィンスタブ - 国家情報

        /// <summary>
        ///     プロヴィンス国家情報の編集項目を初期化する
        /// </summary>
        private void InitProvinceCountryItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CountryCapital, capitalCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryCoreProvinces, coreProvinceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOwnedProvinces, ownedProvinceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryControlledProvinces, controlledProvinceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryClaimedProvinces, claimedProvinceCheckBox);

            capitalCheckBox.Tag = ScenarioEditorItemId.CountryCapital;
            coreProvinceCheckBox.Tag = ScenarioEditorItemId.CountryCoreProvinces;
            ownedProvinceCheckBox.Tag = ScenarioEditorItemId.CountryOwnedProvinces;
            controlledProvinceCheckBox.Tag = ScenarioEditorItemId.CountryControlledProvinces;
            claimedProvinceCheckBox.Tag = ScenarioEditorItemId.CountryClaimedProvinces;
        }

        /// <summary>
        ///     プロヴィンス国家情報の編集項目を更新する
        /// </summary>
        /// <param name="province">プロヴィンス</param>
        /// <param name="settings">国家設定</param>
        private void UpdateProvinceCountryItems(Province province, CountrySettings settings)
        {
            _controller.UpdateItemValue(capitalCheckBox, province, settings);
            _controller.UpdateItemValue(coreProvinceCheckBox, province, settings);
            _controller.UpdateItemValue(ownedProvinceCheckBox, province, settings);
            _controller.UpdateItemValue(controlledProvinceCheckBox, province, settings);
            _controller.UpdateItemValue(claimedProvinceCheckBox, province, settings);

            _controller.UpdateItemColor(capitalCheckBox, province, settings);
            _controller.UpdateItemColor(coreProvinceCheckBox, province, settings);
            _controller.UpdateItemColor(ownedProvinceCheckBox, province, settings);
            _controller.UpdateItemColor(controlledProvinceCheckBox, province, settings);
            _controller.UpdateItemColor(claimedProvinceCheckBox, province, settings);
        }

        /// <summary>
        ///     プロヴィンス国家情報の編集項目の表示をクリアする
        /// </summary>
        private void ClearProvinceCountryItems()
        {
            capitalCheckBox.Checked = false;
            coreProvinceCheckBox.Checked = false;
            ownedProvinceCheckBox.Checked = false;
            controlledProvinceCheckBox.Checked = false;
            claimedProvinceCheckBox.Checked = false;
        }

        /// <summary>
        ///     プロヴィンス国家情報の編集項目を有効化する
        /// </summary>
        private void EnableProvinceCountryItems()
        {
            provinceCountryGroupBox.Enabled = true;
        }

        /// <summary>
        ///     プロヴィンス国家情報の編集項目を無効化する
        /// </summary>
        private void DisableProvinceCountryItems()
        {
            provinceCountryGroupBox.Enabled = false;
        }

        #endregion

        #region プロヴィンスタブ - プロヴィンス情報

        /// <summary>
        ///     プロヴィンス情報の編集項目を初期化する
        /// </summary>
        private void InitProvinceInfoItems()
        {
            _itemControls.Add(ScenarioEditorItemId.ProvinceNameKey, provinceNameKeyTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNameString, provinceNameStringTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceVp, vpTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRevoltRisk, revoltRiskTextBox);

            provinceNameKeyTextBox.Tag = ScenarioEditorItemId.ProvinceNameKey;
            provinceNameStringTextBox.Tag = ScenarioEditorItemId.ProvinceNameString;
            vpTextBox.Tag = ScenarioEditorItemId.ProvinceVp;
            revoltRiskTextBox.Tag = ScenarioEditorItemId.ProvinceRevoltRisk;
        }

        /// <summary>
        ///     プロヴィンス情報の編集項目を更新する
        /// </summary>
        /// <param name="province">プロヴィンス</param>
        /// <param name="settings">プロヴィンス設定</param>
        private void UpdateProvinceInfoItems(Province province, ProvinceSettings settings)
        {
            _controller.UpdateItemValue(provinceIdTextBox, province);
            _controller.UpdateItemValue(provinceNameKeyTextBox, province, settings);
            _controller.UpdateItemValue(provinceNameStringTextBox, province, settings);
            _controller.UpdateItemValue(vpTextBox, settings);
            _controller.UpdateItemValue(revoltRiskTextBox, settings);

            _controller.UpdateItemColor(provinceNameKeyTextBox, settings);
            _controller.UpdateItemColor(provinceNameStringTextBox, settings);
            _controller.UpdateItemColor(vpTextBox, settings);
            _controller.UpdateItemColor(revoltRiskTextBox, settings);
        }

        /// <summary>
        ///     プロヴィンス情報の編集項目の表示をクリアする
        /// </summary>
        private void ClearProvinceInfoItems()
        {
            provinceIdTextBox.Text = "";
            provinceNameKeyTextBox.Text = "";
            provinceNameStringTextBox.Text = "";
            vpTextBox.Text = "";
            revoltRiskTextBox.Text = "";
        }

        /// <summary>
        ///     プロヴィンス情報の編集項目を有効化する
        /// </summary>
        private void EnableProvinceInfoItems()
        {
            provinceInfoGroupBox.Enabled = true;
            provinceNameKeyTextBox.Enabled = Game.Type == GameType.DarkestHour;
        }

        /// <summary>
        ///     プロヴィンス情報の編集項目を無効化する
        /// </summary>
        private void DisableProvinceInfoItems()
        {
            provinceInfoGroupBox.Enabled = false;
        }

        #endregion

        #region プロヴィンスタブ - 資源情報

        /// <summary>
        ///     プロヴィンス資源情報の編集項目を初期化する
        /// </summary>
        private void InitProvinceResourceItems()
        {
            // 資源名ラベル
            provinceManpowerLabel.Text = Config.GetText(TextId.ResourceManpower);
            provinceEnergyLabel.Text = Config.GetText(TextId.ResourceEnergy);
            provinceMetalLabel.Text = Config.GetText(TextId.ResourceMetal);
            provinceRareMaterialsLabel.Text = Config.GetText(TextId.ResourceRareMaterials);
            provinceOilLabel.Text = Config.GetText(TextId.ResourceOil);
            provinceSuppliesLabel.Text = Config.GetText(TextId.ResourceSupplies);

            // 編集項目
            _itemControls.Add(ScenarioEditorItemId.ProvinceManpowerCurrent, manpowerCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceManpowerMax, manpowerMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceEnergyPool, energyPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceEnergyCurrent, energyCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceEnergyMax, energyMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceMetalPool, metalPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceMetalCurrent, metalCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceMetalMax, metalMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRareMaterialsPool, rareMaterialsPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRareMaterialsCurrent, rareMaterialsCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRareMaterialsMax, rareMaterialsMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceOilPool, oilPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceOilCurrent, oilCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceOilMax, oilMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSupplyPool, suppliesPoolTextBox);

            manpowerCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceManpowerCurrent;
            manpowerMaxTextBox.Tag = ScenarioEditorItemId.ProvinceManpowerMax;
            energyPoolTextBox.Tag = ScenarioEditorItemId.ProvinceEnergyPool;
            energyCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceEnergyCurrent;
            energyMaxTextBox.Tag = ScenarioEditorItemId.ProvinceEnergyMax;
            metalPoolTextBox.Tag = ScenarioEditorItemId.ProvinceMetalPool;
            metalCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceMetalCurrent;
            metalMaxTextBox.Tag = ScenarioEditorItemId.ProvinceMetalMax;
            rareMaterialsPoolTextBox.Tag = ScenarioEditorItemId.ProvinceRareMaterialsPool;
            rareMaterialsCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceRareMaterialsCurrent;
            rareMaterialsMaxTextBox.Tag = ScenarioEditorItemId.ProvinceRareMaterialsMax;
            oilPoolTextBox.Tag = ScenarioEditorItemId.ProvinceOilPool;
            oilCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceOilCurrent;
            oilMaxTextBox.Tag = ScenarioEditorItemId.ProvinceOilMax;
            suppliesPoolTextBox.Tag = ScenarioEditorItemId.ProvinceSupplyPool;
        }

        /// <summary>
        ///     プロヴィンス資源情報の編集項目を更新する
        /// </summary>
        /// <param name="settings">プロヴィンス設定</param>
        private void UpdateProvinceResourceItems(ProvinceSettings settings)
        {
            _controller.UpdateItemValue(manpowerCurrentTextBox, settings);
            _controller.UpdateItemValue(manpowerMaxTextBox, settings);
            _controller.UpdateItemValue(energyPoolTextBox, settings);
            _controller.UpdateItemValue(energyCurrentTextBox, settings);
            _controller.UpdateItemValue(energyMaxTextBox, settings);
            _controller.UpdateItemValue(metalPoolTextBox, settings);
            _controller.UpdateItemValue(metalCurrentTextBox, settings);
            _controller.UpdateItemValue(metalMaxTextBox, settings);
            _controller.UpdateItemValue(rareMaterialsPoolTextBox, settings);
            _controller.UpdateItemValue(rareMaterialsCurrentTextBox, settings);
            _controller.UpdateItemValue(rareMaterialsMaxTextBox, settings);
            _controller.UpdateItemValue(oilPoolTextBox, settings);
            _controller.UpdateItemValue(oilCurrentTextBox, settings);
            _controller.UpdateItemValue(oilMaxTextBox, settings);
            _controller.UpdateItemValue(suppliesPoolTextBox, settings);

            _controller.UpdateItemColor(manpowerCurrentTextBox, settings);
            _controller.UpdateItemColor(manpowerMaxTextBox, settings);
            _controller.UpdateItemColor(energyPoolTextBox, settings);
            _controller.UpdateItemColor(energyCurrentTextBox, settings);
            _controller.UpdateItemColor(energyMaxTextBox, settings);
            _controller.UpdateItemColor(metalPoolTextBox, settings);
            _controller.UpdateItemColor(metalCurrentTextBox, settings);
            _controller.UpdateItemColor(metalMaxTextBox, settings);
            _controller.UpdateItemColor(rareMaterialsPoolTextBox, settings);
            _controller.UpdateItemColor(rareMaterialsCurrentTextBox, settings);
            _controller.UpdateItemColor(rareMaterialsMaxTextBox, settings);
            _controller.UpdateItemColor(oilPoolTextBox, settings);
            _controller.UpdateItemColor(oilCurrentTextBox, settings);
            _controller.UpdateItemColor(oilMaxTextBox, settings);
            _controller.UpdateItemColor(suppliesPoolTextBox, settings);
        }

        /// <summary>
        ///     プロヴィンス資源情報の編集項目の表示をクリアする
        /// </summary>
        private void ClearProvinceResourceItems()
        {
            manpowerCurrentTextBox.Text = "";
            manpowerMaxTextBox.Text = "";
            energyPoolTextBox.Text = "";
            energyCurrentTextBox.Text = "";
            energyMaxTextBox.Text = "";
            metalPoolTextBox.Text = "";
            metalCurrentTextBox.Text = "";
            metalMaxTextBox.Text = "";
            rareMaterialsPoolTextBox.Text = "";
            rareMaterialsCurrentTextBox.Text = "";
            rareMaterialsMaxTextBox.Text = "";
            oilPoolTextBox.Text = "";
            oilCurrentTextBox.Text = "";
            oilMaxTextBox.Text = "";
            suppliesPoolTextBox.Text = "";
        }

        /// <summary>
        ///     プロヴィンス資源情報の編集項目を有効化する
        /// </summary>
        private void EnableProvinceResourceItems()
        {
            provinceResourceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     プロヴィンス資源情報の編集項目を無効化する
        /// </summary>
        private void DisableProvinceResourceItems()
        {
            provinceResourceGroupBox.Enabled = false;
        }

        #endregion

        #region プロヴィンスタブ - 建物情報

        /// <summary>
        ///     プロヴィンス建物情報の編集項目を初期化する
        /// </summary>
        private void InitProvinceBuildingItems()
        {
            _itemControls.Add(ScenarioEditorItemId.ProvinceIcCurrent, icCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceIcMax, icMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceIcRelative, icRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceInfrastructureCurrent, infrastructureCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceInfrastructureMax, infrastructureMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceInfrastructureRelative, infrastructureRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceLandFortCurrent, landFortCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceLandFortMax, landFortMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceLandFortRelative, landFortRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceCoastalFortCurrent, coastalFortCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceCoastalFortMax, coastalFortMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceCoastalFortRelative, coastalFortRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAntiAirCurrent, antiAirCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAntiAirMax, antiAirMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAntiAirRelative, antiAirRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAirBaseCurrent, airBaseCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAirBaseMax, airBaseMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAirBaseRelative, airBaseRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNavalBaseCurrent, navalBaseCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNavalBaseMax, navalBaseMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNavalBaseRelative, navalBaseRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRadarStationCurrent, radarStationCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRadarStationMax, radarStationMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRadarStationRelative, radarStationRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearReactorCurrent, nuclearReactorCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearReactorMax, nuclearReactorMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearReactorRelative, nuclearReactorRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRocketTestCurrent, rocketTestCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRocketTestMax, rocketTestMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRocketTestRelative, rocketTestRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticOilCurrent, syntheticOilCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticOilMax, syntheticOilMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticOilRelative, syntheticOilRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticRaresCurrent, syntheticRaresCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticRaresMax, syntheticRaresMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticRaresRelative, syntheticRaresRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearPowerCurrent, nuclearPowerCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearPowerMax, nuclearPowerMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearPowerRelative, nuclearPowerRelativeTextBox);

            icCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceIcCurrent;
            icMaxTextBox.Tag = ScenarioEditorItemId.ProvinceIcMax;
            icRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceIcRelative;
            infrastructureCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceInfrastructureCurrent;
            infrastructureMaxTextBox.Tag = ScenarioEditorItemId.ProvinceInfrastructureMax;
            infrastructureRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceInfrastructureRelative;
            landFortCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceLandFortCurrent;
            landFortMaxTextBox.Tag = ScenarioEditorItemId.ProvinceLandFortMax;
            landFortRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceLandFortRelative;
            coastalFortCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceCoastalFortCurrent;
            coastalFortMaxTextBox.Tag = ScenarioEditorItemId.ProvinceCoastalFortMax;
            coastalFortRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceCoastalFortRelative;
            antiAirCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceAntiAirCurrent;
            antiAirMaxTextBox.Tag = ScenarioEditorItemId.ProvinceAntiAirMax;
            antiAirRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceAntiAirRelative;
            airBaseCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceAirBaseCurrent;
            airBaseMaxTextBox.Tag = ScenarioEditorItemId.ProvinceAirBaseMax;
            airBaseRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceAirBaseRelative;
            navalBaseCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceNavalBaseCurrent;
            navalBaseMaxTextBox.Tag = ScenarioEditorItemId.ProvinceNavalBaseMax;
            navalBaseRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceNavalBaseRelative;
            radarStationCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceRadarStationCurrent;
            radarStationMaxTextBox.Tag = ScenarioEditorItemId.ProvinceRadarStationMax;
            radarStationRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceRadarStationRelative;
            nuclearReactorCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearReactorCurrent;
            nuclearReactorMaxTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearReactorMax;
            nuclearReactorRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearReactorRelative;
            rocketTestCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceRocketTestCurrent;
            rocketTestMaxTextBox.Tag = ScenarioEditorItemId.ProvinceRocketTestMax;
            rocketTestRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceRocketTestRelative;
            syntheticOilCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticOilCurrent;
            syntheticOilMaxTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticOilMax;
            syntheticOilRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticOilRelative;
            syntheticRaresCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticRaresCurrent;
            syntheticRaresMaxTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticRaresMax;
            syntheticRaresRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticRaresRelative;
            nuclearPowerCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearPowerCurrent;
            nuclearPowerMaxTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearPowerMax;
            nuclearPowerRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearPowerRelative;
        }

        /// <summary>
        ///     プロヴィンス建物情報の編集項目を更新する
        /// </summary>
        /// <param name="settings">プロヴィンス設定</param>
        private void UpdateProvinceBuildingItems(ProvinceSettings settings)
        {
            _controller.UpdateItemValue(icCurrentTextBox, settings);
            _controller.UpdateItemValue(icMaxTextBox, settings);
            _controller.UpdateItemValue(icRelativeTextBox, settings);
            _controller.UpdateItemValue(infrastructureCurrentTextBox, settings);
            _controller.UpdateItemValue(infrastructureMaxTextBox, settings);
            _controller.UpdateItemValue(infrastructureRelativeTextBox, settings);
            _controller.UpdateItemValue(landFortCurrentTextBox, settings);
            _controller.UpdateItemValue(landFortMaxTextBox, settings);
            _controller.UpdateItemValue(landFortRelativeTextBox, settings);
            _controller.UpdateItemValue(coastalFortCurrentTextBox, settings);
            _controller.UpdateItemValue(coastalFortMaxTextBox, settings);
            _controller.UpdateItemValue(coastalFortRelativeTextBox, settings);
            _controller.UpdateItemValue(antiAirCurrentTextBox, settings);
            _controller.UpdateItemValue(antiAirMaxTextBox, settings);
            _controller.UpdateItemValue(antiAirRelativeTextBox, settings);
            _controller.UpdateItemValue(airBaseCurrentTextBox, settings);
            _controller.UpdateItemValue(airBaseMaxTextBox, settings);
            _controller.UpdateItemValue(airBaseRelativeTextBox, settings);
            _controller.UpdateItemValue(navalBaseCurrentTextBox, settings);
            _controller.UpdateItemValue(navalBaseMaxTextBox, settings);
            _controller.UpdateItemValue(navalBaseRelativeTextBox, settings);
            _controller.UpdateItemValue(radarStationCurrentTextBox, settings);
            _controller.UpdateItemValue(radarStationMaxTextBox, settings);
            _controller.UpdateItemValue(radarStationRelativeTextBox, settings);
            _controller.UpdateItemValue(nuclearReactorCurrentTextBox, settings);
            _controller.UpdateItemValue(nuclearReactorMaxTextBox, settings);
            _controller.UpdateItemValue(nuclearReactorRelativeTextBox, settings);
            _controller.UpdateItemValue(rocketTestCurrentTextBox, settings);
            _controller.UpdateItemValue(rocketTestMaxTextBox, settings);
            _controller.UpdateItemValue(rocketTestRelativeTextBox, settings);
            _controller.UpdateItemValue(syntheticOilCurrentTextBox, settings);
            _controller.UpdateItemValue(syntheticOilMaxTextBox, settings);
            _controller.UpdateItemValue(syntheticOilRelativeTextBox, settings);
            _controller.UpdateItemValue(syntheticRaresCurrentTextBox, settings);
            _controller.UpdateItemValue(syntheticRaresMaxTextBox, settings);
            _controller.UpdateItemValue(syntheticRaresRelativeTextBox, settings);
            _controller.UpdateItemValue(nuclearPowerCurrentTextBox, settings);
            _controller.UpdateItemValue(nuclearPowerMaxTextBox, settings);
            _controller.UpdateItemValue(nuclearPowerRelativeTextBox, settings);

            _controller.UpdateItemColor(icCurrentTextBox, settings);
            _controller.UpdateItemColor(icMaxTextBox, settings);
            _controller.UpdateItemColor(icRelativeTextBox, settings);
            _controller.UpdateItemColor(infrastructureCurrentTextBox, settings);
            _controller.UpdateItemColor(infrastructureMaxTextBox, settings);
            _controller.UpdateItemColor(infrastructureRelativeTextBox, settings);
            _controller.UpdateItemColor(landFortCurrentTextBox, settings);
            _controller.UpdateItemColor(landFortMaxTextBox, settings);
            _controller.UpdateItemColor(landFortRelativeTextBox, settings);
            _controller.UpdateItemColor(coastalFortCurrentTextBox, settings);
            _controller.UpdateItemColor(coastalFortMaxTextBox, settings);
            _controller.UpdateItemColor(coastalFortRelativeTextBox, settings);
            _controller.UpdateItemColor(antiAirCurrentTextBox, settings);
            _controller.UpdateItemColor(antiAirMaxTextBox, settings);
            _controller.UpdateItemColor(antiAirRelativeTextBox, settings);
            _controller.UpdateItemColor(airBaseCurrentTextBox, settings);
            _controller.UpdateItemColor(airBaseMaxTextBox, settings);
            _controller.UpdateItemColor(airBaseRelativeTextBox, settings);
            _controller.UpdateItemColor(navalBaseCurrentTextBox, settings);
            _controller.UpdateItemColor(navalBaseMaxTextBox, settings);
            _controller.UpdateItemColor(navalBaseRelativeTextBox, settings);
            _controller.UpdateItemColor(radarStationCurrentTextBox, settings);
            _controller.UpdateItemColor(radarStationMaxTextBox, settings);
            _controller.UpdateItemColor(radarStationRelativeTextBox, settings);
            _controller.UpdateItemColor(nuclearReactorCurrentTextBox, settings);
            _controller.UpdateItemColor(nuclearReactorMaxTextBox, settings);
            _controller.UpdateItemColor(nuclearReactorRelativeTextBox, settings);
            _controller.UpdateItemColor(rocketTestCurrentTextBox, settings);
            _controller.UpdateItemColor(rocketTestMaxTextBox, settings);
            _controller.UpdateItemColor(rocketTestRelativeTextBox, settings);
            _controller.UpdateItemColor(syntheticOilCurrentTextBox, settings);
            _controller.UpdateItemColor(syntheticOilMaxTextBox, settings);
            _controller.UpdateItemColor(syntheticOilRelativeTextBox, settings);
            _controller.UpdateItemColor(syntheticRaresCurrentTextBox, settings);
            _controller.UpdateItemColor(syntheticRaresMaxTextBox, settings);
            _controller.UpdateItemColor(syntheticRaresRelativeTextBox, settings);
            _controller.UpdateItemColor(nuclearPowerCurrentTextBox, settings);
            _controller.UpdateItemColor(nuclearPowerMaxTextBox, settings);
            _controller.UpdateItemColor(nuclearPowerRelativeTextBox, settings);
        }

        /// <summary>
        ///     プロヴィンス建物情報の編集項目の表示をクリアする
        /// </summary>
        private void ClearProvinceBuildingItems()
        {
            icCurrentTextBox.Text = "";
            icMaxTextBox.Text = "";
            icRelativeTextBox.Text = "";
            infrastructureCurrentTextBox.Text = "";
            infrastructureMaxTextBox.Text = "";
            infrastructureRelativeTextBox.Text = "";
            landFortCurrentTextBox.Text = "";
            landFortMaxTextBox.Text = "";
            landFortRelativeTextBox.Text = "";
            coastalFortCurrentTextBox.Text = "";
            coastalFortMaxTextBox.Text = "";
            coastalFortRelativeTextBox.Text = "";
            antiAirCurrentTextBox.Text = "";
            antiAirMaxTextBox.Text = "";
            antiAirRelativeTextBox.Text = "";
            airBaseCurrentTextBox.Text = "";
            airBaseMaxTextBox.Text = "";
            airBaseRelativeTextBox.Text = "";
            navalBaseCurrentTextBox.Text = "";
            navalBaseMaxTextBox.Text = "";
            navalBaseRelativeTextBox.Text = "";
            radarStationCurrentTextBox.Text = "";
            radarStationMaxTextBox.Text = "";
            radarStationRelativeTextBox.Text = "";
            nuclearReactorCurrentTextBox.Text = "";
            nuclearReactorMaxTextBox.Text = "";
            nuclearReactorRelativeTextBox.Text = "";
            rocketTestCurrentTextBox.Text = "";
            rocketTestMaxTextBox.Text = "";
            rocketTestRelativeTextBox.Text = "";
            syntheticOilCurrentTextBox.Text = "";
            syntheticOilMaxTextBox.Text = "";
            syntheticOilRelativeTextBox.Text = "";
            syntheticRaresCurrentTextBox.Text = "";
            syntheticRaresMaxTextBox.Text = "";
            syntheticRaresRelativeTextBox.Text = "";
            nuclearPowerCurrentTextBox.Text = "";
            nuclearPowerMaxTextBox.Text = "";
            nuclearPowerRelativeTextBox.Text = "";
        }

        /// <summary>
        ///     プロヴィンス建物情報の編集項目を有効化する
        /// </summary>
        private void EnableProvinceBuildingItems()
        {
            provinceBuildingGroupBox.Enabled = true;

            bool flag = Game.Type == GameType.ArsenalOfDemocracy;
            provinceSyntheticOilLabel.Enabled = flag;
            syntheticOilCurrentTextBox.Enabled = flag;
            syntheticOilMaxTextBox.Enabled = flag;
            syntheticOilRelativeTextBox.Enabled = flag;
            provinceSyntheticRaresLabel.Enabled = flag;
            syntheticRaresCurrentTextBox.Enabled = flag;
            syntheticRaresMaxTextBox.Enabled = flag;
            syntheticRaresRelativeTextBox.Enabled = flag;
            provinceNuclearPowerLabel.Enabled = flag;
            nuclearPowerCurrentTextBox.Enabled = flag;
            nuclearPowerMaxTextBox.Enabled = flag;
            nuclearPowerRelativeTextBox.Enabled = flag;
        }

        /// <summary>
        ///     プロヴィンス建物情報の編集項目を無効化する
        /// </summary>
        private void DisableProvinceBuildingItems()
        {
            provinceBuildingGroupBox.Enabled = false;
        }

        #endregion

        #region プロヴィンスタブ - 編集項目

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            // 文字列を数値に変換できなければ値を戻す
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // 初期値から変更されていなければ何もしない
            if ((settings == null) && (val == 0))
            {
                return;
            }

            // 値に変化がなければ何もしない
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = new ProvinceSettings { Id = province.Id };
                Scenarios.AddProvinceSettings(settings);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // 値を更新する
            _controller.SetItemValue(itemId, val, settings);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     テキストボックスのフォーカス移動後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            // 文字列を数値に変換できなければ値を戻す
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // 初期値から変更されていなければ何もしない
            if ((settings == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // 値に変化がなければ何もしない
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = new ProvinceSettings { Id = province.Id };
                Scenarios.AddProvinceSettings(settings);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, settings);

            // 値を更新する
            _controller.SetItemValue(itemId, val, settings);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     テキストボックスの値変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            // 初期値から変更されていなければ何もしない
            object prev = _controller.GetItemValue(itemId, province, settings);
            string val = control.Text;
            if ((prev == null) && string.IsNullOrEmpty(val))
            {
                return;
            }

            // 値に変化がなければ何もしない
            if (val.Equals(prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = new ProvinceSettings { Id = province.Id };
                Scenarios.AddProvinceSettings(settings);
            }

            _controller.OutputItemValueChangedLog(itemId, val, province, settings);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, settings);

            // 値を更新する
            _controller.SetItemValue(itemId, val, settings);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, province, settings);
        }

        /// <summary>
        ///     チェックボックスのチェック状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // 選択項目がなければ何もしない
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }
            Country country = GetSelectedProvinceCountry();
            if (country == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // 初期値から変更されていなければ何もしない
            bool val = control.Checked;
            if ((settings == null) && !val)
            {
                return;
            }

            // 値に変化がなければ何もしない
            object prev = _controller.GetItemValue(itemId, province, settings);
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, province, settings);

            // 項目値変更前の処理
            _controller.PreItemChanged(itemId, val, province, settings);

            // 値を更新する
            _controller.SetItemValue(itemId, val, province, settings);

            // 編集済みフラグを設定する
            _controller.SetItemDirty(itemId, province, settings);

            // 文字色を変更する
            control.ForeColor = Color.Red;

            // 項目値変更後の処理
            _controller.PostItemChanged(itemId, val, province, settings);
        }

        #endregion

        #endregion

        #region 初期部隊タブ

        /// <summary>
        ///     初期部隊タブのファイル読み込み時の処理
        /// </summary>
        private void OnOobTabPageFileLoad()
        {
            // 初期部隊タブ選択中でなければ何もしない
            if (_tabPageNo != TabPageNo.Oob)
            {
                return;
            }

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Oob])
            {
                return;
            }

            // 指揮官データの読み込み完了まで待機する
            Leaders.WaitLoading();

            // プロヴィンスデータの読み込み完了まで待機する
            Provinces.WaitLoading();

            // ユニットデータの読み込み完了まで待機する
            Units.WaitLoading();

            // 編集項目を初期化する
            _oobPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Oob] = true;
        }

        /// <summary>
        ///     初期部隊タブ選択時の処理
        /// </summary>
        private void OnOobTabPageSelected()
        {
            // タブページを作成する
            if (_oobPage == null)
            {
                _oobPage = new ScenarioEditorOobPage(_controller, this);
                scenarioTabControl.TabPages[(int) TabPageNo.Oob].Controls.Add(_oobPage);
            }

            // シナリオ未読み込みならば何もしない
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // 指揮官データの読み込み完了まで待機する
            Leaders.WaitLoading();

            // プロヴィンスデータの読み込み完了まで待機する
            Provinces.WaitLoading();

            // ユニットデータの読み込み完了まで待機する
            Units.WaitLoading();

            // 初期化済みであれば何もしない
            if (_tabPageInitialized[(int) TabPageNo.Oob])
            {
                return;
            }

            // 編集項目を初期化する
            _oobPage.UpdateItems();

            // 初期化済みフラグをセットする
            _tabPageInitialized[(int) TabPageNo.Oob] = true;
        }

        #endregion

        #region 共通

        #region 共通 - 国家

        /// <summary>
        ///     国家リストボックスを更新する
        /// </summary>
        /// <param name="control">コントロール</param>
        private static void UpdateCountryListBox(ListBox control)
        {
            control.BeginUpdate();
            control.Items.Clear();
            foreach (Country country in Countries.Tags)
            {
                control.Items.Add(Scenarios.GetCountryTagName(country));
            }
            control.EndUpdate();
        }

        /// <summary>
        ///     国家コンボボックスを更新する
        /// </summary>
        /// <param name="control">コントロール</param>
        /// <param name="allowEmpty">空項目を許可するかどうか</param>
        private void UpdateCountryComboBox(ComboBox control, bool allowEmpty)
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            int width = control.Width;
            control.BeginUpdate();
            control.Items.Clear();
            if (allowEmpty)
            {
                control.Items.Add("");
            }
            foreach (Country country in Countries.Tags)
            {
                string s = Countries.GetTagName(country);
                control.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, control.Font).Width + SystemInformation.VerticalScrollBarWidth + margin);
            }
            control.DropDownWidth = width;
            control.EndUpdate();
        }

        /// <summary>
        ///     国家リストボックスの項目描画処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // 項目がなければ何もしない
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // 背景を描画する
            e.DrawBackground();

            // 項目を描画する
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // 変更ありの項目は文字色を変更する
                CountrySettings settings = Scenarios.GetCountrySettings(Countries.Tags[e.Index]);
                brush = new SolidBrush(settings != null
                    ? (settings.IsDirty() ? Color.Red : control.ForeColor)
                    : Color.LightGray);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // フォーカスを描画する
            e.DrawFocusRectangle();
        }

        #endregion

        #region 共通 - 編集項目

        /// <summary>
        ///     編集項目IDに関連付けられたコントロールを取得する
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        internal Control GetItemControl(ScenarioEditorItemId itemId)
        {
            return _itemControls.ContainsKey(itemId) ? _itemControls[itemId] : null;
        }

        #endregion

        #endregion
    }
}