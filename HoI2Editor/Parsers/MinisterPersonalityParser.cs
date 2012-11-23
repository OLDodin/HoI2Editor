﻿using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Properties;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     閣僚特性定義ファイルの構文解析(DH)
    /// </summary>
    public static class MinisterPersonalityParser
    {
        /// <summary>
        ///     閣僚特性定義ファイル内の閣僚地位名とIDの対応付け
        /// </summary>
        private static readonly Dictionary<string, int> PositionMap
            = new Dictionary<string, int>
                  {
                      {"", (int) MinisterPosition.None},
                      {"headofstate", (int) MinisterPosition.HeadOfState},
                      {"headofgovernment", (int) MinisterPosition.HeadOfGovernment},
                      {"foreignminister", (int) MinisterPosition.ForeignMinister},
                      {"armamentminister", (int) MinisterPosition.MinisterOfArmament},
                      {"ministerofsecurity", (int) MinisterPosition.MinisterOfSecurity},
                      {"ministerofintelligence", (int) MinisterPosition.HeadOfMilitaryIntelligence},
                      {"chiefofstaff", (int) MinisterPosition.ChiefOfStaff},
                      {"chiefofarmy", (int) MinisterPosition.ChiefOfArmy},
                      {"chiefofnavy", (int) MinisterPosition.ChiefOfNavy},
                      {"chiefofair", (int) MinisterPosition.ChiefOfAirForce},
                  };

        /// <summary>
        ///     閣僚特性定義ファイルを構文解析する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>閣僚特性リスト</returns>
        public static List<MinisterPersonalityInfo> Parse(string fileName)
        {
            var lexer = new TextLexer(fileName);
            var list = new List<MinisterPersonalityInfo>();

            while (true)
            {
                Token token = lexer.GetToken();

                // ファイルの終端
                if (token == null)
                {
                    break;
                }

                // 無効なトークン
                if (token.Type != TokenType.Identifier || !((string) token.Value).Equals("minister"))
                {
                    Log.Write(string.Format(Resources.InvalidToken, token.Value));
                    continue;
                }

                // ministerセクション
                MinisterPersonalityInfo info = ParseMinister(lexer);
                if (info == null)
                {
                    Log.Write(string.Format(Resources.ParseFailedSection, "minister", "minister_personalities.txt"));
                }

                // 閣僚特性リストへ登録
                list.Add(info);
            }

            return list;
        }

        /// <summary>
        ///     ministerセクションを構文解析する
        /// </summary>
        /// <param name="lexer">字句解析器</param>
        /// <returns>閣僚特性データ</returns>
        private static MinisterPersonalityInfo ParseMinister(TextLexer lexer)
        {
            // =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.Write(string.Format(Resources.InvalidToken, token.Value));
                return null;
            }

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.Write(string.Format(Resources.InvalidToken, token.Value));
                return null;
            }

            var info = new MinisterPersonalityInfo();
            while (true)
            {
                // ファイル終端
                token = lexer.GetToken();
                if (token == null)
                {
                    break;
                }

                // } (セクション終端)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // 無効なトークン
                if (token.Type != TokenType.Identifier)
                {
                    Log.Write(string.Format(Resources.InvalidToken, token.Value));
                    return null;
                }

                var keyword = token.Value as string;
                if (keyword == null)
                {
                    return null;
                }

                // trait
                if (keyword.Equals("trait"))
                {
                    // =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                        continue;
                    }

                    // 無効なトークン
                    token = lexer.GetToken();
                    if (token.Type != TokenType.String)
                    {
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                        continue;
                    }

                    // 閣僚特性文字列
                    info.String = token.Value as string;
                    continue;
                }

                // id
                if (keyword.Equals("id"))
                {
                    // 暫定: 1行単位で読み飛ばす
                    lexer.SkipLine();
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    // =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                        continue;
                    }

                    // 無効なトークン
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                        continue;
                    }

                    // 閣僚特性名
                    info.Name = token.Value as string;
                    continue;
                }

                // desc
                if (keyword.Equals("desc"))
                {
                    // 暫定: 1行単位で読み飛ばす
                    lexer.SkipLine();
                    continue;
                }

                // position
                if (keyword.Equals("position"))
                {
                    // =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                        continue;
                    }

                    // 無効なトークン
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                        continue;
                    }

                    var position = token.Value as string;
                    if (string.IsNullOrEmpty(position))
                    {
                        continue;
                    }
                    position = position.ToLower();

                    // 閣僚地位
                    if (PositionMap.ContainsKey(position))
                    {
                        // いずれか1つ
                        info.Position[PositionMap[position]] = true;
                    }
                    else if (position.Equals("all"))
                    {
                        // 全て
                        for (int i = 0; i < info.Position.Length; i++)
                        {
                            info.Position[i] = true;
                        }
                    }
                    else if (!position.Equals("generic"))
                    {
                        // 無効なトークン
                        Log.Write(string.Format(Resources.InvalidToken, token.Value));
                        lexer.SkipLine();
                    }
                    continue;
                }

                // value
                if (keyword.Equals("value"))
                {
                    // 暫定: 1行単位で読み飛ばす
                    lexer.SkipLine();
                    continue;
                }

                // command
                if (keyword.Equals("command"))
                {
                    // 暫定: 1行単位で読み飛ばす
                    lexer.SkipLine();
                    //continue;
                }
            }

            return info;
        }
    }
}