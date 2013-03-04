﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HoI2Editor.Properties;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     研究機関データ群
    /// </summary>
    public static class Teams
    {
        #region フィールド

        /// <summary>
        ///     マスター閣僚リスト
        /// </summary>
        public static List<Team> List = new List<Team>();

        /// <summary>
        ///     現在解析中のファイル名
        /// </summary>
        private static string _currentFileName = "";

        /// <summary>
        ///     現在解析中の行番号
        /// </summary>
        private static int _currentLineNo;

        /// <summary>
        ///     編集済みフラグ
        /// </summary>
        private static readonly bool[] DirtyFlags = new bool[Enum.GetValues(typeof (CountryTag)).Length];

        /// <summary>
        ///     読み込み済みフラグ
        /// </summary>
        private static bool _loaded;

        #endregion

        #region 定数

        /// <summary>
        ///     CSVファイルの区切り文字
        /// </summary>
        private static readonly char[] CsvSeparator = {';'};

        #endregion

        #region ファイル読み込み

        /// <summary>
        ///     研究機関ファイルの再読み込みを要求する
        /// </summary>
        public static void RequireReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     研究機関ファイル群を読み込む
        /// </summary>
        public static void Load()
        {
            // 読み込み済みならば戻る
            if (_loaded)
            {
                return;
            }

            List.Clear();

            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                case GameType.ArsenalOfDemocracy:
                    LoadHoI2();
                    break;

                case GameType.DarkestHour:
                    if (Game.IsModActive)
                    {
                        LoadDh();
                    }
                    else
                    {
                        LoadHoI2();
                    }
                    break;
            }

            _loaded = true;
        }

        /// <summary>
        ///     研究機関ファイル群を読み込む(HoI2/AoD/DH-MOD未使用時)
        /// </summary>
        private static void LoadHoI2()
        {
            var list = new List<string>();
            string folderName;

            // MODフォルダ内の研究機関ファイルを読み込む
            if (Game.IsModActive)
            {
                folderName = Path.Combine(Game.ModFolderName, Game.TeamPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // 研究機関ファイルを読み込む
                            LoadFile(fileName);

                            // 研究機関ファイル一覧に読み込んだファイル名を登録する
                            string name = Path.GetFileName(fileName);
                            if (!String.IsNullOrEmpty(name))
                            {
                                list.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            Log.Write(string.Format("{0}: {1}\n\n", Resources.FileReadError, fileName));
                        }
                    }
                }
            }

            // バニラフォルダ内の研究機関ファイルを読み込む
            folderName = Path.Combine(Game.FolderName, Game.TeamPathName);
            if (Directory.Exists(folderName))
            {
                foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                {
                    // MODフォルダ内で読み込んだファイルは無視する
                    string name = Path.GetFileName(fileName);
                    if (String.IsNullOrEmpty(name) || list.Contains(name.ToLower()))
                    {
                        continue;
                    }

                    try
                    {
                        // 研究機関ファイルを読み込む
                        LoadFile(fileName);
                    }
                    catch (Exception)
                    {
                        Log.Write(string.Format("{0}: {1}\n\n", Resources.FileReadError, fileName));
                    }
                }
            }
        }

        /// <summary>
        ///     研究機関ファイル群を読み込む(DH-MOD使用時)
        /// </summary>
        private static void LoadDh()
        {
            // 研究機関リストファイルが存在しなければ従来通りの読み込み方法を使用する
            string listFileName = Game.GetReadFileName(Game.DhTeamListPathName);
            if (!File.Exists(listFileName))
            {
                LoadHoI2();
                return;
            }

            // 研究機関リストファイルを読み込む
            IEnumerable<string> fileList;
            try
            {
                fileList = LoadList(listFileName);
            }
            catch (Exception)
            {
                Log.Write(string.Format("{0}: {1}\n\n", Resources.FileReadError, listFileName));
                return;
            }

            foreach (string fileName
                in fileList.Select(name => Game.GetReadFileName(Path.Combine(Game.TeamPathName, name))))
            {
                try
                {
                    // 研究機関ファイルを読み込む
                    LoadFile(fileName);
                }
                catch (Exception)
                {
                    Log.Write(string.Format("{0}: {1}\n\n", Resources.FileReadError, fileName));
                }
            }
        }

        /// <summary>
        ///     研究機関リストファイルを読み込む(DH)
        /// </summary>
        private static IEnumerable<string> LoadList(string fileName)
        {
            var list = new List<string>();
            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    // 空行
                    if (String.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    // コメント行
                    if (line[0] == '#')
                    {
                        continue;
                    }

                    list.Add(line);
                }
                reader.Close();
            }
            return list;
        }

        /// <summary>
        ///     研究機関ファイルを読み込む
        /// </summary>
        /// <param name="fileName">対象ファイル名</param>
        private static void LoadFile(string fileName)
        {
            using (var reader = new StreamReader(fileName, Encoding.GetEncoding(Game.CodePage)))
            {
                _currentFileName = Path.GetFileName(fileName);
                _currentLineNo = 1;

                // 空ファイルを読み飛ばす
                if (reader.EndOfStream)
                {
                    return;
                }

                // 国タグ読み込み
                string line = reader.ReadLine();
                if (String.IsNullOrEmpty(line))
                {
                    return;
                }
                string[] tokens = line.Split(CsvSeparator);
                if (tokens.Length == 0 || String.IsNullOrEmpty(tokens[0]))
                {
                    return;
                }
                // サポート外の国タグの場合は何もしない
                if (!Country.StringMap.ContainsKey(tokens[0].ToUpper()))
                {
                    return;
                }
                CountryTag country = Country.StringMap[tokens[0].ToUpper()];

                _currentLineNo++;

                while (!reader.EndOfStream)
                {
                    ParseLine(reader.ReadLine(), country);
                    _currentLineNo++;
                }
                reader.Close();

                ResetDirty(country);
            }
        }

        /// <summary>
        ///     研究機関定義行を解釈する
        /// </summary>
        /// <param name="line">対象文字列</param>
        /// <param name="country">国家タグ</param>
        private static void ParseLine(string line, CountryTag country)
        {
            // 空行を読み飛ばす
            if (String.IsNullOrEmpty(line))
            {
                return;
            }

            string[] token = line.Split(CsvSeparator);

            // ID指定のない行は読み飛ばす
            if (String.IsNullOrEmpty(token[0]))
            {
                return;
            }

            // トークン数が足りない行は読み飛ばす
            if (token.Length != 39)
            {
                Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidTokenCount, _currentFileName, _currentLineNo));
                Log.Write(String.Format("  {0}\n", line));
                // 末尾のxがない/余分な項目がある場合は解析を続ける
                if (token.Length < 38)
                {
                    return;
                }
            }

            var team = new Team {Country = country};
            int index = 0;

            // ID
            int id;
            if (!Int32.TryParse(token[index], out id))
            {
                Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidID, _currentFileName, _currentLineNo));
                Log.Write(String.Format("  {0}\n", token[index]));
                return;
            }
            team.Id = id;
            index++;

            // 名前
            team.Name = token[index];
            index++;

            // 画像ファイル名
            team.PictureName = token[index];
            index++;

            // スキル
            int skill;
            if (Int32.TryParse(token[index], out skill))
            {
                team.Skill = skill;
            }
            else
            {
                team.Skill = 1;
                Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidSkill, _currentFileName, _currentLineNo));
                Log.Write(String.Format("  {0}: {1} => {2}\n", team.Id, team.Name, token[index]));
            }
            index++;

            // 開始年
            int startYear;
            if (Int32.TryParse(token[index], out startYear))
            {
                team.StartYear = startYear;
            }
            else
            {
                team.StartYear = 1930;
                Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidStartYear, _currentFileName, _currentLineNo));
                Log.Write(String.Format("  {0}: {1} => {2}\n", team.Id, team.Name, token[index]));
            }
            index++;

            // 終了年
            int endYear;
            if (Int32.TryParse(token[index], out endYear))
            {
                team.EndYear = endYear;
            }
            else
            {
                team.EndYear = 1970;
                Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidEndYear, _currentFileName, _currentLineNo));
                Log.Write(String.Format("  {0}: {1} => {2}\n", team.Id, team.Name, token[index]));
            }
            index++;

            // 研究特性
            for (int i = 0; i < Team.SpecialityLength; i++, index++)
            {
                string s = token[index].ToLower();

                // 空文字列
                if (String.IsNullOrEmpty(s))
                {
                    team.Specialities[i] = TechSpeciality.None;
                    continue;
                }

                // 無効な研究特性文字列
                if (!Tech.SpecialityStringMap.ContainsKey(s))
                {
                    team.Specialities[i] = TechSpeciality.None;
                    Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidSpeciality, _currentFileName,
                                            _currentLineNo));
                    Log.Write(String.Format("  {0}: {1} => {2}\n", team.Id, team.Name, token[index]));
                    continue;
                }

                // サポート外の研究特性
                TechSpeciality speciality = Tech.SpecialityStringMap[s];
                if (!Techs.Specialities.Contains(speciality))
                {
                    Log.Write(String.Format("{0}: {1} L{2}\n", Resources.InvalidSpeciality, _currentFileName,
                                            _currentLineNo));
                    Log.Write(String.Format("  {0}: {1} => {2}\n", team.Id, team.Name, token[index]));
                    continue;
                }

                team.Specialities[i] = speciality;
            }

            List.Add(team);
        }

        #endregion

        #region ファイル書き込み

        /// <summary>
        ///     研究機関ファイル群を保存する
        /// </summary>
        public static void Save()
        {
            foreach (
                CountryTag country in
                    Enum.GetValues(typeof (CountryTag))
                        .Cast<CountryTag>()
                        .Where(country => DirtyFlags[(int) country] && country != CountryTag.None))
            {
                try
                {
                    // 研究機関ファイルを保存する
                    SaveFile(country);
                }
                catch (Exception)
                {
                    string folderName = Path.Combine(Game.IsModActive ? Game.ModFolderName : Game.FolderName,
                                                     Game.TeamPathName);
                    string fileName = Path.Combine(folderName, Game.GetTeamFileName(country));
                    Log.Write(string.Format("{0}: {1}\n\n", Resources.FileWriteError, fileName));
                }
            }
        }

        /// <summary>
        ///     研究機関ファイルを保存する
        /// </summary>
        /// <param name="country">国タグ</param>
        private static void SaveFile(CountryTag country)
        {
            // 研究機関フォルダが存在しなければ作成する
            string folderName = Game.GetWriteFileName(Game.TeamPathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            string fileName = Path.Combine(folderName, Game.GetTeamFileName(country));

            using (var writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                _currentFileName = fileName;
                _currentLineNo = 2;

                // ヘッダ行を書き込む
                writer.WriteLine(
                    "{0};Name;Pic Name;Skill;Start Year;End Year;Speciality1;Speciality2;Speciality3;Speciality4;Speciality5;Speciality6;Speciality7;Speciality8;Speciality9;Speciality10;Speciality11;Speciality12;Speciality13;Speciality14;Speciality15;Speciality16;Speciality17;Speciality18;Speciality19;Speciality20;Speciality21;Speciality22;Speciality23;Speciality24;Speciality25;Speciality26;Speciality27;Speciality28;Speciality29;Speciality30;Speciality31;Speciality32;x",
                    Country.Strings[(int) country]);

                // 研究機関定義行を順に書き込む
                foreach (Team team in List.Where(team => team.Country == country))
                {
                    writer.Write(
                        "{0};{1};{2};{3};{4};{5}",
                        team.Id,
                        team.Name,
                        team.PictureName,
                        team.Skill,
                        team.StartYear,
                        team.EndYear);
                    for (int i = 0; i < Team.SpecialityLength; i++)
                    {
                        writer.Write(";{0}",
                                     team.Specialities[i] != TechSpeciality.None
                                         ? Techs.SpecialityStrings[(int) team.Specialities[i]]
                                         : "");
                    }
                    writer.WriteLine(";x");

                    // 編集済みフラグを解除する
                    team.ResetDirty();

                    _currentLineNo++;
                }
                writer.Close();
            }

            ResetDirty(country);
        }

        #endregion

        #region 研究機関リスト操作

        /// <summary>
        ///     研究機関リストに項目を追加する
        /// </summary>
        /// <param name="team">追加対象の項目</param>
        public static void AddItem(Team team)
        {
            List.Add(team);
        }

        /// <summary>
        ///     研究機関リストに項目を挿入する
        /// </summary>
        /// <param name="team">挿入対象の項目</param>
        /// <param name="position">挿入位置の直前の項目</param>
        public static void InsertItem(Team team, Team position)
        {
            List.Insert(List.IndexOf(position) + 1, team);
        }

        /// <summary>
        ///     研究機関リストから項目を削除する
        /// </summary>
        /// <param name="team">削除対象の項目</param>
        public static void RemoveItem(Team team)
        {
            List.Remove(team);
        }

        /// <summary>
        ///     研究機関リストの項目を移動する
        /// </summary>
        /// <param name="src">移動元の項目</param>
        /// <param name="dest">移動先の項目</param>
        public static void MoveItem(Team src, Team dest)
        {
            int srcIndex = List.IndexOf(src);
            int destIndex = List.IndexOf(dest);

            if (srcIndex > destIndex)
            {
                // 上へ移動する場合
                List.Insert(destIndex, src);
                List.RemoveAt(srcIndex + 1);
            }
            else
            {
                // 下へ移動する場合
                List.Insert(destIndex + 1, src);
                List.RemoveAt(srcIndex);
            }
        }

        #endregion

        #region 編集済みフラグ操作

        /// <summary>
        ///     編集済みかどうかを取得する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <returns>編集済みならばtrueを返す</returns>
        public static bool IsDirty(CountryTag country)
        {
            return DirtyFlags[(int) country];
        }

        /// <summary>
        ///     編集済みフラグを設定する
        /// </summary>
        /// <param name="country">国タグ</param>
        public static void SetDirty(CountryTag country)
        {
            DirtyFlags[(int) country] = true;
        }

        /// <summary>
        ///     編集済みフラグを解除する
        /// </summary>
        /// <param name="country">国タグ</param>
        public static void ResetDirty(CountryTag country)
        {
            DirtyFlags[(int) country] = false;
        }

        #endregion
    }
}