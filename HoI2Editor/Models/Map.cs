using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Manage map data
    /// </summary>
    public class Map
    {
        #region Public properties

        /// <summary>
        ///     Map image
        /// </summary>
        public Bitmap Image { get; private set; }

        /// <summary>
        ///     Map level
        /// </summary>
        public MapLevel Level { get; }

        /// <summary>
        ///     Providence ID Array of
        /// </summary>
        public MapProvinceIds ProvinceIds;

        #endregion

        #region Internal field

        /// <summary>
        ///     Width per map block
        /// </summary>
        private readonly int _blockWidth;

        /// <summary>
        ///     Height in map block units
        /// </summary>
        private readonly int _blockHeight;

        /// <summary>
        ///     Arrangement of map pixels
        /// </summary>
        private MapPixels _pixels;

        /// <summary>
        ///     Array of map blocks
        /// </summary>
        private MapBlocks _blocks;

        /// <summary>
        ///     File read data
        /// </summary>
        private static byte[] _data;

        /// <summary>
        ///     Read position
        /// </summary>
        private static int _index;

        /// <summary>
        ///     Offset position of map block
        /// </summary>
        private static uint[] _offsets;

        /// <summary>
        ///     Tree expansion stack
        /// </summary>
        private static MapTreeNode[] _stack;

        /// <summary>
        ///     Buffer for map pixel expansion
        /// </summary>
        private static byte[] _pics;

        /// <summary>
        ///     Providence ID Expansion buffer
        /// </summary>
        private static ushort[] _provs;

        /// <summary>
        ///     Target map block when patrol the tree
        /// </summary>
        private static MapBlock _block;

        /// <summary>
        ///     Reference pixel position when patrol the tree
        /// </summary>
        private static int _base;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Maximum width per map block
        /// </summary>
        private const int MaxWidth = 936;

        /// <summary>
        ///     Maximum height per map block
        /// </summary>
        private const int MaxHeight = 360;

        /// <summary>
        ///     Maximum number of provisions in a map block
        /// </summary>
        private const int MaxBlockProvinces = 256;

        /// <summary>
        ///     Smoothing process threshold
        /// </summary>
        private const int SmoothingThrethold = 0 << 8; // No smoothing

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="level">Map level</param>
        public Map(MapLevel level)
        {
            Level = level;
            _blockWidth = MaxWidth >> (int) level;
            _blockHeight = MaxHeight >> (int) level;
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Load the map file
        /// </summary>
        public void Load()
        {
            // Read map data
            LoadLightMap();

            // Expand the map image
            DecodePixels();

            // Generate a map image
            CreateImage();

            // Province ID Expand an array of
            ExtractIds();
        }

        /// <summary>
        ///     Read map data
        /// </summary>
        private void LoadLightMap()
        {
            // Expand map data to memory
            LoadFile();

            // Read the offset position of the map block
            int count = _blockWidth * _blockHeight;
            LoadOffsets(count);

            int offset = (count + 1) * 4;
            _blocks = new MapBlocks(_blockWidth, _blockHeight);
            _stack = new MapTreeNode[MapBlock.Width * MapBlock.Height * 2];

            // Load map blocks in sequence
            for (int i = 0; i < count; i++)
            {
                _index = offset + (int) _offsets[i];
                _blocks[i] = LoadBlock();
            }

            // Free up used buffers
            _data = null;
            _offsets = null;
            _stack = null;
        }

        /// <summary>
        ///     Expand map data to memory
        /// </summary>
        private void LoadFile()
        {
            string name;
            switch (Level)
            {
                case MapLevel.Level1:
                    name = Game.LightMap1FileName;
                    break;

                case MapLevel.Level2:
                    name = Game.LightMap2FileName;
                    break;

                case MapLevel.Level3:
                    name = Game.LightMap3FileName;
                    break;

                case MapLevel.Level4:
                    name = Game.LightMap4FileName;
                    break;

                default:
                    name = Game.LightMap1FileName;
                    break;
            }
            string fileName = Game.GetReadFileName(Game.GetMapFolderName(), name);

            using (FileStream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                _data = new byte[reader.Length];
                reader.Read(_data, 0, (int) reader.Length);
            }
        }

        /// <summary>
        ///     Read the offset position of the map block
        /// </summary>
        /// <param name="count">Number of map blocks</param>
        private static void LoadOffsets(int count)
        {
            _offsets = new uint[count + 1];

            _index = 0;
            for (int i = 0; i <= count; i++)
            {
                _offsets[i] = _data[_index++] |
                              (uint) (_data[_index++] << 8) |
                              (uint) (_data[_index++] << 16) |
                              (uint) (_data[_index++] << 24);
            }
        }

        /// <summary>
        ///     Load the map block
        /// </summary>
        private static MapBlock LoadBlock()
        {
            MapBlock block = new MapBlock();

            LoadProvinceIds(block);
            LoadMapTree(block);
            LoadNodeIds(block);
            LoadNodeColors(block);

            return block;
        }

        /// <summary>
        ///     Providence ID Read the list
        /// </summary>
        /// <param name="block">Target map block</param>
        private static void LoadProvinceIds(MapBlock block)
        {
            ushort[] ids = new ushort[MaxBlockProvinces];
            int no = 0;

            ushort work;
            do
            {
                work = _data[_index++];
                work |= (ushort) (_data[_index++] << 8);
                ids[no++] = (work & 0x4000) == 0 ? (ushort) (work & 0x7FFF) : ids[((work & 0x3F00) >> 8) - 4];
            } while ((work & 0x8000) == 0);

            block.ProvinceIds = ids;
            block.ProvinceIdCount = no;
        }

        /// <summary>
        ///     Load the map tree
        /// </summary>
        /// <param name="block">Target map block</param>
        private static void LoadMapTree(MapBlock block)
        {
            MapTreeNode[] stack = _stack;
            int sp = 0;

            MapTreeNode node = new MapTreeNode { Level = MapTreeNode.MaxLevel };
            block.Nodes = node;
            int no = 0;

            byte mask = 0x00;
            byte data = 0x00;
            while (true)
            {
                mask <<= 1;
                if (mask == 0x00)
                {
                    data = _data[_index++];
                    mask = 0x01;
                }

                if ((data & mask) == 0)
                {
                    node.No = no++;

                    if (sp == 0)
                    {
                        break;
                    }
                    node = stack[--sp];
                }
                else
                {
                    int level = node.Level - 1;
                    if (level > 0)
                    {
                        int width = 1 << level;

                        int left = node.X;
                        int right = left + width;
                        int top = node.Y;
                        int bottom = top + width;

                        MapTreeNode topLeft = new MapTreeNode { Level = level, X = left, Y = top };
                        node.TopLeftChild = topLeft;
                        stack[sp++] = topLeft;

                        MapTreeNode topRight = new MapTreeNode { Level = level, X = right, Y = top };
                        node.TopRightChild = topRight;
                        stack[sp++] = topRight;

                        MapTreeNode bottomLeft = new MapTreeNode { Level = level, X = left, Y = bottom };
                        node.BottomLeftChild = bottomLeft;
                        stack[sp++] = bottomLeft;

                        MapTreeNode bottomRight = new MapTreeNode { Level = level, X = right, Y = bottom };
                        node.BottomRightChild = bottomRight;
                        node = bottomRight;
                    }
                    else
                    {
                        int left = node.X;
                        int right = left + 1;
                        int top = node.Y;
                        int bottom = top + 1;

                        node.BottomRightChild = new MapTreeNode { No = no++, X = right, Y = bottom };
                        node.BottomLeftChild = new MapTreeNode { No = no++, X = left, Y = bottom };
                        node.TopRightChild = new MapTreeNode { No = no++, X = right, Y = top };
                        node.TopLeftChild = new MapTreeNode { No = no++, X = left, Y = top };

                        if (sp == 0)
                        {
                            break;
                        }
                        node = stack[--sp];
                    }
                }
            }

            block.NodeCount = no;
        }

        /// <summary>
        ///     Providence per tree node ID Read
        /// </summary>
        /// <param name="block">Target map block</param>
        private static void LoadNodeIds(MapBlock block)
        {
            int count = block.NodeCount;
            byte[] ids = new byte[count + 7];

            switch (block.ProvinceIdCount)
            {
                case 1:
                    // The number of provisions 1 Omitted in case of
                    break;

                case 2:
                    for (int i = 0; i < count;)
                    {
                        const byte mask = 0x01;
                        byte data = _data[_index++];
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = (byte) (data & mask);
                        data >>= 1;
                        ids[i++] = data;
                    }
                    break;

                case 3:
                case 4:
                    for (int i = 0; i < count;)
                    {
                        const byte mask = 0x03;
                        byte data = _data[_index++];
                        ids[i++] = (byte) (data & mask);
                        data >>= 2;
                        ids[i++] = (byte) (data & mask);
                        data >>= 2;
                        ids[i++] = (byte) (data & mask);
                        data >>= 2;
                        ids[i++] = data;
                    }
                    break;

                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    for (int i = 0; i < count;)
                    {
                        const byte mask = 0x0F;
                        byte data = _data[_index++];
                        ids[i++] = (byte) (data & mask);
                        data >>= 4;
                        ids[i++] = data;
                    }
                    break;

                default:
                    for (int i = 0; i < count; i++)
                    {
                        ids[i] = _data[_index++];
                    }
                    break;
            }

            block.NodeIds = ids;
        }

        /// <summary>
        ///     Read the color index for each node
        /// </summary>
        /// <param name="block">Target map block</param>
        private static void LoadNodeColors(MapBlock block)
        {
            int count = block.NodeCount;
            byte[] colors = new byte[count + 3];
            const uint mask = 0x3F;

            for (int i = 0; i < count;)
            {
                uint data = (uint) (_data[_index++] | (_data[_index++] << 8) | (_data[_index++] << 16));
                colors[i++] = (byte) (data & mask);
                colors[i++] = (byte) ((data >> 6) & mask);
                colors[i++] = (byte) ((data >> 12) & mask);
                colors[i++] = (byte) (data >> 18);
            }

            block.NodeColors = colors;
        }

        #endregion

        #region Map data expansion

        /// <summary>
        ///     Expand the map image
        /// </summary>
        private void DecodePixels()
        {
            _pixels = new MapPixels(_blockWidth * MapBlock.Width, _blockHeight * MapBlock.Height);
            _pics = new byte[(MapBlock.Width + 1) * (MapBlock.Height + 1)];

            // Decode the block at the bottom right
            DecodeBlockBottomRight();

            // Decode the bottom block
            for (int j = _blocks.Width - 2; j >= 0; j--)
            {
                DecodeBlockBottom(j);
            }

            for (int i = _blocks.Height - 2; i >= 0; i--)
            {
                // Decode the rightmost block
                DecodeBlockRight(i);

                // Decode other blocks
                for (int j = _blocks.Width - 2; j >= 0; j--)
                {
                    DecodeBlock(j, i);
                }
            }

            // Free up used buffers
            _pics = null;
        }

        /// <summary>
        ///     Decode the map block at the bottom right
        /// </summary>
        private void DecodeBlockBottomRight()
        {
            // Prepare out-of-area pixels at the bottom right
            _block = _blocks[0, _blocks.Height - 1];
            VisitTreeBottomLeft(_block.Nodes, PrepareBottomRight);

            // Prepare the rightmost out-of-area pixel
            VisitTreeLeft(_block.Nodes, PrepareRight);

            // Prepare out-of-area pixels at the bottom edge
            _block = _blocks[_blocks.Width - 1, _blocks.Height - 1];
            VisitTreeBottom(_block.Nodes, PrepareBottom);

            // Decode the tree
            VisitTree(_block.Nodes, DrawNodeBuffer);

            // Copy from the expansion buffer to an array of map pixels
            CopyBufferPixels(_blocks.Width - 1, _blocks.Height - 1);
        }

        /// <summary>
        ///     Decode the rightmost map block
        /// </summary>
        /// <param name="y">Y Coordinate</param>
        private void DecodeBlockRight(int y)
        {
            // Prepare out-of-area pixels at the bottom right
            _block = _blocks[0, y];
            VisitTreeBottomLeft(_block.Nodes, PrepareBottomRight);

            // Prepare the rightmost out-of-area pixel
            VisitTreeLeft(_block.Nodes, PrepareRight);

            // Prepare the bottom pixel
            Buffer.BlockCopy(_pixels.Data,
                (y + 1) * MapBlock.Height * _pixels.Width + (_pixels.Width - 1) * MapBlock.Width,
                _pics, MapBlock.Height * (MapBlock.Width + 1), MapBlock.Width);

            // Decode the tree
            _block = _blocks[_blocks.Width - 1, y];
            VisitTree(_block.Nodes, DrawNodeBuffer);

            // Copy from the expansion buffer to an array of map pixels
            CopyBufferPixels(_blocks.Width - 1, y);
        }

        /// <summary>
        ///     Decode the bottom map block
        /// </summary>
        /// <param name="x">X Coordinate</param>
        private void DecodeBlockBottom(int x)
        {
            // Prepare out-of-area pixels at the bottom right
            _block = _blocks[x + 1, _blocks.Height - 1];
            VisitTreeBottomLeft(_block.Nodes, PrepareBottomRight);

            // Prepare the rightmost out-of-area pixel
            int pos = MapBlock.Width;
            int index = (_blocks.Height - 1) * MapBlock.Height * _pixels.Width + (x + 1) * MapBlock.Width;
            for (int i = 0; i < MapBlock.Height; i++)
            {
                _pics[pos] = _pixels[index];
                pos += MapBlock.Width + 1;
                index += _pixels.Width;
            }

            // Prepare the bottom pixel
            _block = _blocks[x, _blocks.Height - 1];
            VisitTreeBottom(_block.Nodes, PrepareBottom);

            // Decode the tree
            VisitTree(_block.Nodes, DrawNodeBuffer);

            // Copy from the expansion buffer to an array of map pixels
            CopyBufferPixels(x, _blocks.Height - 1);
        }

        /// <summary>
        ///     Decode the map block
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        private void DecodeBlock(int x, int y)
        {
            // Decode the tree
            _block = _blocks[x, y];
            _base = y * MapBlock.Height * _pixels.Width + x * MapBlock.Width;
            VisitTree(_block.Nodes, DrawNode);
        }

        /// <summary>
        ///     Prepare out-of-area pixels at the bottom right
        /// </summary>
        /// <param name="node">Target node</param>
        private static void PrepareBottomRight(MapTreeNode node)
        {
            _pics[MapBlock.Height * (MapBlock.Width + 1) + MapBlock.Width] = (byte) (_block.NodeColors[node.No] << 2);
        }

        /// <summary>
        ///     Prepare the rightmost out-of-area pixel
        /// </summary>
        /// <param name="node">Target node</param>
        private static void PrepareRight(MapTreeNode node)
        {
            int pos = node.Y * (MapBlock.Width + 1) + MapBlock.Width;
            if (node.Level == 0)
            {
                _pics[pos] = _block.NodeColors[node.No];
                return;
            }
            int height = 1 << node.Level;
            int top = _block.NodeColors[node.No] << 8;
            int bottom = _pics[pos + height * (MapBlock.Width + 1)] << 8;
            int delta = bottom - top < SmoothingThrethold ? (bottom - top) >> node.Level : 0;
            int color = top;
            for (int i = 0; i < height; i++)
            {
                _pics[pos] = (byte) (color >> 8);
                pos += MapBlock.Width + 1;
                color += delta;
            }
        }

        /// <summary>
        ///     Prepare out-of-area pixels at the bottom edge
        /// </summary>
        /// <param name="node">Target node</param>
        private static void PrepareBottom(MapTreeNode node)
        {
            int pos = MapBlock.Height * (MapBlock.Width + 1) + node.X;
            if (node.Level == 0)
            {
                _pics[pos] = _block.NodeColors[node.No];
                return;
            }
            int width = 1 << node.Level;
            int left = _block.NodeColors[node.No] << 8;
            int right = _pics[pos + width] << 8;
            int delta = right - left < SmoothingThrethold ? (right - left) >> node.Level : 0;
            int color = left;
            for (int i = 0; i < width; i++)
            {
                _pics[pos++] = (byte) (color >> 8);
                color += delta;
            }
        }

        /// <summary>
        ///     Draw the node in the expansion buffer
        /// </summary>
        /// <param name="node">Target node</param>
        private static void DrawNodeBuffer(MapTreeNode node)
        {
            int pos = node.Y * (MapBlock.Width + 1) + node.X;
            if (node.Level == 0)
            {
                _pics[pos] = _block.NodeColors[node.No];
                return;
            }
            int width = 1 << node.Level;
            int step = MapBlock.Width + 1 - width;
            int top = _block.NodeColors[node.No] << 8;
            int bottom = _pics[pos + width * (MapBlock.Width + 1)] << 8;
            int deltaY = bottom - top < SmoothingThrethold ? (bottom - top) >> node.Level : 0;
            int left = top;
            for (int i = 0; i < width; i++)
            {
                int right = _pics[pos + width] << 8;
                int deltaX = right - left < SmoothingThrethold ? (right - left) >> node.Level : 0;
                int color = left;
                for (int j = 0; j < width; j++)
                {
                    _pics[pos++] = (byte) (color >> 8);
                    color += deltaX;
                }
                pos += step;
                left += deltaY;
            }
        }

        /// <summary>
        ///     Expand nodes into an array of map pixels
        /// </summary>
        /// <param name="node">Target node</param>
        private void DrawNode(MapTreeNode node)
        {
            int pos = _base + node.Y * _pixels.Width + node.X;
            byte[] pixels = _pixels.Data;
            if (node.Level == 0)
            {
                pixels[pos] = _block.NodeColors[node.No];
                return;
            }
            int width = 1 << node.Level;
            int step = _pixels.Width - width + 1;
            int top = _block.NodeColors[node.No] << 8;
            int bottom = pixels[pos + width * _pixels.Width] << 8;
            int deltaY = bottom - top < SmoothingThrethold ? (bottom - top) >> node.Level : 0;
            int left = top;
            switch (node.Level)
            {
                case 1:
                    for (int i = 0; i < width; i++)
                    {
                        int right = pixels[pos + width] << 8;
                        if (left == right || right - left >= SmoothingThrethold)
                        {
                            byte color = (byte) (left >> 8);
                            pixels[pos++] = color;
                            pixels[pos] = color;
                        }
                        else
                        {
                            int deltaX = (right - left) >> node.Level;
                            int color = left;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos] = (byte) (color >> 8);
                        }
                        pos += step;
                        left += deltaY;
                    }
                    break;

                case 2:
                    for (int i = 0; i < width; i++)
                    {
                        int right = pixels[pos + width] << 8;
                        if (left == right || right - left >= SmoothingThrethold)
                        {
                            byte color = (byte) (left >> 8);
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos] = color;
                        }
                        else
                        {
                            int deltaX = (right - left) >> node.Level;
                            int color = left;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos] = (byte) (color >> 8);
                        }
                        pos += step;
                        left += deltaY;
                    }
                    break;

                case 3:
                    for (int i = 0; i < width; i++)
                    {
                        int right = pixels[pos + width] << 8;
                        if (left == right || right - left >= SmoothingThrethold)
                        {
                            byte color = (byte) (left >> 8);
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos] = color;
                        }
                        else
                        {
                            int deltaX = (right - left) >> node.Level;
                            int color = left;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos] = (byte) (color >> 8);
                        }
                        pos += step;
                        left += deltaY;
                    }
                    break;

                case 4:
                    for (int i = 0; i < width; i++)
                    {
                        int right = pixels[pos + width] << 8;
                        if (left == right || right - left >= SmoothingThrethold)
                        {
                            byte color = (byte) (left >> 8);
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos] = color;
                        }
                        else
                        {
                            int deltaX = (right - left) >> node.Level;
                            int color = left;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos] = (byte) (color >> 8);
                        }
                        pos += step;
                        left += deltaY;
                    }
                    break;

                case 5:
                    for (int i = 0; i < width; i++)
                    {
                        int right = pixels[pos + width] << 8;
                        if (left == right || right - left >= SmoothingThrethold)
                        {
                            byte color = (byte) (left >> 8);
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos++] = color;
                            pixels[pos] = color;
                        }
                        else
                        {
                            int deltaX = (right - left) >> node.Level;
                            int color = left;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos++] = (byte) (color >> 8);
                            color += deltaX;
                            pixels[pos] = (byte) (color >> 8);
                        }
                        pos += step;
                        left += deltaY;
                    }
                    break;
            }
        }

        /// <summary>
        ///     Copy from the expansion buffer to an array of map pixels
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        private void CopyBufferPixels(int x, int y)
        {
            int pos = 0;
            int index = (y * _pixels.Width + x) * MapBlock.Width;
            int step = _pixels.Width;
            for (int i = 0; i < MapBlock.Width; i++)
            {
                Buffer.BlockCopy(_pics, pos, _pixels.Data, index, MapBlock.Width);
                pos += MapBlock.Width + 1;
                index += step;
            }
        }

        /// <summary>
        ///     Generate a map image
        /// </summary>
        private void CreateImage()
        {
            Image = new Bitmap(_blockWidth * MapBlock.Width, _blockHeight * MapBlock.Height,
                PixelFormat.Format8bppIndexed);

            BitmapData data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            Marshal.Copy(_pixels.Data, 0, data.Scan0, _pixels.Data.Length);
            Image.UnlockBits(data);

            // Free up used buffers
            _pixels = null;
        }

        /// <summary>
        ///     Providence ID Expand an array of
        /// </summary>
        private void ExtractIds()
        {
            ProvinceIds = new MapProvinceIds(_blockWidth * MapBlock.Width, _blockHeight * MapBlock.Height);
            _provs = new ushort[Maps.MaxProvinces];

            for (int i = 0; i < _blocks.Height; i++)
            {
                for (int j = 0; j < _blocks.Width; j++)
                {
                    ExtractBlock(j, i);
                }
            }

            // Free up used buffers
            _provs = null;
        }

        /// <summary>
        ///     Map block provision ID To deploy
        /// </summary>
        /// <param name="x">Map block unit X Coordinate</param>
        /// <param name="y">Map block unit Y Coordinate</param>
        private void ExtractBlock(int x, int y)
        {
            _block = _blocks[x, y];

            // Province ID Prepare the expansion buffer
            for (int i = 0; i < _block.NodeCount; i++)
            {
                _provs[i] = _block.ProvinceIds[_block.NodeIds[i]];
            }

            // Decode the tree
            _base = (y * MapBlock.Height * _blockWidth + x) * MapBlock.Width;
            VisitTree(_block.Nodes, FillNode);
        }

        /// <summary>
        ///     Province the node ID Expand to an array of
        /// </summary>
        /// <param name="node">Target node</param>
        private void FillNode(MapTreeNode node)
        {
            int pos = _base + node.Y * ProvinceIds.Width + node.X;
            ushort[] ids = ProvinceIds.Data;
            ushort id = _provs[node.No];
            int width = 1 << node.Level;
            int step = ProvinceIds.Width - width + 1;
            switch (node.Level)
            {
                case 0:
                    ids[pos] = id;
                    break;

                case 1:
                    ids[pos++] = id;
                    ids[pos] = id;
                    pos += step;
                    ids[pos++] = id;
                    ids[pos] = id;
                    break;

                case 2:
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos] = id;
                    pos += step;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos] = id;
                    pos += step;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos] = id;
                    pos += step;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos++] = id;
                    ids[pos] = id;
                    break;

                case 3:
                    for (int i = 0; i < width; i++)
                    {
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos] = id;
                        pos += step;
                    }
                    break;

                case 4:
                    for (int i = 0; i < width; i++)
                    {
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos] = id;
                        pos += step;
                    }
                    break;

                case 5:
                    for (int i = 0; i < width; i++)
                    {
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos++] = id;
                        ids[pos] = id;
                        pos += step;
                    }
                    break;
            }
        }

        #endregion

        #region Tree patrol

        /// <summary>
        ///     Orbit the tree in the map block
        /// </summary>
        /// <param name="node">Starting node</param>
        /// <param name="callback">Process called by leaf node</param>
        private static void VisitTree(MapTreeNode node, VisitorCallback callback)
        {
            Stack<MapTreeNode> stack = new Stack<MapTreeNode>();
            while (true)
            {
                if (node.TopLeftChild != null)
                {
                    stack.Push(node.TopLeftChild);
                    stack.Push(node.TopRightChild);
                    stack.Push(node.BottomLeftChild);
                    node = node.BottomRightChild;
                }
                else
                {
                    callback(node);
                    if (stack.Count == 0)
                    {
                        break;
                    }
                    node = stack.Pop();
                }
            }
        }

        /// <summary>
        ///     Orbit the tree in the map block (( Lower node only )
        /// </summary>
        /// <param name="node">Starting node</param>
        /// <param name="callback">Process called by leaf node</param>
        private static void VisitTreeBottom(MapTreeNode node, VisitorCallback callback)
        {
            Stack<MapTreeNode> stack = new Stack<MapTreeNode>();
            while (true)
            {
                if (node.BottomLeftChild != null)
                {
                    stack.Push(node.BottomLeftChild);
                    node = node.BottomRightChild;
                }
                else
                {
                    callback(node);
                    if (stack.Count == 0)
                    {
                        break;
                    }
                    node = stack.Pop();
                }
            }
        }

        /// <summary>
        ///     Orbit the tree in the map block ((Left node only )
        /// </summary>
        /// <param name="node">Starting node</param>
        /// <param name="callback">Process called by leaf node</param>
        private static void VisitTreeLeft(MapTreeNode node, VisitorCallback callback)
        {
            Stack<MapTreeNode> stack = new Stack<MapTreeNode>();
            while (true)
            {
                if (node.TopLeftChild != null)
                {
                    stack.Push(node.TopLeftChild);
                    node = node.BottomLeftChild;
                }
                else
                {
                    callback(node);
                    if (stack.Count == 0)
                    {
                        break;
                    }
                    node = stack.Pop();
                }
            }
        }

        /// <summary>
        ///     Tour the tree in the map block (( Lower left node only )
        /// </summary>
        /// <param name="node">Starting node</param>
        /// <param name="callback">Process called by leaf node</param>
        private static void VisitTreeBottomLeft(MapTreeNode node, VisitorCallback callback)
        {
            while (node.BottomLeftChild != null)
            {
                node = node.BottomLeftChild;
            }
            callback(node);
        }

        /// <summary>
        ///     Delegate of the process called by the leaf node
        /// </summary>
        /// <param name="node">Target node</param>
        private delegate void VisitorCallback(MapTreeNode node);

        #endregion

        #region Map image update

        /// <summary>
        ///     Update the color palette
        /// </summary>
        public void UpdateColorPalette()
        {
            ColorPalette palette = Image.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Maps.ColorPalette[i];
            }
            Image.Palette = palette;
        }

        /// <summary>
        ///     Update map image by provision
        /// </summary>
        /// <param name="ids">Providence ID Array of</param>
        public void UpdateProvinces(IEnumerable<ushort> ids)
        {
            BitmapData data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);
            foreach (ushort id in ids)
            {
                UpdateProvinceImage(data, id);
            }
            Image.UnlockBits(data);
        }

        /// <summary>
        ///     Update map image by provision
        /// </summary>
        /// <param name="id">Providence ID</param>
        public void UpdateProvince(ushort id)
        {
            BitmapData data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);
            UpdateProvinceImage(data, id);
            Image.UnlockBits(data);
        }

        /// <summary>
        ///     Update map image by provision
        /// </summary>
        /// <param name="data">Map image data</param>
        /// <param name="id">Providence ID</param>
        private void UpdateProvinceImage(BitmapData data, ushort id)
        {
            Rectangle bound = Maps.BoundBoxes[id];
            int x = bound.X >> (int) Level;
            int y = bound.Y >> (int) Level;
            int width = bound.Width >> (int) Level;
            int height = bound.Height >> (int) Level;

            if (x + width <= data.Width)
            {
                UpdateProvinceImage(data, id, x, y, width, height);
            }
            else
            {
                UpdateProvinceImage(data, id, x, y, data.Width - x, height);
                UpdateProvinceImage(data, id, 0, y, x + width - data.Width, height);
            }
        }

        /// <summary>
        ///     Update map image by provision
        /// </summary>
        /// <param name="data">Map image data</param>
        /// <param name="id">Providence ID</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        private unsafe void UpdateProvinceImage(BitmapData data, ushort id, int x, int y, int width, int height)
        {
            byte* ptr = (byte*) data.Scan0;
            int stride = data.Stride;

            int pos = y * stride + x;
            int step = stride - width;
            byte mask = Maps.ColorMasks[id];
            const byte scale = 0x3F;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (ProvinceIds[pos] == id)
                    {
                        ptr[pos] = (byte) ((ptr[pos] & scale) | mask);
                    }
                    pos++;
                }
                pos += step;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Arrangement of map pixels
    /// </summary>
    public class MapPixels
    {
        #region Public properties

        /// <summary>
        ///     Get map pixels
        /// </summary>
        /// <param name="index">Index of array</param>
        /// <returns>Map pixel</returns>
        public byte this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        /// <summary>
        ///     Get map pixels
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <returns>Map pixel</returns>
        public byte this[int x, int y]
        {
            get { return Data[y * Width + x]; }
            set { Data[y * Width + x] = value; }
        }

        /// <summary>
        ///     Arrangement of map pixels
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        ///     Width in map pixel units
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///     Height in map pixel units
        /// </summary>
        public int Height { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="width">Width of array of map pixels</param>
        /// <param name="height">Height of array of map pixels</param>
        public MapPixels(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new byte[width * height];
        }

        #endregion
    }

    /// <summary>
    ///     Map block
    /// </summary>
    public class MapBlock
    {
        #region Public properties

        /// <summary>
        ///     Province ID list
        /// </summary>
        public ushort[] ProvinceIds { get; set; }

        /// <summary>
        ///     Providence ID Number of
        /// </summary>
        public int ProvinceIdCount { get; set; }

        /// <summary>
        ///     Tree node
        /// </summary>
        public MapTreeNode Nodes { get; set; }

        /// <summary>
        ///     Number of tree nodes
        /// </summary>
        public int NodeCount { get; set; }

        /// <summary>
        ///     Providence per tree node ID
        /// </summary>
        public byte[] NodeIds { get; set; }

        /// <summary>
        ///     Color index per tree node
        /// </summary>
        public byte[] NodeColors { get; set; }

        #endregion

        #region Public constant

        /// <summary>
        ///     Map block width
        /// </summary>
        public const int Width = 32;

        /// <summary>
        ///     Map block height
        /// </summary>
        public const int Height = 32;

        #endregion
    }

    /// <summary>
    ///     Array of map blocks
    /// </summary>
    public class MapBlocks
    {
        #region Public properties

        /// <summary>
        ///     Get a map block
        /// </summary>
        /// <param name="index">Index of array</param>
        /// <returns>Map block</returns>
        public MapBlock this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        /// <summary>
        ///     Get a map block
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <returns>Map block</returns>
        public MapBlock this[int x, int y]
        {
            get { return Data[y * Width + x]; }
            set { Data[y * Width + x] = value; }
        }

        /// <summary>
        ///     Array of map blocks
        /// </summary>
        public MapBlock[] Data { get; }

        /// <summary>
        ///     Width per map block
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///     Height in map block units
        /// </summary>
        public int Height { get; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="width">Width of array of map blocks</param>
        /// <param name="height">Height of array of map blocks</param>
        public MapBlocks(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new MapBlock[width * height];
        }

        #endregion
    }

    /// <summary>
    ///     Map tree nodes
    /// </summary>
    public class MapTreeNode
    {
        #region Public properties

        /// <summary>
        ///     Node number
        /// </summary>
        public int No { get; set; }

        /// <summary>
        ///     Node level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        ///     In the map block X Coordinate
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     In the map block Y Coordinate
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        ///     Lower right child node
        /// </summary>
        public MapTreeNode BottomRightChild { get; set; }

        /// <summary>
        ///     Lower left child node
        /// </summary>
        public MapTreeNode BottomLeftChild { get; set; }

        /// <summary>
        ///     Upper right child node
        /// </summary>
        public MapTreeNode TopRightChild { get; set; }

        /// <summary>
        ///     Upper left child node
        /// </summary>
        public MapTreeNode TopLeftChild { get; set; }

        #endregion

        #region Public constant

        /// <summary>
        ///     Maximum node level
        /// </summary>
        public const int MaxLevel = 5;

        #endregion
    }

    /// <summary>
    ///     Providence ID Array of
    /// </summary>
    public class MapProvinceIds
    {
        #region Public properties

        /// <summary>
        ///     Providence ID To get
        /// </summary>
        /// <param name="index">Index of array</param>
        /// <returns>Providence ID</returns>
        public ushort this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        /// <summary>
        ///     Providence ID To get
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <returns>Providence ID</returns>
        public ushort this[int x, int y]
        {
            get { return Data[y * Width + x]; }
            set { Data[y * Width + x] = value; }
        }

        /// <summary>
        ///     Providence ID Array of
        /// </summary>
        public ushort[] Data { get; }

        /// <summary>
        ///     Width in map pixel units
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///     Height in map pixel units
        /// </summary>
        public int Height { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="width">Width of array of map pixels</param>
        /// <param name="height">Height of array of map pixels</param>
        public MapProvinceIds(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new ushort[width * height];
        }

        #endregion
    }
}
